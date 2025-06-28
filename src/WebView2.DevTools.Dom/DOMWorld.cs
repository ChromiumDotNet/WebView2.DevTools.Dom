using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Helpers;
using WebView2.DevTools.Dom.Input;

namespace WebView2.DevTools.Dom
{
    internal class DOMWorld
    {
        private readonly TimeoutSettings _timeoutSettings;
        private bool _detached;
        private TaskCompletionSource<ExecutionContext> _contextResolveTaskWrapper;
        private TaskCompletionSource<HtmlDocument> _documentCompletionSource;

        internal ICollection<IWaitTask> WaitTasks { get; set; }

        internal Frame Frame { get; }

        public DOMWorld(Frame frame, TimeoutSettings timeoutSettings)
        {
            Frame = frame;
            _timeoutSettings = timeoutSettings;

            _documentCompletionSource = null;
            _contextResolveTaskWrapper = new TaskCompletionSource<ExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);

            WaitTasks = new ConcurrentSet<IWaitTask>();
            _detached = false;
        }

        internal void SetContext(ExecutionContext context)
        {
            if (context == null)
            {
                _documentCompletionSource = null;
                _contextResolveTaskWrapper = new TaskCompletionSource<ExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
            else
            {
                _contextResolveTaskWrapper.TrySetResult(context);

                foreach (var waitTask in WaitTasks)
                {
                    _ = waitTask.Rerun();
                }
            }
        }

        internal bool HasContext => _contextResolveTaskWrapper?.Task.IsCompleted == true;

        internal void Detach()
        {
            _detached = true;
            while (WaitTasks.Count > 0)
            {
                WaitTasks.First().Terminate(new Exception("waitForFunction failed: frame got detached."));
            }
        }

        internal Task<ExecutionContext> GetExecutionContextAsync()
        {
            if (_detached)
            {
                throw new WebView2DevToolsContextException($"Execution Context is not available in detached frame \"{Frame.Url}\"(are you trying to evaluate?)");
            }
            return _contextResolveTaskWrapper.Task;
        }

        internal async Task<JavascriptHandle> EvaluateExpressionHandleAsync(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateExpressionHandleAsync<JavascriptHandle>(script).ConfigureAwait(true);
        }

        internal async Task<JavascriptHandle> EvaluateFunctionHandleAsync(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateFunctionHandleAsync<JavascriptHandle>(script, args).ConfigureAwait(true);
        }

        internal async Task<T> EvaluateFunctionHandleAsync<T>(string script, params object[] args)
            where T : RemoteHandle
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateFunctionHandleAsync<T>(script, args).ConfigureAwait(true);
        }

        internal async Task<T> EvaluateExpressionAsync<T>(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateExpressionAsync<T>(script).ConfigureAwait(true);
        }

