using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ContentFrameTests : DevTooolsContextBaseTest
    {
        public ContentFrameTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            var elementHandle = await DevToolsContext.QuerySelectorAsync<HtmlInlineFrameElement>("#frame1");
            var frame = await elementHandle.ContentFrameAsync();
            Assert.Equal(DevToolsContext.FirstChildFrame().Id, frame.Id);
        }
    }
}
