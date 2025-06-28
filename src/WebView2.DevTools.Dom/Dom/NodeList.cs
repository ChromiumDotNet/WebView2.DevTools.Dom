using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
#if NETCOREAPP
using System.Collections.Generic;
using System.Threading;
#endif

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// The HTMLCollection interface represents a generic collection (array-like object similar to arguments)
    /// of elements (in document order) and offers methods and properties for selecting from the list.
    /// </summary>
#if NETCOREAPP
    public class NodeList : RemoteHandle, IAsyncEnumerable<Node>
#else
    public class NodeList : RemoteHandle
#endif
    {
        internal NodeList(
            ExecutionContext context,
            DevToolsProtocolHelper client,
            ILoggerFactory loggerFactory,
            Runtime.RemoteObject remoteObject) : base(context, client, loggerFactory, remoteObject)
        {
        }

#if NETCOREAPP
        /// <summary>
        /// Exposes an enumerator that provides asynchronous iteration over values of a specified type.
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>IAsyncEnumerator</returns>
        public async IAsyncEnumerator<Node> GetAsyncEnumerator(CancellationToken token)
        {
            var arr = await GetArray<Node>().ConfigureAwait(true);

            foreach (var element in arr)
            {
                yield return element;
            }
        }
#endif

        /// <summary>
        /// Returns the number of items in the collection.
        /// </summary>
        /// <returns>length</returns>
        public Task<int> GetLengthAsync()
        {
            return EvaluateFunctionInternalAsync<int>("(element) => { return element.length; }");
        }

        /// <summary>
        /// Returns a File object representing the file at the specified index in the file list.
        /// https://developer.mozilla.org/en-US/docs/Web/API/FileList#item
        /// </summary>
        /// <param name="index">The position of the Node to be returned. Elements appear in an HTMLCollection in the same order in which they appear in the document's source.</param>
        /// <returns>
        /// A Task that evaluates to the Node at the specified index,
        /// or null if index is less than zero or greater than or equal to the length property.
        /// </returns>
        public Task<File> ItemAsync(int index)
        {
            return EvaluateFunctionHandleInternalAsync<File>("(element, index) => element.item(index)", index);
        }

        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>Task</returns>
        public async Task<Node[]> ToArrayAsync()
        {
            return (await GetArray<Node>().ConfigureAwait(true)).ToArray();
        }
    }
}
