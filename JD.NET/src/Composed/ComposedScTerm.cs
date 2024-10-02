using System;

namespace JDSpace
{
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
}