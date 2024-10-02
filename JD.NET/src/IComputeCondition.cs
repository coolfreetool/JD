using System.Collections.Generic;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Condition depending on task states.
    /// </summary>    
    public interface IComputeCondition
    {
        /// <summary>
        /// Returns decision whether solve or not.
        /// </summary>
        ECondResult CanSolve(Dictionary<int, ETaskState> tasks);
    }
}