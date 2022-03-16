using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.EmulationTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class EmulateTimezoneTests : DevTooolsContextBaseTest
    {
        public EmulateTimezoneTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.EvaluateExpressionAsync("globalThis.date = new Date(1479579154987);");
            await DevToolsContext.EmulateTimezoneAsync("America/Jamaica");
            Assert.Equal(
                "Sat Nov 19 2016 13:12:34 GMT-0500 (Eastern Standard Time)",
                await DevToolsContext.EvaluateExpressionAsync<string>("date.toString()"));

            await DevToolsContext.EmulateTimezoneAsync("Pacific/Honolulu");
            Assert.Equal(
                "Sat Nov 19 2016 08:12:34 GMT-1000 (Hawaii-Aleutian Standard Time)",
                await DevToolsContext.EvaluateExpressionAsync<string>("date.toString()"));

            await DevToolsContext.EmulateTimezoneAsync("America/Buenos_Aires");
            Assert.Equal(
                "Sat Nov 19 2016 15:12:34 GMT-0300 (Argentina Standard Time)",
                await DevToolsContext.EvaluateExpressionAsync<string>("date.toString()"));

            await DevToolsContext.EmulateTimezoneAsync("Europe/Berlin");
            Assert.Equal(
                "Sat Nov 19 2016 19:12:34 GMT+0100 (Central European Standard Time)",
                await DevToolsContext.EvaluateExpressionAsync<string>("date.toString()"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowForInvalidTimezoneId()
        {
            var exception = await Assert.ThrowsAnyAsync<WebView2DevToolsContextException>(
                () => DevToolsContext.EmulateTimezoneAsync("Foo/Bar"));
            Assert.Contains("Invalid timezone ID: Foo/Bar", exception.Message);

            exception = await Assert.ThrowsAnyAsync<WebView2DevToolsContextException>(
                () => DevToolsContext.EmulateTimezoneAsync("Baz/Qux"));
            Assert.Contains("Invalid timezone ID: Baz/Qux", exception.Message);
        }
    }
}
