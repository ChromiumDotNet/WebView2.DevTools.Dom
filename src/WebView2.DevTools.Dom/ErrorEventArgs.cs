using System;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// <see cref="WebView2DevToolsContext.Error"/> arguments.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">Error.</param>
        public ErrorEventArgs(string error)
        {
            Error = error;
        }
    }
}