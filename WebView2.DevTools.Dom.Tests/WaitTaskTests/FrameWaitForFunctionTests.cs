using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.WaitTaskTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class FrameWaitForFunctionTests : DevTooolsContextBaseTest
    {
        public FrameWaitForFunctionTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWhenResolvedRightBeforeExecutionContextDisposal()
        {
            await DevToolsContext.EvaluateFunctionOnNewDocumentAsync("() => window.__RELOADED = true");
            await DevToolsContext.WaitForFunctionAsync(@"() =>
            {
                if (!window.__RELOADED)
                    window.location.reload();
                return true;
            }");
        }

        [WebView2ContextFact]
        public async Task ShouldPollOnInterval()
        {
            var success = false;
            var startTime = DateTime.Now;
            var polling = 100;
            var watchdog = DevToolsContext.WaitForFunctionAsync("() => window.__FOO === 'hit'", new WaitForFunctionOptions { PollingInterval = polling })
                .ContinueWith(_ => success = true);
            await DevToolsContext.EvaluateExpressionAsync("window.__FOO = 'hit'");
            Assert.False(success);
            await DevToolsContext.EvaluateExpressionAsync("document.body.appendChild(document.createElement('div'))");
            await watchdog;
            Assert.True((DateTime.Now - startTime).TotalMilliseconds > polling / 2);
        }
        
        [WebView2ContextFact]
        public async Task ShouldPollOnIntervalAsync()
        {
            var success = false;
            var startTime = DateTime.Now;
            var polling = 100;
            var watchdog = DevToolsContext.WaitForFunctionAsync("async () => window.__FOO === 'hit'", new WaitForFunctionOptions { PollingInterval = polling })
                .ContinueWith(_ => success = true);
            await DevToolsContext.EvaluateFunctionAsync("async () => window.__FOO = 'hit'");
            Assert.False(success);
            await DevToolsContext.EvaluateExpressionAsync("document.body.appendChild(document.createElement('div'))");
            await watchdog;
            Assert.True((DateTime.Now - startTime).TotalMilliseconds > polling / 2);
        }

        [WebView2ContextFact]
        public async Task ShouldPollOnMutation()
        {
            var success = false;
            var watchdog = DevToolsContext.WaitForFunctionAsync("() => window.__FOO === 'hit'",
                new WaitForFunctionOptions { Polling = WaitForFunctionPollingOption.Mutation })
                .ContinueWith(_ => success = true);
            await DevToolsContext.EvaluateExpressionAsync("window.__FOO = 'hit'");
            Assert.False(success);
            await DevToolsContext.EvaluateExpressionAsync("document.body.appendChild(document.createElement('div'))");
            await watchdog;
        }

        [WebView2ContextFact]
        public async Task ShouldPollOnMutationAsync()
        {
            var success = false;
            var watchdog = DevToolsContext.WaitForFunctionAsync("async () => window.__FOO === 'hit'",
                new WaitForFunctionOptions { Polling = WaitForFunctionPollingOption.Mutation })
                .ContinueWith(_ => success = true);
            await DevToolsContext.EvaluateFunctionAsync("async () => window.__FOO = 'hit'");
            Assert.False(success);
            await DevToolsContext.EvaluateExpressionAsync("document.body.appendChild(document.createElement('div'))");
            await watchdog;
        }

        [WebView2ContextFact]
        public async Task ShouldPollOnRaf()
        {
            var watchdog = DevToolsContext.WaitForFunctionAsync("() => window.__FOO === 'hit'",
                new WaitForFunctionOptions { Polling = WaitForFunctionPollingOption.Raf });
            await DevToolsContext.EvaluateExpressionAsync("window.__FOO = 'hit'");
            await watchdog;
        }

        [WebView2ContextFact]
        public async Task ShouldPollOnRafAsync()
        {
            var watchdog = DevToolsContext.WaitForFunctionAsync("async () => window.__FOO === 'hit'",
                new WaitForFunctionOptions { Polling = WaitForFunctionPollingOption.Raf });
            await DevToolsContext.EvaluateFunctionAsync("async () => (globalThis.__FOO = 'hit')");
            await watchdog;
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithStrictCSPPolicy()
        {
            //Server.SetCSP("/empty.html", "script-src " + TestConstants.ServerUrl);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await Task.WhenAll(
                DevToolsContext.WaitForFunctionAsync("() => window.__FOO === 'hit'", new WaitForFunctionOptions
                {
                    Polling = WaitForFunctionPollingOption.Raf
                }),
                DevToolsContext.EvaluateExpressionAsync("window.__FOO = 'hit'"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowNegativePollingInterval()
        {
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
                => DevToolsContext.WaitForFunctionAsync("() => !!document.body", new WaitForFunctionOptions { PollingInterval = -10 }));

            Assert.Contains("Cannot poll with non-positive interval", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnTheSuccessValueAsAJSHandle()
        {
            const int expected = 5;

            var response = await DevToolsContext.WaitForFunctionAsync("() => 5");
            var actual = await response.GetValueAsync<int>();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnTheWindowAsASuccessValue()
            => Assert.NotNull(await DevToolsContext.WaitForFunctionAsync("() => window"));

        [WebView2ContextFact]
        public async Task ShouldAcceptElementHandleArguments()
        {
            await DevToolsContext.SetContentAsync("<div></div>");
            var div = await DevToolsContext.QuerySelectorAsync("div");
            var resolved = false;
            var waitForFunction = DevToolsContext.WaitForFunctionAsync("element => !element.parentElement", div)
                .ContinueWith(_ => resolved = true);
            Assert.False(resolved);
            await DevToolsContext.EvaluateFunctionAsync("element => element.remove()", div);
            await waitForFunction;
        }

        [WebView2ContextFact]
        public async Task ShouldRespectTimeout()
        {
            var exception = await Assert.ThrowsAsync<WaitTaskTimeoutException>(()
                => DevToolsContext.WaitForExpressionAsync("false", new WaitForFunctionOptions { Timeout = 10 }));

            Assert.Contains("waiting for function failed: timeout", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldRespectDefaultTimeout()
        {
            DevToolsContext.DefaultTimeout = 1;
            var exception = await Assert.ThrowsAsync<WaitTaskTimeoutException>(()
                => DevToolsContext.WaitForExpressionAsync("false"));

            Assert.Contains("waiting for function failed: timeout", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldDisableTimeoutWhenItsSetTo0()
        {
            var watchdog = DevToolsContext.WaitForFunctionAsync(@"() => {
                window.__counter = (window.__counter || 0) + 1;
                return window.__injected;
            }", new WaitForFunctionOptions { Timeout = 0, PollingInterval = 10 });
            await DevToolsContext.WaitForFunctionAsync("() => window.__counter > 10");
            await DevToolsContext.EvaluateExpressionAsync("window.__injected = true");
            await watchdog;
        }

        [WebView2ContextFact(Skip = "TODO: Fixme")]
        public async Task ShouldSurviveCrossProcessNavigation()
        {
            var fooFound = false;
            var waitForFunction = DevToolsContext.WaitForExpressionAsync("window.__FOO === 1")
                .ContinueWith(_ => fooFound = true);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            Assert.False(fooFound);
            await WebView.CoreWebView2.ReloadAsync();
            Assert.False(fooFound);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.CrossProcessUrl + "/grid.html");
            Assert.False(fooFound);
            await DevToolsContext.EvaluateExpressionAsync("window.__FOO = 1");
            await waitForFunction;
            Assert.True(fooFound);
        }

        [WebView2ContextFact(Skip = "TODO: Fixme")]
        public async Task ShouldSurviveNavigations()
        {
            var watchdog = DevToolsContext.WaitForFunctionAsync("() => window.__done");
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/consolelog.html");
            await DevToolsContext.EvaluateFunctionAsync("() => window.__done = true");
            await watchdog;
        }
    }
}
