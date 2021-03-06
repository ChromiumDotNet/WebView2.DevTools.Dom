using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class BoxModelTests : DevTooolsContextBaseTest
    {
        public BoxModelTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/resetcss.html");

            // Step 1: Add Frame and position it absolutely.
            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.ServerUrl + "/resetcss.html");
            await DevToolsContext.EvaluateExpressionAsync(@"
              const frame = document.querySelector('#frame1');
              frame.style = `
                position: absolute;
                left: 1px;
                top: 2px;
              `;");

            // Step 2: Add div and position it absolutely inside frame.
            var frame = DevToolsContext.FirstChildFrame();
            var divHandle = await frame.EvaluateFunctionHandleAsync<HtmlElement>(@"() => {
              const div = document.createElement('div');
              document.body.appendChild(div);
              div.style = `
                box-sizing: border-box;
                position: absolute;
                border-left: 1px solid black;
                padding-left: 2px;
                margin-left: 3px;
                left: 4px;
                top: 5px;
                width: 6px;
                height: 7px;
              `;
              return div;
            }");

            // Step 3: query div's boxModel and assert box values.
            var box = await divHandle.BoxModelAsync();
            Assert.Equal(6, box.Width);
            Assert.Equal(7, box.Height);
            Assert.Equal(new BoxModelPoint
            {
                X = 1 + 4, // frame.left + div.left
                Y = 2 + 5
            }, box.Margin[0]);
            Assert.Equal(new BoxModelPoint
            {
                X = 1 + 4 + 3, // frame.left + div.left + div.margin-left
                Y = 2 + 5
            }, box.Border[0]);
            Assert.Equal(new BoxModelPoint
            {
                X = 1 + 4 + 3 + 1, // frame.left + div.left + div.marginLeft + div.borderLeft
                Y = 2 + 5
            }, box.Padding[0]);
            Assert.Equal(new BoxModelPoint
            {
                X = 1 + 4 + 3 + 1 + 2, // frame.left + div.left + div.marginLeft + div.borderLeft + dif.paddingLeft
                Y = 2 + 5
            }, box.Content[0]);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForInvisibleElements()
        {
            await DevToolsContext.SetContentAsync("<div style='display:none'>hi</div>");
            var elementHandle = await DevToolsContext.QuerySelectorAsync<HtmlDivElement>("div");
            Assert.Null(await elementHandle.BoxModelAsync());
        }
    }
}
