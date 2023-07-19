using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JDUtils;
using System.Threading.Tasks;

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

    /// <summary>
    /// Variable (ScVar) attributes enum (to faster serialization).
    /// </summary>
    [Flags]
    internal enum VarAttrs : byte
    {
        /// <summary>
        /// true - CON, false - BIN or INT
        /// </summary>
        CON_TYPE = 1,
        /// <summary>
        /// true - BIN, false - INT
        /// </summary>
        BIN_TYPE = 2,
        NULL_NAME = 4,
        LB_MINUS_INF = 8,
        LB_ZERO = 16,
        UB_INF = 32,
        UB_ZERO = 64,
        NULL_VALUE = 128
    }

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

    /// <summary>
    /// Constraint (ScVar) attributes enum (to faster serialization)
    /// </summary>
    [Flags]
    internal enum ConAttrs : byte
    {
        /// <summary>
        /// true - less equal, false - equal
        /// </summary>
        SENSE_LE = 32,
        NULL_NAME = 64,
        ZERO_CONSTANT = 128
    }

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

    /// <summary>
    /// Scalar term representation (double and scalar optimization variable couple).
    /// </summary>
    public class ScTerm
    {
        /// <summary>
        /// Term variable.
        /// </summary>
        public ScVar Var;
        /// <summary>
        /// Term coefficient.
        /// </summary>
        public virtual double Coeff { get; private set; }

        /// <summary>
        /// Get real coeff representation.
        /// </summary>
        internal virtual object CoeffObj { get { return Coeff; } }

        /// <summary>
        /// ScTerm constructor.
        /// </summary>
        /// <param name="var">Term variable.</param>
        internal ScTerm(ScVar var) { Var = var; }

        /// <summary>
        /// ScTerm constructor.
        /// </summary>
        /// <param name="var">Term variable.</param>
        /// <param name="coeff">Term coefficient.</param>
        internal ScTerm(ScVar var, double coeff)
            : this(var)
        {
            Coeff = coeff;
        }

        /// <summary>
        /// Return string representation of scalar term
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            string varLabel = String.Format("(v{0})", Var.Id);
            if (Var.Name != null)
            {
                varLabel = Var.Name;
            }
            if (Coeff >= 0)
            {
                return String.Format("+{0}.{1} ", Coeff, varLabel);
            }
            else
            {
                return String.Format("{0}.{1} ", Coeff, varLabel);
            }
        }
    }

    /// <summary>
    /// Factory class to create specific kind of ScLinExpr instances. It depends on kind
    /// of JModel.
    /// </summary>
    internal class ScLinExprFactory
    {
        /// <summary>
        /// Method to create specific kind of ScLinExpr.
        /// </summary>
        private Func<ScLinExpr> _makeScLinExpr;

        /// <summary>
        /// Method to create specific kind of ScTerm.
        /// </summary>
        private Func<ScVar, object, ScTerm> _makeScTerm;

        /// <summary>
        /// Composed or non-composed concept.
        /// </summary>
        private bool _composed;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="composed">Whether create composed ScLinExprs or not.</param>
        internal ScLinExprFactory(bool composed = false)
        {
            if (composed)
            {
                _makeScLinExpr = () => new ComposedScLinExpr();
                _makeScTerm = (var, coeff) => new ComposedScTerm(var, new NamedConst(coeff));
            }
            else
            {
                _makeScLinExpr = () => new ScLinExpr();
                _makeScTerm = (var, coeff) => new ScTerm(var, Convert.ToDouble(coeff));
            }
            _composed = composed;
        }

        /// <summary>
        /// Create new empty ScLinExpr.
        /// </summary>
        /// <returns>New ScLinExpr.</returns>
        internal ScLinExpr CreateScLinExpr()
        {
            ScLinExpr expr = _makeScLinExpr();
            return expr;
        }

        /// <summary>
        /// Create new ScTerm.
        /// </summary>
        /// <param name="var">Term variable.</param>
        /// <param name="coeff">Term coefficient.</param>
        /// <returns>New ScTerm.</returns>
        internal ScTerm CreateScTerm(ScVar var, object coeff)
        {
            ScTerm term = _makeScTerm(var, coeff);
            return term;
        }

        /// <summary>
        /// Create new ScLinExpr using input terms and constant.
        /// </summary>
        /// <param name="terms">Existing terms.</param>
        /// <param name="constant">Expression constant.</param>
        /// <returns>New ScLinExpr.</returns>
        internal ScLinExpr CreateScLinExpr(List<ScTerm> terms, object constant)
        {
            if (!_composed) return new ScLinExpr(terms, constant.ToDouble());
            if (constant is ComposedConstant) return new ComposedScLinExpr(terms, constant as ComposedConstant);
            return new ComposedScLinExpr(terms, new NamedConst(constant));
        }
    }

    /// <summary>
    /// Creates ScTerm or ComposedScTerm (depends on inserted coefficient type).
    /// </summary>
    internal class ScTermFactory
    {
        /// <summary>
        /// Create ScTerm using existing variable and coefficient.
        /// </summary>
        /// <param name="var">Existing variable.</param>
        /// <param name="coeffObj">Existing coefficient.</param>
        /// <returns>New scalar term.</returns>
        public static ScTerm CreateTerm(ScVar var, object coeffObj)
        {
            ScTerm term;
            if (coeffObj is ComposedConstant)
            {
                term = new ComposedScTerm(var, coeffObj as ComposedConstant);
            }
            else
            {
                double coeff = Convert.ToDouble(coeffObj);
                term = new ScTerm(var, coeff);
            }
            return term;
        }
    }

    /// <summary>
    /// Scalar linear expression representation.
    /// </summary>
    [Serializable]
    public class ScLinExpr : ISerializable
    {
        /// <summary>
        /// Linear expression constant member.
        /// </summary>
        public virtual double Constant { get; private set; }

        /// <summary>
        /// Map of linear expression terms (over terms variables ids). 
        /// </summary>
        public IList<ScTerm> Terms { get; protected set; }

        /// <summary>
        /// ScLinExpr serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ScLinExpr(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            Constant = sr.ReadDouble();
            IList<int> varIds = sr.ReadList<int>();
            //IList<double> coeffs = sr.ReadList<double>();
            IList<object> coeffs = sr.ReadList<object>();
            //Terms = varIds.Zip(coeffs, (id, coeff) => new ScTerm(JDModelSerializationHelper.VarsMap[id], coeff)).ToList();
            Terms = varIds.Zip(coeffs, (id, coeffObj) => ScTermFactory.CreateTerm(JDModelSerializationHelper.VarsMap[id], coeffObj)).ToList();
        }

        /// <summary>
        /// ScLinExpr deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            //sw.Write(Constant);
            sw.Write(Constant);
            List<int> varIds = Terms.Select(x => x.Var.Id).ToList();
            //List<double> coeffs = Terms.Select(x => x.Coeff).ToList();
            List<object> coeffs = Terms.Select(x => x.CoeffObj).ToList();
            sw.Write<int>(varIds);
            sw.Write<object>(coeffs);
            sw.AddToInfo(info);
        }

        /// <summary>
        /// Constructor to create scalar expression using existing terms.
        /// </summary>
        /// <param name="terms">Existing terms list.</param>
        /// <param name="constant">Expression constant.</param>
        internal ScLinExpr(List<ScTerm> terms, double constant)
        {
            Constant = constant;
            Terms = terms;
        }

        /// <summary>
        /// Constructor to create empty scalar expression.
        /// </summary>
        internal ScLinExpr()
        {
            Terms = new List<ScTerm>();
        }

        /// <summary>
        /// Get most recent solution value.
        /// </summary>
        public double? Value
        {
            get
            {
                double dResult = Constant;
                foreach (ScTerm rVaC in Terms)
                {
                    try
                    {
                        dResult += rVaC.Coeff * (double)rVaC.Var.Value;
                    }
                    catch
                    {
                        Console.WriteLine("No result value for variable {0}.", rVaC.Var.Id);
                        return null;
                    }
                }
                return dResult;
            }
        }

        /// <summary>
        /// Add multiplication of another scalar linear expression.
        /// </summary>
        /// <param name="multiplier">Added linear expression multiplier.</param>
        /// <param name="linExpr">Linear expression to be add.</param>
        internal virtual void Add(object multiplier, ScLinExpr linExpr)
        {
            Constant += Convert.ToDouble(multiplier) * linExpr.Constant;
            foreach (ScTerm term in linExpr.Terms)
            {
                AddTerm(Convert.ToDouble(multiplier) * term.Coeff, term.Var);
            }
        }

        /// <summary>
        /// Add multiplication of another scalar linear expression.
        /// </summary>
        /// <param name="linExpr">Linear expression to be add.</param>
        internal virtual void Add(ScLinExpr linExpr)
        {
            // good to optimize
            Constant += linExpr.Constant;
            //Parallel.ForEach(linExpr.Terms, term =>
            //{
            //    Terms.Add(term);
            //});            
            foreach (ScTerm term in linExpr.Terms)
            {
                Terms.Add(term);
            }
        }

        /// <summary>
        /// Add constant to this linear expression.
        /// </summary>
        /// <param name="constant">Constant to be add.</param>
        internal virtual void Add(object constant)
        {
            double d = constant.ToDouble();
            Constant += d;
        }

        /// <summary>
        /// Add term to this linear expression.
        /// </summary>
        /// <param name="coeff">Term coefficient.</param>
        /// <param name="var">Variable.</param>
        internal virtual void AddTerm(object coeff, ScVar var)
        {
            double d = coeff.ToDouble();
            Terms.Add(new ScTerm(var, d));
        }

        /// <summary>
        /// Add terms to this linear expression.
        /// </summary>
        /// <param name="coeffs">Array of coefficients.</param>
        /// <param name="varList">Array of scalar variables.</param>
        internal virtual void AddTerms(object[] coeffs, List<ScVar> varList)
        {
            for (int i = 0; i < varList.Count; i++)
            {
                AddTerm(coeffs[i], varList[i]);
            }
        }

        /// <summary>
        /// Return ScLinExpr string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ScTerm term in Terms)
            {
                sb.Append(term);
            }
            if (Constant != 0)
            {
                if (Constant > 0)
                {
                    sb.AppendFormat("+{0}", Constant);
                }
                else
                {
                    sb.AppendFormat("{0}", Constant);
                }
            }
            return sb.ToString();
        }
    }
}
