using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JDUtils
{
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
}