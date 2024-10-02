using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JDSpace;
using JDUtils;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics.Contracts;

namespace JDSpace
{
    /// <summary>
    /// JD linear expression class represents two-dimensional matrix
    /// of scalar linear expressions.
    /// </summary>
    public class JDLinExpr : JDElement
    {
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
        /// Returns a member (scalar linear expression) at position x,y.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar linear expression.</returns>
        public JDLinExpr this[int x, int y] { get { return this.Get(x, y); } }

        /// <summary>
        /// Returns a member (scalar linear expression) at position x1,y1,x2,y2
        /// </summary>
        /// <param name="x1">First dimension start coordinate</param>
        /// <param name="x2">First dimension end coordinate</param>
        /// <param name="y1">Second dimension start coordinate</param>
        /// <param name="y2">Second dimension end coordinate</param>
        /// <returns>Scalar linear expression.</returns>
        public JDLinExpr this[int x1, int x2, int y1, int y2]
        { get { return this.Get(x1, x2, y1, y2); } }

        /// <summary>
        /// Get array of all scalar linear expressions encapsulated in this object.
        /// </summary>
        public List<ScLinExpr> LinExprs { get; private set; }

        private Func<int, ScLinExpr> _getMember;

        internal JDLinExpr(ScLinExprFactory scLinExprFactory)
        {
            if (scLinExprFactory == null) { ScLinExprFactory = new ScLinExprFactory(composed: false); }
            else
            {
                ScLinExprFactory = scLinExprFactory;
            }

        }

        /// <summary>
        /// Create new instance of linear expression.
        /// </summary>
        /// <param name="linExprs">Scalar linear expressions array.</param>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory.</param>
        internal JDLinExpr(List<ScLinExpr> linExprs, int xSize, int ySize, ScLinExprFactory scLinExprFactory)
            : this(scLinExprFactory)
        {
            if (linExprs.Count != xSize * ySize)
            {
                throw new JDException("VarList lenght and xSize * ySize is different");
            }
            _xSize = xSize;
            _ySize = ySize;
            _initGetMemberFunc();
            LinExprs = linExprs;
        }

        /// <summary>
        /// Create new instance of scalar linear expression from existing
        /// scalar linear expression.
        /// </summary>
        /// <param name="scLinExpr">Existing scalar linear expression.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory.</param>
        internal JDLinExpr(ScLinExpr scLinExpr, ScLinExprFactory scLinExprFactory)
            : this(scLinExprFactory)
        {
            _xSize = 1;
            _ySize = 1;
            _initGetMemberFunc();
            LinExprs = new List<ScLinExpr>(1);
            LinExprs.Add(scLinExpr);
        }

        /// <summary>
        /// Class constructor. Create instance of two-dimensional linear expression.
        /// </summary>
        /// <param name="xSize">First dimension size.</param>
        /// <param name="ySize">Second dimension size.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory.</param>
        internal JDLinExpr(int xSize, int ySize, ScLinExprFactory scLinExprFactory)
            : this(scLinExprFactory)
        {
            _xSize = xSize;
            _ySize = ySize;
            _initGetMemberFunc();
            int len = xSize * ySize;
            LinExprs = new List<ScLinExpr>(len);
            for (int i = 0; i < len; i++)
            {
                LinExprs.Add(ScLinExprFactory.CreateScLinExpr());
            }
        }

        /// <summary>
        /// Creates two-dimensional new JDLinExpr instance using existing
        /// JDLinExpr object.
        /// </summary>
        /// <param name="subLinExpr">Existing JDLinExpr object.</param>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        /// <param name="scLinExprFactory">ScLinExprFactory.</param>
        internal JDLinExpr(JDLinExpr subLinExpr, int x1, int x2, int y1, int y2, ScLinExprFactory scLinExprFactory)
            : this(scLinExprFactory)
        {
            _xSize = x2 - x1 + 1;
            _ySize = y2 - y1 + 1;
            _initGetMemberFunc();
            int capacity = XSize * YSize;
            LinExprs = new List<ScLinExpr>(capacity);
            int num = 0;
            for (int i = x1; i < x2 + 1; i++)
            {
                for (int j = y1; j < y2 + 1; j++)
                {
                    num = i * subLinExpr.YSize + j;
                    LinExprs.Add(subLinExpr.LinExprs[num]);
                }
            }
        }

