using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDUtils;
using System.Collections;

namespace JDSpace
{
    /// <summary>
    /// JD variable class represents two-dimensional matrix
    /// of scalar optimization variables.
    /// </summary>
    public class JDVar : JDElement
    {
        /// <summary>
        /// Get array of scalar optimization variables encapsulated in this object.
        /// </summary>
        public List<ScVar> VarList { get; private set; }

        private int _xSize;
        /// <summary>
        /// Get first dimension object size.
        /// </summary>
        public override int XSize { get { return _xSize; } }

        private int _ySize;
        /// <summary>
        /// Get second dimension object size.
        /// </summary>
        public override int YSize { get { return _ySize; } }

        /// <summary>
        /// Set the same branch priority of all JDVar variables.
        /// </summary>
        public int BranchPriority
        {
            set
            {
                foreach (ScVar var in VarList)
                {
                    var.BranchPriority = value;
                }
            }
        }

        /// <summary>
        /// IJDComparable implementation
        /// </summary>
        public override ScLinExpr GetScLinExpr(int i)
        {
            List<ScTerm> terms = new List<ScTerm>()
            {
                {ScLinExprFactory.CreateScTerm(VarList[i], 1)}
            };
            return ScLinExprFactory.CreateScLinExpr(terms, 0);
        }

        /// <summary>
        /// Returns a member (scalar variable) at position x,y.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar optimization variable.</returns>
        public JDVar this[int x, int y] { get { return this.Get(x, y); } }

        /// <summary>
        /// Returns subvariable from specific range.
        /// </summary>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        /// <returns>Optimization subvariable.</returns>
        public JDVar this[int x1, int x2, int y1, int y2]
        { get { return this.Get(x1, x2, y1, y2); } }

        /// <summary>
        /// Creates new optimization variable.
        /// </summary>
        /// <param name="xSize">First dimension size for new variable.</param>
        /// <param name="ySize">Second dimension size for new variable.</param>        
        internal JDVar(List<ScVar> varList, int xSize, int ySize, ScLinExprFactory scLinExprFactory)
        {
            ScLinExprFactory = scLinExprFactory;
            if (varList.Count != xSize * ySize)
            {
                throw new JDException("VarList lenght and xSize * ySize is different");
            }
            _xSize = xSize;
            _ySize = ySize;
            this.VarList = varList;
        }

        /// <summary>
        /// Creates scalar new JDVar instance using existing JDVar.
        /// </summary>
        /// <param name="subVar">Existing JDVar object.</param>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        internal JDVar(JDVar subVar, int x, int y, ScLinExprFactory scLinExprFactory)
        {
            ScLinExprFactory = scLinExprFactory;
            _xSize = 1;
            _ySize = 1;
            int idx = x * subVar.YSize + y;
            VarList = new List<ScVar>(1);
            VarList.Add(subVar.VarList[idx]);
        }

        /// <summary>
        /// Creates two-dimensional new JDVar instance using existing JDVar.
        /// </summary>
        /// <param name="supVar">Existing JDVar object.</param>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        internal JDVar(JDVar supVar, int x1, int x2, int y1, int y2, ScLinExprFactory scLinExprFactory)
        {
            if (x2 < x1)
                throw new JDException(String.Format("Second index in first dimension {0} is greater than first one {1} ", x2, x1));
            if (y2 < y1)
                throw new JDException(String.Format("Second index in second dimension {0} is greater than first one {1} ", y2, y1));
            ScLinExprFactory = scLinExprFactory;
            _ySize = y2 - y1 + 1;
            _xSize = x2 - x1 + 1;
            int i, j; // row range
            i = x1 * supVar.YSize + y1;
            j = x1 * supVar.YSize + y2;
            int count = j - i + 1;
            VarList = null;
            try
            { VarList = supVar.VarList.GetRange(i, count); } // prvni radek nove JDVar
            catch (ArgumentOutOfRangeException ex)
            { throw new JDException(String.Format("Error when indexing JDsubvariable with starting index {0} and count {1}.", i, count), ex); }
            catch (ArgumentException ex)
            { throw new JDException(String.Format("JDSubvariable size is {0} x {1}. Start index is {2} and the number of elements is {3}. ", supVar.XSize, supVar.YSize, i, count), ex); }
            catch (Exception ex)
            { throw new JDException(String.Format("Other error when getting {0} JDsubvariables from index {1}.", count, i), ex); }
            for (int ix = x1 + 1; ix < (x2 + 1); ix++) // pridat dalsi radky
            {
                i = ix * supVar.YSize + y1;
                j = ix * supVar.YSize + y2;
                count = j - i + 1;
                VarList.AddRange(supVar.VarList.GetRange(i, count));
            }
        }

