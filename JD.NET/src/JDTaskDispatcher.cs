using System;
using System.Collections.Generic;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JD task cispatcher class
    /// </summary>
    [Serializable]
    public class JDTaskDispatcher : IComputeCondition
    {
        /// <summary>
        /// State table
        /// </summary>
        StateTable stateTable;

        /// <summary>
        /// Create JDTaskDispatcher with initial state table
        /// </summary>
        /// <param name="stateTable"></param>
        public JDTaskDispatcher(StateTable stateTable)
        {
            this.stateTable = stateTable;
        }

        /// <summary>
        /// Return possible IConditionResults
        /// </summary>
        /// <param name="tasks">Dictionary with tasks</param>
        /// <returns>State </returns>
        public ECondResult CanSolve(Dictionary<int, ETaskState> tasks)
        {
            if (tasks == null) throw new JDException("Dictionary with tasks cannot be null.");

            HashSet<Tuple<int, ETaskState>> state = new HashSet<Tuple<int, ETaskState>>();

            foreach (var task in tasks)
            {
                state.Add(new Tuple<int, ETaskState>(task.Key, task.Value));
            }

            if (stateTable.ContainsKey(state))
            {
                return stateTable[state];
            }
            else
            {
                //pripadne by se vracel akci default(ECondResult), coz je WAIT, ale takto je to bezpecnejsi
                throw new JDException("Invalid state encountered.");
            }

        }
    }
}