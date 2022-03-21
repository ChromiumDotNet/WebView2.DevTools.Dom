using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace WebView2.DevTools.Dom.Tests
{
    public static class DevToolsContextExtensions
    {
        public static Frame FirstChildFrame(this WebView2DevToolsContext ctx) => ctx.Frames.FirstOrDefault(f => f.ParentFrame == ctx.MainFrame);

        public static Task NavigateToAsync(this CoreWebView2 coreWebView2, string url)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<CoreWebView2NavigationCompletedEventArgs> evt = null;

            evt = async (s, args) =>
            {
                coreWebView2.NavigationCompleted -= evt;

                if(args.IsSuccess)
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
                    tcs.TrySetException(new Exception("Nav Failed With Error:" + args.WebErrorStatus.ToString()));
                }                
            };

            coreWebView2.NavigationCompleted += evt;

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
