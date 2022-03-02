using Xunit;

namespace WebView2.DevTools.Dom.Tests.Attributes
{
    /// <summary>
    /// WebView2Context Fact
    /// </summary>
    public class WebView2ContextFact : WinFormsFactAttribute
    {
        /// <summary>
        /// Creates a new <seealso cref="WebView2ContextFact"/>
        /// </summary>
        public WebView2ContextFact()
        {
            Timeout = System.Diagnostics.Debugger.IsAttached ? TestConstants.DebuggerAttachedTestTimeout : TestConstants.DefaultTestTimeout;
        }
    }
}
