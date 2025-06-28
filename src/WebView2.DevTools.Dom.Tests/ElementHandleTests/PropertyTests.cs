using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PropertyTests : DevTooolsContextBaseTest
    {
        public PropertyTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetInnerText()
        {
            const string expected = "Updated Inner Text";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/longText.html");
            var p = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("p");
            await p.SetInnerTextAsync(expected);
            var actual = await p.GetInnerTextAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetInnerHtml()
        {
            const string expected = "<div>Welcome To Updated Inner Html</div>";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/longText.html");
            var p = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("p");
            await p.SetInnerHtmlAsync(expected);
            var actual = await p.GetInnerHtmlAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetOuterHtml()
        {
            const string expected = "<div>Welcome To Updated Outer Html</div>";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/longText.html");
            var p = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("p");
            await p.SetOuterHtmlAsync(expected);

            var div = await DevToolsContext.QuerySelectorAsync<HtmlDivElement>("div");

            var actual = await div.GetOuterHtmlAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetOuterText()
        {
            const string expected = "Welcome To Updated Outer Text";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/longText.html");
            var p = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("p");

            await p.SetInnerHtmlAsync("<div>Div for testing</div>");

            var div = await p.QuerySelectorAsync<HtmlDivElement>("div");

            await div.SetOuterTextAsync(expected);

            var actual = await p.GetOuterTextAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetTextContent()
        {
            const string expected = "Updated Text Content";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/longText.html");
            var p = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("p");
            await p.SetTextContentAsync(expected);
            var actual = await p.GetTextContentAsync();

            Assert.Equal(expected, actual);
        }
    }
}
