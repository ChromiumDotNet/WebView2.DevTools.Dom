using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ExposeFunctionTests : DevTooolsContextBaseTest
    {
        public ExposeFunctionTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.ExposeFunctionAsync("compute", (int a, int b) => a * b);
            var result = await DevToolsContext.EvaluateExpressionAsync<int>("compute(9, 4)");
            Assert.Equal(36, result);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowExceptionInPageContext()
        {
            await DevToolsContext.ExposeFunctionAsync("woof", () => throw new Exception("WOOF WOOF"));
            var result = await DevToolsContext.EvaluateFunctionAsync<JsonElement>(@" async () =>{
                try
                {
                    await woof();
                }
                catch (e)
                {
                    return { message: e.message, stack: e.stack};
                }
            }");
            Assert.Equal("WOOF WOOF", result.GetProperty("message").GetString());
            Assert.Contains("ExposeFunctionTests", result.GetProperty("stack").GetString());
        }

        [WebView2ContextFact]
        public async Task ShouldBeCallableFromInsideEvaluateOnNewDocument()
        {
            var called = false;
            await DevToolsContext.ExposeFunctionAsync("woof", () => called = true);
            await DevToolsContext.EvaluateFunctionOnNewDocumentAsync("() => woof()");
            await WebView.CoreWebView2.ReloadAsync();
            Assert.True(called);
        }

        [WebView2ContextFact]
        public async Task ShouldSurviveNavigation()
        {
            await DevToolsContext.ExposeFunctionAsync("compute", (int a, int b) => a * b);
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var result = await DevToolsContext.EvaluateExpressionAsync<int>("compute(9, 4)");
            Assert.Equal(36, result);
        }

        [WebView2ContextFact]
        public async Task ShouldAwaitReturnedValueTask()
        {
            await DevToolsContext.ExposeFunctionAsync("compute", (int a, int b) => Task.FromResult(a * b));
            var result = await DevToolsContext.EvaluateExpressionAsync<int>("compute(3, 5)");
            Assert.Equal(15, result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkOnFrames()
        {
            await DevToolsContext.ExposeFunctionAsync("compute", (int a, int b) => Task.FromResult(a * b));
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");
            var frame = DevToolsContext.FirstChildFrame();
            var result = await frame.EvaluateExpressionAsync<int>("compute(3, 5)");
            Assert.Equal(15, result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkOnFramesBeforeNavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");
            await DevToolsContext.ExposeFunctionAsync("compute", (int a, int b) => Task.FromResult(a * b));

            var frame = DevToolsContext.FirstChildFrame();
            var result = await frame.EvaluateExpressionAsync<int>("compute(3, 5)");
            Assert.Equal(15, result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithComplexObjects()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");
            await DevToolsContext.ExposeFunctionAsync("complexObject", (dynamic a, dynamic b) => Task.FromResult(new { X = a.x + b.x }));

            var result = await DevToolsContext.EvaluateExpressionAsync<JsonElement>("complexObject({x: 5}, {x: 2})");

            var actual = result.GetProperty("x").GetInt32();
            Assert.Equal(7, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldAwaitReturnedTask()
        {
            var called = false;
            await DevToolsContext.ExposeFunctionAsync("changeFlag", () =>
            {
                called = true;
                return Task.CompletedTask;
            });
            await DevToolsContext.EvaluateExpressionAsync("changeFlag()");
            Assert.True(called);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAction()
        {
            var called = false;
            await DevToolsContext.ExposeFunctionAsync("changeFlag", () =>
            {
                called = true;
            });
            await DevToolsContext.EvaluateExpressionAsync("changeFlag()");
            Assert.True(called);
        }
    }
}
