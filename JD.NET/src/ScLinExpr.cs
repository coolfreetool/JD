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
                    if (!rVaC.Var.Value.HasValue)
                    {
                        Console.WriteLine("No result value for variable {0}.", rVaC.Var.Id);
                        return null;
                    }
                    dResult += rVaC.Coeff * rVaC.Var.Value.Value;
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