        internal async Task<JsonElement> EvaluateExpressionAsync(string script)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateExpressionAsync(script).ConfigureAwait(true);
        }

        internal async Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateFunctionAsync<T>(script, args).ConfigureAwait(true);
        }

        internal async Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args)
        {
            var context = await GetExecutionContextAsync().ConfigureAwait(true);
            return await context.EvaluateFunctionAsync(script, args).ConfigureAwait(true);
        }

        internal async Task<T> QuerySelectorAsync<T>(string selector)
            where T : Element
        {
            var document = await GetDocument().ConfigureAwait(true);
            var value = await document.QuerySelectorAsync<T>(selector).ConfigureAwait(true);
            return value;
        }

        internal async Task<T[]> QuerySelectorAllAsync<T>(string selector)
            where T : Element
        {
            var document = await GetDocument().ConfigureAwait(true);
            var value = await document.QuerySelectorAllAsync<T>(selector).ConfigureAwait(true);
            return value;
        }

        internal async Task<Element[]> XPathAsync(string expression)
        {
            var document = await GetDocument().ConfigureAwait(true);
            var value = await document.XPathAsync(expression).ConfigureAwait(true);
            return value;
        }

        internal Task<string> GetContentAsync() => EvaluateFunctionAsync<string>(
            @"() => {
                let retVal = '';
                if (document.doctype)
                    retVal = new XMLSerializer().serializeToString(document.doctype);
                if (document.documentElement)
                    retVal += document.documentElement.outerHTML;
                return retVal;
            }");

        internal async Task<HtmlElement> AddScriptTagAsync(AddTagOptions options)
        {
            const string addScriptUrl = @"async function addScriptUrl(url, type) {
              const script = document.createElement('script');
              script.src = url;
              if(type)
                script.type = type;
              const promise = new Promise((res, rej) => {
                script.onload = res;
                script.onerror = rej;
              });
              document.head.appendChild(script);
              await promise;
              return script;
            }";
            const string addScriptContent = @"function addScriptContent(content, type = 'text/javascript') {
              const script = document.createElement('script');
              script.type = type;
              script.text = content;
              let error = null;
              script.onerror = e => error = e;
              document.head.appendChild(script);
              if (error)
                throw error;
              return script;
            }";

            async Task<HtmlElement> AddScriptTagPrivate(string script, string urlOrContent, string type)
            {
                var context = await GetExecutionContextAsync().ConfigureAwait(true);
                return string.IsNullOrEmpty(type)
                        ? await context.EvaluateFunctionHandleAsync<HtmlElement>(script, urlOrContent).ConfigureAwait(true)
                        : await context.EvaluateFunctionHandleAsync<HtmlElement>(script, urlOrContent, type).ConfigureAwait(true);
            }

            if (!string.IsNullOrEmpty(options.Url))
            {
                var url = options.Url;
                try
                {
                    return await AddScriptTagPrivate(addScriptUrl, url, options.Type).ConfigureAwait(true);
                }
                catch (WebView2DevToolsContextException)
                {
                    throw new WebView2DevToolsContextException($"Loading script from {url} failed");
                }
            }

            if (!string.IsNullOrEmpty(options.Path))
            {
                var contents = await AsyncFileHelper.ReadAllText(options.Path).ConfigureAwait(true);
                contents += "//# sourceURL=" + options.Path.Replace("\n", string.Empty);
                return await AddScriptTagPrivate(addScriptContent, contents, options.Type).ConfigureAwait(true);
            }

            if (!string.IsNullOrEmpty(options.Content))
            {
                return await AddScriptTagPrivate(addScriptContent, options.Content, options.Type).ConfigureAwait(true);
            }

            throw new ArgumentException("Provide options with a `Url`, `Path` or `Content` property");
        }

        internal async Task<HtmlElement> AddStyleTagAsync(AddTagOptions options)
        {
            const string addStyleUrl = @"async function addStyleUrl(url) {
              const link = document.createElement('link');
              link.rel = 'stylesheet';
              link.href = url;
              const promise = new Promise((res, rej) => {
                link.onload = res;
                link.onerror = rej;
              });
              document.head.appendChild(link);
              await promise;
              return link;
            }";
            const string addStyleContent = @"async function addStyleContent(content) {
              const style = document.createElement('style');
              style.type = 'text/css';
              style.appendChild(document.createTextNode(content));
              const promise = new Promise((res, rej) => {
                style.onload = res;
                style.onerror = rej;
              });
              document.head.appendChild(style);
              await promise;
              return style;
            }";

            if (!string.IsNullOrEmpty(options.Url))
            {
                var url = options.Url;
                try
                {
                    var context = await GetExecutionContextAsync().ConfigureAwait(true);
                    return await context.EvaluateFunctionHandleAsync<HtmlElement>(addStyleUrl, url).ConfigureAwait(true);
                }
                catch (WebView2DevToolsContextException)
                {
                    throw new WebView2DevToolsContextException($"Loading style from {url} failed");
                }
            }

            if (!string.IsNullOrEmpty(options.Path))
            {
                var contents = await AsyncFileHelper.ReadAllText(options.Path).ConfigureAwait(true);
                contents += "//# sourceURL=" + options.Path.Replace("\n", string.Empty);
                var context = await GetExecutionContextAsync().ConfigureAwait(true);
                return await context.EvaluateFunctionHandleAsync<HtmlElement>(addStyleContent, contents).ConfigureAwait(true);
            }

            if (!string.IsNullOrEmpty(options.Content))
            {
                var context = await GetExecutionContextAsync().ConfigureAwait(true);
                return await context.EvaluateFunctionHandleAsync<HtmlElement>(addStyleContent, options.Content).ConfigureAwait(true);
            }

            throw new ArgumentException("Provide options with a `Url`, `Path` or `Content` property");
        }

        internal async Task ClickAsync(string selector, ClickOptions options = null)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            await handle.ClickAsync(options).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        internal async Task HoverAsync(string selector)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            await handle.HoverAsync().ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        internal async Task FocusAsync(string selector)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            await handle.FocusAsync(false).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        internal async Task<string[]> SelectAsync(string selector, params string[] values)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);

            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            var result = await handle.SelectAsync(values).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
            return result;
        }

        internal async Task TapAsync(string selector)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            await handle.TapAsync().ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        internal async Task TypeAsync(string selector, string text, TypeOptions options = null)
        {
            var handle = await QuerySelectorAsync<HtmlElement>(selector).ConfigureAwait(true);
            if (handle == null)
            {
                throw new WebView2DevToolsSelectorException($"No node found for selector: {selector}", selector);
            }
            await handle.TypeAsync(text, options).ConfigureAwait(true);
            await handle.DisposeAsync().ConfigureAwait(true);
        }

        internal Task<T> WaitForSelectorAsync<T>(string selector, WaitForSelectorOptions options = null)
            where T : EventTarget
            => WaitForSelectorOrXPathAsync<T>(selector, false, options);

        internal Task<T> WaitForXPathAsync<T>(string xpath, WaitForSelectorOptions options = null)
            where T : EventTarget
            => WaitForSelectorOrXPathAsync<T>(xpath, true, options);

        internal async Task<JavascriptHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options, params object[] args)
        {
            using var waitTask = new WaitTask(
                 this,
                 script,
                 false,
                 "function",
                 options.Polling,
                 options.PollingInterval,
                 options.Timeout ?? _timeoutSettings.Timeout,
                 args);

            var handle = await waitTask
                .Task
                .ConfigureAwait(true);

            return handle;
        }

        internal async Task<JavascriptHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options)
        {
            using var waitTask = new WaitTask(
                this,
                script,
                true,
                "function",
                options.Polling,
                options.PollingInterval,
                options.Timeout ?? _timeoutSettings.Timeout);

            return await waitTask
                .Task
                .ConfigureAwait(true);
        }

        internal Task<string> GetTitleAsync() => EvaluateExpressionAsync<string>("document.title");

        private async Task<HtmlDocument> GetDocument()
        {
            if (_documentCompletionSource == null)
            {
                _documentCompletionSource = new TaskCompletionSource<HtmlDocument>(TaskCreationOptions.RunContinuationsAsynchronously);
                var context = await GetExecutionContextAsync().ConfigureAwait(true);
                var document = await context.EvaluateExpressionHandleAsync<HtmlDocument>("document").ConfigureAwait(true);
                _documentCompletionSource.TrySetResult(document);
            }
            return await _documentCompletionSource.Task.ConfigureAwait(true);
        }

        private async Task<T> WaitForSelectorOrXPathAsync<T>(string selectorOrXPath, bool isXPath, WaitForSelectorOptions options = null)
            where T : EventTarget
        {
            options = options ?? new WaitForSelectorOptions();
            var timeout = options.Timeout ?? _timeoutSettings.Timeout;

            const string predicate = @"
              function predicate(selectorOrXPath, isXPath, waitForVisible, waitForHidden) {
                const node = isXPath
                  ? document.evaluate(selectorOrXPath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue
                  : document.querySelector(selectorOrXPath);
                if (!node)
                  return waitForHidden;
                if (!waitForVisible && !waitForHidden)
                  return node;
                const element = node.nodeType === Node.TEXT_NODE ? node.parentElement : node;

                const style = window.getComputedStyle(element);
                const isVisible = style && style.visibility !== 'hidden' && hasVisibleBoundingBox();
                const success = (waitForVisible === isVisible || waitForHidden === !isVisible);
                return success ? node : null;

                function hasVisibleBoundingBox() {
                  const rect = element.getBoundingClientRect();
                  return !!(rect.top || rect.bottom || rect.width || rect.height);
                }
              }";
            var polling = options.Visible || options.Hidden ? WaitForFunctionPollingOption.Raf : WaitForFunctionPollingOption.Mutation;

            using var waitTask = new WaitTask(
                this,
                predicate,
                false,
                $"{(isXPath ? "XPath" : "selector")} '{selectorOrXPath}'{(options.Hidden ? " to be hidden" : string.Empty)}",
                polling,
                null,
                timeout,
                new object[] { selectorOrXPath, isXPath, options.Visible, options.Hidden });

            var handle = await waitTask.Task.ConfigureAwait(true);

            if (handle == null)
            {
                return null;
            }

            // No matching DomClass, so we'll Release the handle
            if (string.IsNullOrEmpty(handle.RemoteObject.ClassName))
            {
                await handle.DisposeAsync().ConfigureAwait(true);

                return null;
            }

            // Create a DOM Handle from the JavascriptHandle
            return handle.ExecutionContext.CreateJavascriptHandle<T>(handle.RemoteObject);
        }
    }
}
