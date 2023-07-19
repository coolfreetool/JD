using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Collections;

namespace JDUtils
{
    /// <summary>
    /// Mozne stavy tasku.
    /// </summary>
    public enum ETaskState
    {
        /// <summary>
        /// Task state - waiting
        /// </summary>
        WAITING = 0,
        /// <summary>
        /// Task state - In progress
        /// </summary>
        IN_PROGRESS = 1,
        /// <summary>
        /// Task state - Successfully solved
        /// </summary>
        SOLVED_OK = 2,
        /// <summary>
        /// Task state - Solving error
        /// </summary>
        SOLVED_ERR = 3,
        /// <summary>
        /// Task state - refused
        /// </summary>
        REFUSED = 4,
    }

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

    /// <summary>
    /// Object clonning extenders
    /// </summary>
    public static class CopyExtender
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static object Clone(this object source)
        {
            if (!source.GetType().IsSerializable)
            { throw new ArgumentException("The type must be serializable.", "source"); }
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            { return null; }
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }
    }

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