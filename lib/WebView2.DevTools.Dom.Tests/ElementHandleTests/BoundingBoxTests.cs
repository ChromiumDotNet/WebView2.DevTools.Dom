using System.Linq;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class BoundingBoxTests : DevTooolsContextBaseTest
    {
        public BoundingBoxTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.SetViewportAsync(new ViewPortOptions
            {
                Width = 500,
                Height = 500
            });
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/grid.html");
            var elementHandle = await DevToolsContext.QuerySelectorAsync(".box:nth-of-type(13)");
            var box = await elementHandle.BoundingBoxAsync();
            Assert.Equal(new BoundingBox(100, 50, 50, 50), box);
        }

        [WebView2ContextFact]
        public async Task ShouldHandleNestedFrames()
        {
            await DevToolsContext.SetViewportAsync(new ViewPortOptions
            {
                Width = 500,
                Height = 500
            });
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/frames/nested-frames.html");
            var childFrame = DevToolsContext.Frames.First(f => f.Url.Contains("two-frames.html"));
            var nestedFrame = childFrame.ChildFrames.Last();
            var elementHandle = await nestedFrame.QuerySelectorAsync("div");
            var box = await elementHandle.BoundingBoxAsync();

            Assert.Equal(new BoundingBox(28, 182, 264, 18), box);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForInvisibleElements()
        {
            await DevToolsContext.SetContentAsync("<div style='display:none'>hi</div>");
            var elementHandle = await DevToolsContext.QuerySelectorAsync("div");
            Assert.Null(await elementHandle.BoundingBoxAsync());
        }

        [WebView2ContextFact]
        public async Task ShouldForceALayout()
        {
            await DevToolsContext.SetViewportAsync(new ViewPortOptions { Width = 500, Height = 500 });
            await DevToolsContext.SetContentAsync("<div style='width: 100px; height: 100px'>hello</div>");
            var elementHandle = await DevToolsContext.QuerySelectorAsync("div");
            await DevToolsContext.EvaluateFunctionAsync("element => element.style.height = '200px'", elementHandle);
            var box = await elementHandle.BoundingBoxAsync();
            Assert.Equal(new BoundingBox(8, 8, 100, 200), box);
        }

        [WebView2ContextFact]
        public async Task ShouldWworkWithSVGNodes()
        {
            await DevToolsContext.SetContentAsync(@"
                <svg xmlns=""http://www.w3.org/2000/svg"" width=""500"" height=""500"">
                  <rect id=""theRect"" x=""30"" y=""50"" width=""200"" height=""300""></rect>
                </svg>
            ");

            var element = await DevToolsContext.QuerySelectorAsync("#therect");
            var pptrBoundingBox = await element.BoundingBoxAsync();
            var webBoundingBox = await DevToolsContext.EvaluateFunctionAsync<BoundingBox>(@"e =>
            {
                const rect = e.getBoundingClientRect();
                return { x: rect.x, y: rect.y, width: rect.width, height: rect.height};
            }", element);
            Assert.Equal(webBoundingBox, pptrBoundingBox);
        }
    }
}
