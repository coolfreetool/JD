using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Two composed constants algebraic operation representation base class.
    /// </summary>
    [Serializable]
    internal abstract class TwoMemberComposedConst : OneMemberComposedConst, ISerializable
    {
        /// <summary>
        /// Second operand.
        /// </summary>
        protected ComposedConstant B;

        /// <summary>
        /// ComposedConstant method overriding.
        /// </summary>
        public override void RegisterNamedMembers(ref IDictionary<string, NamedConst> namedMembersList)
        {
            base.RegisterNamedMembers(ref namedMembersList);
            B.RegisterNamedMembers(ref namedMembersList);
        }

        /// <summary>
        /// Implementation of inherited abstract method from ComposedConstant.
        /// </summary>
        /// <param name="namedMembersList">Input list to clean duplicities.</param>
        internal override void ClearDupl(ref IDictionary<string, NamedConst> namedMembersList)
        {
            base.ClearDupl(ref namedMembersList);
            ComposedExtenders.ClearDupls(() => B, (x) => B = x, namedMembersList);
        }

        /// <summary>
        /// Create TwoMemberComposedConst with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public TwoMemberComposedConst(ComposedConstant a, ComposedConstant b) : base(a) { B = b; }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// TwoMemberComposedConst serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public TwoMemberComposedConst(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            A = sr.ReadObject() as ComposedConstant;
            B = sr.ReadObject() as ComposedConstant;
        }

        /// <summary>
        /// TwoMemberComposedConst deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.WriteObject(A);
            sw.WriteObject(B);
            sw.AddToInfo(info);
        }
        #endregion
    }
}