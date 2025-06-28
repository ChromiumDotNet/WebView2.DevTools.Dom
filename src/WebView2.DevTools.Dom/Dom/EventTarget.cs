using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The EventTarget interface is implemented by objects that can receive events and may have listeners for them.
    /// In other words, any target of events implements the three methods associated with this interface.
    /// </summary>
    public class EventTarget : RemoteHandle
    {
        internal EventTarget(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject) : base(context, client, loggerFactory, remoteObject)
        {
        }

        // TODO: Add support for removeEventListener
        /// <summary>
        /// AddEventListenerAsync
        /// </summary>
        /// <param name="eventType">A case-sensitive string representing the event type to listen for.</param>
        /// <param name="functionName">name of the function that was created using <see cref="WebView2DevToolsContext.ExposeFunctionAsync(string, Action)"/></param>
        /// <returns>Task</returns>
        public Task AddEventListenerAsync(string eventType, string functionName)
        {
            return EvaluateFunctionInternalAsync(
                @"(element, eventType, functionName) =>
                {
                    element.addEventListener(eventType, (evt) => { let f = window[functionName]; f?.(evt); }, false);
                }",
                eventType,
                functionName);
        }
    }
}
