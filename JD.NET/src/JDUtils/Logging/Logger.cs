using System;
using System.Collections.Generic;
using System.Drawing;

namespace JDUtils
{
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
}
