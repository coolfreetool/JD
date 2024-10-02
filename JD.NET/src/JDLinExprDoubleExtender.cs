namespace JDSpace
{
    internal static class JDLinExprDoubleExtender
    {
        /// <summary>
        /// Converts double to linear expression.
        /// </summary>
        /// <param name="constant">Constant in double format.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory (null default)</param>
        /// <returns>Constant in linear expression format.</returns>
        internal static JDLinExpr ToJDLinExpr(this double constant, ScLinExprFactory scLinExprFactory = null)
        {
            JDLinExpr jdLinExpr = new JDLinExpr(1, 1, scLinExprFactory);
            jdLinExpr.Add(constant);
            return jdLinExpr;
        }

        /// <summary>
        /// Converts two-dimensional double array to linear expression.
        /// </summary>
        /// <param name="constant">Constant in double format.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory (null default)</param>
        /// <returns>Constant in linear expression format.</returns>
        internal static JDLinExpr ToJDLinExpr(this double[,] constant, ScLinExprFactory scLinExprFactory = null)
        {
            JDLinExpr expr = new JDLinExpr(constant.GetLength(0), constant.GetLength(1), scLinExprFactory);
            expr.Add(constant);
            return expr;
        }

        /// <summary>
        /// Converts two-dimensional double array to linear expression.
        /// </summary>
        /// <param name="constant">Constant in double format.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory (null default)</param>
        /// <returns>Constant in linear expression format.</returns>
        internal static JDLinExpr ToJDLinExpr(this double[][] constant, ScLinExprFactory scLinExprFactory = null)
        {
            JDLinExpr expr = new JDLinExpr(constant.Length, constant[0].Length, scLinExprFactory);
            expr.Add(constant);
            return expr;
        }
    }
}