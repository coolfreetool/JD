using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Base ComposedConstant implementation. It enables named model constant declaring
    /// and future value reloading.
    /// </summary>
    [Serializable]
    public class NamedConst : ComposedConstant, ISerializable
    {
        /// <summary>
        /// Constant name (model unique).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set constant value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue { get { return Value; } }

        /// <summary>
        /// ComposedConstant method overriding.
        /// </summary>
        public override void RegisterNamedMembers(ref IDictionary<string, NamedConst> namedMembersList)
        {
            if (Name != null)
            {
                if (!namedMembersList.ContainsKey(Name))
                {
                    namedMembersList.Add(Name, this);
                }
            }
        }

        /// <summary>
        /// Not implemented method becouse NamedConst has no ComposedConstant leafs.
        /// </summary>
        /// <param name="namedMembersList">List to clean duplicities.</param>
        internal override void ClearDupl(ref IDictionary<string, NamedConst> namedMembersList)
        {
            throw new NotImplementedException();
        }

        #region << CONSTRUCTORS >>
        /// <summary>
        /// Create new named constant
        /// </summary>
        /// <param name="name">Constant name</param>
        /// <param name="value">Constant value</param>
        public NamedConst(string name = null, double value = 0.0)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Create new named constant
        /// </summary>
        /// <param name="value">Constant value</param>
        public NamedConst(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Create new named constant
        /// </summary>
        /// <param name="value">Constant value</param>
        public NamedConst(object value)
        {
            Value = Convert.ToDouble(value);
        }
        #endregion

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// Named constant serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public NamedConst(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            Name = sr.ReadString();
            Value = sr.ReadDouble();
        }

        /// <summary>
        /// Named constant deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(Name);
            if (Name != null)
                JDModelSerializationHelper.NamedConstants.Remove(Name);
            sw.Write(Value);
            sw.AddToInfo(info);
        }
        #endregion
    }
}