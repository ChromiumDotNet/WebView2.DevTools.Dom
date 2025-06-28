using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Input;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Inherits from <see cref="RemoteHandle"/>. It represents an in-page DOM element.
    /// ElementHandles can be created by <see cref="WebView2DevToolsContext.QuerySelectorAsync(string)"/> or <see cref="WebView2DevToolsContext.QuerySelectorAllAsync(string)"/>.
    /// </summary>
    public partial class HtmlElement
        : Element
    {
        private readonly ILogger<HtmlElement> _logger;
        private readonly WebView2DevToolsContext _devToolsContext;
        private readonly FrameManager _frameManager;

        internal HtmlElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
            _logger = loggerFactory.CreateLogger<HtmlElement>();
            _devToolsContext = devToolsContext;
            _frameManager = frameManager;
        }

        /// <summary>
        /// Uploads files
        /// </summary>
        /// <param name="filePaths">Sets the value of the file input to these paths. Paths are resolved using <see cref="Path.GetFullPath(string)"/></param>
        /// <remarks>This method expects <c>elementHandle</c> to point to an <c>input element</c> <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input"/> </remarks>
        /// <returns>Task</returns>
        public Task UploadFileAsync(params string[] filePaths) => UploadFileAsync(true, filePaths);

        /// <summary>
        /// Calls <c>focus</c> <see href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/focus"/> on the element.
        /// </summary>
        /// <param name="preventScroll">
        /// A Boolean value indicating whether or not the browser should scroll the document to bring the newly-focused element
        /// into view. A value of false for preventScroll (the default) means that the browser will scroll the element into view
        /// after focusing it. If preventScroll is set to true, no scrolling will occur.
        /// </param>
        /// <returns>Task</returns>
        public Task FocusAsync(bool preventScroll) => EvaluateFunctionInternalAsync("(e, prevent) => e.focus({preventScroll:prevent})", preventScroll);

        /// <summary>
        /// Focuses the element, and sends a <c>keydown</c>, <c>keypress</c>/<c>input</c>, and <c>keyup</c> event for each character in the text.
        /// </summary>
        /// <param name="text">A text to type into a focused element</param>
        /// <param name="options">type options</param>
        /// <remarks>
        /// To press a special key, like <c>Control</c> or <c>ArrowDown</c> use <see cref="HtmlElement.PressAsync(string, PressOptions)"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// elementHandle.TypeAsync("#mytextarea", "Hello"); // Types instantly
        /// elementHandle.TypeAsync("#mytextarea", "World", new TypeOptions { Delay = 100 }); // Types slower, like a user
        /// ]]>
        /// </code>
        /// An example of typing into a text field and then submitting the form:
        /// <code>
        /// <![CDATA[
        /// var elementHandle = await devtoolsContext.QuerySelectorAsync("input");
        /// await elementHandle.TypeAsync("some text");
        /// await elementHandle.PressAsync("Enter");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public async Task TypeAsync(string text, TypeOptions options = null)
        {
            await FocusAsync(false).ConfigureAwait(true);
            await _devToolsContext.Keyboard.TypeAsync(text, options).ConfigureAwait(true);
        }

        /// <summary>
        /// Scrolls element into view if needed, and then uses <see cref="Touchscreen.TapAsync(double, double)"/> to tap in the center of the element.
        /// </summary>
        /// <exception cref="WebView2DevToolsContextException">if the element is detached from DOM</exception>
        /// <returns>Task which resolves when the element is successfully tapped</returns>
        public async Task TapAsync()
        {
            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await _devToolsContext.Touchscreen.TapAsync(x, y).ConfigureAwait(true);
        }

        /// <summary>
        /// Focuses the element, and then uses <see cref="Keyboard.DownAsync(string, DownOptions)"/> and <see cref="Keyboard.UpAsync(string)"/>.
        /// </summary>
        /// <param name="key">Name of key to press, such as <c>ArrowLeft</c>. See <see cref="KeyDefinitions"/> for a list of all key names.</param>
        /// <param name="options">press options</param>
        /// <remarks>
        /// If <c>key</c> is a single character and no modifier keys besides <c>Shift</c> are being held down, a <c>keypress</c>/<c>input</c> event will also be generated. The <see cref="DownOptions.Text"/> option can be specified to force an input event to be generated.
        /// </remarks>
        /// <returns></returns>
        public async Task PressAsync(string key, PressOptions options = null)
        {
            await FocusAsync(false).ConfigureAwait(true);
            await _devToolsContext.Keyboard.PressAsync(key, options).ConfigureAwait(true);
        }

        /// <summary>
        /// Scrolls element into view if needed, and then uses <see cref="WebView2DevToolsContext.Mouse"/> to hover over the center of the element.
        /// </summary>
        /// <returns>Task which resolves when the element is successfully hovered</returns>
        public async Task HoverAsync()
        {
            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await _devToolsContext.Mouse.MoveAsync(x, y).ConfigureAwait(true);
        }

        /// <summary>
        /// Scrolls element into view if needed, and then uses <see cref="WebView2DevToolsContext.Mouse"/> to click in the center of the element.
        /// </summary>
        /// <param name="options">click options</param>
        /// <exception cref="WebView2DevToolsContextException">if the element is detached from DOM</exception>
        /// <returns>Task which resolves when the element is successfully clicked</returns>
        public async Task ClickAsync(ClickOptions options = null)
        {
            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await _devToolsContext.Mouse.ClickAsync(x, y, options).ConfigureAwait(true);
        }

        /// <summary>
        /// Uploads files
        /// </summary>
        /// <param name="resolveFilePaths">Set to true to resolve paths using <see cref="Path.GetFullPath(string)"/></param>
        /// <param name="filePaths">Sets the value of the file input to these paths. Paths are resolved using <see cref="Path.GetFullPath(string)"/></param>
        /// <remarks>This method expects <c>elementHandle</c> to point to an <c>input element</c> <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input"/> </remarks>
        /// <returns>Task</returns>
        public async Task UploadFileAsync(bool resolveFilePaths, params string[] filePaths)
        {
            var isMultiple = await EvaluateFunctionInternalAsync<bool>("element => element.multiple").ConfigureAwait(true);

            if (!isMultiple && (filePaths != null && filePaths.Length > 1))
            {
                throw new WebView2DevToolsContextException("Multiple file uploads only work with <input type=file multiple>");
            }

            var node = await DevToolsProtocolHelper.DOM.DescribeNodeAsync(objectId: RemoteObject.ObjectId).ConfigureAwait(true);
            var backendNodeId = node.BackendNodeId;

            if (!filePaths.Any() || filePaths == null)
            {
                await EvaluateFunctionInternalAsync(@"(element) => {
                    element.files = new DataTransfer().files;

                    // Dispatch events for this case because it should behave akin to a user action.
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                    element.dispatchEvent(new Event('change', { bubbles: true }));
                }").ConfigureAwait(true);
            }
            else
            {
                var files = resolveFilePaths ? filePaths.Select(Path.GetFullPath).ToArray() : filePaths;
                CheckForFileAccess(files);
                await DevToolsProtocolHelper.DOM.SetFileInputFilesAsync(
                    files,
                    objectId: RemoteObject.ObjectId,
                    backendNodeId: backendNodeId).ConfigureAwait(true);
            }
        }

        private void CheckForFileAccess(string[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Open(file, FileMode.Open).Dispose();
                }
                catch (Exception ex)
                {
                    throw new WebView2DevToolsContextException($"{files} does not exist or is not readable", ex);
                }
            }
        }

        /// <summary>
        /// Invokes a member function (method).
        /// </summary>
        /// <param name="memberFunctionName">case sensitive member function name</param>
        /// <returns>Task which resolves when member (method).</returns>
        public Task InvokeMemberAsync(string memberFunctionName)
        {
            return EvaluateFunctionInternalAsync($"(element) => element.{memberFunctionName}()");
        }

        /// <summary>
        /// Evaluates the XPath expression relative to the elementHandle. If there's no such element, the method will resolve to <c>null</c>.
        /// </summary>
        /// <param name="expression">Expression to evaluate <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/evaluate"/></param>
        /// <returns>Task which resolves to an array of <see cref="HtmlElement"/></returns>
        public async Task<HtmlElement[]> XPathAsync(string expression)
        {
            var arrayHandle = await ExecutionContext.EvaluateFunctionHandleAsync<JavascriptHandle>(
                @"(element, expression) => {
                    const document = element.ownerDocument || element;
                    const iterator = document.evaluate(expression, element, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE);
                    const array = [];
                    let item;
                    while ((item = iterator.iterateNext()))
                        array.push(item);
                    return array;
                }",
                this,
                expression).ConfigureAwait(true);
            var properties = await arrayHandle.GetArray<HtmlElement>().ConfigureAwait(true);
            await arrayHandle.DisposeAsync().ConfigureAwait(true);

            return properties.ToArray();
        }

        /// <summary>
        /// This method returns the bounding box of the element (relative to the main frame),
        /// or null if the element is not visible.
        /// </summary>
        /// <returns>The BoundingBox task.</returns>
        public async Task<BoundingBox> BoundingBoxAsync()
        {
            DOM.BoxModel boxModel = null;

            try
            {
                // Improve this when https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609 is resolved.
                boxModel = await DevToolsProtocolHelper.DOM.GetBoxModelAsync(objectId: RemoteObject.ObjectId).ConfigureAwait(true);
            }
            catch (Exception)
            {
            }

            if (boxModel == null)
            {
                return null;
            }

            var quad = boxModel.Border;

            var x = new[] { quad[0], quad[2], quad[4], quad[6] }.Min();
            var y = new[] { quad[1], quad[3], quad[5], quad[7] }.Min();
            var width = new[] { quad[0], quad[2], quad[4], quad[6] }.Max() - x;
            var height = new[] { quad[1], quad[3], quad[5], quad[7] }.Max() - y;

            return new BoundingBox(x, y, width, height);
        }

        /// <summary>
        /// returns boxes of the element, or <c>null</c> if the element is not visible. Box points are sorted clock-wise.
        /// </summary>
        /// <returns>Task BoxModel task.</returns>
        public async Task<BoxModel> BoxModelAsync()
        {
            DOM.BoxModel boxModel = null;

            try
            {
                // Improve this when https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609 is resolved.
                boxModel = await DevToolsProtocolHelper.DOM.GetBoxModelAsync(objectId: RemoteObject.ObjectId).ConfigureAwait(true);
            }
            catch (Exception)
            {
            }

            if (boxModel == null)
            {
                return null;
            }

            return new BoxModel
                {
                    Content = FromProtocolQuad(boxModel.Content),
                    Padding = FromProtocolQuad(boxModel.Padding),
                    Border = FromProtocolQuad(boxModel.Border),
                    Margin = FromProtocolQuad(boxModel.Margin),
                    Width = boxModel.Width,
                    Height = boxModel.Height
                };
        }

        /// <summary>
        /// Content frame for element handles referencing iframe nodes, or null otherwise.
        /// </summary>
        /// <returns>Resolves to the content frame</returns>
        public async Task<Frame> ContentFrameAsync()
        {
            var node = await DevToolsProtocolHelper.DOM.DescribeNodeAsync(objectId: RemoteObject.ObjectId).ConfigureAwait(true);

            if (string.IsNullOrEmpty(node.FrameId))
            {
                return null;
            }

            return _frameManager.GetFrames().FirstOrDefault(x => x.Id == node.FrameId);
        }

        /// <summary>
        /// Evaluates if the element is visible in the current viewport.
        /// </summary>
        /// <returns>A task which resolves to true if the element is visible in the current viewport.</returns>
        public Task<bool> IsIntersectingViewportAsync()
            => ExecutionContext.EvaluateFunctionAsync<bool>(
                @"async element =>
                {
                    const visibleRatio = await new Promise(resolve =>
                    {
                        const observer = new IntersectionObserver(entries =>
                        {
                            resolve(entries[0].intersectionRatio);
                            observer.disconnect();
                        });
                        observer.observe(element);
                    });
                    return visibleRatio > 0;
                }",
                this);

        /// <summary>
        /// Set DOM Element Property. e.g innerText
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="val">value</param>
        /// <returns>Task</returns>
        internal Task SetPropertyValueAsync(string propertyName, object val)
        {
            return EvaluateFunctionInternalAsync("(element, v) => { element." + propertyName + " = v; }", val);
        }

        /// <summary>
        /// The outerText property of the HTMLElement interface returns the same value as HTMLElement.innerText.
        /// </summary>
        /// <returns>The rendered text content of a node and its descendants.</returns>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/outerText"/>
        public Task<string> GetOuterTextAsync()
        {
            return EvaluateFunctionInternalAsync<string>("(element) => { return element.outerText; }");
        }

        /// <summary>
        /// The innerText property of the HTMLElement interface represents the rendered text content of a node and its descendants.
        /// As a getter, it approximates the text the user would get if they highlighted the contents of the element with the cursor and then copied it to the clipboard.
        /// </summary>
        /// <returns>The rendered text content of a node and its descendants.</returns>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/innerText"/>
        public Task<string> GetInnerTextAsync()
        {
            return EvaluateFunctionInternalAsync<string>("(element) => { return element.innerText; }");
        }

        /// <summary>
        /// Sets the innerText property of the HTMLElement.
        /// As a setter this will replace the element's children with the given value, converting any line breaks into br elements.
        /// </summary>
        /// <param name="innerText">inner Text</param>
        /// <returns>A Task that when awaited sets the innerText</returns>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/innerText"/>
        public Task SetInnerTextAsync(string innerText)
        {
            return SetPropertyValueAsync("innerText", innerText);
        }

        /// <summary>
        /// Sets the outerText property of the HTMLElement.
        /// Replaces the whole current node with the given text (this differs from innerText, which replaces the content inside the current node).
        /// </summary>
        /// <param name="outerText">outer Text</param>
        /// <returns>A Task that when awaited sets the innerText</returns>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/outerText"/>
        public Task SetOuterTextAsync(string outerText)
        {
            return SetPropertyValueAsync("outerText", outerText);
        }

        /// <summary>
        /// returns the inline style of the element in the form of a <see cref="CssStyleDeclaration"/>
        /// object that contains a list of all styles properties for that element with values assigned
        /// for the attributes that are defined in the element's inline style attribute.
        /// </summary>
        /// <returns>A Task when awaited returns the inline style of the element.</returns>
        public async Task<CssStyleDeclaration> GetStyleAsync()
        {
            var handle = await EvaluateFunctionHandleInternalAsync<CssStyleDeclaration>(
                @"(object, propertyName) => {
                    return object[propertyName];
                }",
                "style").ConfigureAwait(true);

            return handle;
        }

        /// <summary>
        /// Adds a node to the end of the list of children
        /// </summary>
        /// <param name="htmlElement">html element</param>
        /// <returns>Task</returns>
        public Task AppendChildAsync(HtmlElement htmlElement)
        {
            return EvaluateFunctionInternalAsync("(e, aChild) => { e.appendChild(aChild); }", htmlElement);
        }

        /// <summary>
        /// Removes a child node from the DOM and returns the removed node.
        /// </summary>
        /// <param name="aChild">A Node that is the child node to be removed from the DOM.</param>
        /// <returns>Task</returns>
        public Task RemoveChildAsync(HtmlElement aChild)
        {
            return EvaluateFunctionInternalAsync("(e, aChild) => { e.removeChild(aChild); }", aChild);
        }

        /// <summary>
        /// Inserts a node before a reference node as a child
        /// </summary>
        /// <param name="newNode">The node to be inserted.</param>
        /// <param name="referenceNode">The node before which newNode is inserted. If this is null, then newNode is inserted at the end of node's child nodes.</param>
        /// <returns>Task</returns>
        public Task InsertBeforeAsync(HtmlElement newNode, HtmlElement referenceNode)
        {
            return EvaluateFunctionHandleInternalAsync<RemoteHandle>("(e, newNode, referenceNode) => { e.insertBefore(newNode, referenceNode); }", newNode, referenceNode);
        }

        /// <summary>
        /// Replaces a child node within the given (parent) node.
        /// </summary>
        /// <param name="newNode">The new node to replace oldChild</param>
        /// <param name="oldChild">The child to be replaced.</param>
        /// <returns>Task</returns>
        public Task ReplaceChildAsync(HtmlElement newNode, HtmlElement oldChild)
        {
            return EvaluateFunctionHandleInternalAsync<RemoteHandle>("(e, newNode, oldChild) => { e.replaceChild(newNode, oldChild); }", newNode, oldChild);
        }

        /// <summary>
        /// Scrolls the specified rect of the given node into view if not already visible.
        /// </summary>
        /// <param name="rect">
        /// The rect to be scrolled into view, relative to the node's border box, in CSS pixels. When omitted,
        /// center of the node will be used, similar to Element.scrollIntoView.
        /// </param>
        /// <returns>Task</returns>
        public async Task ScrollIntoViewIfNeededAsync(DOM.Rect rect = null)
        {
            var errorMessage = await EvaluateFunctionInternalAsync<string>(
                @"(element) => {
                    if (!element.isConnected)
                        return 'Node is detached from document';
                    if (element.nodeType !== Node.ELEMENT_NODE)
                        return 'Node is not of type HTMLElement';
                    if(!element.offsetParent)
                        return 'HTMLElement is not visible';
                    return null;
                }").ConfigureAwait(true);

            if (errorMessage != null)
            {
                throw new WebView2DevToolsContextException(errorMessage);
            }

            try
            {
                await DevToolsProtocolHelper.DOM.ScrollIntoViewIfNeededAsync(objectId: RemoteObject.ObjectId, rect: rect).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                throw new WebView2DevToolsContextException("Unable to scroll, check that you element is attached to the document and is visible.", ex);
            }
        }

        private async Task<(double x, double y)> ClickablePointAsync()
        {
            double[][] result = null;

            var contentQuadsTask = DevToolsProtocolHelper.DOM.GetContentQuadsAsync(objectId: RemoteObject.ObjectId);

            var layoutTask = DevToolsProtocolHelper.Page.GetLayoutMetricsAsync();

            try
            {
                await Task.WhenAll(contentQuadsTask, layoutTask).ConfigureAwait(true);
                result = contentQuadsTask.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get content quads");
            }

            if (result == null || result.Length == 0)
            {
                throw new WebView2DevToolsContextException("Node is either not visible or not an HTMLElement");
            }

            // Filter out quads that have too small area to click into.
            var quads = result
                .Select(FromProtocolQuad)
                .Select(q => IntersectQuadWithViewport(q, layoutTask.Result))
                .Where(q => ComputeQuadArea(q.ToArray()) > 1);

            if (!quads.Any())
            {
                throw new WebView2DevToolsContextException("Node is either not visible or not an HTMLElement");
            }

            // Return the middle point of the first quad.
            var quad = quads.First();
            var x = 0d;
            var y = 0d;

            foreach (var point in quad)
            {
                x += point.X;
                y += point.Y;
            }

            return (
                x: x / 4,
                y: y / 4);
        }

        private IEnumerable<BoxModelPoint> IntersectQuadWithViewport(IEnumerable<BoxModelPoint> quad, Page.GetLayoutMetricsReturnType viewport)
            => quad.Select(point => new BoxModelPoint
            {
                X = Math.Min(Math.Max(point.X, 0), viewport.ContentSize.Width),
                Y = Math.Min(Math.Max(point.Y, 0), viewport.ContentSize.Height),
            });

        private async Task ScrollIntoViewIfNeededAsync()
        {
            var errorMessage = await EvaluateFunctionInternalAsync<string>(
                @"async(element, pageJavascriptEnabled) => {
                    if (!element.isConnected)
                        return 'Node is detached from document';
                    if (element.nodeType !== Node.ELEMENT_NODE)
                        return 'Node is not of type HTMLElement';
                    // force-scroll if page's javascript is disabled.
                    if (!pageJavascriptEnabled) {
                        element.scrollIntoView({block: 'center', inline: 'center', behavior: 'instant'});
                        return null;
                    }
                    const visibleRatio = await new Promise(resolve => {
                    const observer = new IntersectionObserver(entries => {
                        resolve(entries[0].intersectionRatio);
                        observer.disconnect();
                    });
                    observer.observe(element);
                    });
                    if (visibleRatio !== 1.0)
                        element.scrollIntoView({block: 'center', inline: 'center', behavior: 'instant'});
                    return null;
                }",
                _devToolsContext.JavascriptEnabled).ConfigureAwait(true);

            if (errorMessage != null)
            {
                throw new WebView2DevToolsContextException(errorMessage);
            }
        }

        private double ComputeQuadArea(BoxModelPoint[] quad)
        {
            var area = 0d;
            for (var i = 0; i < quad.Length; ++i)
            {
                var p1 = quad[i];
                var p2 = quad[(i + 1) % quad.Length];
                area += ((p1.X * p2.Y) - (p2.X * p1.Y)) / 2;
            }
            return Math.Abs(area);
        }

        /*
        /// <summary>
        /// This method creates and captures a dragevent from the element.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser with the drag data</returns>
        public async Task<DragData> DragAsync(decimal x, decimal y)
        {
            if (!Page.IsDragInterceptionEnabled)
            {
                throw new WebView2DevToolsContextException("Drag Interception is not enabled!");
            }

            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var start = await ClickablePointAsync().ConfigureAwait(true);
            return await Page.Mouse.DragAsync(start.x, start.y, x, y).ConfigureAwait(true);
        }

        /// <summary>
        /// Dispatches a `dragenter` event.
        /// </summary>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public async Task DragEnterAsync(DragData data)
        {
            if (!Page.IsDragInterceptionEnabled)
            {
                throw new WebView2DevToolsContextException("Drag Interception is not enabled!");
            }

            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await Page.Mouse.DragEnterAsync(x, y, data).ConfigureAwait(true);
        }

        /// <summary>
        /// Dispatches a `dragover` event.
        /// </summary>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public async Task DragOverAsync(DragData data)
        {
            if (!Page.IsDragInterceptionEnabled)
            {
                throw new WebView2DevToolsContextException("Drag Interception is not enabled!");
            }

            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await Page.Mouse.DragOverAsync(x, y, data).ConfigureAwait(true);
        }

        /// <summary>
        /// Performs a dragenter, dragover, and drop in sequence.
        /// </summary>
        /// <param name="data">Drag data containing items and operations mask.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public async Task DropAsync(DragData data)
        {
            if (!Page.IsDragInterceptionEnabled)
            {
                throw new WebView2DevToolsContextException("Drag Interception is not enabled!");
            }

            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            await Page.Mouse.DropAsync(x, y, data).ConfigureAwait(true);
        }

        /// <summary>
        /// Performs a drag, dragenter, dragover, and drop in sequence.
        /// </summary>
        /// <param name="target">Target element</param>
        /// <param name="delay">If specified, is the time to wait between `dragover` and `drop` in milliseconds.</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public async Task DragAndDropAsync(ElementHandle target, int delay = 0)
        {
            if (target == null)
            {
                throw new ArgumentException("Target cannot be null", nameof(target));
            }

            if (!Page.IsDragInterceptionEnabled)
            {
                throw new WebView2DevToolsContextException("Drag Interception is not enabled!");
            }

            await ScrollIntoViewIfNeededAsync().ConfigureAwait(true);
            var (x, y) = await ClickablePointAsync().ConfigureAwait(true);
            var targetPoint = await target.ClickablePointAsync().ConfigureAwait(true);
            await Page.Mouse.DragAndDropAsync(x, y, targetPoint.x, targetPoint.y, delay).ConfigureAwait(true);
        }
        */
    }
}
