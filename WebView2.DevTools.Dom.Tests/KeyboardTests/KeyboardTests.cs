using System.Collections.Generic;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Input;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.KeyboardTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class KeyboardTests : DevTooolsContextBaseTest
    {
        public KeyboardTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldTypeIntoTheTextarea()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");

            var textarea = await DevToolsContext.QuerySelectorAsync("textarea");
            await textarea.TypeAsync("Type in this text!");
            Assert.Equal("Type in this text!", await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldMoveWithTheArrowKeys()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.TypeAsync("textarea", "Hello World!");
            Assert.Equal("Hello World!", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('textarea').value"));
            for (var i = 0; i < "World!".Length; i++)
            {
                _ = DevToolsContext.Keyboard.PressAsync("ArrowLeft");
            }

            await DevToolsContext.Keyboard.TypeAsync("inserted ");
            Assert.Equal("Hello inserted World!", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('textarea').value"));
            _ = DevToolsContext.Keyboard.DownAsync("Shift");
            for (var i = 0; i < "inserted ".Length; i++)
            {
                _ = DevToolsContext.Keyboard.PressAsync("ArrowLeft");
            }

            _ = DevToolsContext.Keyboard.UpAsync("Shift");
            await DevToolsContext.Keyboard.PressAsync("Backspace");
            Assert.Equal("Hello World!", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('textarea').value"));
        }

        [WebView2ContextFact]
        public async Task ShouldSendACharacterWithElementHandlePress()
        {
            const string expected = "a";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            var textarea = await DevToolsContext.QuerySelectorAsync<HtmlTextAreaElement>("textarea");
            await textarea.PressAsync(expected);

            var actual = await textarea.GetValueAsync();

            Assert.Equal(expected, actual);

            await DevToolsContext.EvaluateExpressionAsync("window.addEventListener('keydown', e => e.preventDefault(), true)");

            await textarea.PressAsync("b");

            actual = await textarea.GetValueAsync();

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ElementHandlePressShouldSupportTextOption()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            var textarea = await DevToolsContext.QuerySelectorAsync<HtmlTextAreaElement>("textarea");
            await textarea.PressAsync("a", new PressOptions { Text = "ё" });
            var actual = await textarea.GetValueAsync();
            Assert.Equal("ё", actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSendACharacterWithSendCharacter()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.FocusAsync("textarea");
            await DevToolsContext.Keyboard.SendCharacterAsync("嗨");
            Assert.Equal("嗨", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('textarea').value"));
            await DevToolsContext.EvaluateExpressionAsync("window.addEventListener('keydown', e => e.preventDefault(), true)");
            await DevToolsContext.Keyboard.SendCharacterAsync("a");
            Assert.Equal("嗨a", await DevToolsContext.EvaluateExpressionAsync<string>("document.querySelector('textarea').value"));
        }

        [WebView2ContextFact]
        public async Task ShouldReportShiftKey()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/keyboard.html");
            var keyboard = DevToolsContext.Keyboard;
            var codeForKey = new Dictionary<string, int> { ["Shift"] = 16, ["Alt"] = 18, ["Control"] = 17 };
            foreach (var modifier in codeForKey)
            {
                await keyboard.DownAsync(modifier.Key);
                Assert.Equal($"Keydown: {modifier.Key} {modifier.Key}Left {modifier.Value} [{modifier.Key}]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
                await keyboard.DownAsync("!");
                // Shift+! will generate a keypress
                if (modifier.Key == "Shift")
                {
                    Assert.Equal($"Keydown: ! Digit1 49 [{modifier.Key}]\nKeypress: ! Digit1 33 33 [{modifier.Key}]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
                }
                else
                {
                    Assert.Equal($"Keydown: ! Digit1 49 [{modifier.Key}]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
                }

                await keyboard.UpAsync("!");
                Assert.Equal($"Keyup: ! Digit1 49 [{modifier.Key}]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
                await keyboard.UpAsync(modifier.Key);
                Assert.Equal($"Keyup: {modifier.Key} {modifier.Key}Left {modifier.Value} []", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            }
        }

        [WebView2ContextFact]
        public async Task ShouldReportMultipleModifiers()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/keyboard.html");
            var keyboard = DevToolsContext.Keyboard;
            await keyboard.DownAsync("Control");
            Assert.Equal("Keydown: Control ControlLeft 17 [Control]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.DownAsync("Alt");
            Assert.Equal("Keydown: Alt AltLeft 18 [Alt Control]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.DownAsync(";");
            Assert.Equal("Keydown: ; Semicolon 186 [Alt Control]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.UpAsync(";");
            Assert.Equal("Keyup: ; Semicolon 186 [Alt Control]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.UpAsync("Control");
            Assert.Equal("Keyup: Control ControlLeft 17 [Alt]", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.UpAsync("Alt");
            Assert.Equal("Keyup: Alt AltLeft 18 []", await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
        }

        [WebView2ContextFact]
        public async Task ShouldSendProperCodesWhileTyping()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/keyboard.html");
            await DevToolsContext.Keyboard.TypeAsync("!");
            Assert.Equal(string.Join("\n", new[] {
                "Keydown: ! Digit1 49 []",
                "Keypress: ! Digit1 33 33 []",
                "Keyup: ! Digit1 49 []" }), await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await DevToolsContext.Keyboard.TypeAsync("^");
            Assert.Equal(string.Join("\n", new[] {
                "Keydown: ^ Digit6 54 []",
                "Keypress: ^ Digit6 94 94 []",
                "Keyup: ^ Digit6 54 []" }), await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
        }

        [WebView2ContextFact]
        public async Task ShouldSendProperCodesWhileTypingWithShift()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/keyboard.html");
            var keyboard = DevToolsContext.Keyboard;
            await keyboard.DownAsync("Shift");
            await DevToolsContext.Keyboard.TypeAsync("~");
            Assert.Equal(string.Join("\n", new[] {
                "Keydown: Shift ShiftLeft 16 [Shift]",
                "Keydown: ~ Backquote 192 [Shift]", // 192 is ` keyCode
                "Keypress: ~ Backquote 126 126 [Shift]", // 126 is ~ charCode
                "Keyup: ~ Backquote 192 [Shift]" }), await DevToolsContext.EvaluateExpressionAsync<string>("getResult()"));
            await keyboard.UpAsync("Shift");
        }

        [WebView2ContextFact]
        public async Task ShouldNotTypeCanceledEvents()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.FocusAsync("textarea");
            await DevToolsContext.EvaluateExpressionAsync(@"{
              window.addEventListener('keydown', event => {
                event.stopPropagation();
                event.stopImmediatePropagation();
                if (event.key === 'l')
                  event.preventDefault();
                if (event.key === 'o')
                  event.preventDefault();
              }, false);
            }");
            await DevToolsContext.Keyboard.TypeAsync("Hello World!");
            Assert.Equal("He Wrd!", await DevToolsContext.EvaluateExpressionAsync<string>("textarea.value"));
        }

        [WebView2ContextFact]
        public async Task ShouldSpecifyRepeatProperty()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");

            await DevToolsContext.FocusAsync("textarea");
            await DevToolsContext.EvaluateExpressionAsync("document.querySelector('textarea').addEventListener('keydown', e => window.lastEvent = e, true)");
            await DevToolsContext.Keyboard.DownAsync("a");

            var expected = await DevToolsContext.EvaluateExpressionAsync<bool>("window.lastEvent.repeat");
            Assert.False(expected);

            await DevToolsContext.Keyboard.PressAsync("a");

            expected = await DevToolsContext.EvaluateExpressionAsync<bool>("window.lastEvent.repeat");
            Assert.True(expected);

            await DevToolsContext.Keyboard.DownAsync("b");

            expected = await DevToolsContext.EvaluateExpressionAsync<bool>("window.lastEvent.repeat");
            Assert.False(expected);
            await DevToolsContext.Keyboard.DownAsync("b");

            expected = await DevToolsContext.EvaluateExpressionAsync<bool>("window.lastEvent.repeat");
            Assert.True(expected);

            await DevToolsContext.Keyboard.UpAsync("a");
            await DevToolsContext.Keyboard.DownAsync("a");

            expected = await DevToolsContext.EvaluateExpressionAsync<bool>("window.lastEvent.repeat");

            Assert.False(expected);
        }

        [WebView2ContextFact]
        public async Task ShouldTypeAllKindsOfCharacters()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.FocusAsync("textarea");
            const string text = "This text goes onto two lines.\nThis character is 嗨.";
            await DevToolsContext.Keyboard.TypeAsync(text);
            Assert.Equal(text, await DevToolsContext.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact]
        public async Task ShouldSpecifyLocation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.EvaluateExpressionAsync(@"{
              window.addEventListener('keydown', event => window.keyLocation = event.location, true);
            }");
            var textarea = await DevToolsContext.QuerySelectorAsync<HtmlTextAreaElement>("textarea");

            await textarea.PressAsync("Digit5");
            Assert.Equal(0, await DevToolsContext.EvaluateExpressionAsync<int>("keyLocation"));

            await textarea.PressAsync("ControlLeft");
            Assert.Equal(1, await DevToolsContext.EvaluateExpressionAsync<int>("keyLocation"));

            await textarea.PressAsync("ControlRight");
            Assert.Equal(2, await DevToolsContext.EvaluateExpressionAsync<int>("keyLocation"));

            await textarea.PressAsync("NumpadSubtract");
            Assert.Equal(3, await DevToolsContext.EvaluateExpressionAsync<int>("keyLocation"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowOnUnknownKeys()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => DevToolsContext.Keyboard.PressAsync("NotARealKey"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => DevToolsContext.Keyboard.PressAsync("ё"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => DevToolsContext.Keyboard.PressAsync("😊"));
        }

        [WebView2ContextFact]
        public async Task ShouldTypeEmoji()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/textarea.html");
            await DevToolsContext.TypeAsync("textarea", "👹 Tokyo street Japan \uD83C\uDDEF\uD83C\uDDF5");
            Assert.Equal(
                "👹 Tokyo street Japan \uD83C\uDDEF\uD83C\uDDF5",
                await DevToolsContext.QuerySelectorAsync("textarea").EvaluateFunctionAsync<string>("t => t.value"));
        }

        [WebView2ContextFact]
        public async Task ShouldTypeEmojiIntoAniframe()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            await FrameUtils.AttachFrameAsync(DevToolsContext, "emoji-test", TestConstants.ServerUrl + "/input/textarea.html");
            var frame = DevToolsContext.FirstChildFrame();
            var textarea = await frame.QuerySelectorAsync<HtmlTextAreaElement>("textarea");
            await textarea.TypeAsync("👹 Tokyo street Japan \uD83C\uDDEF\uD83C\uDDF5");
            Assert.Equal(
                "👹 Tokyo street Japan \uD83C\uDDEF\uD83C\uDDF5",
                await frame.QuerySelectorAsync("textarea").EvaluateFunctionAsync<string>("t => t.value"));
        }

        [WebView2ContextFact]
        public async Task ShouldPressTheMetaKey()
        {
            await DevToolsContext.EvaluateFunctionAsync(@"() =>
            {
                window.result = null;
                document.addEventListener('keydown', event => {
                    window.result = [event.key, event.code, event.metaKey];
                });
            }");
            await DevToolsContext.Keyboard.PressAsync("Meta", PressOptions.WithDelayInMs(10));
            const int key = 0;
            const int code = 1;
            const int metaKey = 2;
            var result = await DevToolsContext.EvaluateExpressionAsync<object[]>("result");
            Assert.Equal("Meta", result[key]);
            Assert.Equal("MetaLeft", result[code]);
            Assert.True(bool.Parse(result[metaKey].ToString()));
        }
    }
}
