using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    [Serializable]
    internal class WebView2DevToolsBufferException : WebView2DevToolsContextException
    {
        public WebView2DevToolsBufferException()
        {
        }

        public WebView2DevToolsBufferException(string message) : base(message)
        {
        }

        public WebView2DevToolsBufferException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WebView2DevToolsBufferException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}