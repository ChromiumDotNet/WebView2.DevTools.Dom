using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Base exception used to identify any exception thrown by WebView2 DevTools Context
    /// </summary>
    [Serializable]
    public class WebView2DevToolsContextException
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsContextException"/> class.
        /// </summary>
        public WebView2DevToolsContextException()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsContextException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public WebView2DevToolsContextException(string message) : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsContextException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public WebView2DevToolsContextException(string message, Exception innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsContextException"/> class.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected WebView2DevToolsContextException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal static string RewriteErrorMeesage(string message)
            => message.Contains("Cannot find context with specified id") || message.Contains("Inspected target navigated or close")
                ? "Execution context was destroyed, most likely because of a navigation."
                : message;
    }
}
