using System;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDModel and ETaskState couple.
    /// </summary>
    [Serializable]
    public class JDModelWithState
    {
        /// <summary>
        /// JD model
        /// </summary>
        public JDModel mdl;
        /// <summary>
        /// Compute state
        /// </summary>
        public ETaskState state;
    }
}