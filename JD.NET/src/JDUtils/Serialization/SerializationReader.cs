using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace JDUtils
{
    /// <summary> 
    /// SerializationReader.  Extends BinaryReader to add additional data types,
    /// handle null strings and simplify use with ISerializable. 
    /// </summary>
    public class SerializationReader : BinaryReader
    {

        private SerializationReader(Stream s) : base(s) { }

        /// <summary> 
        /// Static method to take a SerializationInfo object (an input to an ISerializable constructor)
        /// and produce a SerializationReader from which serialized objects can be read 
        /// </summary>.
        public static SerializationReader GetReader(SerializationInfo info)
        {
            byte[] byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            MemoryStream ms = new MemoryStream(byteArray);
            return new SerializationReader(ms);
        }

        /// <summary> 
        /// Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. 
        /// </summary>
        public override string ReadString()
        {
            ObjType t = (ObjType)ReadByte();
            if (t == ObjType.stringType) return base.ReadString();
            return null;
        }

        /// <summary> 
        /// Reads a byte array from the buffer, handling nulls and the array length.
        /// </summary>
        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        /// <summary> 
        /// Reads a char array from the buffer, handling nulls and the array length.
        /// </summary>
        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;
            return new char[0];
        }

        /// <summary> 
        /// Reads a DateTime from the buffer. 
        /// </summary>
        public DateTime ReadDateTime() { return new DateTime(ReadInt64()); }

        /// <summary>
        /// Reads a generic list from the buffer. 
        /// </summary>
        public IList<T> ReadList<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (int i = 0; i < count; i++) d.Add((T)ReadObject());
            return d;
        }

        /// <summary> 
        /// Reads a generic Dictionary from the buffer. 
        /// </summary>
        public IDictionary<T, U> ReadDictionary<T, U>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary<T, U> d = new Dictionary<T, U>();
            for (int i = 0; i < count; i++) d[(T)ReadObject()] = (U)ReadObject();
            return d;
        }

        /// <summary> 
        /// Reads an object which was added to the buffer by WriteObject. 
        /// </summary>
        public object ReadObject()
        {
            ObjType t = (ObjType)ReadByte();
            switch (t)
            {
                case ObjType.boolType: return ReadBoolean();
                case ObjType.byteType: return ReadByte();
                case ObjType.uint16Type: return ReadUInt16();
                case ObjType.uint32Type: return ReadUInt32();
                case ObjType.uint64Type: return ReadUInt64();
                case ObjType.sbyteType: return ReadSByte();
                case ObjType.int16Type: return ReadInt16();
                case ObjType.int32Type: return ReadInt32();
                case ObjType.int64Type: return ReadInt64();
                case ObjType.charType: return ReadChar();
                case ObjType.stringType: return base.ReadString();
                case ObjType.singleType: return ReadSingle();
                case ObjType.doubleType: return ReadDouble();
                case ObjType.decimalType: return ReadDecimal();
                case ObjType.dateTimeType: return ReadDateTime();
                case ObjType.byteArrayType: return ReadByteArray();
                case ObjType.charArrayType: return ReadCharArray();
                case ObjType.otherType: return new BinaryFormatter().Deserialize(BaseStream);
                default: return null;
            }
        }
    }
}