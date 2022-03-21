using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DevToolsContextQuerySelectorAllEvalTests : DevTooolsContextBaseTest
    {
        public DevToolsContextQuerySelectorAllEvalTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.SetContentAsync("<div>hello</div><div>beautiful</div><div>world!</div>");
            var divsCount = await DevToolsContext.QuerySelectorAllHandleAsync("div").EvaluateFunctionAsync<int>("divs => divs.length");
            Assert.Equal(3, divsCount);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAwaitedElements()
        {
            await DevToolsContext.SetContentAsync("<div>hello</div><div>beautiful</div><div>world!</div>");
            var divs = await DevToolsContext.QuerySelectorAllHandleAsync("div");
            var divsCount = await divs.EvaluateFunctionAsync<int>("divs => divs.length");
            Assert.Equal(3, divsCount);
        }
    }
}
