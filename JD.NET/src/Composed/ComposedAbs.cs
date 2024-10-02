using System;
using System.Runtime.Serialization;

namespace JDSpace
{
    /// <summary>
    /// Absolute value representation class;
    /// </summary>
    [Serializable]
    internal class ComposedAbs : OneMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return Math.Abs(A.DoubleValue); }
        }

        /// <summary>
        /// New ComposedAbs with predefined value
        /// </summary>
        /// <param name="a">Composed constant</param>
        public ComposedAbs(ComposedConstant a) : base(a) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedAbs serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ComposedAbs(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }
}