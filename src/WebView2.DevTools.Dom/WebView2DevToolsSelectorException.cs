using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Exception thrown when an element selector returns null.
    /// </summary>
    /// <seealso cref="RemoteObjectExtensions.EvaluateFunctionAsync{T}(System.Threading.Tasks.Task{Element}, string, object[])"/>
    /// <seealso cref="Frame.SelectAsync(string, string[])"/>
    /// <seealso cref="WebView2DevToolsContext.ClickAsync(string, Input.ClickOptions)"/>
    /// <seealso cref="WebView2DevToolsContext.TapAsync(string)"/>
    /// <seealso cref="WebView2DevToolsContext.HoverAsync(string)"/>
    /// <seealso cref="WebView2DevToolsContext.FocusAsync(string)"/>
    /// <seealso cref="WebView2DevToolsContext.SelectAsync(string, string[])"/>
    [Serializable]
    public class WebView2DevToolsSelectorException
        : WebView2DevToolsContextException
    {
        /// <summary>
        /// Gets the selector.
        /// </summary>
        /// <value>The selector.</value>
        public string Selector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsSelectorException"/> class.
        /// </summary>
        public WebView2DevToolsSelectorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsSelectorException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public WebView2DevToolsSelectorException(string message) : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsSelectorException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="selector">Selector.</param>
        public WebView2DevToolsSelectorException(string message, string selector) : base(message)
        {
            Selector = selector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsSelectorException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public WebView2DevToolsSelectorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsSelectorException"/> class.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <param name="context">Streaming Context.</param>
        protected WebView2DevToolsSelectorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
