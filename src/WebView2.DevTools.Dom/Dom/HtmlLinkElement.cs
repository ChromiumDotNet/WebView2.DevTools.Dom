using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The HTMLLinkElement interface represents reference information for external resources
    /// and the relationship of those resources to a document and vice-versa
    /// (corresponds to link element; not to be confused with a, which is represented by HTMLAnchorElement).
    /// This object inherits all of the properties and methods of the HTMLElement interface.
    /// </summary>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLinkElement" />
    public partial class HtmlLinkElement : HtmlElement
    {
        internal HtmlLinkElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }

        /// <summary>
        /// Gets a string representing the type of content being loaded by the HTML link.
        /// </summary>
        /// <returns>string</returns>
        public Task<string> GetAsAsync()
        {
            return EvaluateFunctionInternalAsync<string>("(element) => { return element.as; }");
        }

        /// <summary>
        /// Gets a string representing the type of content being loaded by the HTML link.
        /// </summary>
        /// <param name="as">string</param>
        /// <returns>A Task that when awaited sets the as property</returns>
        public Task SetAsAsync(string @as)
        {
            return SetPropertyValueAsync("as", @as);
        }
    }
}
