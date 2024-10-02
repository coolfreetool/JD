using System.Collections.Generic;

namespace JDSpace
{
    /// <summary>
    /// Common right hand side constraint representation for (double, double[], double[,] and double[][]).
    /// </summary>
    internal abstract class JdConstant : IJDComparable
    {
        /// <summary>
        /// Number of JDConstant elements
        /// </summary>
        public abstract int Numel { get; }
        internal ScLinExprFactory ScLinExprFactory { get; set; }
        internal ScLinExprFactory GetScLinExprFactory() { return ScLinExprFactory; }
        public abstract object this[int i] { get; }
        /// <summary>
        /// Get ScLinExpr at position i
        /// </summary>
        /// <param name="i">Position of ScLinExpr</param>
        /// <returns>Scalar linear epxpression</returns>
        public ScLinExpr GetScLinExpr(int i)
        {
            return ScLinExprFactory.CreateScLinExpr(new List<ScTerm>(), this[i]);
        }
        internal JdConstant(ScLinExprFactory scFactory)
        {
            ScLinExprFactory = scFactory;
        }
    }
}