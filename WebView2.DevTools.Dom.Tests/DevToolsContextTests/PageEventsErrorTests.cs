using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageEventsErrorTests : DevTooolsContextBaseTest
    {
        public PageEventsErrorTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldThrowWhenPageCrashes()
        {
            string error = null;
            DevToolsContext.Error += (_, e) => error = e.Error;
            var gotoTask = WebView.CoreWebView2.NavigateToAsync("chrome://crash");

            await WaitForError();
            Assert.Equal("Page crashed!", error);
        }
    }
}
