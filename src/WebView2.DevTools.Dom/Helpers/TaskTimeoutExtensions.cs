// The MIT License (MIT)

// Copyright(c).NET Foundation and Contributors
// https://github.com/dotnet/runtime/blob/main/LICENSE.TXT

// Task Async extensions from .Net 6.0
// https://github.com/dotnet/runtime/blob/933988c35c172068652162adf6f20477231f815e/src/libraries/Common/tests/System/Threading/Tasks/TaskTimeoutExtensions.cs#L10
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom.Helpers
{
    internal static class TaskTimeoutExtensions
    {
        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> task, int millisecondsTimeout) =>
            WaitAsync(task, TimeSpan.FromMilliseconds(millisecondsTimeout), default);

        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout) =>
            WaitAsync(task, timeout, default);

        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> task, CancellationToken cancellationToken) =>
            WaitAsync(task, Timeout.InfiniteTimeSpan, cancellationToken);

        public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<TResult>();
            using (new Timer(s => ((TaskCompletionSource<TResult>)s).TrySetException(new TimeoutException()), tcs, timeout, Timeout.InfiniteTimeSpan))
            using (cancellationToken.Register(s => ((TaskCompletionSource<TResult>)s).TrySetCanceled(), tcs))
            {
                return await (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }
    }
}
