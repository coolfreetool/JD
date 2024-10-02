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
    internal abstract class OneMemberComposedConst : ComposedConstant, ISerializable
    {
        /// <summary>
        /// First operand.
        /// </summary>
        protected ComposedConstant A;

        /// <summary>
        /// ComposedConstant method overriding.
        /// </summary>
        public override void RegisterNamedMembers(ref IDictionary<string, NamedConst> namedMembersList)
        {
            A.RegisterNamedMembers(ref namedMembersList);
        }

        /// <summary>
        /// Implementation of inherited abstract method from ComposedConstant.
        /// </summary>
        /// <param name="namedMembersList">Input list to clean duplicities.</param>
        internal override void ClearDupl(ref IDictionary<string, NamedConst> namedMembersList)
        {
            ComposedExtenders.ClearDupls(() => A, (x) => A = x, namedMembersList);
        }

        /// <summary>
        /// OneMemberComposedConst default constructor
        /// </summary>
        public OneMemberComposedConst() { }

        /// <summary>
        /// Create OneMemberComposedConst with predefined value
        /// </summary>
        /// <param name="a">ComposedConstant</param>
        public OneMemberComposedConst(ComposedConstant a)
        {
            A = a;
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// OneMemberComposedConst serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public OneMemberComposedConst(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            A = sr.ReadObject() as ComposedConstant;
        }

        /// <summary>
        /// OneMemberComposedConst deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.WriteObject(A);
            sw.AddToInfo(info);
        }
        #endregion
    }
}