        /// <summary>
        /// Returns a member (scalar variable) at position x,y.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar optimization variable.</returns>
        public JDVar Get(int x, int y)
        {
            return new JDVar(this, x, y, ScLinExprFactory);
        }

        /// <summary>
        /// Returns subvariable from specific range.
        /// </summary>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        /// <returns>Optimization subvariable.</returns>
        public JDVar Get(int x1, int x2, int y1, int y2)
        {
            return new JDVar(this, x1, x2, y1, y2, ScLinExprFactory);
        }

        /// <summary>
        /// Sum of all scalar object subvariables.
        /// </summary>
        /// <returns>New linear expression.</returns>
        public JDLinExpr Sum()
        {
            //Dictionary<int, ScTerm> terms = VarList.ToDictionary(x => x.Id, x => new ScTerm(x, 1));
            List<ScTerm> terms = VarList.Select(x => ScLinExprFactory.CreateScTerm(x, 1)).ToList();
            JDLinExpr varSum = new JDLinExpr(ScLinExprFactory.CreateScLinExpr(terms, 0), ScLinExprFactory);
            return varSum;
        }

        /// <summary>
        /// Returns sums along different dimensions of this variable.
        /// </summary>
        /// <param name="dim">Dimension index {0, 1}. 0 - sum via columns. 1 - sum via rows.</param>
        /// <returns>New linear expression.</returns>
        public JDLinExpr Sum(int dim)
        {
            JDLinExpr sumArr;
            if (dim == 0) // sum via columns
            {
                sumArr = new JDLinExpr(1, YSize, ScLinExprFactory);
                for (int i = 0; i < this.YSize; i++)
                {
                    sumArr[0, i].Add(this[0, this.XSize - 1, i, i].Sum()); // create sum of current column
                }
                return sumArr;
            }
            else if (dim == 1) // sum via rows - JDLinExpr
            {
                sumArr = new JDLinExpr(XSize, 1, ScLinExprFactory);
                for (int i = 0; i < this.XSize; i++)
                {
                    sumArr[i, 0].Add(this[i, i, 0, this.YSize - 1].Sum()); // create sum of current row
                }
                return sumArr;
            }
            else
            {
                throw new JDException("Bad dimension selected: " + dim + "\n");
            }
        }

        /// <summary>
        /// Set upper bound of variables
        /// </summary>
        /// <param name="ub">Upper bound</param>
        public void SetUb(double ub)
        {
            foreach (ScVar var in VarList)
                var.Ub = ub;
        }

        /// <summary>
        /// Set lower bound of variables
        /// </summary>
        /// <param name="lb">Lower bound</param>
        public void SetLb(double lb)
        {
            foreach (ScVar var in VarList)
                var.Lb = lb;
        }

        /// <summary>
        /// Set branch priority of variables
        /// </summary>
        /// <param name="priority">Branch priority</param>
        public void SetBranchPriority(int priority)
        {
            foreach (ScVar var in VarList)
                var.BranchPriority = priority;
        }

        /// <summary>
        /// Product by elements of constant and JD optimization variable. When it's multidimensional
        /// JDVar then via rows multiplication is performed!:
        /// |a b|                   |1*a 2*b|
        /// |c d|.Term([1 2 3 4]) = |3*c 4*d|
        /// </summary>
        /// <param name="coeff">Coefficients array.</param>
        /// <returns>New multidimensional linear expression.</returns>
        public JDLinExpr Term(IList coeff)
        {
            if (Numel == coeff.Count)
            {
                JDLinExpr jdLinExpr = new JDLinExpr(XSize, YSize, ScLinExprFactory);
                jdLinExpr.AddTerm(coeff, this.VarList);
                return jdLinExpr;
            }
            else
            {
                throw new JDException(String.Format("Numel ({0}) of scalar variables disagrees with coefficient array length ({1})",
                    Numel, coeff.Count));
            }
        }

        /// <summary>
        /// Multiple of this linear expression.
        /// </summary>
        /// <param name="multiplier">Expression multiplier.</param>
        /// <returns>New result linear expression.</returns>
        public override JDLinExpr Multip(object multiplier)
        {
            // Create empty lin. expr. of the same size.
            JDLinExpr mJdLinExpr = _multipOld(multiplier);
            return mJdLinExpr;
        }

