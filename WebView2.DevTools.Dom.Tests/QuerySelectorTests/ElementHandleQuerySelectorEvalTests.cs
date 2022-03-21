using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ElementHandleQuerySelectorEvalTests : DevTooolsContextBaseTest
    {
        public ElementHandleQuerySelectorEvalTests(ITestOutputHelper output) : base(output)
        {
        }

        async Task Usage(Microsoft.Web.WebView2.WinForms.WebView2 webView2Browser)
        {
            #region Evaluate

            await webView2Browser.EnsureCoreWebView2Async();

            // WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
            // via await using or await devToolsContext.DisposeAsync();
            // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
            await using var devToolsContext = await webView2Browser.CoreWebView2.CreateDevToolsContextAsync();
            await devToolsContext.IgnoreCertificateErrorsAsync(true);
            var seven = await devToolsContext.EvaluateExpressionAsync<int>("4 + 3");
            // Can evaluate a function that returns a Promise
            var fourtyTwo = await devToolsContext.EvaluateFunctionAsync<int>("() => Promise.resolve(42)");
            // Pass in arguments to a function, including references to HtmlElements and JavascriptHandles
            var someObject = await devToolsContext.EvaluateFunctionAsync<dynamic>("(value) => ({a: value})", 5);
            System.Console.WriteLine(someObject.a);
            #endregion
        }

        [WebView2ContextFact]
        public async Task QuerySelectorShouldWork()
        {
            await DevToolsContext.SetContentAsync("<html><body><div class='tweet'><div class='like'>100</div><div class='retweets'>10</div></div></body></html>");
            var tweet = await DevToolsContext.QuerySelectorAsync(".tweet");
            var content = await tweet.QuerySelectorAsync(".like")
                .EvaluateFunctionAsync<string>("node => node.innerText");
            Assert.Equal("100", content);
        }

        [WebView2ContextFact]
        public async Task QuerySelectorShouldRetrieveContentFromSubtree()
        {
            var htmlContent = "<div class='a'>not-a-child-div</div><div id='myId'><div class='a'>a-child-div</div></div>";
            await DevToolsContext.SetContentAsync(htmlContent);
            var elementHandle = await DevToolsContext.QuerySelectorAsync("#myId");
            var content = await elementHandle.QuerySelectorAsync(".a")
                .EvaluateFunctionAsync<string>("node => node.innerText");
            Assert.Equal("a-child-div", content);
        }

        [WebView2ContextFact]
        public async Task QuerySelectorShouldThrowInCaseOfMissingSelector()
        {
            var htmlContent = "<div class=\"a\">not-a-child-div</div><div id=\"myId\"></div>";
            await DevToolsContext.SetContentAsync(htmlContent);
            var elementHandle = await DevToolsContext.QuerySelectorAsync("#myId");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsSelectorException>(
                () => elementHandle.QuerySelectorAsync(".a").EvaluateFunctionAsync<string>("node => node.innerText")
            );
            Assert.Equal("Error: failed to find element matching selector", exception.Message);
        }
    }
}
