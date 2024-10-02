using System;
using System.Collections.Generic;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Model SOS constraint reprezentation.
    /// </summary>
    [Serializable]
    public class SOSConstr
    {
        /// <summary>
        /// 1 or 2 - SOS constr. type.
        /// </summary>
        public List<ScVar> Vars;

        /// <summary>
        /// SOS constraint weights array.
        /// </summary>
        public double[] Weights;

        /// <summary>
        /// SOS constraint type (1 or 2).
        /// </summary>
        public int Type;

        /// <summary>
        /// Create SOS constraint
        /// </summary>
        /// <param name="vars">List of scalar variables</param>
        /// <param name="weights">Weights</param>
        /// <param name="type">SOS constraint type (1 or 2)</param>
        public SOSConstr(List<ScVar> vars, double[] weights, int type)
        {
            Vars = vars;
            Weights = weights;
            if (type < 1 || type > 2)
                throw new JDException(string.Format("The type={0} of SOS constraint is not supported. Only type 1 and 2 are supported", type));
            Type = type;
        }
    }
}