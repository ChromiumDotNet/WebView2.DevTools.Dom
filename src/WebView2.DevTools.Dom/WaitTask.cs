using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom
{
    internal class WaitTask : IDisposable, IWaitTask
    {
        private readonly DOMWorld _world;
        private readonly string _predicateBody;
        private readonly WaitForFunctionPollingOption _polling;
        private readonly int? _pollingInterval;
        private readonly int _timeout;
        private readonly object[] _args;
        private readonly string _title;
        private readonly Task _timeoutTimer;

        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<JavascriptHandle> _taskCompletion;

        private int _runCount;
        private bool _terminated;
        private bool _isDisposing;

        private const string WaitForPredicatePageFunction = @"
async function waitForPredicatePageFunction(predicateBody, polling, timeout, ...args) {
    const predicate = new Function('...args', predicateBody);
    let timedOut = false;
    if (timeout)
        setTimeout(() => (timedOut = true), timeout);
    if (polling === 'raf')
        return await pollRaf();
    if (polling === 'mutation')
        return await pollMutation();
    if (typeof polling === 'number')
        return await pollInterval(polling);
    /**
     * @return {!Promise<*>}
     */
    async function pollMutation() {
        const success = await predicate(...args);
        if (success)
            return Promise.resolve(success);
        let fulfill;
        const result = new Promise((x) => (fulfill = x));
        const observer = new MutationObserver(async () => {
            if (timedOut) {
                observer.disconnect();
                fulfill();
            }
            const success = await predicate(...args);
            if (success) {
                observer.disconnect();
                fulfill(success);
            }
        });
        observer.observe(document, {
            childList: true,
            subtree: true,
            attributes: true,
        });
        return result;
    }
    async function pollRaf() {
        let fulfill;
        const result = new Promise((x) => (fulfill = x));
        await onRaf();
        return result;
        async function onRaf() {
            if (timedOut) {
                fulfill();
                return;
            }
            const success = await predicate(...args);
            if (success)
                fulfill(success);
            else
                requestAnimationFrame(onRaf);
        }
    }
    async function pollInterval(pollInterval) {
        let fulfill;
        const result = new Promise((x) => (fulfill = x));
        await onTimeout();
        return result;
        async function onTimeout() {
            if (timedOut) {
                fulfill();
                return;
            }
            const success = await predicate(...args);
            if (success)
                fulfill(success);
            else
                setTimeout(onTimeout, pollInterval);
        }
    }
}
";

        internal WaitTask(
            DOMWorld world,
            string predicateBody,
            bool isExpression,
            string title,
            WaitForFunctionPollingOption polling,
            int? pollingInterval,
            int timeout,
            object[] args = null)
        {
            if (string.IsNullOrEmpty(predicateBody))
            {
                throw new ArgumentNullException(nameof(predicateBody));
            }
            if (pollingInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Cannot poll with non-positive interval");
            }

            _world = world;
            _predicateBody = isExpression ? $"return ({predicateBody})" : $"return ({predicateBody})(...args)";
            _polling = polling;
            _pollingInterval = pollingInterval;
            _timeout = timeout;
            _args = args ?? Array.Empty<object>();
            _title = title;

            _cts = new CancellationTokenSource();

            _taskCompletion = new TaskCompletionSource<JavascriptHandle>(TaskCreationOptions.RunContinuationsAsynchronously);

            _world.WaitTasks.Add(this);

            if (timeout > 0)
            {
                _timeoutTimer = System.Threading.Tasks.Task.Delay(timeout, _cts.Token)
                    .ContinueWith(
                        _ => Terminate(new WaitTaskTimeoutException(timeout, title)),
                        TaskScheduler.Default);
            }

            _ = Rerun();
        }

        internal Task<JavascriptHandle> Task => _taskCompletion.Task;

        public async Task Rerun()
        {
            var runCount = Interlocked.Increment(ref _runCount);
            JavascriptHandle success = null;
            Exception exception = null;

            var context = await _world.GetExecutionContextAsync().ConfigureAwait(true);
            try
            {
                success = await context.EvaluateFunctionHandleAsync<JavascriptHandle>(
                    WaitForPredicatePageFunction,
                    new object[] { _predicateBody, _pollingInterval ?? (object)_polling, _timeout }.Concat(_args).ToArray()).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (_terminated || runCount != _runCount)
            {
                if (success != null)
                {
                    await success.DisposeAsync().ConfigureAwait(true);
                }

                return;
            }
            if (exception == null &&
                await _world.EvaluateFunctionAsync<bool>("s => !s", success)
                    .ContinueWith(
                        task => task.IsFaulted || task.Result,
                        TaskScheduler.Default)
                    .ConfigureAwait(true))
            {
                if (success != null)
                {
                    await success.DisposeAsync().ConfigureAwait(true);
                }

                return;
            }

            if (exception?.Message.Contains("Execution context was destroyed") == true)
            {
                _ = Rerun();
                return;
            }

            if (exception?.Message.Contains("Cannot find context with specified id") == true)
            {
                return;
            }

            if (exception != null)
            {
                _taskCompletion.TrySetException(exception);
            }
            else
            {
                _taskCompletion.TrySetResult(success);
            }
            Cleanup();
        }

        public void Terminate(Exception exception)
        {
            _terminated = true;
            _taskCompletion.TrySetException(exception);
            Cleanup();
        }

        private void Cleanup()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _world.WaitTasks.Remove(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (_isDisposing)
            {
                return;
            }

            _cts.Dispose();

            _isDisposing = true;
        }
    }
}
