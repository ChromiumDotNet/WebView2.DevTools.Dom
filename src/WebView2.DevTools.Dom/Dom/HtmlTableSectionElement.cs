using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The HTMLTableSectionElement interface provides special properties and methods
    /// (beyond the HTMLElement interface it also has available to it by inheritance)
    /// for manipulating the layout and presentation of sections, that is headers,
    /// footers and bodies, in an HTML table.
    /// </summary>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLTableSectionElement" />
    public partial class HtmlTableSectionElement : HtmlElement
    {
        internal HtmlTableSectionElement(
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
