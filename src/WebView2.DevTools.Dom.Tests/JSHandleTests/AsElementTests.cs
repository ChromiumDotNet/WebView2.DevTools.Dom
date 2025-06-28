using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.JSHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AsElementTests : DevTooolsContextBaseTest
    {
        public AsElementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            var element = await DevToolsContext.EvaluateExpressionHandleAsync<HtmlElement>("document.body");
            
            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonElements()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync<HtmlElement>("2");

            Assert.Null(aHandle);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNull()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync<HtmlElement>("null");

            Assert.Null(aHandle);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnElementHandleForTextNodes()
        {
            await DevToolsContext.SetContentAsync("<div>ee!</div>");

            var element = await DevToolsContext.EvaluateExpressionHandleAsync<Text>("document.querySelector('div').firstChild");

            Assert.NotNull(element);
            Assert.True(await DevToolsContext.EvaluateFunctionAsync<bool>("e => e.nodeType === HTMLElement.TEXT_NODE", element));
        }
    }
}
