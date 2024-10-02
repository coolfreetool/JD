using System.IO;

namespace JDUtils
{
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