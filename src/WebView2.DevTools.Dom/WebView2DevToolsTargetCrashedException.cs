using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    [Serializable]
    internal class WebView2DevToolsTargetCrashedException : WebView2DevToolsContextException
    {
        public WebView2DevToolsTargetCrashedException()
        {
        }

        public WebView2DevToolsTargetCrashedException(string message) : base(message)
        {
        }

        public WebView2DevToolsTargetCrashedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WebView2DevToolsTargetCrashedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
