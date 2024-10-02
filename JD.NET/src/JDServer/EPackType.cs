namespace JDSpace
{
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
}