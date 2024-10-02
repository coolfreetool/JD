using System.Collections.Generic;

namespace JDUtils
{
    /// <summary>
    /// Parameter extenders
    /// </summary>
    public static class Extenders
    {
        /// <summary>
        /// Moznost nastavit primo hodnotu urciteho parametru v seznamu.
        /// </summary>
        public static void SetParam(this Dictionary<string, Param> t, string parName, object parValue)
        {
            if (t.ContainsKey(parName))
            {
                t[parName].Value = parValue;
            }
            else
            {
                t.Add(parName, new Param(parName, parValue));
            }
        }


    }
}