        private void _initGetMemberFunc()
        {
            if (Numel == 1)
            {
                _getMember = (idx) => { return LinExprs[0]; };
            }
            else
            {
                _getMember = (idx) => { return LinExprs[idx]; };
            }
        }

        /// <summary>
        /// Returns a member (scalar linear expression) at position x,y.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar linear expression.</returns>
        public JDLinExpr Get(int x, int y)
        {
            int idx = x * this.YSize + y;
            return new JDLinExpr(this.LinExprs[idx], ScLinExprFactory);
        }

        /// <summary>
        /// Returns sub-linear expression from specific range.
        /// </summary>
        /// <param name="x1">First line index of choosen line range.</param>
        /// <param name="x2">End line index of choosen line range.</param>
        /// <param name="y1">First column index of choosen column range.</param>
        /// <param name="y2">End column index of choosen column range.</param>
        /// <returns>New JDLinExpr object.</returns>
        public JDLinExpr Get(int x1, int x2, int y1, int y2)
        {
            return new JDLinExpr(this, x1, x2, y1, y2, ScLinExprFactory);
        }

        /// <summary>
        /// Adds an expression into the expression.
        /// </summary>
        /// <param name="comp">Expression to be add.</param>
        public void Add(JDElement comp)
        {
            JDLinExpr jdLinExpr = comp.ToJDLinExpr();
            // Rozmery pridavaneho JDLinExpr (argumentu) musi byt stejne jako rozmery tohoto
            if ((jdLinExpr.XSize == XSize) && (jdLinExpr.YSize == YSize))
            {
                for (int i = 0; i < LinExprs.Count; i++)
                {
                    this.LinExprs[i].Add(jdLinExpr.LinExprs[i]);
                }
            }
            else
            {
                // pridava se jen jednoprvkovy JDLinExpr - prida se ke kazdemu prvku tohoto JDLinExpru
                if ((jdLinExpr.XSize == 1) && (jdLinExpr.YSize == 1))
                {
                    for (int i = 0; i < LinExprs.Count; i++)
                    {
                        // ke kazdemu prvku tohoto pridame jednoprvkovy JD lin. vyr (argument)
                        this.LinExprs[i].Add(jdLinExpr.LinExprs[0]);
                    }
                }
                else
                {
                    // nesedi rozmery 
                    throw new JDException("Dimensions are different!");
                }
            }
        }

        /// <summary>
        /// Add constant object into the expression
        /// </summary>
        /// <param name="constant">Constant object</param>
        public void Add(object constant)
        {
            int xSize, ySize;
            object unWrappedScalar = null;
            if (constant.IsScalar(out unWrappedScalar, out xSize, out ySize))
            {
                _addScalar(unWrappedScalar);
            }
            else
            {
                if ((constant as IList).Is2D())
                {
                    _add2DList(constant as IList, xSize, ySize);
                }
                else
                {
                    _addSimpleList(constant as IList);
                }
            }
        }

        /// <summary>
        /// Adds a constant into the expression.
        /// </summary>
        /// <param name="constant">Constant to be add.</param>
        private void _addScalar(object constant)
        {
            for (int i = 0; i < LinExprs.Count; i++)
            {
                this.LinExprs[i].Add(constant);
            }
        }

        /// <summary>
        /// Adds a constants into the expression.
        /// </summary>
        /// <param name="constant">Constant to be add.</param>
        private void _addList(IList constant)
        {
            int xSize, ySize;
            constant.GetSize(out xSize, out ySize);
            if (constant.Is2D())
            {
                _add2DList(constant, xSize, ySize);
            }
            else
            {
                _addSimpleList(constant);
            }
        }

