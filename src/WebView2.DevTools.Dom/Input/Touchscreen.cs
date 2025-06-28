using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Messaging;

namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// Provides methods to interact with the touch screen
    /// </summary>
    public class Touchscreen
    {
        private readonly DevToolsProtocolHelper _client;
        private readonly CoreWebView2 _coreWebView2;
        private readonly Keyboard _keyboard;

        internal Touchscreen(DevToolsProtocolHelper client, CoreWebView2 coreWebView2, Keyboard keyboard)
        {
            _client = client;
            _coreWebView2 = coreWebView2;
            _keyboard = keyboard;
        }

        /// <summary>
        /// Dispatches a <c>touchstart</c> and <c>touchend</c> event.
        /// </summary>
        /// <param name="x">The touch X location.</param>
        /// <param name="y">The touch Y location.</param>
        /// <returns>Task</returns>
        /// <seealso cref="WebView2DevToolsContext.TapAsync(string)"/>
        public async Task TapAsync(double x, double y)
        {
            // Touches appear to be lost during the first frame after navigation.
            // This waits a frame before sending the tap.
            // @see https://crbug.com/613219
            await _client.Runtime.EvaluateAsync(
                expression: "new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))",
                awaitPromise: true).ConfigureAwait(true);

            var touchStart = new InputDispatchTouchEventRequest
            {
                Type = "touchStart",
                TouchPoints = new[] { new TouchPoint { X = Math.Round(x), Y = Math.Round(y) } },
                Modifiers = _keyboard.Modifiers
            };

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(touchStart, serializeOptions);

            await _coreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchTouchEvent", json).ConfigureAwait(true);

            var touchEnd = new InputDispatchTouchEventRequest
            {
                Type = "touchEnd",
                TouchPoints = Array.Empty<TouchPoint>(),
                Modifiers = _keyboard.Modifiers
            };

            json = JsonSerializer.Serialize(touchEnd, serializeOptions);

            await _coreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchTouchEvent", json).ConfigureAwait(true);
        }
    }
}
