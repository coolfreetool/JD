using System;
using System.Linq;
using System.Text;
using System.Collections;

namespace JDUtils
{
    /// <summary>
    /// Pojmenovany parametr.
    /// </summary>
    [Serializable]
    public class Param
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Parameter value
        /// </summary>
        public object Value;

        /// <summary>
        /// Create new parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Paramater value</param>
        public Param(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Returns parameter value as a string
        /// </summary>
        /// <returns>Parameter value string</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}