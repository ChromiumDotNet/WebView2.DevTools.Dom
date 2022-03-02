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
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync("document.body");
            var element = aHandle as HtmlElement;
            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonElements()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync("2");
            var element = aHandle as HtmlElement;
            Assert.Null(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnElementHandleForTextNodes()
        {
            await DevToolsContext.SetContentAsync("<div>ee!</div>");
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync("document.querySelector('div').firstChild");
            var element = aHandle as HtmlElement;
            Assert.NotNull(element);
            Assert.True(await DevToolsContext.EvaluateFunctionAsync<bool>("e => e.nodeType === HTMLElement.TEXT_NODE", element));
        }
    }
}
