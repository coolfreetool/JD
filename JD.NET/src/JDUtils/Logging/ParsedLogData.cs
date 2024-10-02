using System;
using System.Collections.Generic;

namespace JDUtils
{
    /// <summary>
    /// Log file parsed data object.
    /// </summary>
    [Serializable]
    public class ParsedLogData : ILogClient
    {
        /// <summary>
        /// Parsed data. flag -> message -> param name -> list of values.
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> ParsedData { get; private set; }

        /// <summary>
        /// Parsed log data default constructor
        /// </summary>
        public ParsedLogData()
        {
            ParsedData = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        }

        /// <summary>
        /// Add parsed data item (parameter).
        /// </summary>
        /// <param name="flag">Item flag.</param>
        /// <param name="head">Item head message.</param>
        /// <param name="itemName">Item parameter name.</param>
        /// <param name="itemData">Item parameter value.</param>
        public virtual void AddItem(string flag, string head, string itemName, string itemData)
        {
            if (ParsedData.ContainsKey(flag))
            {
                Dictionary<string, Dictionary<string, List<string>>> data = ParsedData[flag];
                if (data.ContainsKey(head))
                {
                    if (data[head].ContainsKey(itemName))
                    {
                        data[head][itemName].Add(itemData);
                    }
                    else
                    {
                        data[head].Add(itemName, new List<string> { itemData });
                    }
                }
                else
                {
                    data.Add(head, new Dictionary<string, List<string>>());
                    AddItem(flag, head, itemName, itemData);
                }
            }
            else
            {
                ParsedData.Add(flag, new Dictionary<string, Dictionary<string, List<string>>>());
                AddItem(flag, head, itemName, itemData);
            }
        }

        /// <summary>
        /// Clear all parsed log data
        /// </summary>
        public virtual void Reset()
        {
            ParsedData.Clear();
        }

        /// <summary>
        /// Add log item to parsed data log
        /// </summary>
        /// <param name="logItem"></param>
        public virtual void Log(LogItem logItem)
        {
            if (logItem.Parms != null)
            {
                string flag = logItem.Flags.ToString();
                string message = logItem.Message;
                foreach (Param param in logItem.Parms.Values)
                {
                    AddItem(flag, message, param.Name, param.Value.ToString());
                }
            }
        }
    }
}