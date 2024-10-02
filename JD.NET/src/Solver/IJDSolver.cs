using System.Collections.Generic;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Common external solver interface.
    /// </summary>
    public interface IJDSolver
    {
        /// <summary>
        /// Returns true if SOS1 constraints are supported by the solver.
        /// </summary>
        bool SupportsSOS1 { get; }

        /// <summary>
        /// Returns true if SOS2 constraints are supported by the solver.
        /// </summary>
        bool SupportsSOS2 { get; }

        /// <summary>
        /// Set logger object (callback).
        /// </summary>
        void SetLogger(Logger logger);

        /// <summary>
        /// Get logger object (callback).
        /// </summary>
        Logger GetLogger();

        /// <summary>
        /// Update optim. variables set in solver (call befor adding constraints).
        /// </summary>
        void Update();

        /// <summary>
        /// Clear solver state (remove all variables and constraints).
        /// </summary>
        void Reset();

        /// <summary>
        /// Add SOS constraint to solver.
        /// </summary>
        /// <param name="sos">SOS constraint</param>
        void AddSOSConstr(SOSConstr sos);

        /// <summary>
        /// Set inserted model objective function.
        /// </summary>
        void SetObjective(ScLinExpr obj, int sense);

        /// <summary>
        /// Solve inserted model with given parameters.
        /// </summary>
        void Optimize(JDParams pars);

        /// <summary>
        /// Get resolved variable value (or null if not solved).
        /// </summary>
        double? GetVarValue(int id);

        // testing methods
        /// <summary>
        /// Add list of scalar variables
        /// </summary>
        /// <param name="vars">List of scalar variables</param>
        void AddScVars(List<ScVar> vars);
        /// <summary>
        /// Add list of scalar constraints
        /// </summary>
        /// <param name="contstrs">List of scalar constraints</param>
        void AddConstrs(List<ScConstr> contstrs);

        /// <summary>
        /// Export model to file
        /// </summary>
        /// <param name="filenameWithoutExtension">File name without extension</param>
        /// <param name="fileType">File type (mps, lp)</param>
        /// <returns>true if succeeded, false otherwise.</returns>
        bool Export(string filenameWithoutExtension, string fileType);
    }
}