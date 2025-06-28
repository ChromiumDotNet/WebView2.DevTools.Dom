using System;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// <see cref="RemoteHandle"/> and <see cref="HtmlElement"/> Extensions.
    /// </summary>
    public static class RemoteObjectExtensions
    {
        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <param name="elementHandleTask">A task that returns an <see cref="Element"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task</returns>
        /// <exception cref="WebView2DevToolsSelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static Task EvaluateFunctionAsync(this Task<HtmlElement> elementHandleTask, string pageFunction, params object[] args)
            => elementHandleTask.EvaluateFunctionAsync<object>(pageFunction, args);

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <param name="elementHandleTask">A task that returns an <see cref="Element"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task</returns>
        /// <exception cref="WebView2DevToolsSelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static Task EvaluateFunctionAsync(this Task<Element> elementHandleTask, string pageFunction, params object[] args)
            => elementHandleTask.EvaluateFunctionAsync<object>(pageFunction, args);

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="elementHandleTask">A task that returns an <see cref="HtmlElement"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        /// <exception cref="WebView2DevToolsSelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static async Task<T> EvaluateFunctionAsync<T>(this Task<HtmlElement> elementHandleTask, string pageFunction, params object[] args)
        {
            if (elementHandleTask == null)
            {
                throw new ArgumentNullException(nameof(elementHandleTask));
            }

            var elementHandle = await elementHandleTask.ConfigureAwait(true);
            if (elementHandle == null)
            {
                throw new WebView2DevToolsSelectorException("Error: failed to find element matching selector");
            }

            return await elementHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(true);
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="elementHandleTask">A task that returns an <see cref="Element"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        /// <exception cref="WebView2DevToolsSelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static async Task<T> EvaluateFunctionAsync<T>(this Task<Element> elementHandleTask, string pageFunction, params object[] args)
        {
            if (elementHandleTask == null)
            {
                throw new ArgumentNullException(nameof(elementHandleTask));
            }

            var elementHandle = await elementHandleTask.ConfigureAwait(true);
            if (elementHandle == null)
            {
                throw new WebView2DevToolsSelectorException("Error: failed to find element matching selector");
            }

            return await elementHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(true);
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome the <paramref name="elementHandle"/> as the first argument
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="elementHandle">An <see cref="HtmlElement"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        /// <exception cref="WebView2DevToolsSelectorException">If <paramref name="elementHandle"/> is <c>null</c></exception>
        public static async Task<T> EvaluateFunctionAsync<T>(this Element elementHandle, string pageFunction, params object[] args)
        {
            if (elementHandle == null)
            {
                throw new WebView2DevToolsSelectorException("Error: failed to find element matching selector");
            }

            var result = await elementHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(true);
            await elementHandle.DisposeAsync().ConfigureAwait(true);
            return result;
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandleTask"/> as the first argument. Use only after <see cref="WebView2DevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <param name="arrayHandleTask">A task that returns an <see cref="RemoteHandle"/> that represents an array of <see cref="HtmlElement"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task</returns>
        public static Task EvaluateFunctionAsync(this Task<JavascriptHandle> arrayHandleTask, string pageFunction, params object[] args)
            => arrayHandleTask.EvaluateFunctionAsync<object>(pageFunction, args);

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandleTask"/> as the first argument. Use only after <see cref="WebView2DevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="arrayHandleTask">A task that returns an <see cref="RemoteHandle"/> that represents an array of <see cref="HtmlElement"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        public static async Task<T> EvaluateFunctionAsync<T>(this Task<JavascriptHandle> arrayHandleTask, string pageFunction, params object[] args)
        {
            if (arrayHandleTask == null)
            {
                throw new ArgumentNullException(nameof(arrayHandleTask));
            }

            return await (await arrayHandleTask.ConfigureAwait(true)).EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(true);
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandle"/> as the first argument. Use only after <see cref="WebView2DevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="arrayHandle">An <see cref="RemoteHandle"/> that represents an array of <see cref="HtmlElement"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        public static async Task<T> EvaluateFunctionAsync<T>(this JavascriptHandle arrayHandle, string pageFunction, params object[] args)
        {
            if (arrayHandle == null)
            {
                throw new ArgumentNullException(nameof(arrayHandle));
            }

            var result = await arrayHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(true);
            await arrayHandle.DisposeAsync().ConfigureAwait(true);
            return result;
        }
    }
}
