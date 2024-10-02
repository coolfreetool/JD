namespace JDUtils
{
    /// <summary>
    /// Mozne stavy tasku.
    /// </summary>
    public enum ETaskState
    {
        /// <summary>
        /// Task state - waiting
        /// </summary>
        WAITING = 0,
        /// <summary>
        /// Task state - In progress
        /// </summary>
        IN_PROGRESS = 1,
        /// <summary>
        /// Task state - Successfully solved
        /// </summary>
        SOLVED_OK = 2,
        /// <summary>
        /// Task state - Solving error
        /// </summary>
        SOLVED_ERR = 3,
        /// <summary>
        /// Task state - refused
        /// </summary>
        REFUSED = 4,
    }
}