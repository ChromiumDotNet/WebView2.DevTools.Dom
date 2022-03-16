using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ElementHandleQuerySelectorAllEvalTests : DevTooolsContextBaseTest
    {
        public ElementHandleQuerySelectorAllEvalTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.SetContentAsync("<html><body><div class='tweet'><div class='like'>100</div><div class='like'>10</div></div></body></html>");
            var tweet = await DevToolsContext.QuerySelectorAsync(".tweet");
            var content = await tweet.QuerySelectorAllHandleAsync(".like")
                .EvaluateFunctionAsync<string[]>("nodes => nodes.map(n => n.innerText)");
            Assert.Equal(new[] { "100", "10" }, content);
        }

        [WebView2ContextFact]
        public async Task QuerySelectorAllShouldRetrieveContentFromSubtree()
        {
            var htmlContent = "<div class='a'>not-a-child-div</div><div id='myId'><div class='a'>a1-child-div</div><div class='a'>a2-child-div</div></div>";
            await DevToolsContext.SetContentAsync(htmlContent);
            var elementHandle = await DevToolsContext.QuerySelectorAsync("#myId");
            var content = await elementHandle.QuerySelectorAllHandleAsync(".a")
                .EvaluateFunctionAsync<string[]>("nodes => nodes.map(n => n.innerText)");
            Assert.Equal(new[] { "a1-child-div", "a2-child-div" }, content);
        }

        [WebView2ContextFact]
        public async Task QuerySelectorAllShouldNotThrowInCaseOfMissingSelector()
        {
            var htmlContent = "<div class='a'>not-a-child-div</div><div id='myId'></div>";
            await DevToolsContext.SetContentAsync(htmlContent);
            var elementHandle = await DevToolsContext.QuerySelectorAsync("#myId");
            var nodesLength = await elementHandle.QuerySelectorAllHandleAsync(".a")
                .EvaluateFunctionAsync<int>("nodes => nodes.length");
            Assert.Equal(0, nodesLength);
        }
    }
}
