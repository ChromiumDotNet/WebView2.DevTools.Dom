using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Helpers;
using WebView2.DevTools.Dom.Messaging;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The class represents a context for JavaScript execution. Examples of JavaScript contexts are:
    /// Each <see cref="Frame"/> has a separate <see cref="ExecutionContext"/>
    /// All kind of web workers have their own contexts
    /// </summary>
    public class ExecutionContext
    {
        internal const string EvaluationScriptUrl = "__puppeteer_evaluation_script__";

        private readonly string _evaluationScriptSuffix = $"//# sourceURL={EvaluationScriptUrl}";
        private static readonly Regex _sourceUrlRegex = new Regex(@"^[\040\t]*\/\/[@#] sourceURL=\s*(\S*?)\s*$", RegexOptions.Multiline);
        private readonly CoreWebView2 _coreWebView2;
        private readonly DevToolsProtocolHelper _client;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ExecutionContext> _logger;
        private readonly int _contextId;

        internal DOMWorld World { get; }

        internal ExecutionContext(
            CoreWebView2 coreWebView2,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            int contextId,
            DOMWorld world)
        {
            _coreWebView2 = coreWebView2;
            _client = client;
            _loggerFactory = loggerFactory;
            _contextId = contextId;
            World = world;

            _logger = loggerFactory.CreateLogger<ExecutionContext>();
        }

        /// <summary>
        /// Execution Context Id
        /// </summary>
        public int Id => _contextId;

        /// <summary>
        /// Frame associated with this execution context.
        /// </summary>
        /// <remarks>
        /// NOTE Not every execution context is associated with a frame. For example, workers and extensions have execution contexts that are not associated with frames.
        /// </remarks>
        public Frame Frame => World?.Frame;

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="EvaluateExpressionHandleAsync(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<JsonElement> EvaluateExpressionAsync(string script) => EvaluateExpressionAsync<JsonElement>(script);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="EvaluateExpressionHandleAsync(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public async Task<T> EvaluateExpressionAsync<T>(string script)
        {
            var response = await EvaluateExpressionInternalAsync(true, script).ConfigureAwait(true);

            return response == null ? default : (T)RemoteObjectHelper.ValueFromRemoteObject<T>(response);
        }

        internal async Task<T> EvaluateExpressionHandleAsync<T>(string script)
            where T : RemoteHandle
        {
            var response = await EvaluateExpressionInternalAsync(false, script).ConfigureAwait(true);

            return CreateJavascriptHandle<T>(response);
        }

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="JavascriptHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args) => EvaluateFunctionAsync<JsonElement>(script, args);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="JavascriptHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <returns>Task which resolves to script return value</returns>
        public async Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            var response = await EvaluateFunctionInternalAsync(true, script, args).ConfigureAwait(true);

            if (response == null)
            {
                return default(T);
            }

            return (T)RemoteObjectHelper.ValueFromRemoteObject<T>(response);
        }

        internal async Task<T> EvaluateFunctionHandleAsync<T>(string script, params object[] args)
            where T : RemoteHandle
        {
            var runtimeRemoteObject = await EvaluateFunctionInternalAsync(false, script, args).ConfigureAwait(true);

            return CreateJavascriptHandle<T>(runtimeRemoteObject);
        }

        /// <summary>
        /// The method iterates JavaScript heap and finds all the objects with the given prototype.
        /// </summary>
        /// <returns>A task which resolves to a handle to an array of objects with this prototype.</returns>
        /// <param name="prototypeHandle">A handle to the object prototype.</param>
        public async Task<JavascriptHandle> QueryObjectsAsync(JavascriptHandle prototypeHandle)
        {
            if (prototypeHandle == null)
            {
                throw new ArgumentNullException(nameof(prototypeHandle));
            }

            if (prototypeHandle.IsDisposed)
            {
                throw new WebView2DevToolsContextException("Prototype JavascriptHandle is disposed!");
            }

            if (prototypeHandle.RemoteObject.ObjectId == null)
            {
                throw new WebView2DevToolsContextException("Prototype JavascriptHandle must not be referencing primitive value");
            }

            var remoteObject = await _client.Runtime.QueryObjectsAsync(prototypeHandle.RemoteObject.ObjectId).ConfigureAwait(true);

            return CreateJavascriptHandle<JavascriptHandle>(remoteObject);
        }

        private async Task<Runtime.RemoteObject> EvaluateExpressionInternalAsync(bool returnByValue, string script)
        {
            try
            {
                var evaluateRuntimeType = await _client.Runtime.EvaluateAsync(
                       expression: _sourceUrlRegex.IsMatch(script) ? script : $"{script}\n{_evaluationScriptSuffix}",
                       contextId: _contextId,
                       returnByValue: returnByValue,
                       awaitPromise: true,
                       userGesture: true).ConfigureAwait(true);

                if (evaluateRuntimeType.ExceptionDetails != null)
                {
                    var msg = GetExceptionMessage(evaluateRuntimeType.ExceptionDetails);

                    if (msg.Contains("Object reference chain is too long") ||
                        msg.Contains("Object couldn't be returned by value"))
                    {
                        return default;
                    }

                    throw new WebView2DevToolsEvaluationFailedException("Evaluation failed: " + msg);
                }

                return evaluateRuntimeType.Result;
            }
            catch (WebView2DevToolsEvaluationFailedException)
            {
                throw;
            }
            catch (WebView2DevToolsContextException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uknown error - https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609");

                throw new WebView2DevToolsEvaluationFailedException("Failed", ex);
            }
        }

        private async Task<Runtime.RemoteObject> EvaluateFunctionInternalAsync(bool returnByValue, string script, params object[] args)
        {
            try
            {
                var callArguments = args.Select(ConvertToRuntimeCallArgument).ToArray();
                var functionDeclaration = $"{script}\n{_evaluationScriptSuffix}\n";

                var request = new RuntimeCallFunctionOnRequest
                {
                    FunctionDeclaration = functionDeclaration,
                    ExecutionContextId = _contextId,
                    Arguments = callArguments,
                    ReturnByValue = returnByValue,
                    AwaitPromise = true,
                    UserGesture = true
                };

                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(request, serializeOptions);

                var response = await _coreWebView2.CallDevToolsProtocolMethodAsync("Runtime.callFunctionOn", json).ConfigureAwait(true);

                var callFunctionOnReturnType = JsonSerializer.Deserialize<Runtime.CallFunctionOnReturnType>(response);

                // TODO: Report upstream as currently broken, so have to implement custom version
                // var callFunctionOnReturnType = await _client.Runtime.CallFunctionOnAsync(
                //       functionDeclaration: functionDeclaration,
                //       executionContextId: _contextId,
                //       arguments: callArguments,
                //       returnByValue: returnByValue,
                //       awaitPromise: true,
                //       userGesture: true).ConfigureAwait(true);

                if (callFunctionOnReturnType.ExceptionDetails != null)
                {
                    var msg = GetExceptionMessage(callFunctionOnReturnType.ExceptionDetails);

                    if (msg.Contains("Object reference chain is too long") ||
                        msg.Contains("Object couldn't be returned by value"))
                    {
                        return default;
                    }

                    throw new WebView2DevToolsEvaluationFailedException("Evaluation failed: " + msg);
                }

                return callFunctionOnReturnType.Result;
            }
            catch (WebView2DevToolsEvaluationFailedException)
            {
                throw;
            }
            catch (WebView2DevToolsContextException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uknown error - https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609");

                // When https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609 it should be possible to get
                // a meaningful error and distringuish between user error and actual error
                // return default for now
                /*
                throw new EvaluationFailedException(ex.Message, ex);
                */

                // throw new EvaluationFailedException("Failed", ex);

                return default;
            }
        }

        internal T CreateJavascriptHandle<T>(Runtime.RemoteObject remoteObject)
            where T : RemoteHandle
        {
            if (remoteObject == null)
            {
                return default(T);
            }

            var type = typeof(T);

            // Deal with JavascriptHandles early
            if (typeof(JavascriptHandle).IsAssignableFrom(type))
            {
                return (T)(object)new JavascriptHandle(this, _client, _loggerFactory, remoteObject);
            }

            try
            {
                if (!string.IsNullOrEmpty(remoteObject.ClassName))
                {
                    switch (remoteObject.ClassName)
                    {
                        case "CSSStyleDeclaration":
                        {
                            return (T)(object)new CssStyleDeclaration(this, _client, _loggerFactory, remoteObject);
                        }
                        case "HTMLCollection":
                        {
                            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                            return (T)Activator.CreateInstance(type, flags, null, new object[] { this, _client, _loggerFactory, remoteObject }, CultureInfo.InvariantCulture);
                        }
                        case "FileList":
                        {
                            return (T)(object)new FileList(this, _client, _loggerFactory, remoteObject);
                        }
                        case "File":
                        {
                            return (T)(object)new File(this, _client, _loggerFactory, remoteObject);
                        }
                    }

                    var handle = HtmlObjectFactory.CreateObject(remoteObject.ClassName, this, _client, _loggerFactory, remoteObject, Frame.FrameManager.DevToolsContext, Frame.FrameManager);
                    if (handle != null)
                    {
                        return (T)handle;
                    }
                }
            }
            catch (Exception)
            {
                _ = RemoteHandle.ReleaseObjectAsync(_client, remoteObject, _logger);

                throw;
            }

            if (type == typeof(HtmlElement))
            {
                // If T = HtmlElement then we'll release the object if the object isn't a node
                // or the frame is null
                if (remoteObject.Subtype == "node" && Frame != null)
                {
                    return (T)(object)new HtmlElement(this, _client, _loggerFactory, remoteObject, Frame.FrameManager.DevToolsContext, Frame.FrameManager);
                }
            }

            // We cannot map the RemoteObject to the relevant type or is null so we'll release it now.
            _ = RemoteHandle.ReleaseObjectAsync(_client, remoteObject, _logger);

            return null;
        }

        private Runtime.CallArgument ConvertToRuntimeCallArgument(object arg)
        {
            switch (arg)
            {
                case null:
                    {
                        return new Runtime.CallArgument { UnserializableValue = "null" };
                    }

                case BigInteger big:
                    return new Runtime.CallArgument { UnserializableValue = $"{big}n" };

                case int integer when integer == -0:
                    return new Runtime.CallArgument { UnserializableValue = "-0" };

                case double d:
                    if (double.IsPositiveInfinity(d))
                    {
                        return new Runtime.CallArgument { UnserializableValue = "Infinity" };
                    }

                    if (double.IsNegativeInfinity(d))
                    {
                        return new Runtime.CallArgument { UnserializableValue = "-Infinity" };
                    }

                    if (double.IsNaN(d))
                    {
                        return new Runtime.CallArgument { UnserializableValue = "NaN" };
                    }

                    break;

                case RemoteHandle objectHandle:
                    return objectHandle.ToRuntimeCallArgument(this);
            }

            return new Runtime.CallArgument(arg);
        }

        private static string GetExceptionMessage(Runtime.ExceptionDetails exceptionDetails)
        {
            if (exceptionDetails.Exception != null)
            {
                if (exceptionDetails.Exception.Description == null)
                {
                    var json = (JsonElement)exceptionDetails.Exception.Value;

                    return json.GetString();
                }
                return exceptionDetails.Exception.Description;
            }
            var message = exceptionDetails.Text;
            if (exceptionDetails.StackTrace != null)
            {
                foreach (var callframe in exceptionDetails.StackTrace.CallFrames)
                {
                    var location = $"{callframe.Url}:{callframe.LineNumber}:{callframe.ColumnNumber}";
                    var functionName = string.IsNullOrEmpty(callframe.FunctionName) ? "<anonymous>" : callframe.FunctionName;
                    message += $"\n at ${functionName} (${location})";
                }
            }
            return message;
        }

        internal async Task<T> AdoptBackendNodeAsync<T>(int backendNodeId)
            where T : HtmlElement
        {
            var obj = await _client.DOM.ResolveNodeAsync(backendNodeId: backendNodeId, executionContextId: _contextId).ConfigureAwait(true);

            return CreateJavascriptHandle<T>(obj);
        }

        internal async Task<T> AdoptElementHandleAsync<T>(T elementHandle)
            where T : EventTarget
        {
            if (elementHandle.ExecutionContext == this)
            {
                throw new WebView2DevToolsContextException("Cannot adopt handle that already belongs to this execution context");
            }
            if (World == null)
            {
                throw new WebView2DevToolsContextException("Cannot adopt handle without DOMWorld");
            }

            var objectId = elementHandle.RemoteObject.ObjectId;

            var node = await _client.DOM.DescribeNodeAsync(objectId: objectId).ConfigureAwait(true);

            var obj = await _client.DOM.ResolveNodeAsync(backendNodeId: node.BackendNodeId, executionContextId: _contextId).ConfigureAwait(true);

            return CreateJavascriptHandle<T>(obj);
        }
    }
}
