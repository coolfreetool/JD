using System;
using System.Collections;
using System.Collections.Generic;

namespace JDSpace
{
    /// <summary>
    /// Composed features extension methods.
    /// </summary>
    public static class ComposedExtenders
    {
        /// <summary>
        /// Converts object to NamedConst or NamedConst collection (1D or 2D according to inserted object)
        /// with specific name (scalar value) or name prefix (name + index).
        /// </summary>
        public static object Name(this object t, string name)
        {
            object unWrappedObj = null;
            int xSize, ySize;
            if (t.IsScalar(out unWrappedObj, out xSize, out ySize))
            {
                return new NamedConst(name, Convert.ToDouble(unWrappedObj));
            }
            else
            {
                IList list = t as IList;
                if (list.Is2D())
                {
                    Func<int, int, object> xyGetter = list.InitXYGetter();
                    NamedConst[,] ncArr = new NamedConst[xSize, ySize];
                    int num = 0;
                    for (int ix = 0; ix < xSize; ix++)
                    {
                        for (int iy = 0; iy < ySize; iy++)
                        {
                            ncArr[ix, iy] = new NamedConst(_subName(name, num), Convert.ToDouble(xyGetter(ix, iy)));
                            num++;
                        }
                    }

                    return ncArr;
                }
                else
                {
                    // list is standard array or list
                    NamedConst[] ncList = new NamedConst[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        ncList[i] = new NamedConst(_subName(name, i), Convert.ToDouble(list[i]));
                    }

                    return ncList;
                }
            }
        }

        /// <summary>
        /// Connect name prafix with number.
        /// </summary>
        /// <param name="name">Name (prefix).</param>
        /// <param name="num">Number to append to name.</param>
        /// <returns>Result created string.</returns>
        private static string _subName(string name, int num)
        {
            string subName = String.Format("{0}{1}", name, num);
            return subName;
        }

        /// <summary>
        /// Return composed constant absolute value.
        /// </summary>
        public static ComposedConstant Abs(this ComposedConstant t)
        {
            ComposedConstant ret = new ComposedAbs(t);
            return ret;
        }

        /// <summary>
        /// Check for inserted ComposedConstant child NamedConstants and registered NamedConstants duplicities
        /// and replace them with unique registered NamedConstant objects. Function is implemented primary for
        /// JDModel deserialization progress.
        /// </summary>
        internal static void ClearDupls(Func<ComposedConstant> getComCon, Action<ComposedConstant> setComCon,
            IDictionary<string, NamedConst> NamedConsts)
        {
            ComposedConstant comCon = getComCon();
            if (comCon is NamedConst)
            {
                NamedConst comConNamed = comCon as NamedConst;
                if (comConNamed.Name == null) return;
                if (NamedConsts.ContainsKey(comConNamed.Name))
                {
                    NamedConst comConInDic = NamedConsts[comConNamed.Name];
                    if (!comCon.Equals(comConInDic))
                    {
                        setComCon(comConInDic);
                    }
                }
                else
                {
                    NamedConsts.Add(comConNamed.Name, comConNamed);
                }
            }
            else
            {
                comCon.ClearDupl(ref NamedConsts);
            }
        }
    }
}