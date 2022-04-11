using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class HtmlTableRowElementTests : DevTooolsContextBaseTest
    {
        public HtmlTableRowElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableRowElement>("tr");

            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNull()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableRowElement>("#table2 tr");

            Assert.Null(element);
        }

        [WebView2ContextFact]
        public async Task ShouldGetIndex()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            var rows = await element.GetRowsAsync();
            var row = await rows.ItemAsync(0);

            var index = await row.GetRowIndexAsync();

            Assert.True(index > 0);
        }

        [WebView2ContextFact]
        public async Task ShouldGetSelectionIndex()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("table").GetBodyAsync();

            var rows = await element.GetRowsAsync();
            var row = await rows.ItemAsync(0);

            var index = await row.GetSectionRowIndexAsync();

            Assert.Equal(0, index);
        }

        [WebView2ContextFact]
        public async Task ShouldDeleteCell()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableRowElement>("tr");

            var cells = await element.GetCellsAsync();
            var expected = await cells.GetLengthAsync() - 1;

            await element.DeleteCellAsync(0);

            var actual = await cells.GetLengthAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldInsertThenDeleteCell()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var element = await DevToolsContext.QuerySelectorAsync<HtmlTableRowElement>("tr");

            var cell = await element.InsertCellAsync(-1, "Testing");

            var expected = await element.GetCellsAsync().GetLengthAsync() - 1;

            await element.DeleteCellAsync(cell);

            var actual = await element.GetCellsAsync().GetLengthAsync();

            Assert.Equal(expected, actual);
        }
    }
}
