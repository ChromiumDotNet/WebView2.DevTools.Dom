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
    /// JavascriptHandle represents an in-page JavaScript object. JavascriptHandles can be created with the <see cref="WebView2DevToolsContext.EvaluateExpressionHandleAsync(string)"/> and <see cref="WebView2DevToolsContext.EvaluateFunctionHandleAsync(string, object[])"/> methods.
    /// </summary>
    public class JavascriptHandle : RemoteHandle
    {
        internal JavascriptHandle(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject) : base(context, client, loggerFactory, remoteObject)
        {
        }

        /// <summary>
        /// Returns a <see cref="Dictionary{TKey, TValue}"/> with property names as keys and <see cref="RemoteHandle"/> instances for the property values.
        /// </summary>
        /// <returns>Task which resolves to a <see cref="Dictionary{TKey, TValue}"/></returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var handle = await devToolsContext.EvaluateExpressionHandle("({window, document})");
        /// var properties = await handle.GetPropertiesAsync();
        /// var windowHandle = properties["window"];
        /// var documentHandle = properties["document"];
        /// await handle.DisposeAsync();
        /// ]]>
        /// </code>
        /// </example>
        public async Task<Dictionary<string, JavascriptHandle>> GetPropertiesAsync()
        {
            var response = await DevToolsProtocolHelper.Runtime.GetPropertiesAsync(RemoteObject.ObjectId, ownProperties: true).ConfigureAwait(true);

            var result = new Dictionary<string, JavascriptHandle>();

            foreach (var property in response.Result)
            {
                // TODO: property.Enumerable is a bool not bool?, is this safe to ignore?
                /*
                if (property.Enumerable == null)
                {
                    continue;
                }
                */
                if (property.Value == null)
                {
                    result.Add(property.Name, null);

                    continue;
                }

                result.Add(property.Name, ExecutionContext.CreateJavascriptHandle<JavascriptHandle>(property.Value));
            }
            return result;
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
            var list = new List<object>(args);
            list.Insert(0, this);
            return ExecutionContext.EvaluateFunctionHandleAsync<JavascriptHandle>(pageFunction, list.ToArray());
        }

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="pageFunction">Script to be evaluated in browser context</param>
        /// <param name="args">Function arguments</param>
        /// <typeparam name="T">Type derived from <see cref="RemoteHandle"/></typeparam>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        public Task<T> EvaluateFunctionHandleAsync<T>(string pageFunction, params object[] args)
            where T : RemoteHandle
        {
            return EvaluateFunctionHandleInternalAsync<T>(pageFunction, args);
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
        public Task<JsonElement> EvaluateFunctionAsync(string script, params object[] args)
        {
            return EvaluateFunctionInternalAsync(script, args);
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
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args)
        {
            return EvaluateFunctionInternalAsync<T>(script, args);
        }

        /// <summary>
        /// Returns the corrosponding value of the object
        /// </summary>
        /// <typeparam name="T">A strongly typed object to parse to</typeparam>
        /// <returns>Task</returns>
        /// <remarks>
        /// The method will return an default(T) if the referenced object is not stringifiable. It will throw an error if the object has circular references
        /// </remarks>
        public virtual async Task<T> GetValueAsync<T>()
        {
            var objectId = RemoteObject.ObjectId;

            if (objectId == null)
            {
                return (T)RemoteObjectHelper.ValueFromRemoteObject<T>(RemoteObject);
            }

            try
            {
                var callFunctionOnReturnType = await DevToolsProtocolHelper.Runtime.CallFunctionOnAsync(
                        functionDeclaration: "function() { return this; }",
                        objectId: objectId,
                        returnByValue: true,
                        awaitPromise: true).ConfigureAwait(true);

                return (T)RemoteObjectHelper.ValueFromRemoteObject<T>(callFunctionOnReturnType.Result);
            }
            catch (Exception ex)
            {
                throw new WebView2DevToolsContextException("CallFunctionOnAsync failed", ex);
            }
        }

        /// <summary>
        /// Returns a JSON representation of the object
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>
        /// The method will return an empty JSON if the referenced object is not stringifiable. It will throw an error if the object has circular references
        /// </remarks>
        public async Task<JsonElement> JsonValueAsync() => await GetValueAsync<JsonElement>().ConfigureAwait(true);

        /// <summary>
        /// Fetches a single property from the referenced object
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <returns>Task of <see cref="JavascriptHandle"/></returns>
        public Task<JavascriptHandle> GetPropertyAsync(string propertyName)
        {
            return EvaluateFunctionHandleAsync(
                @"(object, propertyName) => {
                    return object[propertyName];
                }",
                propertyName);
        }

        /// <summary>
        /// Gets the property value for a single property
        /// </summary>
        /// <typeparam name="T">Property Value Type e.g. string, int</typeparam>
        /// <param name="propertyName">property to get</param>
        /// <returns>Task of <typeparamref name="T"/></returns>
        /// <exception cref="WebView2DevToolsContextException">Thrown if no matching property is found</exception>
        public async Task<T> GetPropertyValueAsync<T>(string propertyName)
        {
            var property = await GetPropertyAsync(propertyName).ConfigureAwait(true);

            if (property.RemoteObject.Type == "undefined")
            {
                throw new WebView2DevToolsContextException($"Property {propertyName} was not found.");
            }

            return await property.GetValueAsync<T>().ConfigureAwait(true);
        }
    }
}
