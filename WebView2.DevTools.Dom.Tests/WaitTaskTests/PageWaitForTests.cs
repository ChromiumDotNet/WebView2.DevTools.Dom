using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.WaitForTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageWaitForTests : DevTooolsContextBaseTest
    {
        public PageWaitForTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact(Skip = "TODO: Fixme")]
        public async Task ShouldWaitForSelector()
        {
            var found = false;
            var waitFor = DevToolsContext.WaitForSelectorAsync("div").ContinueWith(_ => found = true);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);

            Assert.False(found);

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/grid.html");
            await waitFor;
            Assert.True(found);
        }

        [WebView2ContextFact(Skip = "TODO: Fixme")]
        public async Task ShouldWaitForAnXpath()
        {
            var found = false;
            var waitFor = DevToolsContext.WaitForXPathAsync("//div").ContinueWith(_ => found = true);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            Assert.False(found);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/grid.html");
            await waitFor;
            Assert.True(found);
        }

        [WebView2ContextFact]
        public async Task ShouldNotAllowYouToSelectAnElementWithSingleSlashXpath()
        {
            await DevToolsContext.SetContentAsync("<div>some text</div>");
            var exception = await Assert.ThrowsAsync<EvaluationFailedException>(() =>
                DevToolsContext.WaitForSelectorAsync("/html/body/div"));
            Assert.NotNull(exception);
        }

        [WebView2ContextFact]
        public async Task ShouldTimeout()
        {
            var startTime = DateTime.Now;
            var timeout = 42;
            await DevToolsContext.WaitForTimeoutAsync(timeout);
            Assert.True((DateTime.Now - startTime).TotalMilliseconds > timeout / 2);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithMultilineBody()
        {
            var result = await DevToolsContext.WaitForExpressionAsync(@"
                (() => true)()
            ");
            Assert.True(await result.GetValueAsync<bool>());
        }

        [WebView2ContextFact]
        public Task ShouldWaitForPredicate()
            => Task.WhenAll(
                DevToolsContext.WaitForFunctionAsync("() => window.innerWidth < 100"),
                DevToolsContext.SetViewportAsync(new ViewPortOptions { Width = 10, Height = 10 }));

        [WebView2ContextFact]
        public async Task ShouldWaitForPredicateWithArguments()
            => await DevToolsContext.WaitForFunctionAsync("(arg1, arg2) => arg1 !== arg2", new WaitForFunctionOptions(), 1, 2);
    }
}
