using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using JDUtils;

namespace JDSpace
{
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
}