﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace JDUtils
{
    /// <summary> 
    /// SerializationWriter.  Extends BinaryWriter to add additional data types,
    /// handle null strings and simplify use with ISerializable. 
    /// </summary>
    public class SerializationWriter : BinaryWriter
    {

        private SerializationWriter(Stream s) : base(s) { }

        /// <summary> 
        /// Static method to initialise the writer with a suitable MemoryStream. 
        /// </summary>
        public static SerializationWriter GetWriter()
        {
            MemoryStream ms = new MemoryStream(1024);
            return new SerializationWriter(ms);
        }

        /// <summary> 
        /// Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls 
        /// </summary>
        public override void Write(string str)
        {
            if (str == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                Write((byte)ObjType.stringType);
                base.Write(str);
            }
        }

        /// <summary> 
        /// Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved 
        /// </summary>
        public override void Write(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        /// <summary> 
        /// Writes a char array to the buffer.  Overrides the base implementation to
        /// sends the length of the array which is needed when it is read. 
        /// </summary>
        public override void Write(char[] c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int len = c.Length;
                Write(len);
                if (len > 0) base.Write(c);
            }
        }

        /// <summary>
        /// Writes a DateTime to the buffer.
        /// </summary>
        /// <param name="dt">DateTime value</param>
        public void Write(DateTime dt) { Write(dt.Ticks); }

        /// <summary>
        /// Writes a generic ICollection (such as an IList<T>) to the buffer. 
        /// </summary>
        public void Write<T>(ICollection<T> c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (T item in c) WriteObject(item);
            }
        }

        /// <summary> 
        /// Writes a generic IDictionary to the buffer. 
        /// </summary>
        public void Write<T, U>(IDictionary<T, U> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (KeyValuePair<T, U> kvp in d)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }

        /// <summary> 
        /// Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. 
        /// </summary>
        public void WriteObject(object obj)
        {
            if (obj == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                switch (obj.GetType().Name)
                {

                    case "Boolean": Write((byte)ObjType.boolType);
                        Write((bool)obj);
                        break;

                    case "Byte": Write((byte)ObjType.byteType);
                        Write((byte)obj);
                        break;

                    case "UInt16": Write((byte)ObjType.uint16Type);
                        Write((ushort)obj);
                        break;

                    case "UInt32": Write((byte)ObjType.uint32Type);
                        Write((uint)obj);
                        break;

                    case "UInt64": Write((byte)ObjType.uint64Type);
                        Write((ulong)obj);
                        break;

                    case "SByte": Write((byte)ObjType.sbyteType);
                        Write((sbyte)obj);
                        break;

                    case "Int16": Write((byte)ObjType.int16Type);
                        Write((short)obj);
                        break;

                    case "Int32": Write((byte)ObjType.int32Type);
                        Write((int)obj);
                        break;

                    case "Int64": Write((byte)ObjType.int64Type);
                        Write((long)obj);
                        break;

                    case "Char": Write((byte)ObjType.charType);
                        base.Write((char)obj);
                        break;

                    case "String": Write((byte)ObjType.stringType);
                        base.Write((string)obj);
                        break;

                    case "Single": Write((byte)ObjType.singleType);
                        Write((float)obj);
                        break;

                    case "Double": Write((byte)ObjType.doubleType);
                        Write((double)obj);
                        break;

                    case "Decimal": Write((byte)ObjType.decimalType);
                        Write((decimal)obj);
                        break;

                    case "DateTime": Write((byte)ObjType.dateTimeType);
                        Write((DateTime)obj);
                        break;

                    case "Byte[]": Write((byte)ObjType.byteArrayType);
                        base.Write((byte[])obj);
                        break;

                    case "Char[]": Write((byte)ObjType.charArrayType);
                        base.Write((char[])obj);
                        break;

                    default: Write((byte)ObjType.otherType);
                        new BinaryFormatter().Serialize(BaseStream, obj);
                        break;

                } // switch

            } // if obj==null

        }  // WriteObject

        /// <summary> 
        /// Adds the SerializationWriter buffer to the SerializationInfo at the end of GetObjectData(). 
        /// </summary>
        public void AddToInfo(SerializationInfo info)
        {
            byte[] b = ((MemoryStream)BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }

    }
}