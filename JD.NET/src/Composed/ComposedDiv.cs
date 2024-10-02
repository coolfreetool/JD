using System;
using System.Runtime.Serialization;

namespace JDSpace
{
    /// <summary>
    /// A / B representation class
    /// </summary>
    [Serializable]
    internal class ComposedDiv : TwoMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return A.DoubleValue / B.DoubleValue; }
        }
        /// <summary>
        /// Create ComposedDiv with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public ComposedDiv(ComposedConstant a, ComposedConstant b) : base(a, b) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedDiv serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public ComposedDiv(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }
}