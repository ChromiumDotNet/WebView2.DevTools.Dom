using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Text.Json;

namespace WebView2.DevTools.Dom.Tests.JSHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class JsonValueTests : DevTooolsContextBaseTest
    {
        public JsonValueTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync("({ foo: 'bar'})");
            var json = await aHandle.GetValueAsync<JsonElement>();

            var actual = json.GetProperty("foo").GetString();

            Assert.Equal("bar", actual);
        }

        [WebView2ContextFact]
        public async Task WorksWithJsonValuesThatAreNotObjects()
        {
            var aHandle = await DevToolsContext.EvaluateFunctionHandleAsync("() => ['a', 'b']");
            var json = await aHandle.GetValueAsync<string[]>();
            Assert.Equal(new[] {"a","b" }, json);
        }

        [WebView2ContextFact]
        public async Task WorksWithJsonValuesThatArePrimitives()
        {
            var aHandle = await DevToolsContext.EvaluateFunctionHandleAsync("() => 'foo'");
            var json = await aHandle.GetValueAsync<string>();
            Assert.Equal("foo", json);
        }

        [WebView2ContextFact]
        public async Task ShouldNotWorkWithDates()
        {
            var dateHandle = await DevToolsContext.EvaluateExpressionHandleAsync("new Date('2017-09-26T00:00:00.000Z')");
            var json = await dateHandle.JsonValueAsync();
            Assert.Equal("{}", json.ToString());
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForCircularObjects()
        {
            var windowHandle = await DevToolsContext.EvaluateExpressionHandleAsync("window");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(()
                => windowHandle.JsonValueAsync());

            // Improve this when https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609
            // is resolved.
            Assert.Contains("CallFunctionOnAsync failed", exception.Message);
        }
    }
}
