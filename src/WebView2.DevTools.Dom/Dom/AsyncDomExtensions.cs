using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// AsyncDomExtensions
    /// </summary>
    public static class AsyncDomExtensions
    {
        /// <summary>
        /// ToArray
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="task">task</param>
        /// <returns>HtmlCollection elements as array</returns>
        public static async Task<T[]> ToArrayAsync<T>(this Task<HtmlCollection<T>> task)
            where T : HtmlElement
        {
            if (task == null)
            {
                return Array.Empty<T>();
            }

            var htmlCollection = await task.ConfigureAwait(true);

            var array = await htmlCollection.ToArrayAsync().ConfigureAwait(true);

            await htmlCollection.DisposeAsync().ConfigureAwait(true);

            return array;
        }

        /// <summary>
        /// ToArrayAsync simplifies getting an Array of Files
        /// </summary>
        /// <param name="task">task</param>
        /// <returns>FileList elements as array</returns>
        public static async Task<File[]> ToArrayAsync(this Task<FileList> task)
        {
            if (task == null)
            {
                return Array.Empty<File>();
            }

            var list = await task.ConfigureAwait(true);

            var array = await list.ToArrayAsync().ConfigureAwait(true);

            await list.DisposeAsync().ConfigureAwait(true);

            return array;
        }

        /// <summary>
        /// GetLengthAsync
        /// </summary>
        /// <param name="task">task</param>
        /// <returns>Length</returns>
        public static async Task<int> GetLengthAsync(this Task<FileList> task)
        {
            if (task == null)
            {
                return 0;
            }

            var fileList = await task.ConfigureAwait(true);

            var length = await fileList.GetLengthAsync().ConfigureAwait(true);

            await fileList.DisposeAsync().ConfigureAwait(true);

            return length;
        }

        /// <summary>
        /// GetLengthAsync
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="task">task</param>
        /// <returns>Length</returns>
        public static async Task<int> GetLengthAsync<T>(this Task<HtmlCollection<T>> task)
            where T : HtmlElement
        {
            if (task == null)
            {
                return 0;
            }

            var htmlCollection = await task.ConfigureAwait(true);

            var length = await htmlCollection.GetLengthAsync().ConfigureAwait(true);

            await htmlCollection.DisposeAsync().ConfigureAwait(true);

            return length;
        }

        /// <summary>
        /// Gets the first <see cref="HtmlTableElement"/> from the tBodies HTMLCollection
        /// for the given table.
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns><see cref="HtmlTableSectionElement"/> or null</returns>
        public static async Task<HtmlTableSectionElement> GetBodyAsync(this Task<HtmlTableElement> task)
        {
            if (task == null)
            {
                return default;
            }

            var tableSection = await task.ConfigureAwait(true);

            if (tableSection == null)
            {
                return default;
            }

            var body = await tableSection.GetBodyAsync().ConfigureAwait(true);

            await tableSection.DisposeAsync().ConfigureAwait(true);

            return body;
        }

        /// <summary>
        /// A helper function for chaining Tasks together, useful for retrieving
        /// a DOM elment then obtaining a single property value.
        /// </summary>
        /// <typeparam name="TIn">RemoteHandle</typeparam>
        /// <typeparam name="TOut">Return Type</typeparam>
        /// <param name="inputTask">input task</param>
        /// <param name="func">func</param>
        /// <param name="dispose">if true (default) the <see cref="RemoteHandle"/> will be disposed after <paramref name="func"/> has been executed</param>
        /// <returns>Task</returns>
        /// <remarks>
        /// Disposing of the <see cref="RemoteHandle"/> only frees our DevTools reference, the object in DOM Element/Javascript Object remains unchanged.
        /// </remarks>
        /// <example>
        /// An chaining method calls together
        ///<code>
        ///<![CDATA[
        /// var type = await DevToolsContext.QuerySelectorAsync("body")
        ///     .AndThen(x => x.QuerySelectorAsync("#agree"))
        ///     .AndThen(x => x.GetAttributeAsync<string>("type"));
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<TOut> AndThen<TIn, TOut>(this Task<TIn> inputTask, Func<TIn, Task<TOut>> func, bool dispose = true)
            where TIn : RemoteHandle
        {
            if (inputTask == null)
            {
                throw new ArgumentNullException(nameof(inputTask));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var input = await inputTask.ConfigureAwait(true);

            var result = await func(input).ConfigureAwait(true);

            if (dispose)
            {
                await input.DisposeAsync().ConfigureAwait(true);
            }

            return result;
        }

        /// <summary>
        /// A helper function for chaining Tasks together, useful for retrieving
        /// a DOM elment then obtaining a single property value.
        /// </summary>
        /// <typeparam name="TIn">RemoteHandle</typeparam>
        /// <param name="inputTask">input task</param>
        /// <param name="func">func</param>
        /// <param name="dispose">if true (default) the <see cref="RemoteHandle"/> will be disposed after <paramref name="func"/> has been executed</param>
        /// <returns>Task</returns>
        /// <remarks>
        /// Disposing of the <see cref="RemoteHandle"/> only frees our DevTools reference, the object in DOM Element/Javascript Object remains unchanged.
        /// </remarks>
        /// <example>
        /// An chaining method calls together
        ///<code>
        ///<![CDATA[
        /// var type = await DevToolsContext.QuerySelectorAsync("body")
        ///     .AndThen(x => x.QuerySelectorAsync("#agree"))
        ///     .AndThen(x => x.GetAttributeAsync<string>("type"));
        /// ]]>
        /// </code>
        /// </example>
        public static async Task AndThen<TIn>(this Task<TIn> inputTask, Func<TIn, Task> func, bool dispose = true)
            where TIn : RemoteHandle
        {
            if (inputTask == null)
            {
                throw new ArgumentNullException(nameof(inputTask));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var input = await inputTask.ConfigureAwait(true);

            await func(input).ConfigureAwait(true);

            if (dispose)
            {
                await input.DisposeAsync().ConfigureAwait(true);
            }
        }
    }
}
