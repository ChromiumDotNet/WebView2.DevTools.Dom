using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DevToolsContextQuerySelectorAllTests : DevTooolsContextBaseTest
    {
        public DevToolsContextQuerySelectorAllTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable IDE0051 // Remove unused private members
        void Usage(Microsoft.Web.WebView2.Core.CoreWebView2 coreWebView2)
#pragma warning restore IDE0051 // Remove unused private members
        {
            #region QuerySelectorAll

            // Add using WebView2.DevTools.Dom; to get access to the
            // CreateDevToolsContextAsync extension method

            coreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    // WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
                    // via await using or await devToolsContext.DisposeAsync();
                    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
                    await using var devToolsContext = await coreWebView2.CreateDevToolsContextAsync();

                    // Get elements by tag name
                    // https://developer.mozilla.org/en-US/docs/Web/API/Document/querySelectorAll
                    var inputElements = await devToolsContext.QuerySelectorAllAsync<HtmlInputElement>("input");

                    foreach (var element in inputElements)
                    {
                        var name = await element.GetNameAsync();
                        var id = await element.GetIdAsync();

                        var value = await element.GetValueAsync<int>();
                    }
                }
            };

            #endregion
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var elements = await DevToolsContext.QuerySelectorAllAsync<HtmlTableRowElement>("tr");

            Assert.NotNull(elements);
            Assert.Equal(4, elements.Length);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArray()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/table.html");
            var elements = await DevToolsContext.QuerySelectorAllAsync<HtmlTableRowElement>("#table2 tr");

            Assert.NotNull(elements);
            Assert.Empty(elements);
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElements()
        {
            await DevToolsContext.SetContentAsync("<div>A</div><br/><div>B</div>");
            var elements = await DevToolsContext.QuerySelectorAllAsync("div");
            Assert.Equal(2, elements.Length);
            var tasks = elements.Select(element => DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", element));
            Assert.Equal(new[] { "A", "B" }, await Task.WhenAll(tasks));
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayIfNothingIsFound()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var elements = await DevToolsContext.QuerySelectorAllAsync("div");
            Assert.Empty(elements);
        }
    }
}
