using System.Linq;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.EvaluationTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class FrameEvaluateTests : DevTooolsContextBaseTest
    {
        public FrameEvaluateTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldHaveDifferentExecutionContexts()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            Assert.Equal(2, DevToolsContext.Frames.Count());

            var frame1 = DevToolsContext.MainFrame;
            var frame2 = DevToolsContext.FirstChildFrame();

            await frame1.EvaluateExpressionAsync("window.FOO = 'foo'");
            await frame2.EvaluateExpressionAsync("window.FOO = 'bar'");

            Assert.Equal("foo", await frame1.EvaluateExpressionAsync<string>("window.FOO"));
            Assert.Equal("bar", await frame2.EvaluateExpressionAsync<string>("window.FOO"));
        }

        [WebView2ContextFact]
        public async Task ShouldExecuteAfterCrossSiteNavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var mainFrame = DevToolsContext.MainFrame;
            Assert.Contains("devtools.test", await mainFrame.EvaluateExpressionAsync<string>("window.location.href"));

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.CrossProcessHttpPrefix + "/empty.html");
            Assert.Contains("empty.html", await mainFrame.EvaluateExpressionAsync<string>("window.location.href"));
        }
    }
}
