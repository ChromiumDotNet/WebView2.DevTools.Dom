using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The HTMLOptionElement interface represents option elements and inherits all properties and methods of the <see cref="HtmlElement"/> class.
    /// </summary>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLOptionElement" />
    public partial class HtmlOptionElement : HtmlElement
    {
        internal HtmlOptionElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }
}
