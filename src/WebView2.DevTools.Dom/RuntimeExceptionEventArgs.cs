using System;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Page error event arguments.
    /// </summary>
    public class RuntimeExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Error Message
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public RuntimeExceptionEventArgs(string message) => Message = message;
    }
}
