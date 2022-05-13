using System.Linq;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DomUpdateTests : DevTooolsContextBaseTest
    {
        public DomUpdateTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            const string expected = "Testing123";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var before = await button.GetTextContentAsync();

            Assert.Equal("Click target", before);

            await button.SetTextContentAsync(expected);

            var actual = await button.GetTextContentAsync();

            Assert.Equal(expected, actual);
        }
    }
}
