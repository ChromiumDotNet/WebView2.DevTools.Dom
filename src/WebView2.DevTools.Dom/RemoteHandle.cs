using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Helpers;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Mirror object referencing original JavaScript/DOM object.
    /// </summary>
    public abstract class RemoteHandle
        : IAsyncDisposable
    {
        /// <summary>
        /// Gets the execution context.
        /// </summary>
        /// <value>The execution context.</value>
        public ExecutionContext ExecutionContext { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RemoteHandle"/> is disposed.
        /// </summary>
        /// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets or sets the remote object.
        /// </summary>
        /// <value>The remote object.</value>
        internal Runtime.RemoteObject RemoteObject { get; }
        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        protected DevToolsProtocolHelper DevToolsProtocolHelper { get; }
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger Logger { get; }

        internal RemoteHandle(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject)
        {
            ExecutionContext = context;
            DevToolsProtocolHelper = client;
            Logger = loggerFactory.CreateLogger(GetType());
            RemoteObject = remoteObject;
        }

        /// <summary>
        /// Disposes the Handle. It will mark the JSHandle as disposed and release the <see cref="RemoteHandle.RemoteObject"/>
        /// </summary>
        /// <returns>The async.</returns>
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
            {
                return;
            }

            GC.SuppressFinalize(this);

            IsDisposed = true;
            await ReleaseObjectAsync(DevToolsProtocolHelper, RemoteObject, Logger).ConfigureAwait(true);
        }

        internal async Task<IEnumerable<T>> GetArray<T>()
            where T : RemoteHandle
        {
            var response = await DevToolsProtocolHelper.Runtime.GetPropertiesAsync(RemoteObject.ObjectId, ownProperties: true).ConfigureAwait(true);

            var result = new List<T>();

            foreach (var property in response.Result)
            {
                if (property.Enumerable == false)
                {
                    continue;
                }

                if (property.Value == null)
                {
                    result.Add(default);

                    continue;
                }

                result.Add(ExecutionContext.CreateJavascriptHandle<T>(property.Value));
            }
            return result;
        }

        internal async Task<IEnumerable<string>> GetStringArray()
        {
            var response = await DevToolsProtocolHelper.Runtime.GetPropertiesAsync(RemoteObject.ObjectId, ownProperties: true).ConfigureAwait(true);

            var result = new List<string>();

            foreach (var property in response.Result)
            {
                if (property.Enumerable == false)
                {
                    continue;
                }

                if (property.Value == null)
                {
                    result.Add(default);

                    continue;
                }

                var remoteObject = property.Value;

                result.Add(remoteObject.Value.ToString());
            }
            return result;
        }

        internal async Task<IEnumerable<KeyValuePair<string, string>>> GetStringMapArray()
        {
            var response = await DevToolsProtocolHelper.Runtime.GetPropertiesAsync(RemoteObject.ObjectId, ownProperties: true).ConfigureAwait(true);

            var result = new List<KeyValuePair<string, string>>();

            foreach (var property in response.Result)
            {
                if (property.Enumerable == false)
                {
                    continue;
                }

                if (property.Value == null)
                {
                    result.Add(default);

                    continue;
                }

                var remoteObject = property.Value;

                var kvp = new KeyValuePair<string, string>(property.Name, remoteObject.Value.ToString());

                result.Add(kvp);
            }
            return result;
        }

        internal static async Task ReleaseObjectAsync(DevToolsProtocolHelper devToolsProtocolHelper, Runtime.RemoteObject remoteObject, ILogger logger)
        {
            if (remoteObject.ObjectId == null)
            {
                return;
            }

            try
            {
                await devToolsProtocolHelper.Runtime.ReleaseObjectAsync(remoteObject.ObjectId).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                // Exceptions might happen in case of a page been navigated or closed.
                // Swallow these since they are harmless and we don't leak anything in this case.
                logger.LogWarning(ex.ToString());
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = GetType().Name;

            if (RemoteObject.ObjectId == null)
            {
                return $"{name}:" + RemoteObjectHelper.ValueFromRemoteObject<object>(RemoteObject, true)?.ToString();
            }

            string type;

            if (string.IsNullOrEmpty(RemoteObject.Subtype))
            {
                type = RemoteObject.Type;
            }
            else
            {
                type = RemoteObject.Subtype != "other"
                    ? RemoteObject.Subtype
                    : RemoteObject.Type;
            }

            return $"{name}@" + type.ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }

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
        internal Task<JsonElement> EvaluateFunctionInternalAsync(string script, params object[] args)
        {
            var list = new List<object>(args);
            list.Insert(0, this);
            return ExecutionContext.EvaluateFunctionAsync<JsonElement>(script, list.ToArray());
        }

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
        internal Task<T> EvaluateFunctionInternalAsync<T>(string script, params object[] args)
        {
            var list = new List<object>(args);
            list.Insert(0, this);
            return ExecutionContext.EvaluateFunctionAsync<T>(script, list.ToArray());
        }

        internal Task<T> EvaluateFunctionHandleInternalAsync<T>(string script, params object[] args)
            where T : RemoteHandle
        {
            var list = new List<object>(args);
            list.Insert(0, this);
            return ExecutionContext.EvaluateFunctionHandleAsync<T>(script, list.ToArray());
        }

        internal Runtime.CallArgument ToRuntimeCallArgument(ExecutionContext context)
        {
            if (ExecutionContext.Id != context.Id)
            {
                throw new WebView2DevToolsEvaluationFailedException("RemoteHandles can be evaluated only in the context they were created!");
            }

            if (IsDisposed)
            {
                var name = GetType().Name;
                throw new WebView2DevToolsEvaluationFailedException($"{name} is disposed!");
            }

            var unserializableValue = RemoteObject.UnserializableValue;

            if (unserializableValue != null)
            {
                return new Runtime.CallArgument { UnserializableValue = unserializableValue };
            }

            if (RemoteObject.ObjectId == null)
            {
                return new Runtime.CallArgument { Value = RemoteObject.Value };
            }

            var objectId = RemoteObject.ObjectId;

            return new Runtime.CallArgument { ObjectId = objectId };
        }
    }
}
