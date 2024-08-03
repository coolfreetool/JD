using JDUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JDSpace
{
    /// <summary>
    /// Class encapsulates composed constant (non-variable member of model), which enables constant
    /// values reloading reloading.
    /// </summary>
    [Serializable]
    public abstract class ComposedConstant
    {
        /// <summary>
        /// Get constant double representation (required for putting model to solver).
        /// </summary>
        public abstract double DoubleValue { get; }

        /// <summary>
        /// Register all NamedConst (base ComposedConstant implementation) subconstants.
        /// </summary>
        public abstract void RegisterNamedMembers(ref IDictionary<string, NamedConst> namedMembersList);

        /// <summary>
        /// Clean duplicities in inserted named constants list.
        /// </summary>
        /// <param name="namedMembersList">Named constants list to clean duplicities.</param>
        internal abstract void ClearDupl(ref IDictionary<string, NamedConst> namedMembersList);

        #region << OPERATOR OVERLOADS >>

        /// <summary>
        /// Add two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator +(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedSum(r, l);
            return ret;
        }

        /// <summary>
        /// Substract two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>v
        public static ComposedConstant operator -(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedDec(r, l);
            return ret;
        }

        /// <summary>
        /// Multiply two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator *(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedProduct(r, l);
            return ret;
        }

        /// <summary>
        /// Divide two composed constants
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator /(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedDiv(r, l);
            return ret;
        }

        /// <summary>
        /// Composed power
        /// </summary>
        /// <param name="r">First composed constant</param>
        /// <param name="l">Second composed constant</param>
        /// <returns>New composed constants</returns>
        public static ComposedConstant operator ^(ComposedConstant r, ComposedConstant l)
        {
            ComposedConstant ret = new ComposedPower(r, l);
            return ret;
        }

        #endregion
    }

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

    /// <summary>
    /// A + B representation class
    /// </summary>
    [Serializable]
    internal class ComposedSum : TwoMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return A.DoubleValue + B.DoubleValue; }
        }

        /// <summary>
        /// Create ComposedSum with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public ComposedSum(ComposedConstant a, ComposedConstant b) : base(a, b) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedSum serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public ComposedSum(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }

    /// <summary>
    /// A - B representation class
    /// </summary>
    [Serializable]
    internal class ComposedDec : TwoMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return A.DoubleValue - B.DoubleValue; }
        }
        /// <summary>
        /// Create ComposedDec with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public ComposedDec(ComposedConstant a, ComposedConstant b) : base(a, b) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedDec serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public ComposedDec(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }

    /// <summary>
    /// A * B representation class
    /// </summary>
    [Serializable]
    internal class ComposedProduct : TwoMemberComposedConst, ISerializable
    {
        /// <summary>
        /// ComposedConstant property overriding.
        /// </summary>
        public override double DoubleValue
        {
            get { return A.DoubleValue * B.DoubleValue; }
        }
        /// <summary>
        /// Create ComposedProduct with predefined value
        /// </summary>
        /// <param name="a">First composed constant</param>
        /// <param name="b">Second composed constant</param>
        public ComposedProduct(ComposedConstant a, ComposedConstant b) : base(a, b) { }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedProduct serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">StreamingContext</param>
        public ComposedProduct(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion
    }

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

    /// <summary>
    /// Scalar term representation (double and scalar optimization variable couple).
    /// </summary>
    [Serializable]
    internal class ComposedScTerm : ScTerm
    {
        /// <summary>
        /// Get ScTerm coefficient.
        /// </summary>
        internal override object CoeffObj
        {
            get
            { return CoeffObj2; }
        }

        /// <summary>
        /// Get or set composed constant.
        /// </summary>
        internal ComposedConstant CoeffObj2 { get; set; }
        /// <summary>
        /// Term coefficient.
        /// </summary>
        public override double Coeff { get { return CoeffObj2.DoubleValue; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="var">Existing variable.</param>
        /// <param name="coeff">ScTerm coefficient.</param>
        internal ComposedScTerm(ScVar var, ComposedConstant coeff)
            : base(var)
        {
            CoeffObj2 = coeff;
        }

        /// <summary>
        /// Composed scalar term ToString method
        /// </summary>
        /// <returns>string value</returns>
        public override string ToString()
        {
            string varLabel = String.Format("(v{0})", Var.Id);
            if (Var.Name != null)
                varLabel = Var.Name;
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
    /// Scalar linear expression representation.
    /// </summary>
    [Serializable]
    internal class ComposedScLinExpr : ScLinExpr, ISerializable
    {
        internal ComposedConstant ConstantObj { get; set; }
        /// <summary>
        /// Linear expression constant member.
        /// </summary>
        public override double Constant
        {
            get
            {
                return ConstantObj.DoubleValue;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="terms">Existing terms list.</param>
        /// <param name="constant">Expression constant.</param>
        internal ComposedScLinExpr(List<ScTerm> terms, double constant)
        {
            //Constant = constant;
            ConstantObj = new NamedConst(constant);
            Terms = terms;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="terms">Existing terms list.</param>
        /// <param name="constant">Expression constant.</param>
        internal ComposedScLinExpr(List<ScTerm> terms, ComposedConstant constant)
        {
            //Constant = constant;
            ConstantObj = constant;
            Terms = terms;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ComposedScLinExpr()
        {
            //Constant = 0;
            ConstantObj = new NamedConst();
            Terms = new List<ScTerm>();
        }

        /// <summary>
        /// Add multiplication of another scalar linear expression.
        /// </summary>
        /// <param name="multiplier">Added linear expression multiplier.</param>
        /// <param name="linExpr">Linear expression to be add.</param>
        internal override void Add(object multiplier, ScLinExpr linExpr)
        {
            ComposedConstant multiplierObj;
            if (multiplier is ComposedConstant)
            {
                multiplierObj = multiplier as ComposedConstant;
            }
            else
            {
                multiplierObj = new NamedConst(Convert.ToDouble(multiplier));
            }
            ConstantObj += multiplierObj * _getScLinExprConstant(linExpr);
            foreach (ScTerm term in linExpr.Terms)
            {
                Terms.Add(new ComposedScTerm(term.Var, multiplierObj * _getScTermCoeff(term)));
            }
        }

        /// <summary>
        /// Add multiplication of another scalar linear expression.
        /// </summary>
        /// <param name="multiplierObj">Added linear expression multiplier.</param>
        /// <param name="linExpr">Linear expression to be add.</param>
        internal void Add(ComposedConstant multiplierObj, ScLinExpr linExpr)
        {
            ConstantObj += multiplierObj * _getScLinExprConstant(linExpr);
            foreach (ScTerm term in linExpr.Terms)
            {
                Terms.Add(new ComposedScTerm(term.Var, multiplierObj * _getScTermCoeff(term)));
            }
        }

        /// <summary>
        /// Add multiplication of another scalar linear expression.
        /// </summary>
        /// <param name="linExpr">Linear expression to be add.</param>
        internal override void Add(ScLinExpr linExpr)
        {
            ConstantObj += _getScLinExprConstant(linExpr);
            ((List<ScTerm>)Terms).AddRange(linExpr.Terms);
            //foreach (ScTerm term in linExpr.Terms)
            //{
            //    Terms.Add(term);
            //}
        }

        /// <summary>
        /// Get constant of inserted scalar linear expression.
        /// </summary>
        /// <param name="expr">Linear expression.</param>
        /// <returns>Result linear expression constatn.</returns>
        private ComposedConstant _getScLinExprConstant(ScLinExpr expr)
        {
            if (expr is ComposedScLinExpr) return (expr as ComposedScLinExpr).ConstantObj;
            ComposedConstant cn = new NamedConst(expr.Constant);
            return cn;
        }

        /// <summary>
        /// Get inserted scalar term coefficient.
        /// </summary>
        /// <param name="term">Scalar term.</param>
        /// <returns>Result term coefficient.</returns>
        private ComposedConstant _getScTermCoeff(ScTerm term)
        {
            if (term is ComposedScTerm) return (term as ComposedScTerm).CoeffObj2;
            ComposedConstant cn = new NamedConst(term.Coeff);
            return cn;
        }

        /// <summary>
        /// Add constant to this linear expression.
        /// </summary>
        /// <param name="constant">Constant to be add.</param>
        internal override void Add(object constant)
        {
            // TODO pridat podporu ostatnich objektu, sjednotit (dedicnosti) NamedConstant a ComposedNumber, zavest NamedConstant jako interface
            if (constant is ComposedConstant)
            {
                ConstantObj += (constant as ComposedConstant);
            }
            else
            {
                ConstantObj += new NamedConst(Convert.ToDouble(constant));
            }
        }

        /// <summary>
        /// Add term to this linear expression.
        /// </summary>
        /// <param name="coeff">Term coefficient.</param>
        /// <param name="var">Variable.</param>
        internal override void AddTerm(object coeff, ScVar var)
        {
            ComposedConstant objCoeff;
            if (coeff is ComposedConstant)
            {
                objCoeff = coeff as ComposedConstant;
            }
            else
            {
                objCoeff = new NamedConst(coeff);
            }
            ComposedScTerm comTerm = new ComposedScTerm(var, objCoeff);
            Terms.Add(comTerm);
        }

        /// <summary>
        /// Add term to this linear expression.
        /// </summary>
        /// <param name="coeff">Term coefficient.</param>
        internal void AddTerm(ComposedScTerm coeff)
        {
            Terms.Add(coeff);
        }

        /// <summary>
        /// Add terms to this linear expression.
        /// </summary>
        /// <param name="coeffs">Array of coefficients.</param>
        /// <param name="varList">Array of scalar variables.</param>
        internal override void AddTerms(object[] coeffs, List<ScVar> varList)
        {
            Func<object, ComposedConstant> convert;
            if (coeffs[0] is ComposedConstant)
            {
                convert = (ob) => ob as ComposedConstant;
            }
            else
            {
                convert = (ob) => new NamedConst();
            }

            for (int i = 0; i < varList.Count; i++)
            {
                AddTerm(new ComposedScTerm(varList[i], convert(coeffs[i])));
            }
        }

        /// <summary>
        /// Composed constant ToString method.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ScTerm term in Terms)
            {
                sb.Append(term);
            }
            if (ConstantObj.DoubleValue != 0)
            {
                if (ConstantObj.DoubleValue > 0)
                {
                    sb.AppendFormat("+{0}", ConstantObj.DoubleValue);
                }
                else
                {
                    sb.AppendFormat("{0}", ConstantObj.DoubleValue);
                }
            }
            return sb.ToString();
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// ComposedScLinExpr serialization. 
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ComposedScLinExpr(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            ConstantObj = (ComposedConstant)sr.ReadObject();
            IList<int> varIds = sr.ReadList<int>();
            IList<object> coeffs = sr.ReadList<object>();
            Terms = varIds.Zip(coeffs, (id, coeffObj) => ScTermFactory.CreateTerm(JDModelSerializationHelper.VarsMap[id], coeffObj)).ToList();
        }

        /// <summary>
        /// ComposedScLinExpr deserialization. 
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.WriteObject(ConstantObj);
            List<int> varIds = Terms.Select(x => x.Var.Id).ToList();
            List<object> coeffs = Terms.Select(x => x.CoeffObj).ToList();
            sw.Write<int>(varIds);
            sw.Write<object>(coeffs);
            sw.AddToInfo(info);
        }
        #endregion
    }

    /// <summary>
    /// JDModel named constants manager. It ensures named
    /// constants registration and values changing (over unique name).
    /// </summary>
    internal class NamedConstManager
    {
        /// <summary>
        /// Dictionary with named constants
        /// </summary>
        public IDictionary<string, NamedConst> NamedConsts;

        /// <summary>
        /// Named constants manager Default constructor
        /// </summary>
        public NamedConstManager()
        {
            NamedConsts = new Dictionary<string, NamedConst>();
        }

        /// <summary>
        /// Register constraint named constants to future value reloading.
        /// </summary>
        public void Register(ScConstr constr)
        {
            Register(constr.Lhs);
        }

        /// <summary>
        /// Clear duplicit values
        /// </summary>
        /// <param name="constr">Scalar constraint</param>
        public void ClearDupl(ScConstr constr)
        {
            ClearDupl(constr.Lhs);
        }

        /// <summary>
        /// Register constraints named constants to future value reloading.
        /// </summary>
        public void Register(IList<ScConstr> constrs)
        {
            foreach (ScConstr con in constrs) Register(con);
        }

        /// <summary>
        /// Clear duplicit values
        /// </summary>
        /// <param name="constrs">List of scalar constraints</param>
        public void ClearDupl(IList<ScConstr> constrs)
        {
            foreach (ScConstr con in constrs) ClearDupl(con);
        }

        /// <summary>
        /// Register named constant.
        /// </summary>
        internal void Register(NamedConst namCon)
        {
            namCon.RegisterNamedMembers(ref NamedConsts);
        }

        /// <summary>
        /// Get named constant of specific name or return null (if not provided).
        /// </summary>
        /// <param name="name">Name of constant to return.</param>
        /// <returns>Result named constant or null (if not provided).</returns>
        internal NamedConst GetNamedConst(string name)
        {
            if (NamedConsts.ContainsKey(name)) return NamedConsts[name];
            return null;
        }

        /// <summary>
        /// Register named constants of inserted scalar linear expression.
        /// </summary>
        /// <param name="expr">Scalar linear expression to register named constants in.</param>
        internal void Register(ScLinExpr expr)
        {
            if (expr is ComposedScLinExpr)
            {
                ComposedScLinExpr cExpr = expr as ComposedScLinExpr;
                cExpr.ConstantObj.RegisterNamedMembers(ref NamedConsts);
                foreach (ScTerm term in expr.Terms)
                {
                    (term as ComposedScTerm).CoeffObj2.RegisterNamedMembers(ref NamedConsts);
                }
            }
        }

        /// <summary>
        /// Clear named constant duplicities in inserted scalar linear expression.
        /// </summary>
        /// <param name="expr">Scalare linear expression to clear duplicities in.</param>
        internal void ClearDupl(ScLinExpr expr)
        {
            if (expr is ComposedScLinExpr)
            {
                ComposedScLinExpr cExpr = expr as ComposedScLinExpr;
                cExpr.ConstantObj.ClearDupl(ref NamedConsts);
                foreach (ScTerm term in expr.Terms)
                {
                    ClearDupl(term);
                }
            }
        }

        /// <summary>
        /// Clear named constant duplicities in inserted scalar term.
        /// </summary>
        /// <param name="term">Scalar term to clean duplicities in.</param>
        internal void ClearDupl(ScTerm term)
        {
            ComposedScTerm cTerm = term as ComposedScTerm;
            ComposedExtenders.ClearDupls(() => cTerm.CoeffObj2, (x) => cTerm.CoeffObj2 = x, NamedConsts);
        }

        /// <summary>
        /// Try change value of specific model named constant.
        /// </summary>
        /// <param name="name">Named constant name</param>
        /// <param name="newValue">New constant value to set</param>
        /// <returns></returns>
        public void ChangeValue(string name, double newValue)
        {
            if (NamedConsts.ContainsKey(name))
            {
                NamedConsts[name].Value = newValue;
            }
        }

        /// <summary>
        /// Add set of NamedConsts (for JDModels joining f.e.)
        /// </summary>
        /// <param name="namedConsts"></param>
        public void Join(IDictionary<string, NamedConst> namedConsts)
        {
            foreach (KeyValuePair<string, NamedConst> pair in namedConsts)
            {
                NamedConsts.Add(pair.Key, pair.Value);
            }
        }
    }

    /// <summary>
    /// Composed features extension methods.
    /// </summary>
    public static class ComposedExtenders
    {
        /// <summary>
        /// Converts object to NamedConst or NamedConst collection (1D or 2D according to inserted object)
        /// with specific name (scalar value) or name prefix (name + index).
        /// </summary>
        public static object Name(this object t, string name)
        {
            object unWrappedObj = null;
            int xSize, ySize;
            if (t.IsScalar(out unWrappedObj, out xSize, out ySize))
            {
                return new NamedConst(name, Convert.ToDouble(unWrappedObj));
            }
            else
            {
                IList list = t as IList;
                if (list.Is2D())
                {
                    Func<int, int, object> xyGetter = list.InitXYGetter();
                    NamedConst[,] ncArr = new NamedConst[xSize, ySize];
                    int num = 0;
                    for (int ix = 0; ix < xSize; ix++)
                    {
                        for (int iy = 0; iy < ySize; iy++)
                        {
                            ncArr[ix, iy] = new NamedConst(_subName(name, num), Convert.ToDouble(xyGetter(ix, iy)));
                            num++;
                        }
                    }
                    return ncArr;
                }
                else
                {
                    // list is standard array or list
                    NamedConst[] ncList = new NamedConst[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        ncList[i] = new NamedConst(_subName(name, i), Convert.ToDouble(list[i]));
                    }
                    return ncList;
                }
            }
        }

        /// <summary>
        /// Connect name prafix with number.
        /// </summary>
        /// <param name="name">Name (prefix).</param>
        /// <param name="num">Number to append to name.</param>
        /// <returns>Result created string.</returns>
        private static string _subName(string name, int num)
        {
            string subName = String.Format("{0}{1}", name, num);
            return subName;
        }

        /// <summary>
        /// Return composed constant absolute value.
        /// </summary>
        public static ComposedConstant Abs(this ComposedConstant t)
        {
            ComposedConstant ret = new ComposedAbs(t);
            return ret;
        }

        /// <summary>
        /// Check for inserted ComposedConstant child NamedConstants and registered NamedConstants duplicities
        /// and replace them with unique registered NamedConstant objects. Function is implemented primary for
        /// JDModel deserialization progress.
        /// </summary>
        internal static void ClearDupls(Func<ComposedConstant> getComCon, Action<ComposedConstant> setComCon, IDictionary<string, NamedConst> NamedConsts)
        {
            ComposedConstant comCon = getComCon();
            if (comCon is NamedConst)
            {
                NamedConst comConNamed = comCon as NamedConst;
                if (comConNamed.Name == null) return;
                if (NamedConsts.ContainsKey(comConNamed.Name))
                {
                    NamedConst comConInDic = NamedConsts[comConNamed.Name];
                    if (!comCon.Equals(comConInDic))
                    {
                        setComCon(comConInDic);
                    }
                }
                else
                {
                    NamedConsts.Add(comConNamed.Name, comConNamed);
                }
            }
            else
            {
                comCon.ClearDupl(ref NamedConsts);
            }
        }
    }
}
