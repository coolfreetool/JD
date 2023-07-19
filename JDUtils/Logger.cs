using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Globalization;

namespace JDUtils
{
    /// <summary>
    /// Possible log flags enum
    /// </summary>
    [Flags]
    public enum LogFlags
    {
        /// <summary>
        /// Log level - Solution solver
        /// </summary>
        SOLUTION_SOLVER = 1,
        /// <summary>
        /// Log level - Modeler
        /// </summary>
        MODELER = 2,
        /// <summary>
        /// Log level - Optimizer
        /// </summary>
        OPTIMIZER = 4,
        /// <summary>
        /// Log level - Parsing results
        /// </summary>
        RESULTS_PARSER = 8,
        /// <summary>
        /// Log level - JD
        /// </summary>
        JD = 16,
        /// <summary>
        /// Log level - Model updating
        /// </summary>
        MODEL_UPDATER = 32,
        /// <summary>
        /// Log level - Planning water production
        /// </summary>
        WATTER_PLANNER = 64
    }

    /// <summary>
    /// Universal logger class. Enables to log messages (log items) over
    /// different categories and distributes these messages to several
    /// clients (consoles, log files etc.).
    /// </summary>
    public class Logger : ILogClient
    {
        private List<ILogClient> _clients = new List<ILogClient>();
        private List<LogFlags> _clientFlags = new List<LogFlags>();
        /// <summary>
        /// Return all log flags
        /// </summary>
        public static LogFlags AllFlags
        {
            get
            { return GetAllFlags(); }
        }


        /// <summary>
        /// Register log client for second argument flags.
        /// </summary>
        public void Register(ILogClient logClient, LogFlags flags)
        {
            if (_clients.Contains(logClient)) // klient uz je zaregistrovan
            {
                Console.WriteLine("Thic logger client is already registered. Unregister at first!!!");
            }
            else // registruje se novy klient
            {
                _clients.Add(logClient); // pridat klienta do seznamu klientu
                _clientFlags.Add(flags); // registruje se pro urcite flagy
            }
        }

        /// <summary>
        /// Unregister log client.
        /// </summary>
        public void Unregister(ILogClient logClient)
        {
            if (_clients.Contains(logClient))
            {
                int clientIdx = _clients.IndexOf(logClient);
                _clients.Remove(logClient); // smazat klienta ze seznamu
                _clientFlags.RemoveAt(clientIdx); // smazat seznam flagu klienta
            }
            else
            {
                Console.WriteLine("Tento client neni vubec registrovan!!!");
            }
        }

        /// <summary>
        /// Send log item to all clients registered for its flags.
        /// </summary>
        /// <param name="logItem"></param>
        private void _send(LogItem logItem)
        {
            //_items.Add(logItem); // pridam si logItem do seznamu
            for (int i = 0; i < _clients.Count; i++)
            {
                // pokud je klient zaregistrovan pro dany flag, posli mu LogItem
                if (_clientFlags[i].HasFlag(logItem.Flags))
                {
                    _clients[i].Log(logItem);
                }
            }
        }

        /// <summary>
        /// Add log item over specified flag.
        /// </summary>
        public void Log(LogFlags flags, string message)
        {
            LogItem logItem = new LogItem(DateTime.Now, message, flags);
            _send(logItem);
        }

        /// <summary>
        /// Add log item over specified flag.
        /// </summary>
        public void Log(LogFlags flags, string message, params Param[] parms)
        {
            LogItem logItem = new LogItem(DateTime.Now, message, flags, parms);
            _send(logItem);
        }

        /// <summary>
        /// Return all log flags
        /// </summary>
        /// <returns>Log flags</returns>
        public static LogFlags GetAllFlags()
        {
            LogFlags full = LogFlags.MODELER; //
            foreach (LogFlags next in Enum.GetValues(typeof(LogFlags)))
            {
                full = full | next;
            }
            return full;
        }

        /// <summary>
        /// Send log item to all registered clients
        /// </summary>
        /// <param name="logItem">Log item</param>
        public void Log(LogItem logItem)
        {
            _send(logItem);
        }
    }

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

    /// <summary>
    /// Log client interface.
    /// </summary>
    public interface ILogClient
    {
        /// <summary>
        /// Send log item to logger client.
        /// </summary>
        void Log(LogItem logItem);
    }

