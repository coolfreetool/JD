namespace JDSpace
{
    /// <summary>
    /// Possible IComputeCondition results.
    /// </summary>
    public enum ECondResult
    {
        /// <summary>
        /// Condition state - Wait
        /// </summary>
        WAIT = 0,
        /// <summary>
        /// Condition state - Solve
        /// </summary>
        SOLVE = 1,
        /// <summary>
        /// Condition state - Refuse
        /// </summary>
        REFUSE = 2,
    }
}