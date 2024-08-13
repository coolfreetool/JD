using JDUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JDSpace
{
    /// <summary>
    /// Parent class for JDVar and JDLinExpr.
    /// </summary>
    public abstract class JDElement : IJDComparable
    {
        /// <summary>
        /// Convert to JDLinExpr instance.
        /// </summary>
        /// <returns>JDLinExpr</returns>
        internal abstract JDLinExpr ToJDLinExpr();

        /// <summary>
        /// Get first dimension object size.
        /// </summary>
        public abstract int XSize { get; }

        /// <summary>
        /// Get second dimension object size.
        /// </summary>
        public abstract int YSize { get; }

        /// <summary>
        /// Total count of elements (scalar linear expressions).
        /// </summary>
        public int Numel { get { return XSize * YSize; } }

        /// <summary>
        /// Multiple of this linear expression.
        /// </summary>
        /// <param name="multiplier">Expression multiplier.</param>
        /// <returns>New result linear expression.</returns>
        public abstract JDLinExpr Multip(object multiplier);

        /// <summary>
        /// Get ScLinExpr at given position
        /// </summary>
        /// <param name="i">Position of element</param>
        /// <returns>ScLinExpr</returns>
        public abstract ScLinExpr GetScLinExpr(int i);

        internal ScLinExprFactory ScLinExprFactory { get; set; }

        /// <summary>
        /// Multiplicate one row-column member of matrix multiplication
        /// </summary>
        /// <param name="expr">Multidimensional linear expression argument.</param>
        /// <param name="multiplier">Multiplier for variable argument.</param>
        /// <param name="iRow">Row index.</param>
        /// <param name="iCol">Columnt index.</param>
        /// <returns>Resulst row-column multiplication member.</returns>
        private static JDLinExpr multiplicateOneMatrixElement(JDElement comp, IList multiplier, int iRow, int iCol, Func<int, int, object> getXYmember)
        {
            JDLinExpr expr = comp.ToJDLinExpr();
            JDLinExpr res = expr[iRow, 0].Multip(getXYmember(0, iCol));
            if (expr.YSize > 1)
            {
                for (int i = 1; i < expr.YSize; i++)
                {
                    res.Add(expr[iRow, i].Multip(getXYmember(i, iCol)));
                }
            }
            return res;
        }

        #region << + - * OPERATOR OVERLOAD >>
        #region << + OPERATOR OVERLOAD >>

        /// <summary>
        /// Sum of two JD linear expressions.
        /// </summary>
        /// <param name="lhs">Left-hand side expression</param>
        /// <param name="rhs">Right-hand side expression</param>
        /// <returns>Result expression</returns>
        public static JDLinExpr operator +(JDElement lhs, JDElement rhs)
        {
            JDLinExpr res;
            if (lhs.Numel > rhs.Numel)
            {
                res = new JDLinExpr(lhs.XSize, lhs.YSize, lhs.ScLinExprFactory);
                res.Add(lhs.ToJDLinExpr());
                res.Add(rhs.ToJDLinExpr());
                return res;
            }
            else
            {
                res = new JDLinExpr(rhs.XSize, rhs.YSize, lhs.ScLinExprFactory);
                res.Add(rhs.ToJDLinExpr());
                res.Add(lhs.ToJDLinExpr());
                return res;
            }
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary> 
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        private static JDLinExpr operatorPlus(JDElement lhs, object rhs)
        {
            JDLinExpr res;
            res = new JDLinExpr(lhs.XSize, lhs.YSize, lhs.ScLinExprFactory);
            res.Add(lhs);
            res.Add(rhs);
            return res;
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(JDElement lhs, object rhs)
        {
            return operatorPlus(lhs, rhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side double constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(JDElement lhs, double rhs)
        {
            return operatorPlus(lhs, rhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side double[] constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(JDElement lhs, double[] rhs)
        {
            return operatorPlus(lhs, rhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(JDElement lhs, double[][] rhs)
        {
            return operatorPlus(lhs, rhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary> 
        /// <param name="lhs">Left-hand side constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(object lhs, JDElement rhs)
        {
            return operatorPlus(rhs, lhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary> 
        /// <param name="lhs">Left-hand side double constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(double lhs, JDElement rhs)
        {
            return operatorPlus(rhs, lhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary> 
        /// <param name="lhs">Left-hand side double[] constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(double[] lhs, JDElement rhs)
        {
            return operatorPlus(rhs, lhs);
        }

        /// <summary>
        /// Sum of two JD objects.
        /// </summary> 
        /// <param name="lhs">Left-hand side double[][] constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator +(double[][] lhs, JDElement rhs)
        {
            return operatorPlus(rhs, lhs);
        }
        #endregion << + OPERATOR OVERLOAD >>
        #region << - OPERATOR OVERLOAD >>
        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        private static JDLinExpr operatorMinus(JDElement lhs, object rhs)
        {
            JDLinExpr jDLinExpr = lhs.Multip(-1);
            jDLinExpr.Add(rhs);
            return jDLinExpr.Multip(-1);
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        private static JDLinExpr operatorMinus(object lhs, JDElement rhs)
        {
            JDLinExpr jDLinExpr = rhs.Multip(-1);
            jDLinExpr.Add(lhs);
            return jDLinExpr;
        }

        /// <summary>
        /// Difference of two JD linear expressions.
        /// </summary>
        /// <param name="lhs">Left-hand side expression</param>
        /// <param name="rhs">Right-hand side expression</param>
        /// <returns>Result expression</returns>
        public static JDLinExpr operator -(JDElement lhs, JDElement rhs)
        {
            JDLinExpr negRhs = rhs.Multip(-1);
            JDLinExpr res = lhs + negRhs;
            return res;
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(JDElement lhs, object rhs)
        {
            return operatorMinus(lhs, rhs);
        }
        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side double constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(JDElement lhs, double rhs)
        {
            return operatorMinus(lhs, rhs);
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side double[] constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(JDElement lhs, double[] rhs)
        {
            return operatorMinus(lhs, rhs);
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side double[][] constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(JDElement lhs, double[][] rhs)
        {
            return operatorMinus(lhs, rhs);
        }
        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(object lhs, JDElement rhs)
        {
            return operatorMinus(lhs, rhs);
        }
        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side double constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(double lhs, JDElement rhs)
        {
            return operatorMinus(lhs, rhs);
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side double[] constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(double[] lhs, JDElement rhs)
        {
            return operatorMinus(lhs, rhs);
        }

        /// <summary>
        /// Difference of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side double[][] constant.</param>
        /// <param name="rhs">Right-hand side expression.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator -(double[][] lhs, JDElement rhs)
        {
            return operatorMinus(lhs, rhs);
        }
        #endregion << - OPERATOR OVERLOAD >>
        #region << * OPERATOR OVERLOAD >>
        /// <summary>
        /// Product of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator *(JDElement lhs, object rhs)
        {
            return lhs.Multip(rhs);
        }

        /// <summary>
        /// Product of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator *(JDElement lhs, double rhs)
        {
            return lhs.Multip(rhs);
        }

        /// <summary>
        /// Product of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator *(object lhs, JDElement rhs)
        {
            return rhs.Multip(lhs);
        }

        /// <summary>
        /// Product of two JD objects.
        /// </summary>
        /// <param name="lhs">Left-hand side expression.</param>
        /// <param name="rhs">Right-hand side constant.</param>
        /// <returns>Result expression.</returns>
        public static JDLinExpr operator *(double lhs, JDElement rhs)
        {
            return rhs.Multip(lhs);
        }

        /// <summary>
        /// Create a new expression by matrix multiplying a pair of JD objects.
        /// </summary>
        /// <param name="comp">Variable argument.</param>
        /// <param name="multiplier">Multiplier for variable argument.</param>        
        /// <returns>New linear expression.</returns>
        public static JDLinExpr operator *(JDElement comp, IList multiplier)
        {
            JDLinExpr expr = comp.ToJDLinExpr();
            int xSize, ySize;
            Func<int, int, object> getXYmember = _initXYGetterForMatrixMultiplication(comp, multiplier, out xSize, out ySize);
            if (expr.YSize == xSize)
            {
                // create array for result
                JDLinExpr prodExpr = JD.Zeros(expr.XSize, ySize).ToJDLinExpr();
                // matrix multiplication
                for (int x = 0; x < prodExpr.XSize; x++)
                {
                    for (int y = 0; y < prodExpr.YSize; y++)
                    {
                        prodExpr[x, y].Add(multiplicateOneMatrixElement(expr, multiplier, x, y, getXYmember));

                    }
                }
                return prodExpr;
            }
            else
            {
                throw new JDException(String.Format("Error: Bad JD objects size for multiplication: [{0} x {1}] * [{2} x {3}]",
                    expr.XSize, expr.YSize, xSize, ySize));
            }
        }

        private static Func<int, int, object> _initXYGetterForMatrixMultiplication(JDElement comp, IList multiplier, out int xSize, out int ySize)
        {
            multiplier.GetSize(out xSize, out ySize);
            if (multiplier.Is2D())
            {
                return multiplier.InitXYGetter();
            }
            else
            {
                if (comp.YSize == multiplier.Count)
                {
                    xSize = multiplier.Count;
                    ySize = 1;
                    return multiplier.GetSimpleListColumnVectorXYMember;
                }
                if (comp.XSize == multiplier.Count)
                {
                    xSize = 1;
                    ySize = multiplier.Count;
                    return multiplier.GetSimpleListRowVectorXYMember;
                }
                throw new JDException("First dimension size ({0}) or second dimension size ({1}) of JDElement operand must equal simple list multiplier count ({2})!",
                    comp.XSize, comp.YSize, multiplier.Count);
            }

        }

        /// <summary>
        /// Create a new expression by matrix multiplying a pair of JD objects.
        /// </summary>
        /// <param name="multiplier">Multiplier for variable argument.</param>  
        /// <param name="expr">Variable argument.</param>
        /// <returns>New linear expression.</returns>
        public static JDLinExpr operator *(IList multiplier, JDElement expr)
        {
            int xSize, ySize;
            multiplier.GetSize(out xSize, out ySize);
            IList rhs;
            if (multiplier.Is2D())
            {
                rhs = multiplier.TransposeSquare();
            }
            else
            {
                rhs = multiplier;
            }
            //JDLinExpr tExpr = expr.ToJDLinExpr().Transpose() * multiplier.TransposeSquare();
            JDLinExpr tExpr = expr.ToJDLinExpr().Transpose() * rhs;
            return tExpr.Transpose();
        }

        #endregion << * OPERATOR OVERLOAD >>
        #endregion << + - * OPERATOR OVERLOAD >>
        #region CONSTRAINT OPERATORS OVERLOADING
        /// <summary>
        /// Create constraint with JD.GREATER_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator >=(JDElement lhs, object rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.GREATER_EQUAL, rhs.ToJDConstant(lhs.ScLinExprFactory));
        }

        /// <summary>
        /// Create constraint with JD.GREATER_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator >=(object lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDConstant(rhs.ScLinExprFactory), JD.GREATER_EQUAL, rhs.ToJDLinExpr()); ;
        }

        /// <summary>
        /// Create constraint with JD.GREATER_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator >=(JDElement lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.GREATER_EQUAL, rhs.ToJDLinExpr());
        }

        /// <summary>
        /// Create constraint with JD.LESS_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator <=(JDElement lhs, object rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.LESS_EQUAL, rhs.ToJDConstant(lhs.ScLinExprFactory));
        }

        /// <summary>
        /// Create constraint with JD.LESS_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>        
        public static JDTempConstraint operator <=(object lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDConstant(rhs.ScLinExprFactory), JD.LESS_EQUAL, rhs.ToJDLinExpr());
        }

        /// <summary>
        /// Create constraint with JD.LESS_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator <=(JDElement lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.LESS_EQUAL, rhs.ToJDLinExpr());
        }

        /// <summary>
        /// Create constraint with JD.LESS_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns> 
        public static JDTempConstraint operator ==(JDElement lhs, object rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.EQUAL, rhs.ToJDConstant(lhs.ScLinExprFactory));
        }

        /// <summary>
        /// Create constraint with JD.LESS_EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns> 
        public static JDTempConstraint operator ==(object lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDConstant(rhs.ScLinExprFactory), JD.EQUAL, rhs.ToJDLinExpr());
        }


        /// <summary>
        /// Create constraint with JD.EQUAL sense.
        /// </summary>
        /// <param name="lhs">Left-hand side member.</param>
        /// <param name="rhs">Right-hand side member.</param>
        /// <returns>New constraint temporary object.</returns>
        public static JDTempConstraint operator ==(JDElement lhs, JDElement rhs)
        {
            return new JDTempConstraint(lhs.ToJDLinExpr(), JD.EQUAL, rhs.ToJDLinExpr());
        }

        /// <summary>
        /// Throw exception if != operator is used in JD constraints creating
        /// </summary>
        public static JDTempConstraint operator !=(JDElement lhs, object rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

        /// <summary>
        /// Throw exception if != operator is used in JD constraints creating
        /// </summary>
        public static JDTempConstraint operator !=(object lhs, JDElement rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }

        /// <summary>
        /// Throw exception if != operator is used in JD constraints creating
        /// </summary>
        public static JDTempConstraint operator !=(JDElement lhs, JDElement rhs)
        {
            throw new JDException("Bad operator '!=' used in constraint. Use <=, >= or ==.");
        }
        #endregion CONSTRAINT OPERATORS OVERLOADING
    }
}