    /// <summary>
    /// ILogClient implementation for Console.
    /// </summary>
    public class ConsolLogClient : ILogClient
    {
        private static Dictionary<LogFlags, ConsoleColor> _logColors = new Dictionary<LogFlags, ConsoleColor>()
        {
            {LogFlags.OPTIMIZER, ConsoleColor.Magenta},
            {LogFlags.JD, ConsoleColor.DarkYellow},
            {LogFlags.MODELER, ConsoleColor.Cyan},
            {LogFlags.RESULTS_PARSER, ConsoleColor.Blue},
            {LogFlags.SOLUTION_SOLVER, ConsoleColor.Green},
            {LogFlags.WATTER_PLANNER, ConsoleColor.DarkBlue}

        };
        private ConsoleColor timeColor = ConsoleColor.Yellow;

        static void ConsoleColorWrite(string content, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(content);
            Console.ResetColor();
        }

        /// <summary>
        /// Print log item to console
        /// </summary>
        /// <param name="logItem">Log item</param>
        public void Log(LogItem logItem)
        {
            ConsoleColor col = Console.ForegroundColor;
            if (_logColors.ContainsKey(logItem.Flags))
            {
                col = _logColors[logItem.Flags];
            }
            string logStr = logItem.ToString();
            Regex flagAndTimeRg = new Regex(@"([^>]*)>>");
            Match mc = flagAndTimeRg.Match(logStr);
            string flagAndTime = mc.Groups[1].Value;
            Regex flagRg = new Regex(@"([^:]*:)");
            mc = flagRg.Match(flagAndTime);
            string flag = mc.Groups[1].Value;
            string time = flagAndTime.Substring(flag.Length);
            ConsoleColorWrite(flag, col);
            ConsoleColorWrite(time, timeColor);
            Console.Write(logStr.Substring(flagAndTime.Length));
        }
    }

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




    /// <summary>
    /// Universal log file parser.
    /// </summary>
    public class LogFileParser
    {
        // example: "    |    paramName|   2.55|"
        private static Regex _paramReg = new Regex(@"\s*\|\s*(\S+)\|\s*(\S+)\|");
        // example: "MODELER:             2012_12_12 17:01:58 >> Vars creating  "
        private static Regex _headReg = new Regex(@"(\S+):\s*(\S{10}\s\S{8})\s>>\s([^\n]+)");

        /// <summary>
        /// Parse log file 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ParsedLogData ParseLogFile(string file)
        {
            ParsedLogData result = new ParsedLogData();
            StreamReader sr = new StreamReader(file);
            string prevLine = "";
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
                    if (iParam == 0)
                    {
                        // update head line data
                        _tryGetHeadLineData(prevLine, out headFlag, out headTime, out headMsg);
                    }
                    result.AddItem(headFlag, headMsg, paramName, paramData);
                    iParam++;

                }
                else
                {
                    prevLine = line;
                    iParam = 0;
                }
            }
            return result;
        }

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

    /// <summary>
    /// ILogClient implementation for file.
    /// </summary>
    public class FileLogClient : ILogClient
    {
        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath { get; private set; }
        /// <summary>
        /// Source log file path
        /// </summary>
        public string BasedOnFile { get; private set; }

        /// <summary>
        /// Create file log client
        /// </summary>
        /// <param name="filename">Log file name with path</param>
        /// <param name="basedOnFile">Source file path</param>
        public FileLogClient(string filename, string basedOnFile = null)
        {
            LogFilePath = filename;
            BasedOnFile = basedOnFile;
            if (BasedOnFile != null)
                if (File.Exists(basedOnFile))
                    File.Copy(basedOnFile, LogFilePath);
        }

        /// <summary>
        /// Add log item to file
        /// </summary>
        /// <param name="logItem"></param>
        public void Log(LogItem logItem)
        {
            try
            {
                StreamWriter sw;
                if (!File.Exists(LogFilePath))
                {
                    // Create a file to write to.
                    using (sw = File.CreateText(LogFilePath))
                    {
                        sw.Write(logItem);
                    }
                }
                else
                {
                    using (sw = File.AppendText(LogFilePath))
                    {
                        sw.Write(logItem);
                    }
                }
                sw.Close();
            }
            catch { }
        }
    }
}
