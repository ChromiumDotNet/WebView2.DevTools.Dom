using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class TitleTests : DevTooolsContextBaseTest
    {
        public TitleTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldReturnThePageTitle()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/title.html");
            Assert.Equal("Woof-Woof", await DevToolsContext.GetTitleAsync());
        }
    }
}
