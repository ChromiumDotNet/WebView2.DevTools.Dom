using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ClickTests : DevTooolsContextBaseTest
    {
        public ClickTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");
            await button.ClickAsync();
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForShadowDomV1()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/shadow.html");
            var buttonHandle = (HtmlElement)await DevToolsContext.EvaluateExpressionHandleAsync("button");
            await buttonHandle.ClickAsync();
            Assert.True(await DevToolsContext.EvaluateExpressionAsync<bool>("clicked"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForTextNodes()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var buttonTextNode = (HtmlElement)await DevToolsContext.EvaluateExpressionHandleAsync(
                "document.querySelector('button').firstChild");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(async () => await buttonTextNode.ClickAsync());
            Assert.Equal("Node is not of type HTMLElement", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForDetachedNodes()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");
            await DevToolsContext.EvaluateFunctionAsync("button => button.remove()", button);
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(async () => await button.ClickAsync());
            Assert.Equal("Node is detached from document", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForHiddenNodes()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");
            await DevToolsContext.EvaluateFunctionAsync("button => button.style.display = 'none'", button);
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(async () => await button.ClickAsync());
            Assert.Equal("Node is either not visible or not an HTMLElement", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForRecursivelyHiddenNodes()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");
            await DevToolsContext.EvaluateFunctionAsync("button => button.parentElement.style.display = 'none'", button);
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(async () => await button.ClickAsync());
            Assert.Equal("Node is either not visible or not an HTMLElement", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForBrElements()
        {
            await DevToolsContext.SetContentAsync("hello<br>goodbye");
            var br = await DevToolsContext.QuerySelectorAsync("br");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(async () => await br.ClickAsync());
            Assert.Equal("Node is either not visible or not an HTMLElement", exception.Message);
        }
    }
}
