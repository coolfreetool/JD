// FastSerializer.cs.  Provides SerializationWriter and SerializationReader classes to help high speed serialization.
// This short example shows how they're used:
//
//  [Serializable] public class TestObject : ISerializable {                       // Class must be ISerializable
//    public long   x;
//    public string y;
//
//    public void GetObjectData (SerializationInfo info, StreamingContext ctxt) {  // Serialization method
//      SerializationWriter sw = SerializationWriter.GetWriter ();                 // Get a Writer
//      sw.Write (x);                                                              // Write fields
//      sw.Write (y);                                                              // ditto
//      sw.AddToInfo (info);                                                       // Add the Writer to info
//    }
//
//    public TestObject (SerializationInfo info, StreamingContext ctxt) {          // Deserialization .ctor
//      SerializationReader sr = SerializationReader.GetReader (info);             // Get a Reader from info
//      x = sr.ReadInt64 ();                                                       // Read a field
//      y = sr.ReadInt64 ();                                                       // ditto
//    }
//
//  }
//
// Author: Tim Haynes, May 2006.  Use freely as you see fit.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace JDUtils
{
    // SerializationWriter


    // SerializationReader

    /// <summary>
    /// Serialization extenders
    /// </summary>
    public static class SerializableExtender
    {

        /// <summary>
        /// Save scenario object to specific path.
        /// </summary>
        public static void SaveToFile(this object seriObj, string filename)
        {
            try
            {
                Stream stream = File.Open(filename, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, seriObj);
                stream.Close();
            }
            catch (Exception Exception)
            {
                Console.WriteLine("Error: {0}", Exception.Message);
            }
        }

        /// <summary>
        /// Load scenario object from file.
        /// </summary>
        public static T BuildFromFile<T>(string filename)
        {
            T data;
            try
            {
                Stream stream = File.Open(filename, FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();

                if (typeof(T) == typeof(object))
                {
                    data = (T)bFormatter.Deserialize(stream);
                }
                else
                {
                    //data = (T)Convert.ChangeType(obj, typeof(T));
                    data = (T)Convert.ChangeType(bFormatter.Deserialize(stream), typeof(T));
                }

                stream.Close();
            }
            catch (Exception Exception)
            {
                Console.WriteLine("Error: {0}", Exception.Message);
                return default(T);
            }
            return data;
        }
    }
}
