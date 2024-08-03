using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDSolver(s) remote access server class.
    /// </summary>
    public class JDServer
    {
        /// <summary>
        /// Listen for clients requiring registration.
        /// </summary>
        TcpListener _listener;
        /// <summary>
        /// Shared solvers (between clients).
        /// </summary>
        JDServerShared Shared;

        /// <summary>
        /// Create instance of JDServer with preset port
        /// </summary>
        /// <param name="port"></param>
        public JDServer(int port)
        {
            // find available local solvers
            List<IJDSolver> solvers = JD.GetAvailableSolvers();
            foreach (IJDSolver solver in solvers)
            {
                Console.WriteLine(solver);
            }
            // create solvers map (use solvers objects type names)
            Dictionary<string, IJDSolver> solversDic = new Dictionary<string, IJDSolver>();
            foreach (IJDSolver slr in solvers)
            {
                solversDic.Add(slr.GetType().Name, slr);
            }
            Shared = new JDServerShared(solversDic);
            // init listener for specific port
            _listener = new TcpListener(port);
            _listener.Start();
            while (true)
            {
                // create new thread for each registered client
                Socket socket = _listener.AcceptSocket();
                Console.WriteLine("Connected: {0}", socket.RemoteEndPoint);
                JDServerSession session = new JDServerSession(Shared, socket);
                Thread thr = new Thread(new ThreadStart(session.Service));
                thr.Start();
            }
        }
    }

    /// <summary>
    /// Shared solvers encapsulation class.
    /// </summary>
    public class JDServerShared
    {
        /// <summary>
        /// Solvers (map over solver type names).
        /// </summary>
        private Dictionary<string, IJDSolver> _solvers;
        /// <summary>
        /// Solvers availability (map over solver type names).
        /// </summary>
        private Dictionary<string, bool> _solverAvailable;
        /// <summary>
        /// Shared server instance
        /// </summary>
        /// <param name="solvers">Dictionary with available solvers</param>
        public JDServerShared(Dictionary<string, IJDSolver> solvers)
        {
            _solvers = solvers;
            // create solvers availability map
            _solverAvailable = new Dictionary<string, bool>(solvers.Count);
            foreach (KeyValuePair<string, IJDSolver> pair in _solvers)
            {
                _solverAvailable.Add(pair.Key, true);
            }
        }

        /// <summary>
        /// Try to make solver available for other clients 
        /// when its using is finished.
        /// </summary>
        public bool TryFreeSolver(IJDSolver solverToFree)
        {
            bool ok = false;
            foreach (KeyValuePair<string, IJDSolver> pair in _solvers)
            {
                if (pair.Value == solverToFree)
                {
                    _solverAvailable[pair.Key] = true;
                    ok = true;
                }
            }
            return ok;
        }

        /// <summary>
        /// Get required solver according solverLable parametr.
        /// </summary>
        public bool TryGetSolver(string solverLabel, out IJDSolver solver)
        {
            bool ok = false;
            solver = null;
            if (_solvers.ContainsKey(solverLabel))
            {
                if (_solverAvailable[solverLabel])
                {
                    solver = _solvers[solverLabel];
                    solver.Reset();
                    ok = true;
                    _solverAvailable[solverLabel] = false;
                }
            }
            return ok;
        }

        /// <summary>
        /// Get available solvers types names list.
        /// </summary>
        public List<string> GetAvailableSolversTypesNames()
        {
            List<string> availSolversList = new List<string>();
            foreach (KeyValuePair<string, bool> pair in _solverAvailable)
            {
                if (pair.Value) availSolversList.Add(pair.Key);
            }
            return availSolversList;
        }
    }

    /// <summary>
    /// TransferPack smart serialization head byte. It describes
    /// TransferPack content field for faster explicit serialization.
    /// </summary>
    [Flags]
    internal enum EPackContent : byte
    {
        /// <summary>
        /// Pack contains not null Model field.
        /// </summary>
        MODEL = 1,
        /// <summary>
        /// Pack contains not null LogItem field.
        /// </summary>
        LOG_ITEM = 2,
        /// <summary>
        /// Pack contains not null Data field.
        /// </summary>
        DATA = 4,
    }

    /// <summary>
    /// TransferPack object characterization enum.
    /// </summary>
    public enum EPackType : byte
    {
        /// <summary>
        /// EPackType - Show available solvers 
        /// </summary>
        SHOW_AVAILABLE_SOLVERS = 1,
        /// <summary>
        /// EPackType - Available solvers
        /// </summary>
        AVAILABLE_SOLVERS = 2,
        /// <summary>
        /// EPackType - Message
        /// </summary>
        MESSAGE = 3,
        /// <summary>
        /// EPackType - Solver select
        /// </summary>
        SELECT_SOLVER = 4,
        /// <summary>
        /// EPackType - Model solved
        /// </summary>
        SOLVED_MODEL = 5,
        /// <summary>
        /// EPackType - Solve model
        /// </summary>
        SOLVE_MODEL = 6,
        /// <summary>
        /// EPackType - Log item
        /// </summary>
        LOG_ITEM = 7,
        /// <summary>
        /// EPackType - State  OK
        /// </summary>
        OK = 8,
        /// <summary>
        /// EPackType - State Refused
        /// </summary>
        REFUSED = 9,
        /// <summary>
        /// EPackType - Session end
        /// </summary>
        END_SESSION = 10,
        /// <summary>
        /// EPackType - Solver reser
        /// </summary>
        RESET_SOLVER = 11,
    }

    /// <summary>
    /// JDServer-JDClient data transfer object class.
    /// </summary>
    [Serializable]
    public class TransferPack : ISerializable
    {
        /// <summary>
        /// Describes pack content for smart explicit serialization.
        /// </summary>
        private EPackContent _contentType;
        /// <summary>
        /// Pack type description (key for processing way decission).
        /// </summary>
        public EPackType PackType { get; private set; }
        /// <summary>
        /// JDModel (solved or to solver).
        /// </summary>
        public JDModel Model { get; private set; }
        /// <summary>
        /// JDUtils.Logger log item.
        /// </summary>
        public LogItem LogItem { get; private set; }
        /// <summary>
        /// Any serializable object to transfer.
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Instance of TransferPack
        /// </summary>
        /// <param name="packType">Pack type</param>
        /// <param name="model">JD model (null default)</param>
        /// <param name="logItem">Log item (null default)</param>
        /// <param name="data">Data object (null default)</param>
        public TransferPack(
            EPackType packType,
            JDModel model = null,
            LogItem logItem = null,
            object data = null)
        {
            _contentType = (EPackContent)0;
            PackType = packType;
            Model = model; if (model != null) { _contentType |= EPackContent.MODEL; }
            LogItem = logItem; if (logItem != null) _contentType |= EPackContent.LOG_ITEM;
            Data = data; if (data != null) _contentType |= EPackContent.DATA;
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// Explicit serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public TransferPack(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            _contentType = (EPackContent)sr.ReadByte();
            PackType = (EPackType)sr.ReadByte();
            if (_contentType.HasFlag(EPackContent.MODEL))
            {
                //DateTime t1 = DateTime.Now;
                //Console.WriteLine("JDModel reading..");
                Model = (JDModel)sr.ReadObject();
                //DateTime t2 = DateTime.Now;
                //Console.WriteLine("..{0} s.", (t2 - t1).TotalSeconds);
            }
            if (_contentType.HasFlag(EPackContent.LOG_ITEM)) LogItem = (LogItem)sr.ReadObject();
            if (_contentType.HasFlag(EPackContent.DATA)) Data = sr.ReadObject();
        }
        /// <summary>
        /// Standard explicit serialization method.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write((byte)_contentType);
            sw.Write((byte)PackType);
            if (_contentType.HasFlag(EPackContent.MODEL))
            {
                //DateTime t1 = DateTime.Now;
                //Console.WriteLine("JDModel writing..");
                sw.WriteObject(Model);
                //DateTime t2 = DateTime.Now;
                //Console.WriteLine("..{0} s.", (t2 - t1).TotalSeconds);
            }
            if (_contentType.HasFlag(EPackContent.LOG_ITEM)) sw.WriteObject(LogItem);
            if (_contentType.HasFlag(EPackContent.DATA)) sw.WriteObject(Data);

            sw.AddToInfo(info);
        }
        #endregion << EXPLICIT SERIALIZATION >>
    }

    /// <summary>
    /// One server session (one client serving) class representation.
    /// </summary>
    public class JDServerSession : ILogClient
    {
        /// <summary>
        /// Server shared resources reference object.
        /// </summary>
        JDServerShared Shared;
        /// <summary>
        /// Used socet in session.
        /// </summary>
        Socket Socket;
        /// <summary>
        /// Used stream.
        /// </summary>
        Stream Stream;
        /// <summary>
        /// Binary formatter used for data exchange serialization.
        /// </summary>
        BinaryFormatter Bf;
        /// <summary>
        /// Solver used in this session.
        /// </summary>
        IJDSolver UsedSolver;

        /// <summary>
        /// JD server session instance
        /// </summary>
        /// <param name="shared">Shared server resources</param>
        /// <param name="socket">Socked used in session</param>
        public JDServerSession(JDServerShared shared, Socket socket)
        {
            Bf = new BinaryFormatter();
            Shared = shared;
            Socket = socket;
            UsedSolver = null;
        }

        /// <summary>
        /// Try to unreserve solver.
        /// </summary>
        private bool _freeSolver()
        {
            bool ok = false;
            lock (Shared)
            {
                ok = Shared.TryFreeSolver(UsedSolver);
            }
            if (ok) UsedSolver = null;
            return ok;
        }

        /// <summary>
        /// Send to client currently available solvers offer.
        /// </summary>
        private void _sendAvailableSolvers()
        {
            lock (Shared)
            {
                List<string> availSolvers = Shared.GetAvailableSolversTypesNames();
                TransferPack pack = new TransferPack(EPackType.AVAILABLE_SOLVERS, data: availSolvers);
                Bf.Serialize(Stream, pack);
            }
        }

        /// <summary>
        /// Select solver according to accepted pack.
        /// </summary>
        private void _selectSolver(TransferPack pack)
        {
            string reqSolver = (string)pack.Data;
            bool ok = false;
            lock (Shared)
            {
                ok = Shared.TryGetSolver(reqSolver, out UsedSolver);
            }
            if (ok)
            {
                _answerOk();
            }
            else
            {
                _answerRefused("Solver used by another client");
            }
        }

        /// <summary>
        /// Reset currently reserved solver.
        /// </summary>
        private void _resetSolver()
        {
            if (UsedSolver != null)
            {
                UsedSolver.Reset();
                _answerOk();
            }
            else
            {
                _answerRefused("No solver set");
            }
        }

        /// <summary>
        /// Solve JDModel accepted in inserted pack.
        /// </summary>
        private void _solveModel(TransferPack pack)
        {
            JDModel mdl = pack.Model;
            if (UsedSolver != null)
            {
                Logger logger = new Logger();
                logger.Register(this, Logger.AllFlags);
                UsedSolver.SetLogger(logger);
                UsedSolver.Solve(mdl);
                TransferPack retPack = new TransferPack(EPackType.SOLVED_MODEL, model: mdl);
                Bf.Serialize(Stream, retPack);
            }
            else
            {
                _answerRefused("No solver set");
            }
        }

        /// <summary>
        /// Send fast positive replay.
        /// </summary>
        private void _answerOk()
        {
            TransferPack pack = new TransferPack(EPackType.OK);
            Bf.Serialize(Stream, pack);
        }

        /// <summary>
        /// Send fast negative replay.
        /// </summary>
        private void _answerRefused(string message = null)
        {
            TransferPack pack = new TransferPack(EPackType.REFUSED, data: message);
            Bf.Serialize(Stream, pack);
        }

        /// <summary>
        /// Process pack according to decision field (PackType).
        /// </summary>
        private void _servePack(TransferPack pack)
        {
            try
            {
                switch (pack.PackType)
                {
                    case EPackType.SHOW_AVAILABLE_SOLVERS:
                        _sendAvailableSolvers();
                        break;
                    case EPackType.SELECT_SOLVER:
                        _selectSolver(pack);
                        break;
                    case EPackType.SOLVE_MODEL:
                        _solveModel(pack);
                        break;
                    case EPackType.RESET_SOLVER:
                        _resetSolver();
                        break;
                    default:
                        _answerRefused("Nothing to do with " + pack.PackType.ToString() + " pack type!");
                        break;
                }
            }
            catch (Exception ex)
            {
                _answerRefused(ex.Message);
            }
        }

        /// <summary>
        /// Session main (thread) method.
        /// </summary>
        public void Service()
        {
            try
            {
                Stream = new NetworkStream(Socket);
                while (true)
                {
                    TransferPack pack = (TransferPack)Bf.Deserialize(Stream);
                    if (pack.PackType == EPackType.END_SESSION) break;
                    _servePack(pack);
                }
                Stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Disconnected: {0}",
                                        Socket.RemoteEndPoint);
                if (UsedSolver != null) _freeSolver();
                Socket.Close();
            }
        }

        /// <summary>
        /// ILogClient interface method implementation.
        /// </summary>
        public void Log(LogItem logItem)
        {
            TransferPack pack = new TransferPack(EPackType.LOG_ITEM, logItem: logItem);
            Bf.Serialize(Stream, pack);
        }
    }

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
