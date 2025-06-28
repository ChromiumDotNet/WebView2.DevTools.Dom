using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// CoreWebView2 extensions
    /// </summary>
    public static class CoreWebView2Extensions
    {
        /// <summary>
        /// Create a new <see cref="WebView2DevToolsContext"/> for the <see cref="CoreWebView2"/> instance.
        /// </summary>
        /// <param name="coreWebView2">core webview</param>
        /// <param name="loggerFactory">logger factory</param>
        /// <returns>A task that can be awaited to create a new <see cref="WebView2DevToolsContext"/> instance</returns>
        public static async Task<WebView2DevToolsContext> CreateDevToolsContextAsync(
            this CoreWebView2 coreWebView2,
            ILoggerFactory loggerFactory = null)
        {
            if (coreWebView2 == null)
            {
                throw new ArgumentNullException(nameof(coreWebView2));
            }

            var devToolsProtocolHelper = coreWebView2.GetDevToolsProtocolHelper();

            var ctx = new WebView2DevToolsContext(coreWebView2, devToolsProtocolHelper, loggerFactory ?? new LoggerFactory());

            await ctx.InitializeAsync().ConfigureAwait(true);

            return ctx;
        }
    }
}
