using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;
using WebView2.DevTools.Dom;

namespace WebView2.DevTools.Dom.Tests.FrameTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class FrameManagementTests : DevTooolsContextBaseTest
    {
        public FrameManagementTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldHandleNestedFrames()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");

            var actual = FrameUtils.DumpFrames(DevToolsContext.MainFrame);

            Assert.Equal(
                TestConstants.NestedFramesDumpResult,
                actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSendEventsWhenFramesAreManipulatedDynamically()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            // validate frameattached events
            var attachedFrames = new List<Frame>();

            DevToolsContext.FrameAttached += (_, e) => attachedFrames.Add(e.Frame);

            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", "./Assets/frame.html");

            Assert.Single(attachedFrames);
            Assert.Contains("/Assets/frame.html", attachedFrames[0].Url);

            // validate framenavigated events
            var navigatedFrames = new List<Frame>();
            DevToolsContext.FrameNavigated += (_, e) => navigatedFrames.Add(e.Frame);

            await FrameUtils.NavigateFrameAsync(DevToolsContext, "frame1", "./empty.html");
            Assert.Single(navigatedFrames);
            Assert.Equal(TestConstants.EmptyPage, navigatedFrames[0].Url);

            // validate framedetached events
            var detachedFrames = new List<Frame>();
            DevToolsContext.FrameDetached += (_, e) => detachedFrames.Add(e.Frame);

            await FrameUtils.DetachFrameAsync(DevToolsContext, "frame1");
            Assert.Single(navigatedFrames);
            Assert.True(navigatedFrames[0].Detached);
        }

        [WebView2ContextFact(Skip = "TODO: Fix me")]
        public async Task ShouldSendFrameNavigatedWhenNavigatingOnAnchorURLs()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var frameNavigated = new TaskCompletionSource<bool>();
            DevToolsContext.FrameNavigated += (_, _) => frameNavigated.TrySetResult(true);
            await Task.WhenAll(
                WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage + "#foo"),
                frameNavigated.Task
            );
            Assert.Equal(TestConstants.EmptyPage + "#foo", DevToolsContext.Url);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnUrlFragmentAsPartOfUrl()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/one-frame-url-fragment.html");
            Assert.Equal(2, DevToolsContext.Frames.Length);
            Assert.Equal(TestConstants.ServerUrl + "/frames/frame.html?param=value#fragment", DevToolsContext.FirstChildFrame().Url);
        }

        [WebView2ContextFact]
        public async Task ShouldPersistMainFrameOnCrossProcessNavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var mainFrame = DevToolsContext.MainFrame;
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.CrossProcessUrl + "/empty.html");
            Assert.Equal(mainFrame, DevToolsContext.MainFrame);
        }

        [WebView2ContextFact]
        public async Task ShouldNotSendAttachDetachEventsForMainFrame()
        {
            var hasEvents = false;
            DevToolsContext.FrameAttached += (_, _) => hasEvents = true;
            DevToolsContext.FrameDetached += (_, _) => hasEvents = true;

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            Assert.False(hasEvents);
        }

        [WebView2ContextFact]
        public async Task ShouldDetachChildFramesOnNavigation()
        {
            var attachedFrames = new List<Frame>();
            var detachedFrames = new List<Frame>();
            var navigatedFrames = new List<Frame>();

            DevToolsContext.FrameAttached += (_, e) => attachedFrames.Add(e.Frame);
            DevToolsContext.FrameDetached += (_, e) => detachedFrames.Add(e.Frame);
            DevToolsContext.FrameNavigated += (_, e) => navigatedFrames.Add(e.Frame);

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");
            Assert.Equal(4, attachedFrames.Count);
            Assert.Empty(detachedFrames);
            Assert.Equal(5, navigatedFrames.Count);

            attachedFrames.Clear();
            detachedFrames.Clear();
            navigatedFrames.Clear();

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            Assert.Empty(attachedFrames);
            Assert.Equal(4, detachedFrames.Count);
            Assert.Single(navigatedFrames);
        }

        [WebView2ContextFact]
        public async Task ShouldReportFrameFromInsideShadowDOM()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/shadow.html");
            await DevToolsContext.EvaluateFunctionAsync(@"async url =>
            {
                const frame = document.createElement('iframe');
                frame.src = url;
                document.body.shadowRoot.appendChild(frame);
                await new Promise(x => frame.onload = x);
            }", TestConstants.EmptyPage);
            Assert.Equal(2, DevToolsContext.Frames.Length);
            Assert.Single(DevToolsContext.Frames, frame => frame.Url == TestConstants.EmptyPage);
        }

        [WebView2ContextFact]
        public async Task ShouldReportFrameName()
        {
            await FrameUtils.AttachFrameAsync(DevToolsContext, "theFrameId", TestConstants.EmptyPage);
            await DevToolsContext.EvaluateFunctionAsync(@"url => {
                const frame = document.createElement('iframe');
                frame.name = 'theFrameName';
                frame.src = url;
                document.body.appendChild(frame);
                return new Promise(x => frame.onload = x);
            }", TestConstants.EmptyPage);

            Assert.Single(DevToolsContext.Frames, frame => frame.Name == string.Empty);
            Assert.Single(DevToolsContext.Frames, frame => frame.Name == "theFrameId");
            Assert.Single(DevToolsContext.Frames, frame => frame.Name == "theFrameName");
        }

        [WebView2ContextFact]
        public async Task ShouldReportFrameParent()
        {
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame2", TestConstants.EmptyPage);

            Assert.Single(DevToolsContext.Frames, frame => frame.ParentFrame == null);
            Assert.Equal(2, DevToolsContext.Frames.Count(f => f.ParentFrame == DevToolsContext.MainFrame));
        }

        [WebView2ContextFact]
        public async Task ShouldReportDifferentFrameInstanceWhenFrameReAttaches()
        {
            var frame1 = await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            await DevToolsContext.EvaluateFunctionAsync(@"() => {
                window.frame = document.querySelector('#frame1');
                window.frame.remove();
            }");
            Assert.True(frame1.Detached);
            var frame2tsc = new TaskCompletionSource<Frame>();
            DevToolsContext.FrameAttached += (_, e) => frame2tsc.TrySetResult(e.Frame);
            await DevToolsContext.EvaluateExpressionAsync("document.body.appendChild(window.frame)");
            var frame2 = await frame2tsc.Task;
            Assert.False(frame2.Detached);
            Assert.NotSame(frame1, frame2);
        }
    }
}
