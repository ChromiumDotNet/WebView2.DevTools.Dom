using System;
using System.IO;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AddScriptTagTests : DevTooolsContextBaseTest
    {
        public AddScriptTagTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldThrowAnErrorIfNoOptionsAreProvided()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(()
                => DevToolsContext.AddScriptTagAsync(new AddTagOptions()));
            Assert.Equal("Provide options with a `Url`, `Path` or `Content` property", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAUrl()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var scriptHandle = await DevToolsContext.AddScriptTagAsync(new AddTagOptions { Url = "/injectedfile.js" });
            Assert.NotNull(scriptHandle);
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("__injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAUrlAndTypeModule()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions { Url = "/es6/es6import.js", Type = "module" });
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("__es6injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAPathAndTypeModule()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Assets", "es6", "es6pathimport.js")),
                Type = "module"
            });
            await DevToolsContext.WaitForFunctionAsync("() => window.__es6injected");
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("__es6injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAContentAndTypeModule()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Content = "import num from '/es6/es6module.js'; window.__es6injected = num;",
                Type = "module"
            });
            await DevToolsContext.WaitForFunctionAsync("() => window.__es6injected");
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("__es6injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowAnErrorIfLoadingFromUrlFail()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(()
                => DevToolsContext.AddScriptTagAsync(new AddTagOptions { Url = "/nonexistfile.js" }));
            Assert.Equal("Loading script from /nonexistfile.js failed", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAPath()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var scriptHandle = await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Assets", "injectedfile.js"))
            });
            Assert.NotNull(scriptHandle as HtmlElement);
            Assert.Equal(42, await DevToolsContext.EvaluateExpressionAsync<int>("__injected"));
        }

        [WebView2ContextFact]
        public async Task ShouldIncludeSourcemapWhenPathIsProvided()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddScriptTagAsync(new AddTagOptions
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Assets", "injectedfile.js"))
            });
            var result = await DevToolsContext.EvaluateExpressionAsync<string>("__injectedError.stack");
            Assert.Contains(Path.Combine("Assets", "injectedfile.js"), result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithContent()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var scriptHandle = await DevToolsContext.AddScriptTagAsync(new AddTagOptions { Content = "window.__injected = 35;" });
            Assert.NotNull(scriptHandle);
            Assert.Equal(35, await DevToolsContext.EvaluateExpressionAsync<int>("__injected"));
        }

        [WebView2ContextFact(Skip = "@see https://github.com/GoogleChrome/puppeteer/issues/4840")]
        public async Task ShouldThrowWhenAddedWithContentToTheCSPPage()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            var exception = await Assert.ThrowsAsync<EvaluationFailedException>(
                () => DevToolsContext.AddScriptTagAsync(new AddTagOptions
                {
                    Content = "window.__injected = 35;"
                }));
            Assert.NotNull(exception);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowWhenAddedWithURLToTheCSPPage()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(
                () => DevToolsContext.AddScriptTagAsync(new AddTagOptions
                {
                    Url = TestConstants.CrossProcessUrl + "/injectedfile.js"
                }));
            Assert.NotNull(exception);
        }
    }
}
