using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.JSHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ToStringTests : DevTooolsContextBaseTest
    {
        public ToStringTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForPrimitives()
        {
            var numberHandle = await DevToolsContext.EvaluateExpressionHandleAsync("2");
            Assert.Equal("JavascriptHandle:2", numberHandle.ToString());
            var stringHandle = await DevToolsContext.EvaluateExpressionHandleAsync("'a'");
            Assert.Equal("JavascriptHandle:a", stringHandle.ToString());
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForComplicatedObjects()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync("window");
            Assert.Equal("JavascriptHandle@object", aHandle.ToString());
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithDifferentSubtypes()
        {
            Assert.Equal("JavascriptHandle@function", (await DevToolsContext.EvaluateExpressionHandleAsync("(function(){})")).ToString());
            Assert.Equal("JavascriptHandle:12", (await DevToolsContext.EvaluateExpressionHandleAsync("12")).ToString());
            Assert.Equal("JavascriptHandle:True", (await DevToolsContext.EvaluateExpressionHandleAsync("true")).ToString());
            Assert.Equal("JavascriptHandle:undefined", (await DevToolsContext.EvaluateExpressionHandleAsync("undefined")).ToString());
            Assert.Equal("JavascriptHandle:foo", (await DevToolsContext.EvaluateExpressionHandleAsync("'foo'")).ToString());
            Assert.Equal("JavascriptHandle@symbol", (await DevToolsContext.EvaluateExpressionHandleAsync("Symbol()")).ToString());
            Assert.Equal("JavascriptHandle@map", (await DevToolsContext.EvaluateExpressionHandleAsync("new Map()")).ToString());
            Assert.Equal("JavascriptHandle@set", (await DevToolsContext.EvaluateExpressionHandleAsync("new Set()")).ToString());
            Assert.Equal("JavascriptHandle@array", (await DevToolsContext.EvaluateExpressionHandleAsync("[]")).ToString());
            Assert.Equal("JavascriptHandle:null", (await DevToolsContext.EvaluateExpressionHandleAsync("null")).ToString());
            Assert.Equal("JavascriptHandle@regexp", (await DevToolsContext.EvaluateExpressionHandleAsync("/foo/")).ToString());
            Assert.Equal("HTMLBodyElement@body", (await DevToolsContext.EvaluateExpressionHandleAsync("document.body")).ToString());
            Assert.Equal("JavascriptHandle@date", (await DevToolsContext.EvaluateExpressionHandleAsync("new Date()")).ToString());
            Assert.Equal("JavascriptHandle@weakmap", (await DevToolsContext.EvaluateExpressionHandleAsync("new WeakMap()")).ToString());
            Assert.Equal("JavascriptHandle@weakset", (await DevToolsContext.EvaluateExpressionHandleAsync("new WeakSet()")).ToString());
            Assert.Equal("JavascriptHandle@error", (await DevToolsContext.EvaluateExpressionHandleAsync("new Error()")).ToString());
            Assert.Equal("JavascriptHandle@typedarray", (await DevToolsContext.EvaluateExpressionHandleAsync("new Int32Array()")).ToString());
            Assert.Equal("JavascriptHandle@proxy", (await DevToolsContext.EvaluateExpressionHandleAsync("new Proxy({}, {})")).ToString());
        }
    }
}
