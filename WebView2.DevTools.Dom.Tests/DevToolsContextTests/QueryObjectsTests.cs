using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class QueryObjectsTests : DevTooolsContextBaseTest
    {
        public QueryObjectsTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            // Instantiate an object
            await DevToolsContext.EvaluateExpressionAsync("window.set = new Set(['hello', 'world'])");
            var prototypeHandle = await DevToolsContext.EvaluateExpressionHandleAsync("Set.prototype");
            var objectsHandle = await DevToolsContext.QueryObjectsAsync(prototypeHandle);
            var count = await DevToolsContext.EvaluateFunctionAsync<int>("objects => objects.length", objectsHandle);
            Assert.Equal(1, count);
            var values = await DevToolsContext.EvaluateFunctionAsync<string[]>("objects => Array.from(objects[0].values())", objectsHandle);
            Assert.Equal(new[] { "hello", "world" }, values);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForNonBlankPage()
        {
            // Instantiate an object
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.EvaluateFunctionAsync("() => window.set = new Set(['hello', 'world'])");
            var prototypeHandle = await DevToolsContext.EvaluateFunctionHandleAsync("() => Set.prototype");
            var objectsHandle = await DevToolsContext.QueryObjectsAsync(prototypeHandle);
            var count = await DevToolsContext.EvaluateFunctionAsync<int>("objects => objects.length", objectsHandle);
            Assert.Equal(1, count);
        }

        [WebView2ContextFact]
        public async Task ShouldFailForDisposedHandles()
        {
            var prototypeHandle = await DevToolsContext.EvaluateExpressionHandleAsync("HTMLBodyElement.prototype");
            await prototypeHandle.DisposeAsync();
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(()
                => DevToolsContext.QueryObjectsAsync(prototypeHandle));
            Assert.Equal("Prototype JavascriptHandle is disposed!", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldFailPrimitiveValuesAsPrototypes()
        {
            var prototypeHandle = await DevToolsContext.EvaluateExpressionHandleAsync("42");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(()
                => DevToolsContext.QueryObjectsAsync(prototypeHandle));
            Assert.Equal("Prototype JavascriptHandle must not be referencing primitive value", exception.Message);
        }
    }
}
