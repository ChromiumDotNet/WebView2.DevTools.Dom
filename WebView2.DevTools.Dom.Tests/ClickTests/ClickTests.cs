using System.Collections.Generic;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Input;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.ClickTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ClickTests : DevTooolsContextBaseTest
    {
        public ClickTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.ClickAsync("button");
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickSvg()
        {
            await DevToolsContext.SetContentAsync($@"
                <svg height=""100"" width=""100"">
                  <circle onclick=""javascript:window.__CLICKED=42"" cx=""50"" cy=""50"" r=""40"" stroke=""black"" stroke-width=""3"" fill=""red""/>
                </svg>
            ");
            await DevToolsContext.ClickAsync("circle");
            Assert.Equal(42, await DevToolsContext.EvaluateFunctionAsync<int>("() => window.__CLICKED"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheButtonIfWindowNodeIsRemoved()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.EvaluateExpressionAsync("delete window.Node");
            await DevToolsContext.ClickAsync("button");
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickOnASpanWithAnInlineElementInside()
        {
            await DevToolsContext.SetContentAsync($@"
                <style>
                span::before {{
                    content: 'q';
                }}
                </style>
                <span onclick='javascript:window.CLICKED=42'></span>
            ");
            await DevToolsContext.ClickAsync("span");
            Assert.Equal(42, await DevToolsContext.EvaluateFunctionAsync<int>("() => window.CLICKED"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheButtonAfterNavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.ClickAsync("button");
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.ClickAsync("button");
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickWhenOneOfInlineBoxChildrenIsOutsideOfViewport()
        {
            await DevToolsContext.SetContentAsync($@"
            <style>
            i {{
                position: absolute;
                top: -1000px;
            }}
            </style>
            <span onclick='javascript:window.CLICKED = 42;'><i>woof</i><b>doggo</b></span>
            ");

            await DevToolsContext.ClickAsync("span");
            Assert.Equal(42, await DevToolsContext.EvaluateFunctionAsync<int>("() => window.CLICKED"));
        }

        [WebView2ContextFact]
        public async Task ShouldSelectTheTextByTripleClicking()
        {
            const string expected = "This is the text that we are going to try to select. Let's see how it goes.";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.FocusAsync("textarea");

            await DevToolsContext.Keyboard.TypeAsync(expected);
            await DevToolsContext.ClickAsync("textarea");
            await DevToolsContext.ClickAsync("textarea", new ClickOptions { ClickCount = 2 });
            await DevToolsContext.ClickAsync("textarea", new ClickOptions { ClickCount = 3 });

            var actual = await DevToolsContext.EvaluateFunctionAsync<string>(@"() => {
                const textarea = document.querySelector('textarea');
                return textarea.value.substring(
                    textarea.selectionStart,
                    textarea.selectionEnd
                );
            }");

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldClickOffscreenButtons()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/offscreenbuttons.html");
            var messages = new List<string>();

            WebView.CoreWebView2.WebMessageReceived += (_, args) => messages.Add(args.TryGetWebMessageAsString());

            for (var i = 0; i < 11; ++i)
            {
                // We might've scrolled to click a button - reset to (0, 0).
                await DevToolsContext.EvaluateFunctionAsync("() => window.scrollTo(0, 0)");
                await DevToolsContext.ClickAsync($"#btn{i}");
            }

            //Little dely to ensure messages from window.chrome.webview.postMessage have been processed.
            await Task.Delay(100);

            Assert.Equal(new List<string>
            {
                "button #0 clicked",
                "button #1 clicked",
                "button #2 clicked",
                "button #3 clicked",
                "button #4 clicked",
                "button #5 clicked",
                "button #6 clicked",
                "button #7 clicked",
                "button #8 clicked",
                "button #9 clicked",
                "button #10 clicked"
            }, messages);
        }

        [WebView2ContextFact]
        public async Task ShouldClickWrappedLinks()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/wrappedlink.html");
            await DevToolsContext.ClickAsync("a");
            Assert.True(await DevToolsContext.EvaluateExpressionAsync<bool>("window.__clicked"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickOnCheckboxInputAndToggle()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            Assert.Null(await DevToolsContext.EvaluateExpressionAsync<object>("result.check"));
            await DevToolsContext.ClickAsync("input#agree");
            Assert.True(await DevToolsContext.EvaluateExpressionAsync<bool>("result.check"));
            Assert.Equal(new[] {
                "mouseover",
                "mouseenter",
                "mousemove",
                "mousedown",
                "mouseup",
                "click",
                "input",
                "change"
            }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.events"));
            await DevToolsContext.ClickAsync("input#agree");
            Assert.False(await DevToolsContext.EvaluateExpressionAsync<bool>("result.check"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickOnCheckboxLabelAndToggle()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            Assert.Null(await DevToolsContext.EvaluateExpressionAsync<object>("result.check"));
            await DevToolsContext.ClickAsync("label[for=\"agree\"]");
            Assert.True(await DevToolsContext.EvaluateExpressionAsync<bool>("result.check"));
            Assert.Equal(new[] {
                "click",
                "input",
                "change"
            }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.events"));
            await DevToolsContext.ClickAsync("label[for=\"agree\"]");
            Assert.False(await DevToolsContext.EvaluateExpressionAsync<bool>("result.check"));
        }

        [WebView2ContextFact]
        public async Task ShouldFailToClickAMissingButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsSelectorException>(()
                => DevToolsContext.ClickAsync("button.does-not-exist"));
            Assert.Equal("No node found for selector: button.does-not-exist", exception.Message);
            Assert.Equal("button.does-not-exist", exception.Selector);
        }

        [WebView2ContextFact]
        public async Task ShouldScrollAndClickTheButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            await DevToolsContext.ClickAsync("#button-5");
            Assert.Equal("clicked", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector(\"#button-5\").textContent"));
            await DevToolsContext.ClickAsync("#button-80");
            Assert.Equal("clicked", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector(\"#button-80\").textContent"));
        }

        [WebView2ContextFact]
        public async Task ShouldDoubleClickTheButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.EvaluateExpressionAsync(@"{
               window.double = false;
               const button = document.querySelector('button');
               button.addEventListener('dblclick', event => {
                 window.double = true;
               });
            }");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");
            await button.ClickAsync(new ClickOptions { ClickCount = 2 });
            Assert.True(await DevToolsContext.EvaluateExpressionAsync<bool>("double"));
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickAPartiallyObscuredButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            await DevToolsContext.EvaluateExpressionAsync(@"{
                const button = document.querySelector('button');
                button.textContent = 'Some really long text that will go offscreen';
                button.style.position = 'absolute';
                button.style.left = '368px';
            }");
            await DevToolsContext.ClickAsync("button");
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickARotatedButton()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/rotatedButton.html");
            await DevToolsContext.ClickAsync("button");
            Assert.Equal("Clicked", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldFireContextmenuEventOnRightClick()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            await DevToolsContext.ClickAsync("#button-8", new ClickOptions { Button = MouseButton.Right });
            Assert.Equal("context menu", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('#button-8').textContent"));
        }

        // @see https://github.com/GoogleChrome/puppeteer/issues/206
        [WebView2ContextFact]
        public async Task ShouldClickLinksWhichCauseNavigation()
        {
            await DevToolsContext.SetContentAsync($"<a href=\"{TestConstants.EmptyPage}\">empty.html</a>");
            // This await should not hang.
            await DevToolsContext.ClickAsync("a");
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheButtonInsideAnIframe()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.SetContentAsync("<div style=\"width:100px;height:100px\">spacer</div>");
            await FrameUtils.AttachFrameAsync(DevToolsContext, "button-test", TestConstants.ServerUrl + "/input/button.html");
            var frame = DevToolsContext.FirstChildFrame();
            var button = await frame.QuerySelectorAsync<HtmlButtonElement>("button");
            await button.ClickAsync();
            Assert.Equal("Clicked", await frame.EvaluateExpressionAsync<string>("window.result"));
        }

        [WebView2ContextFact(Skip = "see https://github.com/GoogleChrome/puppeteer/issues/4110")]
        public async Task ShouldClickTheButtonWithFixedPositionInsideAnIframe()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.SetViewportAsync(new ViewPortOptions
            {
                Width = 500,
                Height = 500
            });
            await DevToolsContext.SetContentAsync("<div style=\"width:100px;height:2000px\">spacer</div>");
            await FrameUtils.AttachFrameAsync(DevToolsContext, "button-test", TestConstants.ServerUrl + "/input/button.html");
            var frame = DevToolsContext.FirstChildFrame();
            await frame.QuerySelectorAsync<HtmlButtonElement>("button")
                .AndThen(x => x.EvaluateFunctionAsync("button => button.style.setProperty('position', 'fixed')"));
            await frame.ClickAsync("button");
            Assert.Equal("Clicked", await frame.EvaluateExpressionAsync<string>("window.result"));
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheButtonWithDeviceScaleFactorSet()
        {
            await DevToolsContext.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 400, DeviceScaleFactor = 5 });
            Assert.Equal(5, await DevToolsContext.EvaluateExpressionAsync<int>("window.devicePixelRatio"));
            await DevToolsContext.SetContentAsync("<div style=\"width:100px;height:100px\">spacer</div>");
            await FrameUtils.AttachFrameAsync(DevToolsContext, "button-test", TestConstants.ServerUrl + "/input/button.html");
            var frame = DevToolsContext.FirstChildFrame();
            var button = await frame.QuerySelectorAsync<HtmlButtonElement>("button");
            await button.ClickAsync();
            Assert.Equal("Clicked", await frame.EvaluateExpressionAsync<string>("window.result"));
        }
    }
}
