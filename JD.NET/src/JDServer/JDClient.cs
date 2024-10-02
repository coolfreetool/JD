using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDSolver(s) remote access client class.
    /// </summary>
    public class JDClient : IJDSolverWrapper
    {
        /// <summary>
        /// Server ip address.
        /// </summary>
        string ServerHostName;
        /// <summary>
        /// Server app port.
        /// </summary>
        int Port;
        /// <summary>
        /// Communication stream.
        /// </summary>
        Stream Stream;
        /// <summary>
        /// Data exchange serialization formatter.
        /// </summary>
        BinaryFormatter Bf;
        /// <summary>
        /// .NET TcpClient class instance. 
        /// </summary>
        TcpClient TcpClient;
        /// <summary>
        /// Client on/off flag.
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// Currently used solver label.
        /// </summary>
        public string UsedSolverLabel { get; private set; }
        /// <summary>
        /// Logger - needed for JDModel solving log items distributing.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Standard class constructor.
        /// </summary>
        /// <param name="serverHostName">Server ip address "xxx.xxx.xxx.xxx"</param>
        /// <param name="port">16bit required server port</param>
        public JDClient(string serverHostName, int port, bool selectFirstSolver = true)
        {
            Port = port;
            ServerHostName = serverHostName;
            IsInitialized = false;
            Bf = new BinaryFormatter();
            // reload assemblies if dependencies problem appears
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (selectFirstSolver) Start();
        }

        /// <summary>
        /// Reload assemblies from domain.
        /// </summary>
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly ayResult = null;
            string sShortAssemblyName = args.Name.Split(',')[0];
            System.Reflection.Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly ayAssembly in ayAssemblies)
            {
                if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                {
                    ayResult = ayAssembly;
                    break;
                }
            }
            return ayResult;
        }

        /// <summary>
        /// Find and return all available JDSolvers (reserve and get as JDClients).
        /// Return empty list if no solvers available.
        /// </summary>
        /// <param name="serverHostName">Server ip address.</param>
        /// <param name="port">Server port.</param>
        public static List<JDClient> GetAllAvailableSolversClients(string serverHostName, int port)
        {
            List<JDClient> clients = new List<JDClient>();
            try
            {
                JDClient cli1 = new JDClient(serverHostName, port);
                cli1.Start();
                List<string> availSolvers = cli1.GetAvalilableSolvers();
                cli1.Close();
                foreach (string solverStr in availSolvers)
                {
                    JDClient client = InitClientWithSolver(serverHostName, port, solverStr);
                    if (client != null) clients.Add(client);
                }
                return clients;
            }
            catch
            {
                return clients;
            }
        }

        /// <summary>
        /// Try to reserve solver of inserted label and qet it encapsulated with JDClient or return null.
        /// </summary>
        /// <param name="serverHostname">Server host name</param>
        /// <param name="port">Server port</param>
        /// <param name="solverLabel">Solver labe;</param>
        /// <returns>JDClient</returns>
        public static JDClient InitClientWithSolver(string serverHostname, int port, string solverLabel)
        {
            JDClient cli = new JDClient(serverHostname, port);
            cli.Start();
            bool succ = cli.TrySelectSolver(solverLabel);
            if (succ)
            {
                return cli;
            }
            else
            {
                cli.Close();
                return null;
            }
        }

        /// <summary>
        /// Shut down current client.
        /// </summary>
        public void Close()
        {
            if (Stream != null)
            {
                TransferPack pack = new TransferPack(EPackType.END_SESSION);
                Bf.Serialize(Stream, pack);
                Stream.Flush();
                Stream.Close();
            }
            if (TcpClient != null) TcpClient.Close();
            IsInitialized = false;
        }

        /// <summary>
        /// Init current client (init TcpClient and Stream fields).
        /// </summary>
        /// <param name="getFirst">Use first avaialble solver to solve problem (true default)</param>
        public void Start(bool getFirst = true)
        {
            TcpClient = new TcpClient(ServerHostName, Port);
            Stream = TcpClient.GetStream();
            IsInitialized = true;
            if (getFirst)
            {
                // reserve first free solver
                List<string> solvers = GetAvalilableSolvers();
                if (solvers.Count == 0)
                    throw new JDException("No solvers available!");
                if (solvers.Count > 0)
                {
                    bool succ = TrySelectSolver(solvers[0]);
                    if (!succ)
                        throw new JDException("Can not reserve {0} solver!", solvers[0]);
                }
            }
        }

        /// <summary>
        /// Get currently available solvers labels list.
        /// <summary>
        public List<string> GetAvalilableSolvers()
        {
            TransferPack pack = new TransferPack(EPackType.SHOW_AVAILABLE_SOLVERS);
            Bf.Serialize(Stream, pack);
            TransferPack retPack = (TransferPack)Bf.Deserialize(Stream);
            List<string> list = (List<string>)retPack.Data;
            return list;
        }

        /// <summary>
        /// Try to reserve solver of inserted label. Return false if not success.
        /// </summary>
        /// <param name="solverLabel">Solver label</param>
        public bool TrySelectSolver(string solverLabel)
        {
            TransferPack pack = new TransferPack(EPackType.SELECT_SOLVER, data: solverLabel);
            Bf.Serialize(Stream, pack);
            TransferPack retPack = (TransferPack)Bf.Deserialize(Stream);
            if (retPack.PackType == EPackType.OK)
            {
                UsedSolverLabel = solverLabel;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Try reset reserved solver. Retrun false if not success.
        /// </summary>
        public bool TryResetSolver()
        {
            TransferPack pack = new TransferPack(EPackType.RESET_SOLVER);
            Bf.Serialize(Stream, pack);
            TransferPack retPack = (TransferPack)Bf.Deserialize(Stream);
            if (retPack.PackType == EPackType.OK) { return true; } else { return false; }
        }

        /// <summary>
        /// Try to solve inserted JDModel (mdl). Return solved JDModel using solvedMdl out
        /// parametr. Return false if not success and give error message using "message"
        /// out parameter.
        /// </summary>
        /// <param name="mdl">JD Model to solve</param>
        /// <param name="solvedMdl">Solved model</param>
        /// <param name="message">Message</param>
        public bool TrySolveModel(JDModel mdl, out JDModel solvedMdl, out string message)
        {
            bool ok = false;
            solvedMdl = null;
            message = null;
            TransferPack pack = new TransferPack(EPackType.SOLVE_MODEL, model: mdl);
            Bf.Serialize(Stream, pack);
            while (true)
            {
                TransferPack retPack = (TransferPack)Bf.Deserialize(Stream);
                if (retPack.PackType == EPackType.LOG_ITEM)
                {
                    if (_logger != null) _logger.Log(retPack.LogItem);
                }
                else if (retPack.PackType == EPackType.SOLVED_MODEL)
                {
                    solvedMdl = retPack.Model;
                    ok = true;
                    break;
                }
                else
                {
                    break;
                }
            }
            return ok;
        }

        #region << IJDSolverWrapper IMPLEMENTATION >>

        /// <summary>
        /// Solve JDModel and return model with results
        /// </summary>
        /// <param name="jdMdl">Model to solve</param>
        /// <param name="solvedModel">Solved model</param>
        /// <returns>True if model is solved, false otherwise</returns>
        public bool Solve(JDModel jdMdl, out JDModel solvedModel)
        {
            string msg = "";
            bool solved = TrySolveModel(jdMdl, out solvedModel, out msg);
            return solved;
        }

        /// <summary>
        /// Set logger for logging callbacks.
        /// </summary>
        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reset currently used solver.
        /// </summary>
        public void Reset()
        {
            TryResetSolver();
        }

        #endregion << IJDSolverWrapper IMPLEMENTATION >>

        /// <summary>
        /// Solve JDModel 
        /// </summary>
        /// <param name="jdMdl">Model to solve</param>
        /// <returns>Solved model</returns>
        public JDModel Solve(JDModel jdMdl)
        {
            JDModel mdlSolved = new JDModel();
            bool solved = Solve(jdMdl, out mdlSolved);
            if (solved)
            {
                return mdlSolved;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// UsedSolver (remote: sever ip addr./port)
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} (remote: {1}/{2})", UsedSolverLabel, ServerHostName, Port);
        }
    }
}