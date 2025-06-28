using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Helpers;

namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// Provides methods to interact with the mouse
    /// </summary>
    public class Mouse
    {
        private readonly DevToolsProtocolHelper _client;
        private readonly Keyboard _keyboard;

        private double _x = 0;
        private double _y = 0;
        private MouseButton _button = MouseButton.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class.
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="keyboard">The keyboard</param>
        public Mouse(DevToolsProtocolHelper client, Keyboard keyboard)
        {
            _client = client;
            _keyboard = keyboard;
        }

        /// <summary>
        /// Dispatches a <c>mousemove</c> event.
        /// </summary>
        /// <param name="x">The destination mouse X coordinate.</param>
        /// <param name="y">The destination mouse Y coordinate.</param>
        /// <param name="options">Options to apply to the move operation.</param>
        /// <returns>Task</returns>
        public async Task MoveAsync(double x, double y, MoveOptions options = null)
        {
            var steps = options?.Steps ?? MoveOptions.DefaultSteps;

            if (steps == 1)
            {
                _x = x;
                _y = y;

                await _client.Input.DispatchMouseEventAsync(
                        type: "mouseMoved",
                        button: _button.ToLowerInvariant(),
                        x: x,
                        y: y,
                        modifiers: _keyboard.Modifiers).ConfigureAwait(true);
            }
            else
            {
                var fromX = _x;
                var fromY = _y;
                _x = x;
                _y = y;

                for (var i = 1; i <= steps; i++)
                {
                    await _client.Input.DispatchMouseEventAsync(
                        type: "mouseMoved",
                        button: _button.ToLowerInvariant(),
                        x: fromX + ((_x - fromX) * ((double)i / steps)),
                        y: fromY + ((_y - fromY) * ((double)i / steps)),
                        modifiers: _keyboard.Modifiers).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Shortcut for <see cref="MoveAsync(double, double, MoveOptions)"/>, <see cref="DownAsync(ClickOptions)"/> and <see cref="UpAsync(ClickOptions)"/>
        /// </summary>
        /// <param name="x">The target mouse X location to click.</param>
        /// <param name="y">The target mouse Y location to click.</param>
        /// <param name="options">Options to apply to the click operation.</param>
        /// <returns>Task</returns>
        public async Task ClickAsync(double x, double y, ClickOptions options = null)
        {
            options = options ?? new ClickOptions();

            if (options.Delay > 0)
            {
                await Task.WhenAll(
                    MoveAsync(x, y),
                    DownAsync(options)).ConfigureAwait(true);

                await Task.Delay(options.Delay).ConfigureAwait(true);
                await UpAsync(options).ConfigureAwait(true);
            }
            else
            {
                await Task.WhenAll(
                   MoveAsync(x, y),
                   DownAsync(options),
                   UpAsync(options)).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Dispatches a <c>mousedown</c> event.
        /// </summary>
        /// <param name="options">Options to apply to the mouse down operation.</param>
        /// <returns>Task</returns>
        public Task DownAsync(ClickOptions options = null)
        {
            options = options ?? new ClickOptions();

            _button = options.Button;

            return _client.Input.DispatchMouseEventAsync(
                type: "mousePressed",
                button: _button.ToLowerInvariant(),
                x: _x,
                y: _y,
                modifiers: _keyboard.Modifiers,
                clickCount: options.ClickCount);
        }

        /// <summary>
        /// Dispatches a <c>mouseup</c> event.
        /// </summary>
        /// <param name="options">Options to apply to the mouse up operation.</param>
        /// <returns>Task</returns>
        public Task UpAsync(ClickOptions options = null)
        {
            options = options ?? new ClickOptions();

            _button = MouseButton.None;

            return _client.Input.DispatchMouseEventAsync(
                type: "mouseReleased",
                button: options.Button.ToLowerInvariant(),
                x: _x,
                y: _y,
                modifiers: _keyboard.Modifiers,
                clickCount: options.ClickCount);
        }

        /// <summary>
        /// Dispatches a <c>wheel</c> event.
        /// </summary>
        /// <param name="deltaX">Delta X.</param>
        /// <param name="deltaY">Delta Y.</param>
        /// <returns>Task</returns>
        public Task WheelAsync(double deltaX, double deltaY)
            => _client.Input.DispatchMouseEventAsync(
                    type: "mouseWheel",
                    deltaX: deltaX,
                    deltaY: deltaY,
                    x: _x,
                    y: _y,
                    modifiers: _keyboard.Modifiers,
                    pointerType: "mouse");

        /*

        /// <summary>
        /// Dispatches a `drag` event.
        /// </summary>
        /// <param name="startX">Start X coordinate</param>
        /// <param name="startY">Start Y coordinate</param>
        /// <param name="endX">End X coordinate</param>
        /// <param name="endY">End Y coordinate</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser with the drag data</returns>
        public async Task<DragData> DragAsync(double startX, double startY, double endX, double endY)
        {
            var result = new TaskCompletionSource<DragData>();

            void DragIntercepted(object sender, MessageEventArgs e)
            {
                if (e.MessageID == "Input.dragIntercepted")
                {
                    result.TrySetResult(e.MessageData.SelectToken("data").ToObject<DragData>());
                    _client.MessageReceived -= DragIntercepted;
                }
            }
            _client.MessageReceived += DragIntercepted;
            await MoveAsync(startX, startY).ConfigureAwait(true);
            await DownAsync().ConfigureAwait(true);
            await MoveAsync(endX, endY).ConfigureAwait(true);

            return await result.Task.ConfigureAwait(true);
        }

        /// <summary>
        /// Dispatches a `dragenter` event.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public Task DragEnterAsync(decimal x, decimal y, DragData data)
            => _client.Input.DispatchDragEventAsync(
                    type: DragEventType.DragEnter,
                    x: x,
                    y: y,
                    modifiers: _keyboard.Modifiers,
                    data: data
                );

        /// <summary>
        /// Dispatches a `dragover` event.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public Task DragOverAsync(double x, double y, DragData data)
            => _client.SendAsync(
                "Input.dispatchDragEvent",
                new InputDispatchDragEventRequest
                {
                    Type = DragEventType.DragOver,
                    X = x,
                    Y = y,
                    Modifiers = _keyboard.Modifiers,
                    Data = data,
                });

        /// <summary>
        /// Dispatches a `drop` event.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public Task DropAsync(double x, double y, DragData data)
            => _client.SendAsync(
                "Input.dispatchDragEvent",
                new InputDispatchDragEventRequest
                {
                    Type = DragEventType.Drop,
                    X = x,
                    Y = y,
                    Modifiers = _keyboard.Modifiers,
                    Data = data,
                });

        /// <summary>
        /// Performs a drag, dragenter, dragover, and drop in sequence.
        /// </summary>
        /// <param name="startX">Start X coordinate</param>
        /// <param name="startY">Start Y coordinate</param>
        /// <param name="endX">End X coordinate</param>
        /// <param name="endY">End Y coordinate</param>
        /// <param name="delay">If specified, is the time to wait between `dragover` and `drop` in milliseconds.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public async Task DragAndDropAsync(double startX, double startY, double endX, double endY, int delay = 0)
        {
            var data = await DragAsync(startX, startY, endX, endY).ConfigureAwait(true);
            await DragEnterAsync(endX, endY, data).ConfigureAwait(true);
            await DragOverAsync(endX, endY, data).ConfigureAwait(true);

            if (delay > 0)
            {
                await Task.Delay(delay).ConfigureAwait(true);
            }
            await DropAsync(endX, endY, data).ConfigureAwait(true);
            await UpAsync().ConfigureAwait(true);
        }
        */
    }
}
