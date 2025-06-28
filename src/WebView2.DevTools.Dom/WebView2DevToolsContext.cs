using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Helpers.Json;
using WebView2.DevTools.Dom.Input;
using WebView2.DevTools.Dom.Media;
using WebView2.DevTools.Dom.Messaging;
using WebView2.DevTools.Dom.Mobile;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Provides methods for accessing/modifying the DOM, executing javascript, adding CSS elements,
    /// basic automation tasks.
    /// A WebView2DevToolsContext is associated with a single <see cref="CoreWebView2"/> instance
    /// </summary>
    [DebuggerDisplay("WebView2DevToolsContext {Url}")]
    public sealed class WebView2DevToolsContext
        : IAsyncDisposable
    {
        private readonly Dictionary<string, Delegate> _pageBindings;
        private readonly ILogger _logger;
        private readonly DevToolsProtocolHelper _devToolsProtocolHelper;
        private readonly CoreWebView2 _coreWebView2;
        private readonly ILoggerFactory _loggerFactory;
        private readonly TimeoutSettings _timeoutSettings;

        internal WebView2DevToolsContext(
            CoreWebView2 coreWebView2, DevToolsProtocolHelper devToolsProtocolHelper, ILoggerFactory loggerFactory)
        {
            _coreWebView2 = coreWebView2;
            _devToolsProtocolHelper = devToolsProtocolHelper;
            Keyboard = new Keyboard(devToolsProtocolHelper);
            Mouse = new Mouse(devToolsProtocolHelper, Keyboard);
            Touchscreen = new Touchscreen(devToolsProtocolHelper, _coreWebView2, Keyboard);

            _loggerFactory = loggerFactory;
            _timeoutSettings = new TimeoutSettings();
            _pageBindings = new Dictionary<string, Delegate>();
            _logger = loggerFactory.CreateLogger<WebView2DevToolsContext>();
        }

        /// <summary>
        /// Raised when the page crashes
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Raised when a JavaScript dialog appears, such as <c>alert</c>, <c>prompt</c>, <c>confirm</c> or <c>beforeunload</c>. You can respond to the dialog via <see cref="Dialog"/>'s <see cref="Dialog.Accept(string)"/> or <see cref="Dialog.Dismiss"/> methods.
        /// </summary>
        public event EventHandler<DialogEventArgs> Dialog;

        /// <summary>
        /// Raised when a frame is attached.
        /// </summary>
        public event EventHandler<FrameEventArgs> FrameAttached;

        /// <summary>
        /// Raised when a frame is detached.
        /// </summary>
        public event EventHandler<FrameEventArgs> FrameDetached;

        /// <summary>
        /// Raised when a frame is navigated to a new url.
        /// </summary>
        public event EventHandler<FrameEventArgs> FrameNavigated;

        /// <summary>
        /// Raised when an uncaught exception happens within the page.
        /// </summary>
        public event EventHandler<RuntimeExceptionEventArgs> RuntimeException;

        /// <summary>
        /// Raised when a File chooser Dialog is opened.
        /// Will only be raised when Dialog Interception is enabled
        /// </summary>
        public event EventHandler<FileChooserOpenedEventArgs> FileChooserOpened;

        /// <summary>
        /// This setting will change the default maximum times for the following methods:
        /// - <see cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// - <see cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// - <see cref="WaitForExpressionAsync(string, WaitForFunctionOptions)"/>
        /// </summary>
        public int DefaultTimeout
        {
            get => _timeoutSettings.Timeout;
            set => _timeoutSettings.Timeout = value;
        }

        /// <summary>
        /// Gets page's main frame
        /// </summary>
        /// <remarks>
        /// Page is guaranteed to have a main frame which persists during navigations.
        /// </remarks>
        public Frame MainFrame => FrameManager.MainFrame;

        /// <summary>
        /// Gets all frames attached to the page.
        /// </summary>
        /// <value>An array of all frames attached to the page.</value>
        public Frame[] Frames => FrameManager.GetFrames();

        /// <summary>
        /// Shortcut for <c>page.MainFrame.Url</c>
        /// </summary>
        public string Url => MainFrame.Url;

        /// <summary>
        /// Gets this page's keyboard
        /// </summary>
        public Keyboard Keyboard { get; }

        /// <summary>
        /// Gets this page's touchscreen
        /// </summary>
        public Touchscreen Touchscreen { get; }

        /// <summary>
        /// Gets this page's mouse
        /// </summary>
        public Mouse Mouse { get; }

        internal bool JavascriptEnabled { get; set; } = true;

        internal FrameManager FrameManager { get; private set; }

        /// <summary>
        /// Sets the HTML markup to the page
        /// </summary>
        /// <param name="html">HTML markup to assign to the page.</param>
        /// <returns>Task.</returns>
        public Task SetContentAsync(string html)
        {
            return FrameManager.MainFrame.SetContentAsync(html);
        }

        /// <summary>
        /// Gets the full HTML contents of the page, including the doctype.
        /// </summary>
        /// <returns>Task which resolves to the HTML content.</returns>
        /// <seealso cref="Frame.GetContentAsync"/>
        public Task<string> GetContentAsync() => FrameManager.MainFrame.GetContentAsync();

        /*
        /// <summary>
        /// Whether to enable drag interception.
        /// </summary>
        /// <remarks>
        /// Activating drag interception enables the `Input.drag`,
        /// methods This provides the capability to capture drag events emitted
        /// on the page, which can then be used to simulate drag-and-drop.
        /// </remarks>
        /// <param name="enabled">Interception enabled</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public Task SetDragInterceptionAsync(bool enabled)
        {
            IsDragInterceptionEnabled = enabled;
            return Client.SendAsync("Input.setInterceptDrags", new InputSetInterceptDragsRequest { Enabled = enabled });
        }
        */

        /// <summary>
        /// When enabled intercepts file chooser requests and transfer control to client. When
        /// file chooser interception is enabled, native file chooser dialog is not shown.
        /// Instead, the <see cref="FileChooserOpened"/> event is raised.
        /// </summary>
        /// <param name="enabled">Interception enabled</param>
        /// <returns>A Task that resolves when the message was confirmed by the browser</returns>
        public Task SetInterceptFileChooserDialogAsync(bool enabled)
        {
            return _devToolsProtocolHelper.Page.SetInterceptFileChooserDialogAsync(enabled);
        }

        /// <summary>
        /// Fetches an element with <paramref name="querySelector"/>, scrolls it into view if needed, and then uses <see cref="Touchscreen"/> to tap in the center of the element.
        /// </summary>
        /// <param name="querySelector">A selector to search for element to tap. If there are multiple elements satisfying the selector, the first will be clicked.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="querySelector"/> is successfully tapped</returns>
        public async Task TapAsync(string querySelector)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(querySelector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {querySelector}", querySelector);
            }
            await handle.TapAsync().ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// The method runs <c>document.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="HtmlElement"/> or derived type</typeparam>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to <see cref="HtmlElement"/> pointing to the frame element</returns>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.QuerySelectorAsync(selector)</c>
        /// </remarks>
        /// <seealso cref="Frame.QuerySelectorAsync(string)"/>
        public Task<T> QuerySelectorAsync<T>(string querySelector)
            where T : Element
            => MainFrame.QuerySelectorAsync<T>(querySelector);

        /// <summary>
        /// The method runs <c>document.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to <see cref="Element"/> pointing to the frame element</returns>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.QuerySelectorAsync(selector)</c>
        /// </remarks>
        /// <seealso cref="Frame.QuerySelectorAsync(string)"/>
        public Task<HtmlElement> QuerySelectorAsync(string querySelector)
            => MainFrame.QuerySelectorAsync(querySelector);

        /// <summary>
        /// Runs <c>document.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="Frame.QuerySelectorAllAsync(string)"/>
        public Task<HtmlElement[]> QuerySelectorAllAsync(string querySelector)
            => MainFrame.QuerySelectorAllAsync(querySelector);

        /// <summary>
        /// Runs <c>document.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="Element"/></typeparam>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="Frame.QuerySelectorAllAsync(string)"/>
        public Task<T[]> QuerySelectorAllAsync<T>(string querySelector)
            where T : Element
            => MainFrame.QuerySelectorAllAsync<T>(querySelector);

        /// <summary>
        /// A utility function to be used with <see cref="RemoteObjectExtensions.EvaluateFunctionAsync{T}(Task{JavascriptHandle}, string, object[])"/>
        /// </summary>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to a <see cref="JavascriptHandle"/> of <c>document.querySelectorAll</c> result</returns>
        public Task<JavascriptHandle> QuerySelectorAllHandleAsync(string querySelector)
            => EvaluateFunctionHandleAsync("selector => Array.from(document.querySelectorAll(selector))", querySelector);

        /// <summary>
        /// Evaluates the XPath expression
        /// </summary>
        /// <param name="expression">Expression to evaluate <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/evaluate"/></param>
        /// <returns>Task which resolves to an array of <see cref="Element"/></returns>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.XPathAsync(expression)</c>
        /// </remarks>
        /// <seealso cref="Frame.XPathAsync(string)"/>
        public Task<Element[]> XPathAsync(string expression) => MainFrame.XPathAsync(expression);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        public async Task<JavascriptHandle> EvaluateExpressionHandleAsync(string script)
        {
            var context = await MainFrame.GetExecutionContextAsync().ConfigureAwait(true);

            return await context.EvaluateExpressionHandleAsync<JavascriptHandle>(script).ConfigureAwait(true);
        }

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        public async Task<T> EvaluateExpressionHandleAsync<T>(string script)
            where T : RemoteHandle
        {
            var context = await MainFrame.GetExecutionContextAsync().ConfigureAwait(true);

            return await context.EvaluateExpressionHandleAsync<T>(script).ConfigureAwait(true);
        }

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="pageFunction">Script to be evaluated in browser context</param>
        /// <param name="args">Function arguments</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        public Task<JavascriptHandle> EvaluateFunctionHandleAsync(string pageFunction, params object[] args)
        {
            return EvaluateFunctionHandleAsync<JavascriptHandle>(pageFunction, args);
        }

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="pageFunction">Script to be evaluated in browser context</param>
        /// <param name="args">Function arguments</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        public async Task<T> EvaluateFunctionHandleAsync<T>(string pageFunction, params object[] args)
            where T : RemoteHandle
        {
            var context = await MainFrame.GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateFunctionHandleAsync<T>(pageFunction, args).ConfigureAwait(true);
        }

        /// <summary>
        /// Adds a function which would be invoked in one of the following scenarios:
        /// - whenever the page is navigated
        /// - whenever the child frame is attached or navigated. In this case, the function is invoked in the context of the newly attached frame
        /// </summary>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <remarks>
        /// The function is invoked after the document was created but before any of its scripts were run. This is useful to amend JavaScript environment, e.g. to seed <c>Math.random</c>.
        /// </remarks>
        /// <example>
        /// An example of setting a custom property before the page loads:
        /// <code>
        /// <![CDATA[
        /// await devToolsContext.EvaluateFunctionOnNewDocumentAsync("() => window.__example = true");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public Task EvaluateFunctionOnNewDocumentAsync(string pageFunction, params object[] args)
        {
            var source = EvaluationString(pageFunction, args);

            return _devToolsProtocolHelper.Page.AddScriptToEvaluateOnNewDocumentAsync(source);
        }

        /// <summary>
        /// Adds a function which would be invoked in one of the following scenarios:
        /// - whenever the page is navigated
        /// - whenever the child frame is attached or navigated. In this case, the function is invoked in the context of the newly attached frame
        /// </summary>
        /// <param name="expression">Javascript expression to be evaluated in browser context</param>
        /// <remarks>
        /// The function is invoked after the document was created but before any of its scripts were run. This is useful to amend JavaScript environment, e.g. to seed <c>Math.random</c>.
        /// </remarks>
        /// <example>
        /// An example of setting a custom property before the page loads:
        /// <code>
        /// <![CDATA[
        /// await devToolsContext.EvaluateExpressionOnNewDocumentAsync("window.__example = true;");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public Task EvaluateExpressionOnNewDocumentAsync(string expression)
            => _devToolsProtocolHelper.Page.AddScriptToEvaluateOnNewDocumentAsync(expression);

        /// <summary>
        /// The method iterates JavaScript heap and finds all the objects with the given prototype.
        /// Shortcut for <c>page.MainFrame.GetExecutionContextAsync().QueryObjectsAsync(prototypeHandle)</c>.
        /// </summary>
        /// <returns>A task which resolves to a handle to an array of objects with this prototype.</returns>
        /// <param name="prototypeHandle">A handle to the object prototype.</param>
        public async Task<JavascriptHandle> QueryObjectsAsync(JavascriptHandle prototypeHandle)
        {
            var context = await MainFrame.GetExecutionContextAsync().ConfigureAwait(true);
            return await context.QueryObjectsAsync(prototypeHandle).ConfigureAwait(true);
        }

        /// <summary>
        /// Emulates network conditions
        /// </summary>
        /// <param name="networkConditions">Passing <c>null</c> disables network condition emulation.</param>
        /// <returns>Result task</returns>
        /// <remarks>
        /// **NOTE** This does not affect WebSockets and WebRTC PeerConnections (see https://crbug.com/563644)
        /// </remarks>
        public Task EmulateNetworkConditionsAsync(NetworkConditions networkConditions) => FrameManager.NetworkManager.EmulateNetworkConditionsAsync(networkConditions);

        /// <summary>
        /// Returns the page's cookies
        /// </summary>
        /// <param name="urls">Url's to return cookies for</param>
        /// <returns>Array of cookies</returns>
        /// <remarks>
        /// If no URLs are specified, this method returns cookies for the current page URL.
        /// If URLs are specified, only cookies for those URLs are returned.
        /// </remarks>
        public Task<Network.Cookie[]> GetCookiesAsync(params string[] urls)
        {
            return _devToolsProtocolHelper.Network.GetCookiesAsync(urls);
        }

        /// <summary>
        /// Adds a <c><![CDATA[<script>]]></c> tag into the page with the desired url or content
        /// </summary>
        /// <param name="options">add script tag options</param>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.AddScriptTagAsync(options)</c>
        /// </remarks>
        /// <returns>Task which resolves to the added tag when the script's onload fires or when the script content was injected into frame</returns>
        /// <seealso cref="Frame.AddScriptTagAsync(AddTagOptions)"/>
        public Task<HtmlElement> AddScriptTagAsync(AddTagOptions options) => MainFrame.AddScriptTagAsync(options);

        /// <summary>
        /// Adds a <c><![CDATA[<script>]]></c> tag into the page with the desired url or content
        /// </summary>
        /// <param name="url">script url</param>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.AddScriptTagAsync(new AddTagOptions { Url = url })</c>
        /// </remarks>
        /// <returns>Task which resolves to the added tag when the script's onload fires or when the script content was injected into frame</returns>
        public Task<HtmlElement> AddScriptTagAsync(string url) => AddScriptTagAsync(new AddTagOptions { Url = url });

        /// <summary>
        /// Adds a <c><![CDATA[<link rel="stylesheet">]]></c> tag into the page with the desired url or a <c><![CDATA[<link rel="stylesheet">]]></c> tag with the content
        /// </summary>
        /// <param name="options">add style tag options</param>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.AddStyleTagAsync(options)</c>
        /// </remarks>
        /// <returns>Task which resolves to the added tag when the stylesheet's onload fires or when the CSS content was injected into frame</returns>
        public Task<HtmlElement> AddStyleTagAsync(AddTagOptions options) => MainFrame.AddStyleTagAsync(options);

        /// <summary>
        /// Adds a <c><![CDATA[<link rel="stylesheet">]]></c> tag into the page with the desired url or a <c><![CDATA[<link rel="stylesheet">]]></c> tag with the content
        /// </summary>
        /// <param name="url">stylesheel url</param>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.AddStyleTagAsync(new AddTagOptions { Url = url })</c>
        /// </remarks>
        /// <returns>Task which resolves to the added tag when the stylesheet's onload fires or when the CSS content was injected into frame</returns>
        public Task<HtmlElement> AddStyleTagAsync(string url) => AddStyleTagAsync(new AddTagOptions { Url = url });

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves when <paramref name="puppeteerFunction"/> completes.
        /// </summary>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in Puppeteer's context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync(string, Action)"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync(string name, Action puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves to the return value of <paramref name="puppeteerFunction"/>.
        /// </summary>
        /// <typeparam name="TResult">The result of <paramref name="puppeteerFunction"/></typeparam>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in the DevTools context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync{TResult}(string, Func{TResult})"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync<TResult>(string name, Func<TResult> puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves to the return value of <paramref name="puppeteerFunction"/>.
        /// </summary>
        /// <typeparam name="T">The parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="TResult">The result of <paramref name="puppeteerFunction"/></typeparam>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in DevTools context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync{T, TResult}(string, Func{T, TResult})"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync<T, TResult>(string name, Func<T, TResult> puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves to the return value of <paramref name="puppeteerFunction"/>.
        /// </summary>
        /// <typeparam name="T1">The first parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T2">The second parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="TResult">The result of <paramref name="puppeteerFunction"/></typeparam>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in Puppeteer's context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync{T1, T2, TResult}(string, Func{T1, T2, TResult})"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync<T1, T2, TResult>(string name, Func<T1, T2, TResult> puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves to the return value of <paramref name="puppeteerFunction"/>.
        /// </summary>
        /// <typeparam name="T1">The first parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T2">The second parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T3">The third parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="TResult">The result of <paramref name="puppeteerFunction"/></typeparam>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in Puppeteer's context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync{T1, T2, T3, TResult}(string, Func{T1, T2, T3, TResult})"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Adds a function called <c>name</c> on the page's <c>window</c> object.
        /// When called, the function executes <paramref name="puppeteerFunction"/> in C# and returns a <see cref="Task"/> which resolves to the return value of <paramref name="puppeteerFunction"/>.
        /// </summary>
        /// <typeparam name="T1">The first parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T2">The second parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T3">The third parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="T4">The fourth parameter of <paramref name="puppeteerFunction"/></typeparam>
        /// <typeparam name="TResult">The result of <paramref name="puppeteerFunction"/></typeparam>
        /// <param name="name">Name of the function on the window object</param>
        /// <param name="puppeteerFunction">Callback function which will be called in Puppeteer's context.</param>
        /// <remarks>
        /// If the <paramref name="puppeteerFunction"/> returns a <see cref="Task"/>, it will be awaited.
        /// Functions installed via <see cref="ExposeFunctionAsync{T1, T2, T3, T4, TResult}(string, Func{T1, T2, T3, T4, TResult})"/> survive navigations
        /// </remarks>
        /// <returns>Task</returns>
        public Task ExposeFunctionAsync<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> puppeteerFunction)
            => ExposeFunctionAsync(name, (Delegate)puppeteerFunction);

        /// <summary>
        /// Enables/Disables Javascript on the page
        /// </summary>
        /// <returns>Task.</returns>
        /// <param name="enabled">Whether or not to enable JavaScript on the page.</param>
        public Task SetJavaScriptEnabledAsync(bool enabled)
        {
            if (enabled == JavascriptEnabled)
            {
                return Task.CompletedTask;
            }
            JavascriptEnabled = enabled;

            return _devToolsProtocolHelper.Emulation.SetScriptExecutionDisabledAsync(!enabled);
        }

        /// <summary>
        /// Toggles bypassing page's Content-Security-Policy.
        /// </summary>
        /// <param name="enabled">sets bypassing of page's Content-Security-Policy.</param>
        /// <returns></returns>
        /// <remarks>
        /// CSP bypassing happens at the moment of CSP initialization rather then evaluation.
        /// Usually this means that <see cref="SetBypassCSPAsync(bool)"/> should be called before navigating to the domain.
        /// </remarks>
        public Task SetBypassCSPAsync(bool enabled) => _devToolsProtocolHelper.Page.SetBypassCSPAsync(enabled);

        /// <summary>
        /// Returns page's title
        /// </summary>
        /// <returns>page's title</returns>
        /// <see cref="Frame.GetTitleAsync"/>
        public Task<string> GetTitleAsync() => MainFrame.GetTitleAsync();

        /// <summary>
        /// Toggles ignoring cache for each request based on the enabled state. By default, caching is enabled.
        /// </summary>
        /// <param name="enabled">sets the <c>enabled</c> state of the cache</param>
        /// <returns>Task</returns>
        public Task SetCacheEnabledAsync(bool enabled = true)
        {
            return _devToolsProtocolHelper.Network.SetCacheDisabledAsync(enabled);
        }

        /// <summary>
        /// Fetches an element with <paramref name="querySelector"/>, scrolls it into view if needed, and then uses <see cref="Mouse"/> to click in the center of the element.
        /// </summary>
        /// <param name="querySelector">A selector to search for element to click. If there are multiple elements satisfying the selector, the first will be clicked.</param>
        /// <param name="options">click options</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="querySelector"/> is successfully clicked</returns>
        public Task ClickAsync(string querySelector, ClickOptions options = null) => FrameManager.MainFrame.ClickAsync(querySelector, options);

        /// <summary>
        /// Fetches an element with <paramref name="querySelector"/>, scrolls it into view if needed, and then uses <see cref="Mouse"/> to hover over the center of the element.
        /// </summary>
        /// <param name="querySelector">A selector to search for element to hover. If there are multiple elements satisfying the selector, the first will be hovered.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="querySelector"/> is successfully hovered</returns>
        public Task HoverAsync(string querySelector) => FrameManager.MainFrame.HoverAsync(querySelector);

        /// <summary>
        /// Fetches an element with <paramref name="querySelector"/> and focuses it
        /// </summary>
        /// <param name="querySelector">A selector to search for element to focus. If there are multiple elements satisfying the selector, the first will be focused.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="querySelector"/> is successfully focused</returns>
        public Task FocusAsync(string querySelector) => FrameManager.MainFrame.FocusAsync(querySelector);

        /// <summary>
        /// Sends a <c>keydown</c>, <c>keypress</c>/<c>input</c>, and <c>keyup</c> event for each character in the text.
        /// </summary>
        /// <param name="querySelector">A selector of an element to type into. If there are multiple elements satisfying the selector, the first will be used.</param>
        /// <param name="text">A text to type into a focused element</param>
        /// <param name="options">The options to apply to the type operation.</param>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <remarks>
        /// To press a special key, like <c>Control</c> or <c>ArrowDown</c> use <see cref="Input.Keyboard.PressAsync(string, PressOptions)"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// await devToolsContext.TypeAsync("#mytextarea", "Hello"); // Types instantly
        /// await devToolsContext.TypeAsync("#mytextarea", "World", new TypeOptions { Delay = 100 }); // Types slower, like a user
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public Task TypeAsync(string querySelector, string text, TypeOptions options = null)
            => FrameManager.MainFrame.TypeAsync(querySelector, text, options);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <example>
        /// An example of scraping information from all hyperlinks on the page.
        /// <code>
        /// <![CDATA[
        /// var hyperlinkInfo = await devToolsContext.EvaluateExpressionAsync(@"
        ///     Array
        ///        .from(document.querySelectorAll('a'))
        ///        .map(n => ({
        ///            text: n.innerText,
        ///            href: n.getAttribute('href'),
        ///            target: n.getAttribute('target')
        ///         }))
        /// ");
        /// Console.WriteLine(hyperlinkInfo.ToString()); // Displays JSON array of hyperlinkInfo objects
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<JsonElement> EvaluateExpressionAsync(string script)
            => FrameManager.MainFrame.EvaluateExpressionAsync<JsonElement>(script);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<T> EvaluateExpressionAsync<T>(string script)
            => FrameManager.MainFrame.EvaluateExpressionAsync<T>(script);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args)
            => FrameManager.MainFrame.EvaluateFunctionAsync<JsonElement>(script, args);

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
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
            => FrameManager.MainFrame.EvaluateFunctionAsync<T>(script, args);

        /// <summary>
        /// Sets the user agent to be used in this page
        /// </summary>
        /// <param name="userAgent">Specific user agent to use in this page</param>
        /// <returns>Task</returns>
        public Task SetUserAgentAsync(string userAgent)
        {
            return _devToolsProtocolHelper.Network.SetUserAgentOverrideAsync(userAgent);
        }

        /// <summary>
        /// Triggers a change and input event once all the provided options have been selected.
        /// If there's no <![CDATA[<select>]]> element matching selector, the method throws an error.
        /// </summary>
        /// <exception cref="WebView2DevToolsSelectorException">If there's no element matching <paramref name="querySelector"/></exception>
        /// <param name="querySelector">A selector to query page for</param>
        /// <param name="values">Values of options to select. If the <![CDATA[<select>]]> has the multiple attribute,
        /// all values are considered, otherwise only the first one is taken into account.</param>
        /// <returns>Returns an array of option values that have been successfully selected.</returns>
        /// <seealso cref="Frame.SelectAsync(string, string[])"/>
        public Task<string[]> SelectAsync(string querySelector, params string[] values)
            => MainFrame.SelectAsync(querySelector, values);

        /// <summary>
        /// Waits for a timeout
        /// </summary>
        /// <param name="milliseconds">The amount of time to wait.</param>
        /// <returns>A task that resolves when after the timeout</returns>
        /// <seealso cref="Frame.WaitForTimeoutAsync(int)"/>
        public Task WaitForTimeoutAsync(int milliseconds)
            => MainFrame.WaitForTimeoutAsync(milliseconds);

        /// <summary>
        /// Waits for a function to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Function to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <param name="args">Arguments to pass to <c>script</c></param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="Frame.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
        public Task<JavascriptHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options = null, params object[] args)
            => MainFrame.WaitForFunctionAsync(script, options ?? new WaitForFunctionOptions(), args);

        /// <summary>
        /// Waits for a function to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>script</c></param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        public Task<JavascriptHandle> WaitForFunctionAsync(string script, params object[] args) => WaitForFunctionAsync(script, null, args);

        /// <summary>
        /// Waits for an expression to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Expression to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="Frame.WaitForExpressionAsync(string, WaitForFunctionOptions)"/>
        public Task<JavascriptHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options = null)
            => MainFrame.WaitForExpressionAsync(script, options ?? new WaitForFunctionOptions());

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <param name="querySelector">A selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when element specified by selector string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and selector is not found in DOM.</returns>
        /// <seealso cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="Frame.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        public Task<HtmlElement> WaitForSelectorAsync(string querySelector, WaitForSelectorOptions options = null)
            => MainFrame.WaitForSelectorAsync<HtmlElement>(querySelector, options ?? new WaitForSelectorOptions());

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="querySelector">A selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when element specified by selector string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and selector is not found in DOM.</returns>
        /// <seealso cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="Frame.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        public Task<T> WaitForSelectorAsync<T>(string querySelector, WaitForSelectorOptions options = null)
            where T : EventTarget
            => MainFrame.WaitForSelectorAsync<T>(querySelector, options ?? new WaitForSelectorOptions());

        /// <summary>
        /// Waits for a xpath selector to be added to the DOM
        /// </summary>
        /// <param name="xpath">A xpath selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task which resolves when element specified by xpath string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and xpath is not found in DOM.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Add using WebView2.DevTools.Dom
        /// var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
        /// string currentURL = null;
        /// devToolsContext
        ///     .WaitForXPathAsync("//img")
        ///     .ContinueWith(_ => Console.WriteLine("First URL with image: " + currentURL));
        /// foreach (var current in new[] { "https://example.com", "https://google.com", "https://bbc.com" })
        /// {
        ///     currentURL = current;
        ///     await devToolsContext.GoToAsync(currentURL);
        /// }
        /// await devToolsContext.DsiposeAsync();
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="Frame.WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        public Task<HtmlElement> WaitForXPathAsync(string xpath, WaitForSelectorOptions options = null)
            => MainFrame.WaitForXPathAsync<HtmlElement>(xpath, options ?? new WaitForSelectorOptions());

        /// <summary>
        /// Waits for a xpath selector to be added to the DOM
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="xpath">A xpath selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task which resolves when element specified by xpath string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and xpath is not found in DOM.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var browser = await Puppeteer.LaunchAsync(new LaunchOptions());
        /// var page = await browser.NewPageAsync();
        /// string currentURL = null;
        /// page
        ///     .WaitForXPathAsync("//img")
        ///     .ContinueWith(_ => Console.WriteLine("First URL with image: " + currentURL));
        /// foreach (var current in new[] { "https://example.com", "https://google.com", "https://bbc.com" })
        /// {
        ///     currentURL = current;
        ///     await page.GoToAsync(currentURL);
        /// }
        /// await browser.CloseAsync();
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="Frame.WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        public Task<T> WaitForXPathAsync<T>(string xpath, WaitForSelectorOptions options = null)
            where T : EventTarget
            => MainFrame.WaitForXPathAsync<T>(xpath, options ?? new WaitForSelectorOptions());

        /// <summary>
        /// Brings page to front (activates tab).
        /// </summary>
        /// <returns>A task that resolves when the message has been sent to Chromium.</returns>
        public Task BringToFrontAsync() => _devToolsProtocolHelper.Page.BringToFrontAsync();

        /// <summary>
        /// Changes the timezone of the page.
        /// </summary>
        /// <param name="timezoneId">Timezone to set. See <seealso href="https://cs.chromium.org/chromium/src/third_party/icu/source/data/misc/metaZones.txt?rcl=faee8bc70570192d82d2978a71e2a615788597d1" >ICU’s `metaZones.txt`</seealso>
        /// for a list of supported timezone IDs. Passing `null` disables timezone emulation.</param>
        /// <returns>The viewport task.</returns>
        public async Task EmulateTimezoneAsync(string timezoneId)
        {
            try
            {
                await _devToolsProtocolHelper.Emulation.SetTimezoneOverrideAsync(timezoneId ?? string.Empty).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                throw new WebView2DevToolsContextException($"Invalid timezone ID: {timezoneId}", ex);
            }
        }

        /// <summary>
        /// Enables CPU throttling to emulate slow CPUs.
        /// </summary>
        /// <param name="factor">Throttling rate as a slowdown factor (1 is no throttle, 2 is 2x slowdown, etc).</param>
        /// <returns>A task that resolves when the message has been sent to the browser.</returns>
        public Task EmulateCPUThrottlingAsync(double? factor = null)
        {
            if (factor != null && factor < 1)
            {
                throw new ArgumentException("Throttling rate should be greater or equal to 1", nameof(factor));
            }

            return _devToolsProtocolHelper.Emulation.SetCPUThrottlingRateAsync(factor ?? 1);
        }

        /// <summary>
        /// Enable/disable whether all certificate errors should be ignored.
        /// </summary>
        /// <param name="ignore">If true, all certificate errors will be ignored.</param>
        /// <returns>A task that resolves when the message has been sent to the browser.</returns>
        public Task IgnoreCertificateErrorsAsync(bool ignore)
        {
            return _devToolsProtocolHelper.Security.SetIgnoreCertificateErrorsAsync(ignore);
        }

        /// <summary>
        /// Creates the HTML element specified
        /// </summary>
        /// <typeparam name="T">HtmlElementType</typeparam>
        /// <param name="tagName">
        /// A string that specifies the type of element to be created.
        /// The nodeName of the created element is initialized with the
        /// value of tagName. Don't use qualified names (like "html:a")
        /// with this method.
        /// </param>
        /// <returns>Created element</returns>
        public Task<T> CreateHtmlElementAsync<T>(string tagName)
            where T : HtmlElement
        {
            return EvaluateFunctionHandleAsync<T>(
                @"(tagName) => {
                    return document.createElement(tagName);
                }",
                tagName);
        }

        /// <summary>
        /// Creates the HTML element specified
        /// </summary>
        /// <typeparam name="T">HtmlElementType</typeparam>
        /// <param name="tagName">
        /// A string that specifies the type of element to be created.
        /// The nodeName of the created element is initialized with the
        /// value of tagName. Don't use qualified names (like "html:a")
        /// with this method.
        /// </param>
        /// <param name="id">element id</param>
        /// <returns>Created element</returns>
        public Task<T> CreateHtmlElementAsync<T>(string tagName, string id)
            where T : HtmlElement
        {
            return EvaluateFunctionHandleAsync<T>(
                @"(tagName, id) => {
                    let e = document.createElement(tagName);
                    e.id = id;
                    return e;
                }",
                tagName,
                id);
        }

        /// <summary>
        /// Equivilent to calling document.body.appendChild
        /// </summary>
        /// <param name="htmlElement">html element</param>
        /// <returns>Task</returns>
        public Task AppendChildAsync(HtmlElement htmlElement)
        {
            return EvaluateFunctionHandleAsync("(e) => { document.body.appendChild(e); }", htmlElement);
        }

        /// <summary>
        /// Emulates given device metrics and user agent.
        /// </summary>
        /// <remarks>
        /// This method is a shortcut for calling two methods:
        /// <see cref="SetViewportAsync(ViewPortOptions)"/>
        /// <see cref="SetUserAgentAsync(string)"/>
        /// To aid emulation, device descriptor can be obtained via the <see cref="Emulate.Device(DeviceName)"/>.
        /// <see cref="EmulateAsync(DeviceDescriptor)"/> will resize the page. A lot of websites don't expect phones to change size, so you should emulate before navigating to the page.
        /// </remarks>
        /// <example>
        ///<![CDATA[
        /// var iPhone = Emulate.Device(DeviceName.IPhone6);
        /// await devToolsContext.EmulateAsync(iPhone);
        /// ]]>
        /// </example>
        /// <returns>Task.</returns>
        /// <param name="options">Emulation options.</param>
        public Task EmulateAsync(DeviceDescriptor options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return Task.WhenAll(
                SetViewportAsync(options.ViewPort),
                SetUserAgentAsync(options.UserAgent));
        }

        /// <summary>
        /// Returns navigation history for the current browser.
        /// </summary>
        /// <returns>Task that when awaited gets the navigation history for the current browser.</returns>
        public Task<Page.GetNavigationHistoryReturnType> GetNavigationHistoryAsync()
        {
            // https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-getNavigationHistory

            return _devToolsProtocolHelper.Page.GetNavigationHistoryAsync();
        }

        /// <summary>
        /// Navigates current browser to the given history entry.
        /// </summary>
        /// <param name="historyEntry">Navigation History Entry</param>
        /// <returns>Task</returns>
        public Task NavigateToHistoryEntryAsync(Page.NavigationEntry historyEntry)
        {
            if (historyEntry == null)
            {
                throw new ArgumentNullException(nameof(historyEntry));
            }

            return _devToolsProtocolHelper.Page.NavigateToHistoryEntryAsync(historyEntry.Id);
        }

        internal async Task InitializeAsync()
        {
            FrameManager = new FrameManager(_coreWebView2, _devToolsProtocolHelper, _loggerFactory, this, _timeoutSettings);

            _devToolsProtocolHelper.Page.JavascriptDialogOpening += OnPageJavascriptDialogOpening;
            _devToolsProtocolHelper.Page.FileChooserOpened += OnPageFileChooserOpened;
            _devToolsProtocolHelper.Runtime.ExceptionThrown += OnRuntimeExceptionThrown;

            _devToolsProtocolHelper.Inspector.TargetCrashed += OnInspectorTargetCrashed;
            _devToolsProtocolHelper.Runtime.BindingCalled += OnRuntimeBindingCalled;

            FrameManager.FrameAttached += (_, e) => FrameAttached?.Invoke(this, e);
            FrameManager.FrameDetached += (_, e) => FrameDetached?.Invoke(this, e);
            FrameManager.FrameNavigated += (_, e) => FrameNavigated?.Invoke(this, e);

            await _devToolsProtocolHelper.Page.EnableAsync().ConfigureAwait(true);

            var frameTree = await _devToolsProtocolHelper.Page.GetFrameTreeAsync().ConfigureAwait(true);

            FrameManager.LoadFrameTree(new FrameTree(frameTree));

            await _devToolsProtocolHelper.Page.SetLifecycleEventsEnabledAsync(true).ConfigureAwait(true);

            await _devToolsProtocolHelper.Runtime.EnableAsync().ConfigureAwait(true);

            await FrameManager.EnsureIsolatedWorldAsync(FrameManager.UtilityWorldName).ConfigureAwait(true);

            // TODO: Fix ordering of events and remove this
            // await Task.Delay(200).ConfigureAwait(true);
        }

        private async void OnPageFileChooserOpened(object sender, Page.FileChooserOpenedEventArgs e)
        {
            var handler = FileChooserOpened;

            if (handler == null)
            {
                return;
            }

            var frame = FrameManager.GetFrames().FirstOrDefault(x => x.Id == e.FrameId);

            if (frame == null)
            {
                return;
            }

            var context = await frame.GetExecutionContextAsync().ConfigureAwait(true);

            var element = await context.AdoptBackendNodeAsync<HtmlInputElement>(e.BackendNodeId).ConfigureAwait(true);

            if (element != null)
            {
                handler.Invoke(this, new FileChooserOpenedEventArgs { Element = element, Multiple = string.Equals(e.Mode, "selectMultiple", StringComparison.OrdinalIgnoreCase) });
            }
        }

        private async void OnRuntimeBindingCalled(object sender, Runtime.BindingCalledEventArgs args)
        {
            string expression;

            // TODO: Do we need to handle exception here??
            var bindingPayload = JsonSerializer.Deserialize<BindingCalledResponsePayload>(args.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            try
            {
                var result = await ExecuteBinding(args, bindingPayload).ConfigureAwait(true);

                expression = EvaluationString(
                    @"function deliverResult(name, seq, result) {
                        window[name]['callbacks'].get(seq).resolve(result);
                        window[name]['callbacks'].delete(seq);
                    }",
                    bindingPayload.Name,
                    bindingPayload.Seq,
                    result);
            }
            catch (Exception ex)
            {
                var innerException = ex.GetBaseException();

                expression = EvaluationString(
                    @"function deliverError(name, seq, message, stack) {
                        const error = new Error(message);
                        error.stack = stack;
                        window[name]['callbacks'].get(seq).reject(error);
                        window[name]['callbacks'].delete(seq);
                    }",
                    args.Name,
                    bindingPayload.Seq,
                    innerException.Message,
                    innerException.StackTrace);
            }

            _ = _devToolsProtocolHelper.Runtime.EvaluateAsync(
                expression,
                contextId: args.ExecutionContextId);
        }

        private async Task<object> ExecuteBinding(Runtime.BindingCalledEventArgs e, BindingCalledResponsePayload payload)
        {
            const string taskResultPropertyName = "Result";
            object result;
            if (!_pageBindings.TryGetValue(e.Name, out var binding))
            {
                return null;
            }

            var methodParams = binding.Method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

            object[] args = null;

            if (methodParams.Length > 0)
            {
                args = payload.Args.Select((token, i) => token.ToObject(methodParams[i])).ToArray();
            }

            result = binding.DynamicInvoke(args);
            if (result is Task taskResult)
            {
                await taskResult.ConfigureAwait(true);

                if (taskResult.GetType().IsGenericType)
                {
                    // the task is already awaited and therefore the call to property Result will not deadlock
                    result = taskResult.GetType().GetProperty(taskResultPropertyName).GetValue(taskResult);
                }
            }

            return result;
        }

        private void OnInspectorTargetCrashed(object sender, Inspector.TargetCrashedEventArgs args)
        {
            if (Error == null)
            {
                throw new WebView2DevToolsTargetCrashedException();
            }

            Error.Invoke(this, new ErrorEventArgs("Page crashed!"));
        }

        private void OnRuntimeExceptionThrown(object sender, Runtime.ExceptionThrownEventArgs args)
            => RuntimeException?.Invoke(this, new RuntimeExceptionEventArgs(GetExceptionMessage(args)));

        private string GetExceptionMessage(Runtime.ExceptionThrownEventArgs args)
        {
            var exceptionDetails = args.ExceptionDetails;

            if (exceptionDetails.Exception != null)
            {
                return exceptionDetails.Exception.Description;
            }
            var message = exceptionDetails.Text;
            if (exceptionDetails.StackTrace != null)
            {
                foreach (var callframe in exceptionDetails.StackTrace.CallFrames)
                {
                    var location = $"{callframe.Url}:{callframe.LineNumber}:{callframe.ColumnNumber}";
                    var functionName = callframe.FunctionName ?? "<anonymous>";
                    message += $"\n at {functionName} ({location})";
                }
            }
            return message;
        }

        private void OnPageJavascriptDialogOpening(object sender, Page.JavascriptDialogOpeningEventArgs args)
        {
            var dialog = new Dialog(_devToolsProtocolHelper, args.Type, args.Message, args.DefaultPrompt);
            Dialog?.Invoke(this, new DialogEventArgs(dialog));
        }

        private async Task ExposeFunctionAsync(string name, Delegate puppeteerFunction)
        {
            if (_pageBindings.ContainsKey(name))
            {
                throw new WebView2DevToolsContextException($"Failed to add page binding with name {name}: window['{name}'] already exists!");
            }
            _pageBindings.Add(name, puppeteerFunction);

            const string addPageBinding = @"function addPageBinding(bindingName) {
              const binding = window[bindingName];
              window[bindingName] = (...args) => {
                const me = window[bindingName];
                let callbacks = me['callbacks'];
                if (!callbacks) {
                  callbacks = new Map();
                  me['callbacks'] = callbacks;
                }
                const seq = (me['lastSeq'] || 0) + 1;
                me['lastSeq'] = seq;
                const promise = new Promise((resolve, reject) => callbacks.set(seq, {resolve, reject}));
                binding(JSON.stringify({name: bindingName, seq, args}));
                return promise;
              };
            }";
            var expression = EvaluationString(addPageBinding, name);
            await _devToolsProtocolHelper.Runtime.AddBindingAsync(name).ConfigureAwait(true);
            await _devToolsProtocolHelper.Page.AddScriptToEvaluateOnNewDocumentAsync(expression).ConfigureAwait(true);

            await Task.WhenAll(Frames.Select(
                frame => frame
                    .EvaluateExpressionAsync(expression)
                    .ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                            {
                                _logger.LogError(task.Exception.ToString());
                            }
                        },
                        TaskScheduler.Default)))
                .ConfigureAwait(true);
        }

        private static string EvaluationString(string fun, params object[] args)
        {
            return $"({fun})({string.Join(",", args.Select(SerializeArgument))})";

            string SerializeArgument(object arg)
            {
                return arg == null
                    ? "undefined"
                    : JsonSerializer.Serialize(arg, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebView2DevToolsContext"/> object
        /// </summary>
        /// <remarks>Call <see cref="DisposeAsync"/> when you are finished using the <see cref="WebView2DevToolsContext"/>. The
        /// <see cref="DisposeAsync"/> method leaves the <see cref="WebView2DevToolsContext"/> in an unusable state. After
        /// calling <see cref="DisposeAsync"/>, you must release all references to the <see cref="WebView2DevToolsContext"/> so
        /// the garbage collector can reclaim the memory that the <see cref="WebView2DevToolsContext"/> was occupying.</remarks>
        /// <returns>ValueTask</returns>
        public async ValueTask DisposeAsync()
        {
            FrameManager.Dispose();

            _devToolsProtocolHelper.Page.JavascriptDialogOpening -= OnPageJavascriptDialogOpening;
            _devToolsProtocolHelper.Page.FileChooserOpened -= OnPageFileChooserOpened;
            _devToolsProtocolHelper.Runtime.ExceptionThrown -= OnRuntimeExceptionThrown;

            _devToolsProtocolHelper.Inspector.TargetCrashed -= OnInspectorTargetCrashed;
            _devToolsProtocolHelper.Runtime.BindingCalled -= OnRuntimeBindingCalled;

            try
            {
                await _devToolsProtocolHelper.Runtime.DisableAsync().ConfigureAwait(true);
                await _devToolsProtocolHelper.Network.DisableAsync().ConfigureAwait(true);
                await _devToolsProtocolHelper.Page.SetLifecycleEventsEnabledAsync(false).ConfigureAwait(true);
                await _devToolsProtocolHelper.Page.DisableAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("DsiposeAsync failed", ex);
            }
        }

        /// <summary>
        /// Sets the viewport.
        /// In the case of multiple pages in a single browser, each page can have its own viewport size.
        /// <see cref="SetViewportAsync(ViewPortOptions)"/> will resize the page. A lot of websites don't expect phones to change size, so you should set the viewport before navigating to the page.
        /// </summary>
        /// <param name="viewport">Viewport options.</param>
        /// <returns>A Task that when awaited if returns true then a Reload is required. Otherwise false.</returns>
        public async Task<bool> SetViewportAsync(ViewPortOptions viewport)
        {
            if (viewport == null)
            {
                throw new ArgumentNullException(nameof(viewport));
            }

            var screenOrientation = viewport.IsLandscape ?
                new Emulation.ScreenOrientation
                {
                    Angle = 90,
                    Type = ScreenOrientationType.LandscapePrimary
                } :
                new Emulation.ScreenOrientation
                {
                    Angle = 0,
                    Type = ScreenOrientationType.PortraitPrimary
                };

            await _devToolsProtocolHelper.Emulation.SetDeviceMetricsOverrideAsync(viewport.Width, viewport.Height, viewport.DeviceScaleFactor, viewport.IsMobile, screenOrientation: screenOrientation).ConfigureAwait(true);

            int? touchPoints = viewport.HasTouch ? 10 : null;

            await _devToolsProtocolHelper.Emulation.SetTouchEmulationEnabledAsync(viewport.HasTouch, touchPoints).ConfigureAwait(true);

            var needsReload = viewport.HasTouch || viewport.IsMobile;

            return needsReload;
        }
    }
}
