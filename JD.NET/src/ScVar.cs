using System;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Scalar optimization variable reprezentation.
    /// </summary>
    [Serializable]
    public class ScVar : ISerializable
    {
        /// <summary>
        /// Variable unique id.
        /// </summary>
        public readonly int Id; // _data[0 - 7]

        /// <summary>
        /// Variable unique name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Variable type.
        /// </summary>
        public char Type; // _data[8 - 23]

        /// <summary>
        /// Variable lower bound.
        /// </summary>
        public double Lb;

        /// <summary>
        /// Variable upper bound.
        /// </summary>
        public double Ub;

        /// <summary>
        /// Defines if variable is used in optimization.
        /// </summary>
        public bool Use = true;
        
        private byte _branchPriority;

        /// <summary>
        /// Variable branch priority
        /// </summary>
        public int BranchPriority
        {
            get
            {
                return (int)_branchPriority;
            }
            set
            {
                _branchPriority = (byte)value;
            }
        }

        /// <summary>
        /// Variable to store evaluated value.
        /// </summary>
        private double _value;

        /// <summary>
        /// Variable flag attributes. It reduces variable properties to one variable using binary flags.
        /// It enables efficient sotring via binary serialization (less properties to store).
        /// </summary>
        private VarAttrs _attrs;

        /// <summary>
        /// Variable value (after optimization).
        /// </summary>
        public double? Value
        {
            get
            {
                if (_attrs.HasFlag(VarAttrs.NULL_VALUE)) return null;
                return _value;
            }

            set
            {
                if (value == null)
                {
                    _on(VarAttrs.NULL_VALUE);
                }
                else
                {
                    _off(VarAttrs.NULL_VALUE);
                    _value = (double)value;
                }
            }
        }

        /// <summary>
        /// ScVar object constructor.
        /// </summary>
        /// <param name="id">Variable id (unique in JDModel).</param>
        /// <param name="name">Variable name.</param>
        /// <param name="type">Variable type (JD.BINARY, JD.INTEGER, JD.CONTINUOUS).</param>
        /// <param name="lb">Variable low bound.</param>
        /// <param name="ub">Variable top bound.</param>
        /// <param name="branchPriority">Variable branching priority.</param>
        internal ScVar(int id, string name, char type, double lb, double ub, int branchPriority = 0)
        {
            Id = id;
            Name = name;
            Type = type;
            Lb = lb;
            Ub = ub;
            BranchPriority = branchPriority;
            _attrs = VarAttrs.NULL_VALUE;
        }

        /// <summary>
        /// Return ScVar name.
        /// </summary>
        /// <returns>ScVar name</returns>
        public override string ToString()
        {
            return Name;
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ScVar serialization
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="context">Streaming context</param>
        public ScVar(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            Id = sr.ReadInt32();
            _attrs = (VarAttrs)sr.ReadByte();
            if (!_attrs.HasFlag(VarAttrs.NULL_NAME)) Name = sr.ReadString();
            Type = _varAttr2Type(_attrs);
            if (_attrs.HasFlag(VarAttrs.LB_ZERO))
            {
                Lb = 0;
            }
            else if (_attrs.HasFlag(VarAttrs.LB_MINUS_INF))
            {
                Lb = -JD.INFINITY;
            }
            else
            {
                Lb = sr.ReadDouble();
            }

            if (_attrs.HasFlag(VarAttrs.UB_ZERO))
            {
                Ub = 0;
            }
            else if (_attrs.HasFlag(VarAttrs.UB_INF))
            {
                Ub = JD.INFINITY;
            }
            else
            {
                Ub = sr.ReadDouble();
            }
            if (!_attrs.HasFlag(VarAttrs.NULL_VALUE))
            {
                _value = sr.ReadDouble();
            }
            _branchPriority = sr.ReadByte();
        }

        /// <summary>
        /// ScVar deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(Id);
            _updateVarAttrs();
            sw.Write((byte)_attrs);
            if (Name != null) sw.Write(Name);
            if (!(_attrs.HasFlag(VarAttrs.LB_ZERO) | _attrs.HasFlag(VarAttrs.LB_MINUS_INF)))
            {
                sw.Write(Lb);
            }
            if (!(_attrs.HasFlag(VarAttrs.UB_ZERO) | _attrs.HasFlag(VarAttrs.UB_INF)))
            {
                sw.Write(Ub);
            }
            if (!_attrs.HasFlag(VarAttrs.NULL_VALUE)) sw.Write(_value);
            sw.Write(_branchPriority);
            sw.AddToInfo(info);
        }

        /// <summary>
        /// Read variable type from variable flag attributes.
        /// </summary>
        /// <param name="attrs">Variable flag attributes.</param>
        /// <returns>Evaluated variable type.</returns>
        private char _varAttr2Type(VarAttrs attrs)
        {
            char type;
            if (attrs.HasFlag(VarAttrs.CON_TYPE))
            {
                type = JD.CONTINUOUS;
            }
            else if (attrs.HasFlag(VarAttrs.BIN_TYPE))
            {
                type = JD.BINARY;
            }
            else
            {
                type = JD.INTEGER;
            }
            return type;
        }

        /// <summary>
        /// Update variable flag attributes with current state of variable properties.
        /// </summary>
        private void _updateVarAttrs()
        {
            _updateType();
            _updateUbAndLb();
            if (Name == null)
            {
                _on(VarAttrs.NULL_NAME);
            }
            else
            {
                _off(VarAttrs.NULL_NAME);
            }
        }

        /// <summary>
        /// Update variable flag attributes with current Lb and Ub properties.
        /// </summary>
        private void _updateUbAndLb()
        {
            if (Lb == 0)
            {
                _on(VarAttrs.LB_ZERO);
            }
            else
            {
                _off(VarAttrs.LB_ZERO);
                if (Lb == -JD.INFINITY)
                {
                    _on(VarAttrs.LB_MINUS_INF);
                }
                else
                {
                    _off(VarAttrs.LB_MINUS_INF);
                }
            }
            if (Ub == 0)
            {
                _on(VarAttrs.UB_ZERO);
            }
            else
            {
                _off(VarAttrs.UB_ZERO);
                if (Ub == JD.INFINITY)
                {
                    _on(VarAttrs.UB_INF);
                }
                else
                {
                    _off(VarAttrs.UB_INF);
                }
            }
        }

        /// <summary>
        /// Update variable flag attributes with current variable type.
        /// </summary>
        private void _updateType()
        {
            if (Type == JD.CONTINUOUS)
            {
                _on(VarAttrs.CON_TYPE);
            }
            else
            {
                _off(VarAttrs.CON_TYPE);
                if (Type == JD.BINARY)
                {
                    _on(VarAttrs.BIN_TYPE);
                }
                else
                {
                    _off(VarAttrs.BIN_TYPE);
                }
            }
        }

        /// <summary>
        /// Set input flag(s) to ON.
        /// </summary>
        /// <param name="flag">Input flag(s).</param>
        private void _on(VarAttrs flag)
        {
            _attrs |= flag;
        }

        /// <summary>
        /// Set input flag(s) to OFF.
        /// </summary>
        /// <param name="flag">Input flag(s).</param>
        private void _off(VarAttrs flag)
        {
            _attrs &= ~flag;
        }
        #endregion << EXPLICIT SERIALIZATION >>
    }
}