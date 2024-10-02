using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace JDUtils
{
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
}