        /// <summary>
        /// Adds a constants into the expression.
        /// </summary>
        /// <param name="constants">Constant to be add.</param>
        /// <param name="xSize">First coordinate where the constants are added</param>
        /// <param name="ySize">Second coordinate where the constants are added</param>
        private void _add2DList(IList constants, int xSize, int ySize)
        {
            if ((xSize == this.XSize) && (ySize == this.YSize))
            {
                Func<int, int, object> getXYmember = constants.InitXYGetter();
                int idx = 0;
                for (int ix = 0; ix < this.XSize; ix++) // via column iteration
                {
                    for (int iy = 0; iy < this.YSize; iy++) // via row iteration
                    {
                        LinExprs[idx].Add(getXYmember(ix, iy));
                        idx++;
                    }
                }
            }
            else
            {
                // nesedi rozmery 
                throw new JDException("Dimensions are different!");
            }
        }

        /// <summary>
        /// Adds a constants into the expression.
        /// </summary>
        /// <param name="constants">Constant to be add.</param>
        private void _addSimpleList(IList constants)
        {
            if (Numel == constants.Count)
            {
                for (int i = 0; i < Numel; i++)
                {
                    LinExprs[i].Add(constants[i]);
                }
            }
            else
            {
                throw new JDException("Different numels: Lin. expr. numel: {0}, added array numel {1}",
                    Numel, constants.Count);
            }
        }

        /// <summary>
        /// Adds a terms to ScalarLinExprs array. For multidimensional 
        /// JDLinExpr is not recommended this method usage.
        /// </summary>
        /// <param name="coeff">Coefficient array.</param>
        /// <param name="vars">Optimization variable list object.</param>
        internal void AddTerm(IList coeff, List<ScVar> vars)
        {
            for (int i = 0; i < vars.Count; i++)
            {
                LinExprs[i].AddTerm(coeff[i], vars[i]);
            }
        }

        /// <summary>
        /// Create transposotion of this two-dimensional lin. expr.
        /// </summary>
        /// <returns>New linear expression.</returns>
        public JDLinExpr Transpose()
        {
            List<ScLinExpr> tLinExprs = LinExprs.Transpose(XSize, YSize).ToList();
            return new JDLinExpr(tLinExprs, YSize, XSize, ScLinExprFactory);
        }

        /// <summary>
        /// Sum of all scalar object subvariables.
        /// </summary>
        /// <returns>New linear expression.</returns>
        public JDLinExpr Sum()
        {
            ScLinExpr exprSum = ScLinExprFactory.CreateScLinExpr();
            for (int i = 0; i < LinExprs.Count; i++)
            {
                exprSum.Add(LinExprs[i]);
            }
            return new JDLinExpr(exprSum, ScLinExprFactory);
        }

