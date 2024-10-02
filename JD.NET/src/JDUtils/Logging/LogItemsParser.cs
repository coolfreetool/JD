using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace JDUtils
{
    /// <summary>
    /// Universal log file parser.
    /// </summary>
    public class LogItemsParser
    {
        // example: "    |    paramName|   2.55|"
        private static Regex _paramReg = new Regex(@"\s*\|\s*(\S+)\|\s*(\S+)\|");
        //private static Regex _paramReg = new Regex(@"\s*\|\s*([^|]+)\|\s*([^|]+)\|");
        //private static Regex _paramReg = new Regex(@"\s*\|\s*([\S+\s+]*\S+)\|\s*([\S+\s+]*\S+)\|");        
        // example: "MODELER:             2012_12_12 17:01:58 >> Vars creating  "
        private static Regex _headReg = new Regex(@"(\S+):\s*(\S{10}\s\S{8})\s>>\s([^\n]*)");
        //private static Regex _headReg = new Regex(@"(\S+):\s*(\S{10}\s\S{8})\s>>\s([^\n]+)");

        /// <summary>
        /// List of Log items
        /// </summary>
        protected List<LogItem> _result = new List<LogItem>();

        /// <summary>
        /// Add parameter to Log item
        /// </summary>
        /// <param name="item">Log item</param>
        /// <param name="paramName">Parameter name</param>
        /// <param name="paramData">Parameter value</param>
        protected virtual void _addParamToItem(LogItem item, string paramName, string paramData)
        {
            Param p = new Param(paramName, paramData);
            item.Parms.Add(paramName, p);
        }

        /// <summary>
        /// Add item to the log
        /// </summary>
        /// <param name="item">LogItem</param>
        protected virtual void _addLogItem(LogItem item)
        {
            _result.Add(item);
        }

        /// <summary>
        /// Parse log file to List of Log items
        /// </summary>
        /// <param name="file">File path string</param>
        /// <returns>List of Log items</returns>
        public List<LogItem> ParseLogFile(string file)
        {
            StreamReader sr = new StreamReader(file);
            LogItem logItem = null;
            //string prevLine = "";
            // init parsing out params
            // .. for head line
            string headFlag = "";
            string headMsg = "";
            DateTime headTime = new DateTime();
            // .. for param line
            string paramName = "";
            string paramData = "";
            // process all lines
            int iParam = 0; // previous params count in current head line
            while (true)
            {
                string line = sr.ReadLine();
                //_isHead(line, out headFlag, out headTime, out headMsg);
                if (line == null) break;
                bool isParam = _tryGetParamData(line, out paramName, out paramData);
                if (isParam)
                {
                    //result.AddItem(headFlag, headMsg, paramName, paramData);
                    if (iParam == 0) logItem.Parms = new Dictionary<string, Param>();
                    _addParamToItem(logItem, paramName, paramData);
                    iParam++;

                }
                else
                {
                    if (logItem != null) _addLogItem(logItem);
                    bool readHead = _tryGetHeadLineData(line, out headFlag, out headTime, out headMsg);
                    if (!readHead) throw new JDException("Unable to parse data");
                    LogFlags flag = (LogFlags)Enum.Parse(typeof(LogFlags), headFlag);
                    logItem = new LogItem(headTime, headMsg, flag);
                    iParam = 0;
                }
            }
            if (logItem != null)
                _addLogItem(logItem); // add the last item
            return _result;
        }

        /// <summary>
        /// Return Log flag, time mark and message from given Log line string
        /// </summary>
        /// <param name="line">Log line string</param>
        /// <param name="flag">Log flag</param>
        /// <param name="timeMark">Time stamp</param>
        /// <param name="message">Parameter message</param>
        /// <returns>True if parsing was succesful and false if not</returns>
        private bool _tryGetHeadLineData(string line, out string flag, out DateTime timeMark, out string message)
        {
            Match mc = _headReg.Match(line);
            if (mc.Groups.Count == 4)
            {
                flag = mc.Groups[1].Value;
                timeMark = DateTime.ParseExact(mc.Groups[2].Value, "yyyy_MM_dd HH:mm:ss", CultureInfo.InvariantCulture);
                message = mc.Groups[3].Value;
                return true;
            }
            else
            {
                flag = null;
                message = null;
                timeMark = new DateTime();
                return false;
            }
        }

        /// <summary>
        /// Return parameter value in given Log line string.
        /// </summary>
        /// <param name="line">Log line string</param>
        /// <param name="name">Parameter name</param>
        /// <param name="data">Parameter value</param>
        /// <returns>True if parameter found and false if not</returns>
        private bool _tryGetParamData(string line, out string name, out string data)
        {
            Match mc = _paramReg.Match(line);
            if (mc.Groups.Count == 3)
            {
                name = mc.Groups[1].Value;
                data = mc.Groups[2].Value;
                return true;
            }
            else
            {
                name = null;
                data = null;
                return false;
            }
        }
    }
}