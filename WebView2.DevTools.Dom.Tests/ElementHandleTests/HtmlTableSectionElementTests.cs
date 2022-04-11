using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlTableSectionElementTests : DevTooolsContextBaseTest
    {
        public HtmlTableSectionElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNull()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("#table2").GetBodyAsync();

            Assert.Null(element);
        }

        [WebView2ContextFact]
        public async Task ShouldInsertRow()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            var row = await element.InsertRowAsync(-1);

            Assert.NotNull(row);
        }

        [WebView2ContextFact]
        public async Task ShouldDeleteRow()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            var initialLength = await element.GetRowsAsync().GetLengthAsync();
            var expected = initialLength - 1;

            await element.DeleteRowAsync(0);

            var actual = await element.GetRowsAsync().GetLengthAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldInsertThenDeleteRow()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            var expected = await element.GetRowsAsync().GetLengthAsync();

            var row = await element.InsertRowAsync(-1);

            var rowIndex = await row.GetSectionRowIndexAsync();

            await element.DeleteRowAsync(rowIndex);

            var actual = await element.GetRowsAsync().GetLengthAsync();

            Assert.Equal(expected, actual);
        }
    }
}
