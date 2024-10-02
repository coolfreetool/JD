using System.Collections.Generic;

namespace JDSpace
{
    internal class JDModelSerializationHelper
    {
        /// <summary>
        /// Scalar variables map
        /// </summary>
        public static Dictionary<int, ScVar> VarsMap;
        /// <summary>
        /// Scalar linear expression factory
        /// </summary>
        public static ScLinExprFactory ScLinExprFactory;
        /// <summary>
        /// Named constants list
        /// </summary>
        public static List<string> NamedConstants;
    }
}