        /// <summary>
        /// Returns sums along different dimensions of this matrix-lin. expr.
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
            else
            {
                if (dim == 1) // sum via rows - JDLinExpr
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
                    throw new JDException("Error: Bad dimension selected: " + dim + "\n");
                }
            }
        }

        /// <summary>
        /// Returns solved value of a member at x, y position from
        /// most recent optimization.
        /// </summary>
        /// <param name="x">First dimension required member coordinate.</param>
        /// <param name="y">Second dimension required member coordinate.</param>
        /// <returns>Scalar lin. expr. value.</returns>
        public double? ToDouble(int x, int y)
        {
            int idx = x * YSize + y;
            return LinExprs[idx].Value;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent
        /// optimization. 
        /// </summary>
        /// <returns>Variable value.</returns>
        public double?[,] ToDouble()
        {
            double?[,] arr = new double?[this.XSize, this.YSize];
            for (int xi = 0; xi < XSize; xi++)
            {
                for (int yi = 0; yi < YSize; yi++)
                {
                    arr[xi, yi] = ToDouble(xi, yi);
                }
            }
            return arr;
        }

        /// <summary>
        /// Returns solved value of a variable from most recent
        /// optimization. 
        /// </summary>
        /// <returns>Variable value.</returns>
        public double?[][] ToDoubleArrays()
        {
            double?[][] array = new double?[XSize][];
            for (int i = 0; i < XSize; i++)
            {
                array[i] = new double?[YSize];
                for (int j = 0; j < YSize; j++)
                {
                    ref double? reference = ref array[i][j];
                    reference = ToDouble(i, j);
                }
            }
            return array;
        }

        /// <summary>
        /// Returns solved value of a member at x dimension from
        /// most recent optimization.
        /// </summary>
        /// <param name="xIndex">Dimension required member coordinate.</param>
        /// <returns>Scalar lin. expr. value.</returns>
        public double?[] ToDoubleArrayX(int xIndex)
        {
            double?[] array = new double?[YSize];
            for (int i = 0; i < YSize; i++)
            {
                ref double? reference = ref array[i];
                reference = ToDouble(xIndex, i);
            }

            return array;
        }

        /// <summary>
        /// Returns solved value of a member at y dimension from
        /// most recent optimization.
        /// </summary>
        /// <param name="yIndex">Dimension required member coordinate.</param>
        /// <returns>Scalar lin. expr. value.</returns>
        public double?[] ToDoubleArrayY(int yIndex)
        {
            double?[] array = new double?[XSize];
            for (int i = 0; i < XSize; i++)
            {
                ref double? reference = ref array[i];
                reference = ToDouble(i, yIndex);
            }

            return array;
        }

        /// <summary>
        /// Writes a variable value from most recent optimization
        /// to the standard output stream.
        /// </summary>
        public void Print()
        {
            double?[,] arr = ToDouble();
            for (int i = 0; i < this.XSize; i++)
            {
                for (int j = 0; j < YSize; j++)
                {
                    Console.Write(arr[i, j].ToString2() + "\t");
                }
                Console.Write("\n");
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
            JDLinExpr mJdLinExpr = new JDLinExpr(XSize, YSize, ScLinExprFactory);
            for (int i = 0; i < LinExprs.Count; i++)
            {
                mJdLinExpr.LinExprs[i].Add(multiplier, LinExprs[i]);
            }
            return mJdLinExpr;
        }

        /// <summary>
        /// JDComparable implementation.
        /// </summary>
        internal override JDLinExpr ToJDLinExpr()
        {
            return this;
        }

        /// <summary>
        /// IJDComparable implementation
        /// </summary>
        public override ScLinExpr GetScLinExpr(int i)
        {
            return _getMember(i);
        }

        /// <summary>
        /// Multiply expesions element wise.
        /// </summary>
        /// <param name="coefficients">Array with constants.</param>
        /// <returns>New multiplied result linear expression.</returns>
        public JDLinExpr DotMultiply(double[] coefficients)
        {
            return DotMultiply(new double[1][] { coefficients });
        }

        /// <summary>
        /// Multiply expesions element wise.
        /// </summary>
        /// <param name="coefficients">Array with constants.</param>
        /// <returns>New multiplied result linear expression.</returns>
        public JDLinExpr DotMultiply(double[][] coefficients)
        {
            if (coefficients.Any((double[] x) => x == null) || coefficients.Length == 0 || coefficients.Any((double[] x) => x.Length != coefficients[0].Length))
            {
                throw new JDException("Dimensions doesn't match, some collums are null or collumn lenght doesn't match.");
            }

            JDLinExpr jDLinExpr;
            ScLinExpr[][] array2;
            if (coefficients.Length == XSize && coefficients[0].Length == YSize)
            {
                jDLinExpr = new JDLinExpr(XSize, YSize, base.ScLinExprFactory);
                double[][] array = coefficients;
                array2 = AsArray();
            }
            else if (coefficients.Length == 1 && coefficients[0].Length == 1)
            {
                jDLinExpr = new JDLinExpr(XSize, YSize, base.ScLinExprFactory);
                double[][] array = new double[XSize][];
                array2 = AsArray();
                for (int i = 0; i < XSize; i++)
                {
                    array[i] = new double[YSize];
                    for (int j = 0; j < YSize; j++)
                    {
                        array[i][j] = coefficients[0][0];
                    }
                }
            }
            else
            {
                if (XSize != 1 || YSize != 1)
                {
                    throw new JDException($"Dimensions doesn't match");
                }

                jDLinExpr = new JDLinExpr(coefficients.Length, coefficients[0].Length, base.ScLinExprFactory);
                double[][] array = coefficients;
                array2 = new ScLinExpr[coefficients.Length][];
                for (int i = 0; i < coefficients.Length; i++)
                {
                    array2[i] = new ScLinExpr[coefficients[0].Length];
                    for (int j = 0; j < coefficients[0].Length; j++)
                    {
                        array2[i][j] = LinExprs[0];
                    }
                }
            }
            for (int i = 0; i < jDLinExpr.XSize; i++)
            {
                for (int j = 0; j < jDLinExpr.YSize; j++)
                {
                    int index = i * YSize + j;
                    jDLinExpr.LinExprs[index].Add(array2[i][j].Constant.ToDouble() * coefficients[i][j]);
                    foreach (ScTerm term in array2[i][j].Terms)
                    {
                        jDLinExpr.LinExprs[index].AddTerm(term.Coeff.ToDouble() * coefficients[i][j], term.Var);
                    }
                }
            }

            return jDLinExpr;
        }

        /// <summary>
        /// Convert LinExprs to array
        /// </summary>
        public ScLinExpr[][] AsArray()
        {
            ScLinExpr[][] array = new ScLinExpr[XSize][];
            for (int i = 0; i < XSize; i++)
            {
                array[i] = new ScLinExpr[YSize];
                for (int j = 0; j < YSize; j++)
                {
                    Contract.Assert(this[i, j].LinExprs.Count == 1);
                    array[i][j] = this[i, j].LinExprs[0];
                }
            }
            return array;
        }

        /// <summary>
        /// Product by elements of constant and JD optimization variable. 
        /// When it's multidimensional JDVar then via rows multiplication is performed!
        /// |a b c| |3*a 3*b 3*c| |d e f|.Term([3 4]) = |4*d 4*e 4*f|
        /// </summary>
        /// <param name="coeff">Coefficients array.</param>
        /// <returns>New multidimensional linear expression.</returns>
        public JDLinExpr TermY(IList coeff)
        {
            if (YSize == coeff.Count)
            {
                JDLinExpr jDLinExpr = new JDLinExpr(XSize, YSize, base.ScLinExprFactory);
                for (int i = 0; i < XSize; i++)
                {
                    for (int j = 0; j < YSize; j++)
                    {
                        jDLinExpr.LinExprs[i + j * XSize].Add(LinExprs[i + j * XSize].Constant.ToDouble() * coeff[j].ToDouble());
                        foreach (ScTerm term in LinExprs[i + j * XSize].Terms)
                        {
                            jDLinExpr.LinExprs[i + j * XSize].AddTerm(term.Coeff.ToDouble() * coeff[j].ToDouble(), term.Var);
                        }
                    }
                }

                return jDLinExpr;
            }

            throw new Exception($"YSize ({YSize}) of scalar variables disagrees with coefficient array length ({coeff.Count})");
        }

        /// <summary>
        /// Product by elements of constant and JD optimization variable. 
        /// When it's multidimensional JDVar then via rows multiplication is performed!
        /// |a b| |1*a 2*b| |c d|.Term([1 2 3 4]) = |3*c 4*d|
        /// </summary>
        /// <param name="coeff">Coefficients array.</param>
        /// <returns>New multidimensional linear expression.</returns>
        public JDLinExpr Term(IList coeff)
        {
            if (base.Numel == coeff.Count)
            {
                JDLinExpr jDLinExpr = new JDLinExpr(XSize, YSize, base.ScLinExprFactory);
                for (int i = 0; i < coeff.Count; i++)
                {
                    jDLinExpr.LinExprs[i].Add(LinExprs[i].Constant.ToDouble() * coeff[i].ToDouble());
                    foreach (ScTerm term in LinExprs[i].Terms)
                    {
                        jDLinExpr.LinExprs[i].AddTerm(term.Coeff.ToDouble() * coeff[i].ToDouble(), term.Var);
                    }
                }
                return jDLinExpr;
            }

            throw new Exception($"Numel ({base.Numel}) of scalar variables disagrees with coefficient array length ({coeff.Count})");
        }
    }
}
