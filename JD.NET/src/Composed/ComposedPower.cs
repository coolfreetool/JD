using System;
using System.Runtime.Serialization;

namespace JDSpace
{
    /// <summary>
    /// A^B representation class
    /// </summary>
    [Serializable]
    internal class ComposedPower : TwoMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return Math.Pow(A.DoubleValue, B.DoubleValue); }
        }
        /// <summary>
        /// Create ComposedPower with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public ComposedPower(ComposedConstant a, ComposedConstant b) : base(a, b) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedPower serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public ComposedPower(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }
}