using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// Scalar model constraint representation (right hand side is always zero).
    /// </summary>
    [Serializable]
    public class ScConstr : ISerializable
    {
        /// <summary>
        /// Constraint unique id.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Constraint left hand side lin expr with constant value.
        /// </summary>
        public ScLinExpr Lhs;

        /// <summary>
        /// Constraint comparing sign.
        /// </summary>
        public char Sense;

        /// <summary>
        /// Constraint name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Lazy property (0 or 1 are possible values actually, 0 - by default).
        /// </summary>
        public byte LazyLevel
        {
            get
            {
                return (byte)(((byte)_attrs) & 31);
            }
            set
            {
                if (value < 32)
                {
                    _attrs = (ConAttrs)((byte)_attrs | value);
                }
                else
                {
                    throw new JDException("Lazy level must be from interval <0,31>");
                }
            }
        }

        /// <summary>
        /// Constraint flag attributes. It reduces constraint properties to one variable using binary flags.
        /// It enables efficient sotring via binary serialization (less properties to store).
        /// </summary>
        private ConAttrs _attrs;

        /// <summary>
        /// Right hand side value (double) will be substructed from left hand side lin. expr., so
        /// constraint right hand side will be zero.
        /// </summary>
        internal ScConstr(int id, ScLinExpr lhs, char sense, double rhs, string name, byte lazyLevel = 0)
        {
            Id = id;
            if (rhs != 0)
            {
                lhs.Add(-rhs);
            }
            Lhs = lhs;
            Sense = sense;
            Name = name;
            LazyLevel = lazyLevel;
        }

        internal bool IsValid()
        {
            if (Lhs.Terms.Count > 0)
            {
                return true;
            }
            else
            {
                bool eval;
                switch (Sense)
                {
                    case JD.LESS_EQUAL:
                        eval = (Lhs.Constant <= 0);
                        break;
                    case JD.EQUAL:
                        eval = (Lhs.Constant == 0);
                        break;
                    case JD.GREATER_EQUAL:
                        eval = (Lhs.Constant >= 0);
                        break;
                    default:
                        throw new JDException("Unknown comparing symbol: {0}!", Sense);
                }
                if (!eval)
                {
                    throw new JDException("Unfeasible constant constraint: ({0})!", ToString());
                }
                return false;
            }
        }

        /// <summary>
        /// Return scalar constraint string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Lhs);
            sb.AppendFormat(" {0} {1}", Sense, 0);
            if (Name != null)
            {
                sb.AppendFormat(", name: {0}", Name);
            }
            if (LazyLevel > 0)
            {
                sb.AppendFormat(", lazy: {0}", LazyLevel);
            }
            return sb.ToString();
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ScConstr serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ScConstr(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            Id = sr.ReadInt32();
            _attrs = (ConAttrs)sr.ReadByte();
            if (!_attrs.HasFlag(ConAttrs.NULL_NAME)) Name = sr.ReadString();
            //double constant = 0;
            object constant = null;

            //if (!_attrs.HasFlag(ConAttrs.ZERO_CONSTANT))
            //{
            constant = sr.ReadObject();
            //ReadDouble();
            //}
            Sense = JD.EQUAL;
            if (_attrs.HasFlag(ConAttrs.SENSE_LE)) Sense = JD.LESS_EQUAL;
            IList<int> varIds = sr.ReadList<int>();
            //IList<double> coeffs = sr.ReadList<double>();
            IList<object> coeffs = sr.ReadList<object>();
            //List<ScTerm> lhsTerms = varIds.Zip(coeffs, (id, coeff) => new ScTerm(JDModelSerializationHelper.VarsMap[id], coeff)).ToList();
            List<ScTerm> lhsTerms = varIds.Zip(coeffs, (id, coeffObj) => ScTermFactory.CreateTerm(JDModelSerializationHelper.VarsMap[id], coeffObj)).ToList();
            //Lhs = new ScLinExpr(lhsTerms, constant);
            Lhs = JDModelSerializationHelper.ScLinExprFactory.CreateScLinExpr(lhsTerms, constant);
        }

        /// <summary>
        /// ScConstr deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(Id);
            _updateAttrs();
            sw.Write((byte)_attrs);
            if (Name != null) sw.Write(Name);
            //if (Lhs.Constant != 0) sw.Write(Lhs.Constant);
            if (Lhs is ComposedScLinExpr)
            {
                sw.WriteObject((Lhs as ComposedScLinExpr).ConstantObj);
            }
            else
            {
                sw.WriteObject(Lhs.Constant);
            }
            List<int> varIds = Lhs.Terms.Select(x => x.Var.Id).ToList();
            //List<double> coeffs = Lhs.Terms.Select(x => x.Coeff).ToList();
            List<object> coeffs = Lhs.Terms.Select(x => x.CoeffObj).ToList();
            sw.Write<int>(varIds);
            //sw.Write<double>(coeffs);
            sw.Write<object>(coeffs);
            sw.AddToInfo(info);
        }

        /// <summary>
        /// Set input flags to ON.
        /// </summary>
        /// <param name="flag">Input flags.</param>
        private void _on(ConAttrs flag)
        {
            _attrs |= flag;
        }

        /// <summary>
        /// Set input flags to OFF.
        /// </summary>
        /// <param name="flag">Input flags.</param>
        private void _off(ConAttrs flag)
        {
            _attrs &= ~flag;
        }

        /// <summary>
        /// Update constraint flag attributes according to current state of constraint properties.
        /// </summary>
        private void _updateAttrs()
        {
            if (Lhs.Constant == 0)
            {
                _on(ConAttrs.ZERO_CONSTANT);
            }
            else
            {
                _off(ConAttrs.ZERO_CONSTANT);
            }

            if (Name == null)
            {
                _on(ConAttrs.NULL_NAME);
            }
            else
            {
                _off(ConAttrs.NULL_NAME);
            }

            if (Sense == JD.LESS_EQUAL)
            {
                _on(ConAttrs.SENSE_LE);
            }
            else
            {
                _off(ConAttrs.SENSE_LE);
            }
        }
        #endregion << EXPLICIT SERIALIZATION >>
    }
}