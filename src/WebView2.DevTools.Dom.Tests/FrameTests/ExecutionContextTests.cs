using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.FrameTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ExecutionContextTests : DevTooolsContextBaseTest
    {
        public ExecutionContextTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            Assert.Equal(2, DevToolsContext.Frames.Length);

            var context1 = await DevToolsContext.MainFrame.GetExecutionContextAsync();
            var context2 = await DevToolsContext.FirstChildFrame().GetExecutionContextAsync();
            Assert.NotNull(context1);
            Assert.NotNull(context2);
            Assert.NotEqual(context1, context2);
            Assert.Equal(DevToolsContext.MainFrame, context1.Frame);
            Assert.Equal(DevToolsContext.FirstChildFrame(), context2.Frame);

            await Task.WhenAll(
                context1.EvaluateExpressionAsync("window.a = 1"),
                context2.EvaluateExpressionAsync("window.a = 2")
            );

            var a1 = context1.EvaluateExpressionAsync<int>("window.a");
            var a2 = context2.EvaluateExpressionAsync<int>("window.a");

            await Task.WhenAll(a1, a2);

            Assert.Equal(1, a1.Result);
            Assert.Equal(2, a2.Result);
        }
    }
}
