using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JDUtils;

namespace JDSpace
{
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
}