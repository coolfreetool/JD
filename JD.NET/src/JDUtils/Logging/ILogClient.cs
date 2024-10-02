namespace JDUtils
{
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
}