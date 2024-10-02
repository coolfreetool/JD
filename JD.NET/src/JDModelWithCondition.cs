using System;

namespace JDSpace
{
    /// <summary>
    /// JDModel and IComputeCondition couple.
    /// </summary>
    [Serializable]
    public class JDModelWithCondition
    {
        /// <summary>
        /// JD model
        /// </summary>
        public JDModel mdl;
        /// <summary>
        /// Compute condition
        /// </summary>
        public IComputeCondition cond;
    }
}