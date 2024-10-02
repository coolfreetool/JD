using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JDUtils
{
    /// <summary>
    /// Single log item.
    /// </summary>
    [Serializable]
    public class LogItem
    {
        /// <summary>
        /// Log item time stamp.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Log item message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Log item parameters.
        /// </summary>
        public Dictionary<string, Param> Parms { get; set; }

        //ESource source = ESource.A | ESource.B;

        /// <summary>
        /// Log item flags (category, tag, prefix)
        /// </summary>
        public LogFlags Flags { get; private set; }

        /// <summary>
        /// Create new log item.
        /// </summary>
        /// <param name="time">Log item time stamp.</param>
        /// <param name="message">Log item message.</param>
        /// <param name="flags">Log item category(ies) (flag, prefix).</param>
        public LogItem(DateTime time, string message, LogFlags flags)
        {
            Time = time;
            Message = message;
            Flags = flags;
            //source.HasFlag(ESource.A); // vrati true;
        }

        /// <summary>
        /// Create new log item.
        /// </summary>
        /// <param name="time">Log item time stamp.</param>
        /// <param name="message">Log item message.</param>
        /// <param name="flags">Log item category(ies) (flag, prefix).</param>
        /// <param name="parms">Log item parameters.</param>
        public LogItem(DateTime time, string message, LogFlags flags, params Param[] parms)
            : this(time, message, flags)
        {
            Parms = parms.ToDictionary(x => x.Name, x => x);
        }

        /// <summary>
        /// ToString method reimplementation.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0,-20} {1} >> {2}\n", Flags + ":", Time.ToString("yyyy_MM_dd HH:mm:ss"), Message);
            if (Parms != null)
            {
                foreach (Param par in Parms.Values)
                {
                    sb.AppendFormat("\t\t\t|{0,20}|{1,20}|\n", par.Name, par);
                }
            }
            return sb.ToString();
        }
    }
}