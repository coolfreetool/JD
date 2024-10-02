using System;
using System.Collections.Generic;

namespace JDSpace
{
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
}