using System;

namespace JDSpace
{
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
}