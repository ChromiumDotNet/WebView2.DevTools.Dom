using System;
using System.IO;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AddStyleTagTests : DevTooolsContextBaseTest
    {
        public AddStyleTagTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldThrowAnErrorIfNoOptionsAreProvided()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(()
                => DevToolsContext.AddStyleTagAsync(new AddTagOptions()));
            Assert.Equal("Provide options with a `Url`, `Path` or `Content` property", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAUrl()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var styleHandle = await DevToolsContext.AddStyleTagAsync(new AddTagOptions { Url = "/injectedstyle.css" });
            Assert.NotNull(styleHandle as HtmlElement);
            Assert.Equal("rgb(255, 0, 0)", await DevToolsContext.EvaluateExpressionAsync<string>(
                "window.getComputedStyle(document.querySelector('body')).getPropertyValue('background-color')"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowAnErrorIfLoadingFromUrlFail()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(()
                => DevToolsContext.AddStyleTagAsync(new AddTagOptions { Url = "/nonexistfile.js" }));
            Assert.Equal("Loading style from /nonexistfile.js failed", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAPath()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var styleHandle = await DevToolsContext.AddStyleTagAsync(new AddTagOptions { Path = "Assets/injectedstyle.css" });
            Assert.NotNull(styleHandle as HtmlElement);
            Assert.Equal("rgb(255, 0, 0)", await DevToolsContext.EvaluateExpressionAsync<string>(
                "window.getComputedStyle(document.querySelector('body')).getPropertyValue('background-color')"));
        }

        [WebView2ContextFact]
        public async Task ShouldIncludeSourcemapWhenPathIsProvided()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.AddStyleTagAsync(new AddTagOptions
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Assets", "injectedstyle.css"))
            });
            var styleHandle = await DevToolsContext.QuerySelectorAsync("style");
            var styleContent = await DevToolsContext.EvaluateFunctionAsync<string>("style => style.innerHTML", styleHandle);
            Assert.Contains(Path.Combine("Assets", "injectedstyle.css"), styleContent);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithContent()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var styleHandle = await DevToolsContext.AddStyleTagAsync(new AddTagOptions { Content = "body { background-color: green; }" });
            Assert.NotNull(styleHandle as HtmlElement);
            Assert.Equal("rgb(0, 128, 0)", await DevToolsContext.EvaluateExpressionAsync<string>(
                "window.getComputedStyle(document.querySelector('body')).getPropertyValue('background-color')"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowWhenAddedWithContentToTheCSPPage()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(
                () => DevToolsContext.AddStyleTagAsync(new AddTagOptions
                {
                    Content = "body { background-color: green; }"
                }));
            Assert.NotNull(exception);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowWhenAddedWithURLToTheCSPPage()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/csp.html");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(
                () => DevToolsContext.AddStyleTagAsync(new AddTagOptions
                {
                    Url = TestConstants.CrossProcessUrl + "/injectedstyle.css"
                }));
            Assert.NotNull(exception);
        }
    }
}
