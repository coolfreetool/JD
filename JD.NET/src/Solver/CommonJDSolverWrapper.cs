using System;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Local IJDSolver wrapper.
    /// </summary>
    public class CommonJDSolverWrapper : IJDSolverWrapper
    {
        /// <summary>
        /// Used solver.
        /// </summary>
        IJDSolver Solver;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="solver">Used solver.</param>
        public CommonJDSolverWrapper(IJDSolver solver)
        {
            Solver = solver;
        }

        #region << IJDSolverWrapper IMPLEMENTATION >>
        /// <summary>
        /// Solve JD model
        /// </summary>
        /// <param name="jdMdl">JD model to solve</param>
        /// <param name="solvedMdl">Solved JD model</param>
        /// <returns>True</returns>
        public bool Solve(JDModel jdMdl, out JDModel solvedMdl)
        {
            Solver.Solve(jdMdl);
            solvedMdl = jdMdl; // the same instance in this case (fast)
            return true; // easy case - always retrun true.
        }

        /// <summary>
        /// Reset solver state
        /// </summary>
        public void Reset()
        {
            Solver.Reset();
        }

        /// <summary>
        /// Set logger to solver
        /// </summary>
        /// <param name="logger">Logger object</param>
        public void SetLogger(Logger logger)
        {
            Solver.SetLogger(logger);
        }

        #endregion << IJDSolverWrapper IMPLEMENTATION >>

        /// <summary>
        /// Solver label (local).
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} (local)", Solver.GetType().Name);
        }
    }
}