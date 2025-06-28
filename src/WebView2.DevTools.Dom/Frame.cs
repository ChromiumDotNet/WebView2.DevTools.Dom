using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Input;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Provides methods to interact with a single page frame in Chromium. One <see cref="WebView2DevToolsContext"/> instance might have multiple <see cref="Frame"/> instances.
    /// At every point of time, page exposes its current frame tree via the <see cref="WebView2DevToolsContext.MainFrame"/> and <see cref="ChildFrames"/> properties.
    ///
    /// <see cref="Frame"/> object's lifecycle is controlled by three events, dispatched on the page object
    /// - <see cref="WebView2DevToolsContext.FrameAttached"/> - fires when the frame gets attached to the page. A Frame can be attached to the page only once
    /// - <see cref="WebView2DevToolsContext.FrameNavigated"/> - fired when the frame commits navigation to a different URL
    /// - <see cref="WebView2DevToolsContext.FrameDetached"/> - fired when the frame gets detached from the page.  A Frame can be detached from the page only once
    /// </summary>
    /// <example>
    /// An example of dumping frame tree
    /// <code>
    /// <![CDATA[
    /// var ctx = await corewWebView.CreateDevToolsContextAsync();
    /// await ctx.GoToAsync("https://www.google.com/chrome/browser/canary.html");
    /// dumpFrameTree(ctx.MainFrame, string.Empty);
    ///
    /// void dumpFrameTree(Frame frame, string indent)
    /// {
    ///     Console.WriteLine(indent + frame.Url);
    ///     foreach (var child in frame.ChildFrames)
    ///     {
    ///         dumpFrameTree(child, indent + "  ");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class Frame
    {
        private readonly DevToolsProtocolHelper _client;
        private readonly List<Frame> _childFrames = new List<Frame>();

        /// <summary>
        /// Frame Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// returns true if Main Frame otherwise false.
        /// </summary>
        public bool IsMainFrame { get; set; }

        internal string LoaderId { get; set; }

        internal List<string> LifecycleEvents { get; }

        internal DOMWorld MainWorld { get; }

        internal DOMWorld SecondaryWorld { get; }

        internal Frame(FrameManager frameManager, DevToolsProtocolHelper client, Frame parentFrame, string frameId, bool isMainFrame)
        {
            FrameManager = frameManager;
            _client = client;
            ParentFrame = parentFrame;
            Id = frameId;
            IsMainFrame = isMainFrame;

            LifecycleEvents = new List<string>();

            MainWorld = new DOMWorld(this, FrameManager.TimeoutSettings);
            SecondaryWorld = new DOMWorld(this, FrameManager.TimeoutSettings);

            if (parentFrame != null)
            {
                ParentFrame.AddChildFrame(this);
            }
        }

        /// <summary>
        /// Gets the child frames of the this frame
        /// </summary>
        public List<Frame> ChildFrames
        {
            get
            {
                lock (_childFrames)
                {
                    return _childFrames.ToList();
                }
            }
        }

        /// <summary>
        /// Gets the frame's name attribute as specified in the tag
        /// If the name is empty, returns the id attribute instead
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the frame's url
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the frame is detached or not
        /// </summary>
        public bool Detached { get; set; }

        /// <summary>
        /// Gets the parent frame, if any. Detached frames and main frames return <c>null</c>
        /// </summary>
        public Frame ParentFrame { get; private set; }

        internal FrameManager FrameManager { get; }

        /// <summary>
        /// Sets the HTML markup to the page
        /// </summary>
        /// <param name="html">HTML markup to assign to the page.</param>
        /// <returns>Task.</returns>
        public Task SetContentAsync(string html)
        {
            return _client.Page.SetDocumentContentAsync(Id, html);
        }

        /// <summary>
        /// Gets the full HTML contents of the page, including the doctype.
        /// </summary>
        /// <returns>Task which resolves to the HTML content.</returns>
        /// <seealso cref="WebView2DevToolsContext.GetContentAsync"/>
        public Task<string> GetContentAsync() => SecondaryWorld.GetContentAsync();

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="WebView2DevToolsContext.EvaluateExpressionAsync{T}(string)"/>
        public Task<JsonElement> EvaluateExpressionAsync(string script) => MainWorld.EvaluateExpressionAsync(script);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="WebView2DevToolsContext.EvaluateExpressionAsync{T}(string)"/>
        public Task<T> EvaluateExpressionAsync<T>(string script) => MainWorld.EvaluateExpressionAsync<T>(script);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <seealso cref="WebView2DevToolsContext.EvaluateFunctionAsync{T}(string, object[])"/>
        public Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args) => MainWorld.EvaluateFunctionAsync(script, args);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <seealso cref="WebView2DevToolsContext.EvaluateFunctionAsync{T}(string, object[])"/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args) => MainWorld.EvaluateFunctionAsync<T>(script, args);

        /// <summary>
        /// Passes an expression to the <see cref="ExecutionContext.EvaluateExpressionHandleAsync(string)"/>, returns a <see cref="Task"/>, then <see cref="ExecutionContext.EvaluateExpressionHandleAsync(string)"/> would wait for the <see cref="Task"/> to resolve and return its value.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var frame = devToolsContext.MainFrame;
        /// var handle = await frame.EvaluateExpressionHandleAsync("1 + 2");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Resolves to the return value of <paramref name="script"/></returns>
        /// <param name="script">Expression to be evaluated in the <seealso cref="ExecutionContext"/></param>
        public Task<JavascriptHandle> EvaluateExpressionHandleAsync(string script) => MainWorld.EvaluateExpressionHandleAsync(script);

        /// <summary>
        /// Passes a function to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>, returns a <see cref="Task"/>, then <see cref="ExecutionContext.EvaluateFunctionHandleAsync(string, object[])"/> would wait for the <see cref="Task"/> to resolve and return its value.
        /// </summary>
        /// <example>
        /// Get a handle to the Global Object
        /// <code>
        /// <![CDATA[
        /// var frame = devToolsContext.MainFrame;
        /// var handle = frame.EvaluateFunctionHandleAsync("() => Promise.resolve(self)");
        /// return handle; // Handle for the global object.
        /// ]]>
        /// </code>
        /// Example of passing a <see cref="RemoteHandle"/> as a param to a javascript function.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>:
        /// <code>
        /// <![CDATA[
        /// const handle = await devToolsContext.EvaluateExpressionHandleAsync("document.body");
        /// const resultHandle = await devToolsContext.EvaluateFunctionHandleAsync("body => body.innerHTML", handle);
        /// return await resultHandle.GetInnerHtmlAsync(); // gets body's innerHTML
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="function">Function to be evaluated in the <see cref="ExecutionContext"/></param>
        /// <param name="args">Arguments to pass to <paramref name="function"/></param>
        /// <returns>Resolves to the return value of <paramref name="function"/></returns>
        public Task<JavascriptHandle> EvaluateFunctionHandleAsync(string function, params object[] args) => MainWorld.EvaluateFunctionHandleAsync(function, args);

        /// <summary>
        /// Passes a function to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>, returns a <see cref="Task"/>, then <see cref="ExecutionContext.EvaluateFunctionHandleAsync(string, object[])"/> would wait for the <see cref="Task"/> to resolve and return its value.
        /// </summary>
        /// <example>
        /// <code>
        /// var frame = page.MainFrame;
        /// const handle = Page.MainFrame.EvaluateFunctionHandleAsync("() => Promise.resolve(self)");
        /// return handle; // Handle for the global object.
        /// </code>
        /// <see cref="RemoteHandle"/> instances can be passed as arguments to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>:
        ///
        /// const handle = await Page.MainFrame.EvaluateExpressionHandleAsync("document.body");
        /// const resultHandle = await Page.MainFrame.EvaluateFunctionHandleAsync("body => body.innerHTML", handle);
        /// return await resultHandle.JsonValueAsync(); // prints body's innerHTML
        /// </example>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="function">Function to be evaluated in the <see cref="ExecutionContext"/></param>
        /// <param name="args">Arguments to pass to <paramref name="function"/></param>
        /// <returns>Resolves to the return value of <paramref name="function"/></returns>
        public Task<T> EvaluateFunctionHandleAsync<T>(string function, params object[] args)
            where T : RemoteHandle
        {
            return MainWorld.EvaluateFunctionHandleAsync<T>(function, args);
        }

        /// <summary>
        /// Gets the <see cref="ExecutionContext"/> associated with the frame.
        /// </summary>
        /// <returns><see cref="ExecutionContext"/> associated with the frame.</returns>
        public Task<ExecutionContext> GetExecutionContextAsync() => MainWorld.GetExecutionContextAsync();

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <param name="selector">A selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when element specified by selector string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and selector is not found in DOM.</returns>
        /// <seealso cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<HtmlElement> WaitForSelectorAsync(string selector, WaitForSelectorOptions options = null)
        {
            return WaitForSelectorAsync<HtmlElement>(selector, options);
        }

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="selector">A selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when element specified by selector string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and selector is not found in DOM.</returns>
        /// <seealso cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public async Task<T> WaitForSelectorAsync<T>(string selector, WaitForSelectorOptions options = null)
            where T : EventTarget
        {
            var handle = await SecondaryWorld.WaitForSelectorAsync<T>(selector, options).ConfigureAwait(true);
            if (handle == null)
            {
                return null;
            }
            var mainExecutionContext = await MainWorld.GetExecutionContextAsync().ConfigureAwait(true);
            var result = await mainExecutionContext.AdoptElementHandleAsync<T>(handle).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
            return result;
        }

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <param name="xpath">A xpath selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task which resolves when element specified by xpath string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and xpath is not found in DOM.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var ctx = await corewWebView.CreateDevToolsContextAsync();
        /// string currentURL = null;
        /// ctx.MainFrame
        ///     .WaitForXPathAsync("//img")
        ///     .ContinueWith(_ => Console.WriteLine("First URL with image: " + currentURL));
        /// foreach (var current in new[] { "https://example.com", "https://google.com", "https://bbc.com" })
        /// {
        ///     currentURL = current;
        ///     await ctx.GoToAsync(currentURL);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.WaitForXPathAsync{T}(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<HtmlElement> WaitForXPathAsync(string xpath, WaitForSelectorOptions options = null)
        {
            return WaitForXPathAsync<HtmlElement>(xpath, options);
        }

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="xpath">A xpath selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task which resolves when element specified by xpath string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and xpath is not found in DOM.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var ctx = await corewWebView.CreateDevToolsContextAsync();
        /// string currentURL = null;
        /// ctx.MainFrame
        ///     .WaitForXPathAsync("//img")
        ///     .ContinueWith(_ => Console.WriteLine("First URL with image: " + currentURL));
        /// foreach (var current in new[] { "https://example.com", "https://google.com", "https://bbc.com" })
        /// {
        ///     currentURL = current;
        ///     await ctx.GoToAsync(currentURL);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.WaitForXPathAsync{T}(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public async Task<T> WaitForXPathAsync<T>(string xpath, WaitForSelectorOptions options = null)
            where T : EventTarget
        {
            var handle = await SecondaryWorld.WaitForXPathAsync<T>(xpath, options).ConfigureAwait(true);
            if (handle == null)
            {
                return null;
            }
            var mainExecutionContext = await MainWorld.GetExecutionContextAsync().ConfigureAwait(true);
            var result = await mainExecutionContext.AdoptElementHandleAsync<T>(handle).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
            return result;
        }

        /// <summary>
        /// Waits for a timeout
        /// </summary>
        /// <param name="milliseconds">The amount of time to wait.</param>
        /// <returns>A task that resolves when after the timeout</returns>
        /// <seealso cref="WebView2DevToolsContext.WaitForTimeoutAsync(int)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task WaitForTimeoutAsync(int milliseconds) => Task.Delay(milliseconds);

        /// <summary>
        /// Waits for a function to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Function to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <param name="args">Arguments to pass to <c>script</c></param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="WebView2DevToolsContext.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<JavascriptHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options, params object[] args)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.WaitForFunctionAsync(script, options, args);
        }

        /// <summary>
        /// Waits for an expression to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Expression to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="WebView2DevToolsContext.WaitForExpressionAsync(string, WaitForFunctionOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<JavascriptHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.WaitForExpressionAsync(script, options);
        }

        /// <summary>
        /// Triggers a change and input event once all the provided options have been selected.
        /// If there's no <![CDATA[<select>]]> element matching selector, the method throws an error.
        /// </summary>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <param name="selector">A selector to query page for</param>
        /// <param name="values">Values of options to select. If the <![CDATA[<select>]]> has the multiple attribute,
        /// all values are considered, otherwise only the first one is taken into account.</param>
        /// <returns>Returns an array of option values that have been successfully selected.</returns>
        /// <seealso cref="WebView2DevToolsContext.SelectAsync(string, string[])"/>
        public Task<string[]> SelectAsync(string selector, params string[] values) => SecondaryWorld.SelectAsync(selector, values);

        /// <summary>
        /// Queries frame for the selector. If there's no such element within the frame, the method will resolve to <c>null</c>.
        /// </summary>
        /// <param name="selector">Selector to query frame for</param>
        /// <returns>Task which resolves to <see cref="Element"/> pointing to the frame element</returns>
        /// <seealso cref="WebView2DevToolsContext.QuerySelectorAsync(string)"/>
        public Task<HtmlElement> QuerySelectorAsync(string selector) => MainWorld.QuerySelectorAsync<HtmlElement>(selector);

        /// <summary>
        /// Queries frame for the selector. If there's no such element within the frame, the method will resolve to <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="Element"/> or derived type</typeparam>
        /// <param name="selector">Selector to query frame for</param>
        /// <returns>Task which resolves to <see cref="Element"/> pointing to the frame element</returns>
        /// <seealso cref="WebView2DevToolsContext.QuerySelectorAsync(string)"/>
        public Task<T> QuerySelectorAsync<T>(string selector)
            where T : Element
            => MainWorld.QuerySelectorAsync<T>(selector);

        /// <summary>
        /// Queries frame for the selector. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <param name="selector">A selector to query frame for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="WebView2DevToolsContext.QuerySelectorAllAsync(string)"/>
        public Task<HtmlElement[]> QuerySelectorAllAsync(string selector) => MainWorld.QuerySelectorAllAsync<HtmlElement>(selector);

        /// <summary>
        /// Queries frame for the selector. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="Element"/></typeparam>
        /// <param name="selector">A selector to query frame for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="WebView2DevToolsContext.QuerySelectorAllAsync(string)"/>
        public Task<T[]> QuerySelectorAllAsync<T>(string selector)
            where T : Element
        {
            return MainWorld.QuerySelectorAllAsync<T>(selector);
        }

        /// <summary>
        /// Evaluates the XPath expression
        /// </summary>
        /// <param name="expression">Expression to evaluate <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/evaluate"/></param>
        /// <returns>Task which resolves to an array of <see cref="Element"/></returns>
        /// <seealso cref="WebView2DevToolsContext.XPathAsync(string)"/>
        public Task<Element[]> XPathAsync(string expression) => MainWorld.XPathAsync(expression);

        /// <summary>
        /// Adds a <c><![CDATA[<link rel="stylesheet">]]></c> tag into the page with the desired url or a <c><![CDATA[<link rel="stylesheet">]]></c> tag with the content
        /// </summary>
        /// <param name="options">add style tag options</param>
        /// <returns>Task which resolves to the added tag when the stylesheet's onload fires or when the CSS content was injected into frame</returns>
        /// <seealso cref="WebView2DevToolsContext.AddStyleTagAsync(AddTagOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.AddStyleTagAsync(string)"/>
        public Task<HtmlElement> AddStyleTagAsync(AddTagOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.AddStyleTagAsync(options);
        }

        /// <summary>
        /// Adds a <c><![CDATA[<script>]]></c> tag into the page with the desired url or content
        /// </summary>
        /// <param name="options">add script tag options</param>
        /// <returns>Task which resolves to the added tag when the script's onload fires or when the script content was injected into frame</returns>
        /// <seealso cref="WebView2DevToolsContext.AddScriptTagAsync(AddTagOptions)"/>
        /// <seealso cref="WebView2DevToolsContext.AddScriptTagAsync(string)"/>
        public Task<HtmlElement> AddScriptTagAsync(AddTagOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.AddScriptTagAsync(options);
        }

        /// <summary>
        /// Returns page's title
        /// </summary>
        /// <returns>page's title</returns>
        /// <seealso cref="WebView2DevToolsContext.GetTitleAsync"/>
        public Task<string> GetTitleAsync() => SecondaryWorld.GetTitleAsync();

        /// <summary>
        /// Fetches an element with <paramref name="selector"/>, scrolls it into view if needed, and then uses <see cref="WebView2DevToolsContext.Mouse"/> to click in the center of the element.
        /// </summary>
        /// <param name="selector">A selector to search for element to click. If there are multiple elements satisfying the selector, the first will be clicked.</param>
        /// <param name="options">click options</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully clicked</returns>
        public Task ClickAsync(string selector, ClickOptions options = null)
            => SecondaryWorld.ClickAsync(selector, options);

        /// <summary>
        /// Fetches an element with <paramref name="selector"/>, scrolls it into view if needed, and then uses <see cref="WebView2DevToolsContext.Mouse"/> to hover over the center of the element.
        /// </summary>
        /// <param name="selector">A selector to search for element to hover. If there are multiple elements satisfying the selector, the first will be hovered.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully hovered</returns>
        public Task HoverAsync(string selector) => SecondaryWorld.HoverAsync(selector);

        /// <summary>
        /// Fetches an element with <paramref name="selector"/> and focuses it
        /// </summary>
        /// <param name="selector">A selector to search for element to focus. If there are multiple elements satisfying the selector, the first will be focused.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully focused</returns>
        public Task FocusAsync(string selector) => SecondaryWorld.FocusAsync(selector);

        /// <summary>
        /// Sends a <c>keydown</c>, <c>keypress</c>/<c>input</c>, and <c>keyup</c> event for each character in the text.
        /// </summary>
        /// <param name="selector">A selector of an element to type into. If there are multiple elements satisfying the selector, the first will be used.</param>
        /// <param name="text">A text to type into a focused element</param>
        /// <param name="options">The options to apply to the type operation.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <remarks>
        /// To press a special key, like <c>Control</c> or <c>ArrowDown</c> use <see cref="Keyboard.PressAsync(string, PressOptions)"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// await frame.TypeAsync("#mytextarea", "Hello"); // Types instantly
        /// await frame.TypeAsync("#mytextarea", "World", new TypeOptions { Delay = 100 }); // Types slower, like a user
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public Task TypeAsync(string selector, string text, TypeOptions options = null)
             => SecondaryWorld.TypeAsync(selector, text, options);

        internal void AddChildFrame(Frame frame)
        {
            lock (_childFrames)
            {
                _childFrames.Add(frame);
            }
        }

        internal void RemoveChildFrame(Frame frame)
        {
            lock (_childFrames)
            {
                _childFrames.Remove(frame);
            }
        }

        internal void OnLoadingStopped()
        {
            LifecycleEvents.Add("DOMContentLoaded");
            LifecycleEvents.Add("load");
        }

        internal void OnLifecycleEvent(string loaderId, string name)
        {
            if (name == "init")
            {
                LoaderId = loaderId;
                LifecycleEvents.Clear();
            }
            LifecycleEvents.Add(name);
        }

        internal void NavigatedWithinDocument(string url) => Url = url;

        internal void Detach()
        {
            Detached = true;
            MainWorld.Detach();
            SecondaryWorld.Detach();
            if (ParentFrame != null)
            {
                ParentFrame.RemoveChildFrame(this);
            }
            ParentFrame = null;
        }
    }
}
