using System;

namespace JDUtils
{
    /// <summary>
    /// JD exception
    /// </summary>
    [Serializable]
    public class JDException : Exception
    {
        /// <summary>
        /// JD exception - Default constructor
        /// </summary>
        public JDException()
            : base() { }

        /// <summary>
        /// JD exception with message in format JD: message.
        /// </summary>
        /// <param name="message">Message string</param>
        public JDException(string message)
            : base(String.Format("JD: " + message)) { }
        
        /// <summary>
        /// JD exception with message and parameters 
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="par">Exception parameters</param>
        public JDException(string message, params object[] par)
            : base(String.Format("JD: " + message, par)) { }

        /// <summary>
        /// JD exception with message and inner exception
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="inner">Inner exception</param>
        public JDException(string message, System.Exception inner)
            : base(message, inner) { }

        /// <summary>
        /// JD exception - Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Streaming context</param>
        protected JDException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
