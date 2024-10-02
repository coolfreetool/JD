using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDModel solving service interface. Encapsulation for local and
    /// remote IJDSolver objects.
    /// </summary>
    public interface IJDSolverWrapper
    {
        /// <summary>
        /// Solve inserted model. Return solved model
        /// with out parameter. Retrun false if not success.
        /// </summary>
        bool Solve(JDModel jdMdl, out JDModel solvedMdl);
        /// <summary>
        /// Set logging callback object.
        /// </summary>
        void SetLogger(Logger logger);
        /// <summary>
        /// Reset used solver.
        /// </summary>
        void Reset();
    }
}