using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Input;

namespace WebView2.DevTools.Dom
{
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1601 // Partial elements should be documented
#nullable enable
    public static partial class HtmlObjectFactory
    {
        internal static object? CreateObject(string className, ExecutionContext executionContext, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager)
        {
            return CreateObjectInternal(className, executionContext, client, loggerFactory, remoteObject, devToolsContext, frameManager);
        }
    }

    public partial class DocumentType
    {
        internal DocumentType(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class Range
    {
        internal Range(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class StyleSheetList
    {
        internal StyleSheetList(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class StringList
    {
        internal StringList(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class ValidityState
    {
        internal ValidityState(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class MediaList
    {
        internal MediaList(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class StyleSheet
    {
        internal StyleSheet(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    /// <summary>
    /// Node partial
    /// </summary>
    public partial class Node
    {
        internal Node(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class UrlUtilities
    {
        internal UrlUtilities(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class NavigatorId
    {
        internal NavigatorId(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class Navigator
    {
        internal Navigator(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class Location
    {
        internal Location(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class History
    {
        internal History(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class Window
    {
        internal Window(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class HtmlBaseElement
    {
        internal HtmlBaseElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlBodyElement
    {
        internal HtmlBodyElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlBreakRowElement
    {
        internal HtmlBreakRowElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlDetailsElement
    {
        internal HtmlDetailsElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlDialogElement
    {
        internal HtmlDialogElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlEmbedElement
    {
        internal HtmlEmbedElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlFieldSetElement
    {
        internal HtmlFieldSetElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlHeadElement
    {
        internal HtmlHeadElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlHeadingElement
    {
        internal HtmlHeadingElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlHrElement
    {
        internal HtmlHrElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlInlineFrameElement
    {
        internal HtmlInlineFrameElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlKeygenElement
    {
        internal HtmlKeygenElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlLabelElement
    {
        internal HtmlLabelElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlListItemElement
    {
        internal HtmlListItemElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlAreaElement
    {
        internal HtmlAreaElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTitleElement
    {
        internal HtmlTitleElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlUnorderedListElement
    {
        internal HtmlUnorderedListElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlUnknownElement
    {
        internal HtmlUnknownElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTimeElement
    {
        internal HtmlTimeElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTemplateElement
    {
        internal HtmlTemplateElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlCommandElement
    {
        internal HtmlCommandElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlDataElement
    {
        internal HtmlDataElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlDocument
    {
        internal HtmlDocument(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }

        /// <summary>
        /// The method runs <c>element.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="HtmlElement"/> or derived type</typeparam>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to a <see cref="HtmlElement"/> or derived type pointing to the frame element</returns>
        public async Task<T> QuerySelectorAsync<T>(string selector)
            where T : Element
        {
            var handle = await EvaluateFunctionHandleInternalAsync<T>(
                "(element, selector) => element.querySelector(selector)",
                selector).ConfigureAwait(true);

            return handle;
        }

        /// <summary>
        /// The method runs <c>element.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to <see cref="HtmlElement"/> pointing to the frame element</returns>
        public Task<Element> QuerySelectorAsync(string selector)
        {
            return QuerySelectorAsync<Element>(selector);
        }

        /// <summary>
        /// Runs <c>element.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="HtmlElement"/></typeparam>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        public async Task<T[]> QuerySelectorAllAsync<T>(string selector)
            where T : Element
        {
            var arrayHandle = await EvaluateFunctionHandleInternalAsync<JavascriptHandle>(
                "(element, selector) => element.querySelectorAll(selector)",
                selector).ConfigureAwait(true);

            var properties = await arrayHandle.GetArray<T>().ConfigureAwait(true);
            await arrayHandle.DisposeAsync().ConfigureAwait(true);

            return properties.ToArray();
        }

        /// <summary>
        /// Runs <c>element.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        public Task<Element[]> QuerySelectorAllAsync(string selector)
        {
            return QuerySelectorAllAsync<Element>(selector);
        }

        /// <summary>
        /// Evaluates the XPath expression relative to the elementHandle. If there's no such element, the method will resolve to <c>null</c>.
        /// </summary>
        /// <param name="expression">Expression to evaluate <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/evaluate"/></param>
        /// <returns>Task which resolves to an array of <see cref="HtmlElement"/></returns>
        public async Task<Element[]> XPathAsync(string expression)
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
    }

    public partial class HtmlLegendElement
    {
        internal HtmlLegendElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMapElement
    {
        internal HtmlMapElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMarqueeElement
    {
        internal HtmlMarqueeElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlHtmlElement
    {
        internal HtmlHtmlElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMenuItemElement
    {
        internal HtmlMenuItemElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMetaElement
    {
        internal HtmlMetaElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMeterElement
    {
        internal HtmlMeterElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlModElement
    {
        internal HtmlModElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlObjectElement
    {
        internal HtmlObjectElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlOrderedListElement
    {
        internal HtmlOrderedListElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlOutputElement
    {
        internal HtmlOutputElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlParamElement
    {
        internal HtmlParamElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlPictureElement
    {
        internal HtmlPictureElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlPreElement
    {
        internal HtmlPreElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlProgressElement
    {
        internal HtmlProgressElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlQuoteElement
    {
        internal HtmlQuoteElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlScriptElement
    {
        internal HtmlScriptElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlSourceElement
    {
        internal HtmlSourceElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlStyleElement
    {
        internal HtmlStyleElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTableColumnElement
    {
        internal HtmlTableColumnElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTableDataCellElement
    {
        internal HtmlTableDataCellElement(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    /// <summary>
    /// Element is the most general base class from which all element objects (i.e. objects that represent elements) in a Document inherit.
    /// It only has methods and properties common to all kinds of elements. More specific classes inherit from Element.
    /// </summary>
    public partial class Element
    {
        internal Element(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }

        /// <summary>
        /// Readonly HtmlElement ClassName e.g. HTMLScriptElement, HTMLDivElement
        /// </summary>
        public string ClassName => RemoteObject?.ClassName ?? string.Empty;

        /// <summary>
        /// Readonly HtmlElement Description
        /// </summary>
        public string Description => RemoteObject?.Description ?? string.Empty;

        /// <summary>
        /// Triggers a `change` and `input` event once all the provided options have been selected.
        /// If there's no `select` element matching `selector`, the method throws an exception.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// await handle.SelectAsync("blue"); // single selection
        /// await handle.SelectAsync("red", "green", "blue"); // multiple selections
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="values">Values of options to select. If the `select` has the `multiple` attribute, all values are considered, otherwise only the first one is taken into account.</param>
        /// <returns>A task that resolves to an array of option values that have been successfully selected.</returns>
        public Task<string[]> SelectAsync(params string[] values)
            => EvaluateFunctionInternalAsync<string[]>(
                @"(element, values) =>
                {
                    if (element.nodeName.toLowerCase() !== 'select')
                        throw new Error('Element is not a <select> element.');

                    const options = Array.from(element.options);
                    element.value = undefined;
                    for (const option of options) {
                        option.selected = values.includes(option.value);
                        if (option.selected && !element.multiple)
                            break;
                    }
                    element.dispatchEvent(new Event('input', { 'bubbles': true }));
                    element.dispatchEvent(new Event('change', { 'bubbles': true }));
                    return options.filter(option => option.selected).map(option => option.value);
                }",
                new[] { values });

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="RemoteHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task</returns>
        public Task EvaluateFunctionAsync(string script, params object[] args)
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
        /// Get element attribute value
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="attribute">attribute</param>
        /// <returns>Task which resolves to the attributes value.</returns>
        public async Task<T> GetAttributeAsync<T>(string attribute)
        {
            var attr = await EvaluateFunctionHandleInternalAsync<JavascriptHandle>("(element, attr) => element.getAttribute(attr)", attribute).ConfigureAwait(true);

            var val = await attr.GetValueAsync<T>().ConfigureAwait(true);

            return val;
        }

        /// <summary>
        /// Set element attribute value
        /// </summary>
        /// <param name="attribute">attribute name</param>
        /// <param name="value">attribute value</param>
        /// <returns>Task which resolves when the attribute value has been set.</returns>
        public Task SetAttributeAsync(string attribute, object value)
        {
            return EvaluateFunctionInternalAsync("(element, attr, val) => element.setAttribute(attr, val)", attribute, value);
        }

        /// <summary>
        /// The method runs <c>element.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="HtmlElement"/> or derived type</typeparam>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to a <see cref="HtmlElement"/> or derived type pointing to the frame element</returns>
        public async Task<T> QuerySelectorAsync<T>(string selector)
            where T : Element
        {
            var handle = await EvaluateFunctionHandleInternalAsync<T>(
                "(element, selector) => element.querySelector(selector)",
                selector).ConfigureAwait(true);

            return handle;
        }

        /// <summary>
        /// The method runs <c>element.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to <see cref="HtmlElement"/> pointing to the frame element</returns>
        public Task<Element> QuerySelectorAsync(string selector)
        {
            return QuerySelectorAsync<Element>(selector);
        }

        /// <summary>
        /// Runs <c>element.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="HtmlElement"/></typeparam>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        public async Task<T[]> QuerySelectorAllAsync<T>(string selector)
            where T : Element
        {
            var arrayHandle = await EvaluateFunctionHandleInternalAsync<JavascriptHandle>(
                "(element, selector) => element.querySelectorAll(selector)",
                selector).ConfigureAwait(true);

            var properties = await arrayHandle.GetArray<T>().ConfigureAwait(true);
            await arrayHandle.DisposeAsync().ConfigureAwait(true);

            return properties.ToArray();
        }

        /// <summary>
        /// Runs <c>element.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        public Task<Element[]> QuerySelectorAllAsync(string selector)
        {
            return QuerySelectorAllAsync<Element>(selector);
        }

        /// <summary>
        /// A utility function to be used with <see cref="RemoteObjectExtensions.EvaluateFunctionAsync{T}(Task{JavascriptHandle}, string, object[])"/>
        /// </summary>
        /// <param name="selector">A selector to query element for</param>
        /// <returns>Task which resolves to a <see cref="JavascriptHandle"/> of <c>document.querySelectorAll</c> result</returns>
        public Task<JavascriptHandle> QuerySelectorAllHandleAsync(string selector)
            => ExecutionContext.EvaluateFunctionHandleAsync<JavascriptHandle>(
                "(element, selector) => Array.from(element.querySelectorAll(selector))", this, selector);

        ///  <summary>
        ///  Inserts nodes just before the current node.
        ///  </summary>
        ///  <param name="nodes">The nodes to insert.</param>
        ///  <returns>Task</returns>
        public virtual Task BeforeAsync(params Node[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Length == 0)
            {
                throw new ArgumentException("Must specify at least one node.");
            }

            return EvaluateFunctionInternalAsync("(element, nodes) => { element.before(nodes); }", nodes);
        }

        ///  <summary>
        ///  Inserts nodes just after the current node.
        ///  </summary>
        ///  <param name="nodes">The nodes to insert.</param>
        ///  <returns>Task</returns>
        public virtual Task AfterAsync(params Node[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Length == 0)
            {
                throw new ArgumentException("Must specify at least one node.");
            }

            return EvaluateFunctionInternalAsync("(element, nodes) => { element.after(nodes); }", nodes);
        }

        ///  <summary>
        ///  Replaces the current node with nodes.
        ///  </summary>
        ///  <param name="nodes">The nodes to insert.</param>
        ///  <returns>Task</returns>
        public virtual Task ReplaceAsync(params Node[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Length == 0)
            {
                throw new ArgumentException("Must specify at least one node.");
            }

            return EvaluateFunctionInternalAsync("(element, nodes) => { element.replace(nodes); }", nodes);
        }

        ///  <summary>
        ///  Removes the current node.
        ///  </summary>
        ///  <returns>Task</returns>
        public virtual Task RemoveAsync()
        {
            return EvaluateFunctionInternalAsync("(element) => { element.remove(); }");
        }

        ///  <summary>
        ///  Appends nodes to current document.
        ///  </summary>
        ///  <param name="nodes">The nodes to append.</param>
        ///  <returns>Task</returns>
        public virtual Task AppendAsync(params Node[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Length == 0)
            {
                throw new ArgumentException("Must append at least one node.");
            }

            return EvaluateFunctionInternalAsync("(element, nodes) => { element.append(nodes); }", nodes);
        }

        ///  <summary>
        ///  Prepends nodes to the current document.
        ///  </summary>
        ///  <param name="nodes">The nodes to prepend.</param>
        ///  <returns>Task</returns>
        public virtual Task PrependAsync(params Node[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Length == 0)
            {
                throw new ArgumentException("Must prepend at least one node.");
            }

            return EvaluateFunctionInternalAsync("(element, nodes) => { element.prepend(nodes); }", nodes);
        }

        ///  <summary>
        ///  Gets the child elements.
        ///  </summary>
        ///  <typeparam name="T">Type</typeparam>
        ///  <returns>Task</returns>
        public virtual Task<HtmlCollection<T>> GetChildrenAsync<T>()
            where T : Element
        {
            return EvaluateFunctionHandleInternalAsync<HtmlCollection<T>>("(element) => { return element.children; }");
        }

        ///  <summary>
        ///  Gets the first child element of this element.
        ///  </summary>
        ///  <typeparam name="T">Type</typeparam>
        ///  <returns>Task</returns>
        public virtual Task<T?> GetFirstElementChildAsync<T>()
            where T : Element
        {
            return EvaluateFunctionHandleInternalAsync<T?>("(element) => { return element.firstElementChild; }");
        }

        ///  <summary>
        ///  Gets the last child element of this element.
        ///  </summary>
        ///  <typeparam name="T">Type</typeparam>
        ///  <returns>Task</returns>
        public virtual Task<T?> GetLastElementChildAsync<T>()
            where T : Element
        {
            return EvaluateFunctionHandleInternalAsync<T?>("(element) => { return element.lastElementChild; }");
        }

        ///  <summary>
        ///  Gets the Element immediately following this ChildNode in its
        ///  parent's children list, or null if there is no Element in the list
        ///  following this ChildNode.
        ///  </summary>
        ///  <typeparam name="T">type</typeparam>
        ///  <returns>Element</returns>
        public virtual Task<T?> GetNextElementSiblingAsync<T>()
            where T : Element
        {
            return EvaluateFunctionHandleInternalAsync<T?>("(element) => { return element.nextElementSibling; }");
        }

        ///  <summary>
        ///  Gets the Element immediately prior to this ChildNode in its
        ///  parent's children list, or null if there is no Element in the list
        ///  prior to this ChildNode.
        ///  </summary>
        ///  <typeparam name="T">type</typeparam>
        ///  <returns>Element</returns>
        public virtual Task<T?> GetPreviousElementSiblingAsync<T>()
            where T : Element
        {
            return EvaluateFunctionHandleInternalAsync<T?>("(element) => { return element.previousElementSibling; }");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (RemoteObject.ObjectId == null)
            {
                return base.ToString();
            }

            if (!string.IsNullOrEmpty(RemoteObject.ClassName))
            {
                return $"{RemoteObject.ClassName}@{RemoteObject.Description}";
            }

            return base.ToString();
        }

        internal BoxModelPoint[] FromProtocolQuad(double[] quad) => new[]
        {
            new BoxModelPoint { X = quad[0], Y = quad[1] },
            new BoxModelPoint { X = quad[2], Y = quad[3] },
            new BoxModelPoint { X = quad[4], Y = quad[5] },
            new BoxModelPoint { X = quad[6], Y = quad[7] }
        };
    }

    /// <summary>
    /// The Document interface represents any web page loaded in the browser and serves as an entry point into the web page's content, which is the DOM tree.
    /// </summary>
    public partial class Document : Node
    {
        internal Document(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class CharacterData
    {
        internal CharacterData(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class Text
    {
        internal Text(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    /// <summary>
    /// The ShadowRoot interface represents the shadow root.
    /// </summary>
    public partial class ShadowRoot : DocumentFragment
    {
        internal ShadowRoot(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    /// <summary>
    /// The DocumentFragment interface represents a minimal document object
    /// that has no parent.
    /// </summary>
    public partial class DocumentFragment : RemoteHandle
    {
        internal DocumentFragment(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    /// <summary>
    /// The Attr interface represents one of an element's attributes as an object. In most situations,
    /// you will directly retrieve the attribute value as a string (e.g., Element.getAttribute()),
    /// but certain functions (e.g., Element.getAttributeNode()) or means of iterating return Attr instances.
    /// </summary>
    public partial class Attr
    {
        internal Attr(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (RemoteObject == null)
            {
                return base.ToString();
}

            return $"{RemoteObject.ClassName}@{RemoteObject.Description}";
        }
    }

#if NETCOREAPP
    public partial class NamedNodeMap : IAsyncEnumerable<Attr>
#else
    public partial class NamedNodeMap
#endif
    {
        internal NamedNodeMap(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }

#if NETCOREAPP
        /// <summary>
        /// Exposes an enumerator that provides asynchronous iteration over values of a specified type.
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>IAsyncEnumerator</returns>
        public async IAsyncEnumerator<Attr> GetAsyncEnumerator(CancellationToken token)
        {
            var arr = await GetArray<Attr>().ConfigureAwait(true);

            foreach (var element in arr)
            {
                yield return element;
            }
        }
#endif

        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>Task</returns>
        public async Task<Attr[]> ToArrayAsync()
        {
            return (await GetArray<Attr>().ConfigureAwait(true)).ToArray();
        }
    }

#if NETCOREAPP
    public partial class TokenList : IAsyncEnumerable<string>
#else
    public partial class TokenList
#endif
    {
        internal TokenList(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }

#if NETCOREAPP
        /// <summary>
        /// Exposes an enumerator that provides asynchronous iteration over values of a specified type.
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>IAsyncEnumerator</returns>
        public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken token)
        {
            var arr = await GetStringArray().ConfigureAwait(true);

            foreach (var element in arr)
            {
                yield return element;
            }
        }
#endif
        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>Task</returns>
        public async Task<string[]> ToArrayAsync()
        {
            return (await GetStringArray().ConfigureAwait(true)).ToArray();
        }
    }

#if NETCOREAPP
    public partial class StringMap : IAsyncEnumerable<KeyValuePair<string, string>>
#else
    public partial class StringMap
#endif
    {
        internal StringMap(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }

#if NETCOREAPP
        /// <summary>
        /// Exposes an enumerator that provides asynchronous iteration over values of a specified type.
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>IAsyncEnumerator</returns>
        public async IAsyncEnumerator<KeyValuePair<string, string>> GetAsyncEnumerator(CancellationToken token)
        {
            var arr = await GetStringMapArray().ConfigureAwait(true);

            foreach (var element in arr)
            {
                yield return element;
            }
        }
#endif
        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>Task</returns>
        public async Task<KeyValuePair<string, string>[]> ToArrayAsync()
        {
            return (await GetStringMapArray().ConfigureAwait(true)).ToArray();
        }
    }

    public partial class SettableTokenList
    {
        internal SettableTokenList(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlOptionsCollection
    {
        internal HtmlOptionsCollection(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class HtmlFormControlsCollection
    {
        internal HtmlFormControlsCollection(ExecutionContext context, DevToolsProtocolHelper client, ILoggerFactory loggerFactory, Runtime.RemoteObject remoteObject, WebView2DevToolsContext devToolsContext, FrameManager frameManager) : base(context, client, loggerFactory, remoteObject)
        {
        }
    }

    public partial class HtmlOptionsGroupElement
    {
        internal HtmlOptionsGroupElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlDataListElement
    {
        internal HtmlDataListElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlTableCaptionElement
    {
        internal HtmlTableCaptionElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }

    public partial class HtmlMenuElement
    {
        internal HtmlMenuElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }
    }
#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1402 // File may only contain a single type
}
