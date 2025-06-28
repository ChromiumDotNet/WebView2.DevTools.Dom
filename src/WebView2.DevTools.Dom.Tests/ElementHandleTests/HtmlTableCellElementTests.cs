using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlTableCellElementTests : DevTooolsContextBaseTest
    {
        public HtmlTableCellElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNull()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("#table2 td");

            Assert.Null(element);
        }

        [WebView2ContextFact]
        public async Task ShouldGetIndex()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            var index = await element.GetCellIndexAsync();

            Assert.True(index > -1);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetAbbr()
        {
            const string expected = "Testing";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            await element.SetAbbrAsync(expected);
            var actual = await element.GetAbbrAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldGetScope()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            var actual = await element.GetScopeAsync();

            Assert.Equal(string.Empty, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetScope()
        {
            const string expected = "col";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            await element.SetScopeAsync(expected);
            var actual = await element.GetScopeAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetRowSpan()
        {
            const int expected = 3;

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            await element.SetRowSpanAsync(expected);
            var actual = await element.GetRowSpanAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetThenGetColSpan()
        {
            const int expected = 3;

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableCellElement>("td");

            await element.SetColSpanAsync(expected);
            var actual = await element.GetColSpanAsync();

            Assert.Equal(expected, actual);
        }
    }
}
