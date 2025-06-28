using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The File interface provides information about files and allows JavaScript in a web page to access their content.
    /// </summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File
    public class File : RemoteHandle
    {
        internal File(
                    ExecutionContext context,
                    DevToolsProtocolHelper client,
                    ILoggerFactory loggerFactory,
                    Runtime.RemoteObject remoteObject) : base(context, client, loggerFactory, remoteObject)
        {
        }

        /// <summary>
        /// Returns the name of the file referenced by the File object.
        /// </summary>
        /// <returns>string</returns>
        public Task<string> GetNameAsync()
        {
            return EvaluateFunctionInternalAsync<string>("e => e.name");
        }

        /// <summary>
        /// Returns the MIME type of the file.
        /// </summary>
        /// <returns>string</returns>
        public Task<string> GetTypeAsync()
        {
            return EvaluateFunctionInternalAsync<string>("e => e.type");
        }

        /// <summary>
        /// Returns the size of the file in bytes.
        /// </summary>
        /// <returns>int</returns>
        public Task<int> GetSizeAsync()
        {
            return EvaluateFunctionInternalAsync<int>("e => e.size");
        }

        /// <summary>
        /// Transforms the File into a stream and reads it to completion. It returns a promise that resolves with a USVString (text).
        /// </summary>
        /// <returns>string</returns>
        public Task<string> TextAsync()
        {
            return EvaluateFunctionInternalAsync<string>("e => e.text()");
        }
    }
}
