using System;
using System.Runtime.Serialization;
using JDUtils;

namespace JDSpace
{
    /// <summary>
    /// JDServer-JDClient data transfer object class.
    /// </summary>
    [Serializable]
    public class TransferPack : ISerializable
    {
        /// <summary>
        /// Describes pack content for smart explicit serialization.
        /// </summary>
        private EPackContent _contentType;
        /// <summary>
        /// Pack type description (key for processing way decission).
        /// </summary>
        public EPackType PackType { get; private set; }
        /// <summary>
        /// JDModel (solved or to solver).
        /// </summary>
        public JDModel Model { get; private set; }
        /// <summary>
        /// JDUtils.Logger log item.
        /// </summary>
        public LogItem LogItem { get; private set; }
        /// <summary>
        /// Any serializable object to transfer.
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Instance of TransferPack
        /// </summary>
        /// <param name="packType">Pack type</param>
        /// <param name="model">JD model (null default)</param>
        /// <param name="logItem">Log item (null default)</param>
        /// <param name="data">Data object (null default)</param>
        public TransferPack(
            EPackType packType,
            JDModel model = null,
            LogItem logItem = null,
            object data = null)
        {
            _contentType = (EPackContent)0;
            PackType = packType;
            Model = model; if (model != null) { _contentType |= EPackContent.MODEL; }
            LogItem = logItem; if (logItem != null) _contentType |= EPackContent.LOG_ITEM;
            Data = data; if (data != null) _contentType |= EPackContent.DATA;
        }

        #region << EXPLICIT SERIALIZATION >>
        /// <summary>
        /// Explicit serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public TransferPack(SerializationInfo info, StreamingContext context)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            _contentType = (EPackContent)sr.ReadByte();
            PackType = (EPackType)sr.ReadByte();
            if (_contentType.HasFlag(EPackContent.MODEL))
            {
                //DateTime t1 = DateTime.Now;
                //Console.WriteLine("JDModel reading..");
                Model = (JDModel)sr.ReadObject();
                //DateTime t2 = DateTime.Now;
                //Console.WriteLine("..{0} s.", (t2 - t1).TotalSeconds);
            }
            if (_contentType.HasFlag(EPackContent.LOG_ITEM)) LogItem = (LogItem)sr.ReadObject();
            if (_contentType.HasFlag(EPackContent.DATA)) Data = sr.ReadObject();
        }
        /// <summary>
        /// Standard explicit serialization method.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write((byte)_contentType);
            sw.Write((byte)PackType);
            if (_contentType.HasFlag(EPackContent.MODEL))
            {
                //DateTime t1 = DateTime.Now;
                //Console.WriteLine("JDModel writing..");
                sw.WriteObject(Model);
                //DateTime t2 = DateTime.Now;
                //Console.WriteLine("..{0} s.", (t2 - t1).TotalSeconds);
            }
            if (_contentType.HasFlag(EPackContent.LOG_ITEM)) sw.WriteObject(LogItem);
            if (_contentType.HasFlag(EPackContent.DATA)) sw.WriteObject(Data);

            sw.AddToInfo(info);
        }
        #endregion << EXPLICIT SERIALIZATION >>
    }
}