using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The HTMLInputElement interface provides special properties and methods for manipulating the options, layout, and presentation of input elements.
    /// </summary>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLInputElement" />
    public partial class HtmlInputElement : HtmlElement
    {
        internal HtmlInputElement(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject,
            WebView2DevToolsContext devToolsContext,
            FrameManager frameManager) : base(context, client, loggerFactory, remoteObject, devToolsContext, frameManager)
        {
        }

        /// <summary>
        /// Returns the direction in which selection occurred. This is "forward" if selection was performed
        /// in the start-to-end direction of the current locale, or "backward" for the opposite direction.
        /// This can also be "none" if the direction is unknown.
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>A Task that when awaited sets the value property</returns>
        public Task SetSelectionDirectionAsync(string value)
        {
            return SetPropertyValueAsync("selectionDirection", value);
        }

        /// <summary>
        /// Calls the click() method on the element
        /// Not to be confused with <see cref="HtmlElement.ClickAsync(Input.ClickOptions)"/> which
        /// actually simulates a mouse click, moves the mouse, sends down and up events
        /// /// </summary>
        /// <returns>A Task that when awaited clicks the input element.</returns>
        public Task ClickElementAsync()
        {
            return EvaluateFunctionInternalAsync("(e) => e.click()");
        }

        /// <summary>
        /// A browser picker is shown when the element is one of these types: "date", "month", "week", "time", "datetime-local", "color", or "file". It can also be prepopulated with items from a datalist element or autocomplete attribute.
        /// </summary>
        /// <returns>A Task that when awaited calls the showPicker method on the input element.</returns>
        public Task ShowPickerAsync()
        {
            return EvaluateFunctionInternalAsync("(e) => e.showPicker()");
        }

        /// <summary>
        /// Replaces a range of text in the element with new text.
        /// </summary>
        /// <param name="replacement">replacement text</param>
        /// <param name="start">start index</param>
        /// <param name="end">end index</param>
        /// <param name="selectMode">how the selection should be set after the text has been replaced.</param>
        /// <returns>A Task that when awaited selects the contents of the control</returns>
        public Task SetRangeTextAsync(string replacement, int? start = null, int? end = null, HtmlElementSelectModeType? selectMode = null)
        {
            return EvaluateFunctionInternalAsync("(e, replacement, start, end, selectMode) => e.setRangeText(replacement, start, end, selectMode)", replacement, start, end, selectMode);
        }

        /// <summary>
        /// Selects a range of text in the element (but does not focus it).
        /// </summary>
        /// <param name="start">start index</param>
        /// <param name="end">end index</param>
        /// <param name="selectionDirection ">indicating the direction in which the selection is considered to have been performed.</param>
        /// <returns>A Task that when awaited sets the start and end positions of the current text selection</returns>
        public Task SetSelectionRangeAsync(int start, int end, HtmlElementSelectionDirectionType? selectionDirection = null)
        {
            return EvaluateFunctionInternalAsync("(e, start, end, selectMode) => e.setSelectionRange(start, end, selectMode)", start, end, selectionDirection);
        }

        /// <summary>
        /// Returns current value of the control. If the user enters a value different from the value expected, this may return an empty string.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Task</returns>
        public Task<T> GetValueAsync<T>()
        {
            return EvaluateFunctionInternalAsync<T>("(element) => { return element.value; }");
        }
    }
}
