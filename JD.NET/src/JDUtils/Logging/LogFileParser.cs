using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace JDUtils
{
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
}