using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom.Tests
{
    public static class DevToolsContextExtensions
    {
        public static Frame FirstChildFrame(this WebView2DevToolsContext ctx) => ctx.Frames.FirstOrDefault(f => f.ParentFrame == ctx.MainFrame);

        public static Task WaitForRenderIdle(this CoreWebView2 coreWebView2, int idleTime = 500)
        {
            var renderIdleTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var idleTimer = new System.Timers.Timer
            {
                Interval = idleTime,
                AutoReset = false
            };

            var syncContext = SynchronizationContext.Current;

            var devToolsProtocolHelper = coreWebView2.GetDevToolsProtocolHelper();

            EventHandler<Page.ScreencastFrameEventArgs> handler = null;

            idleTimer.Elapsed += (sender, args) =>
            {
                idleTimer.Stop();
                idleTimer.Dispose();

                syncContext.Post((s) =>
                {
                    devToolsProtocolHelper.Page.ScreencastFrame -= handler;

                    _ = devToolsProtocolHelper.Page.StopScreencastAsync();
                }, null);                

                renderIdleTcs.TrySetResult(true);
            };

            handler = (s, args) =>
            {
                _ = devToolsProtocolHelper.Page.ScreencastFrameAckAsync(args.SessionId);

                idleTimer.Stop();
                idleTimer.Start();
            };

            idleTimer.Start();

            devToolsProtocolHelper.Page.ScreencastFrame += handler;

            _ = devToolsProtocolHelper.Page.StartScreencastAsync();

            return renderIdleTcs.Task;
        }

        /// <summary>
        /// WebView2 needs to render before attempting to send Keyboard/Mouse clicks, this adds a delay after
        /// setting the content to allow the page to render.
        /// </summary>
        /// <param name="ctx">ctx</param>
        /// <param name="html">html</param>
        /// <param name="delay">delay</param>
        /// <returns>Task</returns>
        public static async Task SetContentAsync(this WebView2DevToolsContext ctx, string html, int delay)
        {
            await ctx.SetContentAsync(html).ConfigureAwait(true);

            await Task.Delay(delay).ConfigureAwait(true);
        }

        public static Task NavigateToAsync(this CoreWebView2 coreWebView2, string url)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<CoreWebView2NavigationCompletedEventArgs> evt = null;

            evt = async (s, args) =>
            {
                coreWebView2.NavigationCompleted -= evt;

                if (args.IsSuccess)
                {
                    // NavigationComplete is called before the page has finished rendering for cases like
                    // our very simple test pages.
                    // Mouse and Keyboard tests require the page to actually finish rendering, so we add a delay here
                    // So WebView2 has time to render. 
                    await Task.Delay(500);

                    tcs.TrySetResult(true);
                }
                else
                {
                    var ex = new Exception("Nav Failed With Error:" + args.WebErrorStatus.ToString());

                    ex.Data.Add("WebErrorStatus", args.WebErrorStatus);

                    tcs.TrySetException(ex);
                }
            };

            coreWebView2.NavigationCompleted += evt;

            coreWebView2.Navigate(url);

            return tcs.Task;
        }

        public static Task NavigateAndWaitForDomContentLoadedAsync(this CoreWebView2 coreWebView2, string url)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<CoreWebView2NavigationCompletedEventArgs> evt = null;
            EventHandler<CoreWebView2DOMContentLoadedEventArgs> domLoadedEvt = null;

            evt = (s, args) =>
            {
                coreWebView2.NavigationCompleted -= evt;
                coreWebView2.DOMContentLoaded -= domLoadedEvt;

                if (!args.IsSuccess)
                {
                    tcs.TrySetException(new Exception("Navigation Failed With Error:" + args.WebErrorStatus.ToString()));
                }
            };

            domLoadedEvt = (s, args) =>
            {
                tcs.TrySetResult(true);
            };

            coreWebView2.NavigationCompleted += evt;
            coreWebView2.DOMContentLoaded += domLoadedEvt;

            coreWebView2.Navigate(url);

            return tcs.Task;
        }

        public static Task<bool> ReloadAsync(this CoreWebView2 coreWebView2)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<CoreWebView2NavigationCompletedEventArgs> evt = null;

            evt = (s, args) =>
            {
                coreWebView2.NavigationCompleted -= evt;

                if (args.IsSuccess)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    tcs.TrySetException(new Exception("Nav Failed With Error:" + args.WebErrorStatus.ToString()));
                }
            };

            coreWebView2.NavigationCompleted += evt;

            coreWebView2.Reload();

            return tcs.Task;
        }

        public static Task WaitForNavigationAsync(this CoreWebView2 coreWebView2)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<CoreWebView2NavigationCompletedEventArgs> evt = null;

            evt = (s, args) =>
            {
                coreWebView2.NavigationCompleted -= evt;

                if (args.IsSuccess)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    tcs.TrySetException(new Exception("Nav Failed With Error:" + args.WebErrorStatus.ToString()));
                }
            };

            coreWebView2.NavigationCompleted += evt;

            return tcs.Task;
        }
    }
}