        /// <summary>
        /// Multiple of this linear expression.
        /// </summary>
        /// <param name="multiplier">Expression multiplier.</param>
        /// <returns>New result linear expression.</returns>
        protected JDLinExpr _multipOld(object multiplier)
        {
            // Create empty lin. expr. of the same size.
            JDLinExpr mJdLinExpr = new JDLinExpr(XSize, YSize, ScLinExprFactory);
            for (int i = 0; i < Numel; i++)
            {
                mJdLinExpr.LinExprs[i].AddTerm(multiplier, VarList[i]);
            }
            return mJdLinExpr;
        }

        /// <summary>
        /// Create transpose version of this variable.
        /// </summary>
        /// <returns>New variable - transposition of this variable.</returns>
        public JDVar Transpose()
        {
            List<ScVar> tVarList = VarList.Transpose(XSize, YSize).ToList();
            return new JDVar(tVarList, YSize, XSize, ScLinExprFactory);
        }

        /// <summary>
        /// Returns solved value of a member at x, y position from
        /// most recent optimization.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar variable value.</returns>
        public double ToDouble(int x, int y)
        {
            int idx = x * this.YSize + y;
            return (double)VarList[idx].Value;
        }

        /// <summary>
        /// Returns solved value of a member at x, y position from
        /// most recent optimization.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar variable value.</returns>
        public double? ToDouble2(int x, int y)
        {
            int idx = x * this.YSize + y;
            return VarList[idx].Value;
        }

        /// <summary>
        /// Converts this variable to linear expression.
        /// </summary>
        /// <returns>New linear expression.</returns>
        internal override JDLinExpr ToJDLinExpr()
        {
            return Multip(1);
        }

        /// <summary>
        /// Writes a variable value from most recent optimization
        /// to the standard output stream.
        /// </summary>
        public void Print()
        {
            double?[,] arr = ToDoubleMat2();
            for (int i = 0; i < XSize; i++)
            {
                for (int j = 0; j < YSize; j++)
                {
                    Console.Write(arr[i, j].ToString2() + "\t");
                }
                Console.Write("\n");
            }
        }

        /// <summary>
        /// Returns solved value of a variable from most recent
        /// optimization. 
        /// </summary>
        /// <returns>Variable value.</returns>
        public double[,] ToDoubleMat()
        {
            double[,] arr = new double[this.XSize, this.YSize];
            for (int ix = 0; ix < this.XSize; ix++)
            {
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    arr[ix, iy] = this.ToDouble(ix, iy);
                }
            }
            return arr;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent optimization. 
        /// </summary>
        /// <returns>Variable value.</returns>
        public double?[,] ToDoubleMat2()
        {
            double?[,] arr = new double?[this.XSize, this.YSize];
            for (int ix = 0; ix < this.XSize; ix++)
            {
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    arr[ix, iy] = this.ToDouble2(ix, iy);
                }
            }
            return arr;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent optimization. 
        /// </summary>
        /// <returns></returns>
        public double[][] ToDoubleArrays()
        {
            double[][] arrs = new double[this.XSize][];
            for (int ix = 0; ix < this.XSize; ix++)
            {
                arrs[ix] = new double[this.YSize];
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    arrs[ix][iy] = this.ToDouble(ix, iy);
                }
            }
            return arrs;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent optimization. Values can be null.
        /// </summary>
        /// <returns></returns>
        public double?[][] ToDoubleArrays2()
        {
            double?[][] arrs = new double?[this.XSize][];
            for (int ix = 0; ix < this.XSize; ix++)
            {
                arrs[ix] = new double?[this.YSize];
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    arrs[ix][iy] = this.ToDouble2(ix, iy);
                }
            }
            return arrs;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent optimization. 
        /// All values are in one vector.
        /// </summary>
        /// <returns></returns>
        public double[] ToDoubleArray()
        {
            double[] arrs = new double[this.XSize * this.YSize];
            for (int ix = 0; ix < this.XSize; ix++)
            {
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    arrs[ix * this.YSize + iy] = this.ToDouble(ix, iy);
                }
            }
            return arrs;
        }

        /// <summary>
        /// Overrided ToString() method
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int ix = 0; ix < this.XSize; ix++)
            {
                for (int iy = 0; iy < this.YSize; iy++)
                {
                    sb.AppendFormat("{0,5}", this.ToDouble(ix, iy));
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }

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
