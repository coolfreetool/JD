using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// StateTable class
    /// </summary>
    [Serializable]
    public class StateTable : Dictionary<HashSet<Tuple<int, ETaskState>>, ECondResult>
    {
        /// <summary>
        /// Default StateTable constructor
        /// </summary>
        public StateTable()
            : base(HashSet<Tuple<int, ETaskState>>.CreateSetComparer())
        { }

        /// <summary>
        /// StateTable serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(this.Count);
            foreach (var pair in this)
            {
                sw.WriteObject(pair);
            }
            sw.AddToInfo(info);
        }

        /// <summary>
        /// StateTable deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public StateTable(SerializationInfo info, StreamingContext context)
            : base(HashSet<Tuple<int, ETaskState>>.CreateSetComparer())
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            int cnt = sr.ReadInt32();
            for (int k = 0; k < cnt; k++)
            {
                var kvp = (KeyValuePair<HashSet<Tuple<int, ETaskState>>, ECondResult>)sr.ReadObject();
                this.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Add table to current state table
        /// </summary>
        /// <param name="table">State table</param>
        public void Add(StateTable table)
        {
            foreach (var pair in table)
            {
                this.Add(pair.Key, pair.Value);
            }
        }
    }
}