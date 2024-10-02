using System;

namespace JDSpace
{
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
}