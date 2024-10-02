using System;
using System.Collections;
using System.Collections.Generic;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JD extenders
    /// </summary>
    public static class JDExtenders
    {
        /// <summary>
        /// To string method that returns "null" when object is null
        /// </summary>
        /// <param name="t">Object</param>
        /// <returns>String</returns>
        public static string ToString2(this double? t)
        {
            if (t == null)
            {
                return "null";
            }
            return t.ToString();
        }

        /// <summary>
        /// Creates JDConstant from given object using SsLinExprFactory 
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="scFactory">Scalar linear expression factory</param>
        /// <returns>JD constant</returns>
        internal static JdConstant ToJDConstant(this object obj, ScLinExprFactory scFactory)
        {
            int xSize, ySize;
            object unwr = null;
            if (obj.IsScalar(out unwr, out xSize, out ySize))
            {
                return new ObjectJDConstant(unwr, scFactory); ;
            }
            else if (obj is Array)
            {
                Array arr = obj as Array;
                int rank = arr.Rank;
                if (rank == 2) return new DoubleArr2dJDConstant(arr, scFactory);
            }
            if (obj is IList)
            {
                IList list = obj as IList;
                if (list[0] is IList) return new DoubleArrJagRhs(list, scFactory);
                return new DoubleArrJDConstant(list, scFactory);
            }
            throw new JDException("JDConstant conversion unsupported for {0}!", obj.GetType().Name);
        }

        /// <summary>
        /// Convert object to ComposedConstant.
        /// </summary>
        internal static ComposedConstant ToComposedConstant(this object obj)
        {
            if (obj is ComposedConstant) return obj as ComposedConstant;
            return new NamedConst(obj);
        }

        internal static object GetJaggXYMember(this IList jagList, int x, int y)
        {
            return (jagList[x] as IList)[y];
        }

        internal static object Get2DArrXYMember(this IList List2DArr, int x, int y)
        {
            return (List2DArr as Array).GetValue(x, y);
        }

        internal static object GetSimpleListColumnVectorXYMember(this IList List2DArr, int x, int y)
        {
            if (y == 0)
            {
                return List2DArr[x];
            }
            throw new JDException("Second coordinate must be zero for simple list two-dimensional indexing as column vector! Inserted indices ({0},{1})!", x, y);
        }

        internal static object GetSimpleListRowVectorXYMember(this IList List2DArr, int x, int y)
        {
            if (x == 0)
            {
                return List2DArr[y];
            }
            throw new JDException("First coordinate must be zero for simple list two-dimensional indexing as row vector! Inserted indices ({0},{1})!", x, y);
        }

        internal static double ToDouble(this object obj)
        {
            double d;
            if (obj is ComposedConstant) return (obj as ComposedConstant).DoubleValue;
            return d = Convert.ToDouble(obj);
        }

        internal static Func<int, int, object> InitXYGetter(this IList list)
        {
            if (list.IsJagged())
                return list.GetJaggXYMember;
            return list.Get2DArrXYMember;
        }

        internal static void GetSize(this object obj, out int xSize, out int ySize)
        {
            if (obj is IList)
            {
                (obj as IList)._getSize(out xSize, out ySize);
                return;
            }
            xSize = 1;
            ySize = 1;
        }

        internal static bool Is2D(this IList list)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2) return true;
            }
            if (list[0] is IList) return true;
            return false;
        }

        internal static bool IsJagged(this IList list)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2) return false;
            }
            if (list[0] is IList) return true;
            return false;
        }

        private static void _getSize(this IList list, out int xSize, out int ySize)
        {
            if (list is Array)
            {
                Array arr = list as Array;
                if (arr.Rank == 2)
                {
                    xSize = arr.GetLength(0);
                    ySize = arr.GetLength(1);
                    return;
                }
            }
            xSize = list.Count;
            ySize = 1;
            if (list[0] is IList)
            {
                ySize = (list[0] as IList).Count;
            }
        }

        internal static IList TransposeSquare(this IList toTranspose)
        {
            int xSize, ySize;
            toTranspose.GetSize(out xSize, out ySize);
            Func<int, int, object> xyGetter = toTranspose.InitXYGetter();
            object[,] tList = new object[ySize, xSize];
            for (int ix = 0; ix < xSize; ix++)
            {
                for (int iy = 0; iy < ySize; iy++)
                {
                    tList[iy, ix] = xyGetter(ix, iy);
                }
            }
            return tList;
        }

        internal static bool IsScalar(this object obj, out object unWrapedScalar, out int xSize, out int ySize)
        {
            obj.GetSize(out xSize, out ySize);
            if ((xSize == 1) && (ySize == 1))
            {
                if (obj is IList)
                {
                    IList list = obj as IList;
                    if (list.Is2D())
                    {
                        if (list.IsJagged()) { unWrapedScalar = list.GetJaggXYMember(0, 0); }
                        else { unWrapedScalar = list.Get2DArrXYMember(0, 0); }
                        return true;
                    }
                    else
                    {
                        unWrapedScalar = list[0];
                        return true;
                    }
                }
                unWrapedScalar = obj;
                return true;
            }
            unWrapedScalar = null;
            return false;
        }

        internal static IList<T> Transpose<T>(this IList<T> toTranspose, int xSize, int ySize)
        {
            T[] tList = new T[toTranspose.Count];
            int x, y, i1, i2;
            for (int i = 0; i < toTranspose.Count; i++)
            {
                y = i % ySize;
                x = i / ySize;
                i1 = x * ySize + y;
                i2 = y * xSize + x;
                tList[i2] = toTranspose[i1];
            }
            return tList;
        }
    }
}