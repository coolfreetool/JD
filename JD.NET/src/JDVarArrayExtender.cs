using System;

namespace JDSpace
{
    public static class JDVarArrayExtender
    {
        /// <summary>
        /// Sum of variables in argument array via specific dimension.
        /// </summary>
        /// <param name="varArr">Array of variables to be sum.</param>
        /// <param name="dim">Dimension index {0, 1}. 0 - sum via columns. 1 - sum via rows.</param>
        /// <returns>Result sum of variables.</returns>
        public static JDLinExpr Sum(this JDVar[] varArr, int dim)
        {
            JDElement varSum;
            if (dim == 0)
            {
                varSum = new JDLinExpr(1, varArr[0].YSize, varArr[0].ScLinExprFactory); // Sum of sums via variable columns.
            }
            else
            {
                if (dim == 1)
                {
                    varSum = new JDLinExpr(varArr[0].XSize, 1, varArr[0].ScLinExprFactory); // Sum of sums via variable rows.
                }
                else
                {
                    throw new EntryPointNotFoundException("Error: Bad dimension selected: " + dim + "\n");
                }
            }
            for (int i = 0; i < varArr.Length; i++)
            {
                varSum += varArr[i].Sum(dim);
            }
            return varSum.ToJDLinExpr();
        }

        /// <summary>
        /// Sum of variables in argument array.
        /// </summary>
        /// <param name="varArr">Array of variables to be sum.</param>
        /// <returns>Result sum of variables in array.</returns>
        public static JDLinExpr Sum(this JDVar[] varArr)
        {
            JDElement varSum = new JDLinExpr(1, 1, varArr[0].ScLinExprFactory);
            for (int i = 0; i < varArr.Length; i++)
            {
                varSum += varArr[i].Sum();
            }
            return varSum.ToJDLinExpr();
        }


        /// <summary>
        /// Sum of variables in argument array via elements. Equal size of variables is required.
        /// </summary>
        /// <param name="varArr">Array of variables to be sum.</param>
        /// <returns>Result sum of variables in array. It has the same size as variables.</returns>
        public static JDLinExpr ElementSum(this JDVar[] varArr)
        {
            JDElement varSum = new JDLinExpr(varArr[0].XSize, varArr[0].YSize, varArr[0].ScLinExprFactory);
            for (int i = 0; i < varArr.Length; i++)
            {
                varSum += varArr[i];
            }
            return varSum.ToJDLinExpr();
        }
    }
}