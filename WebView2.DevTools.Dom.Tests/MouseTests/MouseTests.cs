using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Input;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.MouseTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class MouseTests : DevTooolsContextBaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private const string Dimensions = @"function dimensions() {
            const rect = document.querySelector('textarea').getBoundingClientRect();
            return {
                x: rect.left,
                y: rect.top,
                width: rect.width,
                height: rect.height
            };
        }";

        public MouseTests(ITestOutputHelper output) : base(output)
        {
            _testOutputHelper = output;
        }

        [WebView2ContextFact]
        public async Task ShouldClickTheDocument()
        {
            await DevToolsContext.EvaluateFunctionAsync(@"() => {
                globalThis.clickPromise = new Promise((resolve) => {
                    document.addEventListener('click', (event) => {
                    resolve({
                        type: event.type,
                        detail: event.detail,
                        clientX: event.clientX,
                        clientY: event.clientY,
                        isTrusted: event.isTrusted,
                        button: event.button,
                    });
                    });
                });
            }");
            await DevToolsContext.Mouse.ClickAsync(50, 60);
            var e = await DevToolsContext.EvaluateFunctionAsync<MouseEvent>("() => globalThis.clickPromise");

            Assert.Equal("click", e.Type);
            Assert.Equal(1, e.Detail);
            Assert.Equal(50, e.ClientX);
            Assert.Equal(60, e.ClientY);
            Assert.True(e.IsTrusted);
            Assert.Equal(0, e.Button);
        }

        [WebView2ContextFact]
        public async Task ShouldResizeTheTextarea()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");

            var dimensions = await DevToolsContext.EvaluateFunctionAsync<Dimensions>(Dimensions);
            var mouse = DevToolsContext.Mouse;
            await mouse.MoveAsync(dimensions.X + dimensions.Width - 4, dimensions.Y + dimensions.Height - 4);
            await mouse.DownAsync();
            await mouse.MoveAsync(dimensions.X + dimensions.Width + 100, dimensions.Y + dimensions.Height + 100);
            await mouse.UpAsync();
            var newDimensions = await DevToolsContext.EvaluateFunctionAsync<Dimensions>(Dimensions);
            Assert.Equal(Math.Round(dimensions.Width + 104, MidpointRounding.AwayFromZero), newDimensions.Width);
            Assert.Equal(Math.Round(dimensions.Height + 104, MidpointRounding.AwayFromZero), newDimensions.Height);
        }

        [WebView2ContextFact]
        public async Task ShouldSelectTheTextWithMouse()
        {
            const string expectedText = "This is the text that we are going to try to select. Let's see how it goes.";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.FocusAsync("textarea");
            
            await DevToolsContext.Keyboard.TypeAsync(expectedText);
            await DevToolsContext.EvaluateExpressionAsync("document.querySelector('textarea').scrollTop = 0");
            var dimensions = await DevToolsContext.EvaluateFunctionAsync<Dimensions>(Dimensions);
            await DevToolsContext.Mouse.MoveAsync(dimensions.X + 2, dimensions.Y + 2);
            await DevToolsContext.Mouse.DownAsync();
            await DevToolsContext.Mouse.MoveAsync(dimensions.Width, dimensions.Height + 20);
            await DevToolsContext.Mouse.UpAsync();
            var actualTest = await DevToolsContext.EvaluateFunctionAsync<string>(@"() => {
                const textarea = document.querySelector('textarea');
                return textarea.value.substring(textarea.selectionStart, textarea.selectionEnd);
            }");
            Assert.Equal(expectedText, actualTest);
        }

        [WebView2ContextFact]
        public async Task ShouldTriggerHoverState()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            await DevToolsContext.HoverAsync("#button-6");
            Assert.Equal("button-6", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('button:hover').id"));
            await DevToolsContext.HoverAsync("#button-2");
            Assert.Equal("button-2", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('button:hover').id"));
            await DevToolsContext.HoverAsync("#button-91");
            Assert.Equal("button-91", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('button:hover').id"));
        }

        [WebView2ContextFact]
        public async Task ShouldTriggerHoverStateWithRemovedWindowNode()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            await DevToolsContext.EvaluateExpressionAsync("delete window.Node");
            await DevToolsContext.HoverAsync("#button-6");
            Assert.Equal("button-6", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('button:hover').id"));
        }

        [WebView2ContextFact]
        public async Task ShouldSetModifierKeysOnClick()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/scrollable.html");
            await DevToolsContext.EvaluateExpressionAsync("document.querySelector('#button-3').addEventListener('mousedown', e => window.lastEvent = e, true)");
            var modifiers = new Dictionary<string, string> { ["Shift"] = "shiftKey", ["Control"] = "ctrlKey", ["Alt"] = "altKey", ["Meta"] = "metaKey" };
            foreach (var modifier in modifiers)
            {
                await DevToolsContext.Keyboard.DownAsync(modifier.Key);
                await DevToolsContext.ClickAsync("#button-3");
                if (!(await DevToolsContext.EvaluateFunctionAsync<bool>("mod => window.lastEvent[mod]", modifier.Value)))
                {
                    Assert.Fail($"{modifier.Value} should be true");
                }

                await DevToolsContext.Keyboard.UpAsync(modifier.Key);
            }
            await DevToolsContext.ClickAsync("#button-3");
            foreach (var modifier in modifiers)
            {
                if (await DevToolsContext.EvaluateFunctionAsync<bool>("mod => window.lastEvent[mod]", modifier.Value))
                {
                    Assert.Fail($"{modifiers.Values} should be false");
                }
            }
        }

        [WebView2ContextFact]
        public async Task ShouldSendMouseWheelEvents()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/wheel.html");
            var elem = await DevToolsContext.QuerySelectorAsync<HtmlDivElement>("div");
            var boundingBoxBefore = await elem.BoundingBoxAsync();
            Assert.Equal(115, boundingBoxBefore.Width);
            Assert.Equal(115, boundingBoxBefore.Height);

            await DevToolsContext.Mouse.MoveAsync(
                boundingBoxBefore.X + (boundingBoxBefore.Width / 2),
                boundingBoxBefore.Y + (boundingBoxBefore.Height / 2)
            );

            await DevToolsContext.Mouse.WheelAsync(0, -100);
            var boundingBoxAfter = await elem.BoundingBoxAsync();
            Assert.Equal(230, boundingBoxAfter.Width);
            Assert.Equal(230, boundingBoxAfter.Height);
        }

        [WebView2ContextFact]
        public async Task ShouldTweenMouseMovement()
        {
            await DevToolsContext.Mouse.MoveAsync(100, 100);
            await DevToolsContext.EvaluateExpressionAsync(@"{
                window.result = [];
                document.addEventListener('mousemove', event => {
                    window.result.push([event.clientX, event.clientY]);
                });
            }");
            await DevToolsContext.Mouse.MoveAsync(200, 300, new MoveOptions { Steps = 5 });
            Assert.Equal(new[] {
                new[]{ 120, 140 },
                new[]{ 140, 180 },
                new[]{ 160, 220 },
                new[]{ 180, 260 },
                new[]{ 200, 300 }
            }, await DevToolsContext.EvaluateExpressionAsync<int[][]>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithMobileViewportsAndCrossProcessNavigations()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await DevToolsContext.SetViewportAsync(new ViewPortOptions
            {
                Width = 360,
                Height = 640,
                IsMobile = true
            });
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.CrossProcessUrl + "/mobile.html");
            await WebView.CoreWebView2.WaitForRenderIdle();

            await DevToolsContext.EvaluateFunctionAsync(@"() => {
                document.addEventListener('click', event => {
                    window.result = { x: event.clientX, y: event.clientY };
                });
            }");

            await DevToolsContext.Mouse.ClickAsync(30, 40);

            Assert.Equal(new DomPointInternal()
            {
                X = 30,
                Y = 40
            }, await DevToolsContext.EvaluateExpressionAsync<DomPointInternal>("result"));
        }

        internal struct WheelEventInternal
        {
            public WheelEventInternal(decimal deltaX, decimal deltaY)
            {
                DeltaX = deltaX;
                DeltaY = deltaY;
            }

            public decimal DeltaX { get; set; }

            public decimal DeltaY { get; set; }

            public override string ToString() => $"({DeltaX}, {DeltaY})";
        }

        internal struct DomPointInternal
        {
            public decimal X { get; set; }

            public decimal Y { get; set; }

            public override string ToString() => $"({X}, {Y})";

            public void Scroll(decimal deltaX, decimal deltaY)
            {
                X = Math.Max(0, X + deltaX);
                Y = Math.Max(0, Y + deltaY);
            }
        }

        internal struct MouseEvent
        {
            public string Type { get; set; }

            public int Detail { get; set; }

            public int ClientX { get; set; }

            public int ClientY { get; set; }

            public bool IsTrusted { get; set; }

            public int Button { get; set; }
        }
    }
}
