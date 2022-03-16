using Xunit;

namespace WebView2.DevTools.Dom.Tests.Attributes
{
    /// <summary>
    /// WebView2Context Theory
    /// </summary>
    public class WebView2ContextTheory : WinFormsTheoryAttribute
    {
        /// <summary>
        /// Creates a new <seealso cref="WebView2ContextTheory"/>
        /// </summary>
        public WebView2ContextTheory()
        {
            Timeout = System.Diagnostics.Debugger.IsAttached ? TestConstants.DebuggerAttachedTestTimeout : TestConstants.DefaultTestTimeout;
        }
    }
}
