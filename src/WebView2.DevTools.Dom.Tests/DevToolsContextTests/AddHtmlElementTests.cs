using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AddHtmlElementTests : DevTooolsContextBaseTest
    {
        public AddHtmlElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldCreateDiv()
        {
            const string expected = "";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var div = await DevToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div");
            Assert.NotNull(div);

            var actual = await div.GetIdAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldCreateDivWithId()
        {
            const string expected = "myDiv";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var div = await DevToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div", expected);
            Assert.NotNull(div);

            var actual = await div.GetIdAsync();

            Assert.Equal(expected, actual);
        }
    }
}
