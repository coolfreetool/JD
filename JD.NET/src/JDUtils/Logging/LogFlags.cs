using System;

namespace JDUtils
{
    /// <summary>
    /// Possible log flags enum
    /// </summary>
    [Flags]
    public enum LogFlags
    {
        /// <summary>
        /// Log level - Solution solver
        /// </summary>
        SOLUTION_SOLVER = 1,
        /// <summary>
        /// Log level - Modeler
        /// </summary>
        MODELER = 2,
        /// <summary>
        /// Log level - Optimizer
        /// </summary>
        OPTIMIZER = 4,
        /// <summary>
        /// Log level - Parsing results
        /// </summary>
        RESULTS_PARSER = 8,
        /// <summary>
        /// Log level - JD
        /// </summary>
        JD = 16,
        /// <summary>
        /// Log level - Model updating
        /// </summary>
        MODEL_UPDATER = 32,
        /// <summary>
        /// Log level - Planning water production
        /// </summary>
        WATTER_PLANNER = 64
    }
}