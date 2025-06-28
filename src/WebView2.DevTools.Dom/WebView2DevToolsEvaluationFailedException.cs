using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// WebView2 Evaluation Failed Exception
    /// </summary>
    [Serializable]
    public class WebView2DevToolsEvaluationFailedException
        : WebView2DevToolsContextException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsEvaluationFailedException"/> class.
        /// </summary>
        public WebView2DevToolsEvaluationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsEvaluationFailedException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public WebView2DevToolsEvaluationFailedException(string message) : base(RewriteErrorMeesage(message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsEvaluationFailedException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public WebView2DevToolsEvaluationFailedException(string message, Exception innerException)
            : base(RewriteErrorMeesage(message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2DevToolsEvaluationFailedException"/> class.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected WebView2DevToolsEvaluationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
