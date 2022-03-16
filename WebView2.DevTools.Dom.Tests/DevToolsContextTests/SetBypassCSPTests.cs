using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class SetBypassCSPTests : DevTooolsContextBaseTest
    {
        public SetBypassCSPTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldBypassCSPMetaTag()
        {
            // Make sure CSP prohibits addScriptTag.
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            }).ContinueWith(_ => Task.CompletedTask);
            Assert.Null(await DevToolsContext.EvaluateExpressionAsync<object>("window.__injected"));

            // By-pass CSP and try one more time.
            await DevToolsContext.SetBypassCSPAsync(true);
            await WebView.CoreWebView2.ReloadAsync();
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            });
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("window.__injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldBypassCSPHeader()
        {
            // Make sure CSP prohibits addScriptTag.
            AddCspHeader("default-src 'self'");
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            }).ContinueWith(_ => Task.CompletedTask);
            Assert.Null(await DevToolsContext.EvaluateExpressionAsync<object>("window.__injected"));

            // By-pass CSP and try one more time.
            await DevToolsContext.SetBypassCSPAsync(true);
            await WebView.CoreWebView2.ReloadAsync();
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            });
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("window.__injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldBypassAfterCrossProcessNavigation()
        {
            await DevToolsContext.SetBypassCSPAsync(true);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            });
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("window.__injected"));

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.CrossProcessUrl + "/csp.html");
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            });
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("window.__injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldBypassCSPInIframesAsWell()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);

            // Make sure CSP prohibits addScriptTag in an iframe.
            var frame = await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.ServerUrl + "/csp.html");
            await frame.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            }).ContinueWith(_ => Task.CompletedTask);
            Assert.Null(await frame.EvaluateFunctionAsync<int?>("() => window.__injected"));

            // By-pass CSP and try one more time.
            await DevToolsContext.SetBypassCSPAsync(true);
            await WebView.CoreWebView2.ReloadAsync();

            // Make sure CSP prohibits addScriptTag in an iframe.
            frame = await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.ServerUrl + "/csp.html");
            await frame.AddScriptTagAsync(new AddTagOptions
            {
                Content = "window.__injected = 42;"
            }).ContinueWith(_ => Task.CompletedTask);
            Assert.Equal(42, await frame.EvaluateFunctionAsync<double?>("() => window.__injected"));
        }
    }
}
