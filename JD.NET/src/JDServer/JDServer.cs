using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
}
