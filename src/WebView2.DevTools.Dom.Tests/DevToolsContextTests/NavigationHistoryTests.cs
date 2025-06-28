using System.Linq;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class NavigationHistoryTests : DevTooolsContextBaseTest
    {
        public NavigationHistoryTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable IDE0051 // Remove unused private members
        async Task Usage(CoreWebView2 coreWebView2)
#pragma warning restore IDE0051 // Remove unused private members
        {
            #region GetNavigationHistoryAsync
            // Add using WebView2.DevTools.Dom; to get access to the
            // CreateDevToolsContextAsync extension method

            // WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
            // via await using or await devToolsContext.DisposeAsync();
            // Only DisposeAsync is supported. It's very important the WebView2DevToolsContext is Disposed
            // When you have finished. Only create a single instance at a time, reuse an instance rather than
            // creaeting a new WebView2DevToolsContext. Dispose the old WebView2DevToolsContext instance before
            // creating a new instance if you need to manage the lifespan manually.
            // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
            await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();

            //Get Histroy entries
            var history = await DevToolsContext.GetNavigationHistoryAsync();

            //Get the first and navigate
            var firstEntry = history.Entries.First();

            var title = firstEntry.Title;
            var url = firstEntry.Url;
            var transitionType = firstEntry.TransitionType;
            
            await DevToolsContext.NavigateToHistoryEntryAsync(firstEntry);

            #endregion
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            var history = await DevToolsContext.GetNavigationHistoryAsync();

            Assert.NotNull(history);
            Assert.NotEmpty(history.Entries);
            Assert.Equal(0, history.CurrentIndex);
        }

        [WebView2ContextFact]
        public async Task ShouldNavigate()
        {
            const string expected = TestConstants.ServerUrl + "/input/button.html";
            await WebView.CoreWebView2.NavigateToAsync(expected);

            var history = await DevToolsContext.GetNavigationHistoryAsync();

            Assert.NotNull(history);
            Assert.NotEmpty(history.Entries);
            Assert.Equal(1, history.CurrentIndex);

            var lastEntry = history.Entries.LastOrDefault();

            Assert.NotNull(lastEntry);

            Assert.Equal(expected, lastEntry.Url);
        }

        [WebView2ContextFact]
        public async Task ShouldNavigateToHistory()
        {
            const string expected = TestConstants.ServerUrl + "/input/button.html";
            await WebView.CoreWebView2.NavigateToAsync(expected);

            var history = await DevToolsContext.GetNavigationHistoryAsync();

            Assert.NotNull(history);
            Assert.NotEmpty(history.Entries);
            Assert.Equal(1, history.CurrentIndex);

            var lastEntry = history.Entries.LastOrDefault();

            Assert.NotNull(lastEntry);
            Assert.Equal(expected, lastEntry.Url);

            var firstEntry = history.Entries.First();

            await DevToolsContext.NavigateToHistoryEntryAsync(firstEntry);

            history = await DevToolsContext.GetNavigationHistoryAsync();

            Assert.NotNull(history);
            Assert.NotEmpty(history.Entries);
            Assert.Equal(0, history.CurrentIndex);

            var entry = history.Entries[history.CurrentIndex];

            Assert.Equal(firstEntry.Url, entry.Url);
        }
    }
}
