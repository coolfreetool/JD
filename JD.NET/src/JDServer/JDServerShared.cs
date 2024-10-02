using System.Collections.Generic;

namespace JDSpace
{
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
}