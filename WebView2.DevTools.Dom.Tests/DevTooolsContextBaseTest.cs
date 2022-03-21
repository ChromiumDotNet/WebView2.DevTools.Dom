using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using Microsoft.Web.WebView2.WinForms;
using Xunit;
using Xunit.Abstractions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using Chromium.AspNetCore.Bridge;
using System.IO;

namespace WebView2.DevTools.Dom.Tests
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DevTooolsContextBaseTest : IAsyncLifetime
    {
        private readonly bool _ignoreHTTPSerrors;
        private Form _form;
        private AppFunc _appFunc;
        private readonly TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private static int _environmentCounter = 1;
        private readonly Dictionary<string, string> _additionalHeaders = new Dictionary<string, string>();

        public DevTooolsContextBaseTest(ITestOutputHelper output, bool ignoreHTTPSerrors = true)
        {
            _ignoreHTTPSerrors = ignoreHTTPSerrors;
        }

        protected WebView2DevToolsContext DevToolsContext { get; set; }
        protected Microsoft.Web.WebView2.WinForms.WebView2 WebView { get; private set; }

        protected void AddCspHeader(string val)
        {
            _additionalHeaders.Add("Content-Security-Policy", val);
        }

        public async Task InitializeAsync()
        {
            WebView = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill
            };

            _form = new Form
            {
                WindowState = FormWindowState.Normal,
                Text = "WebView2 DevTools Context Test",
                // Puppeteer default size is 800,600
                Size = new System.Drawing.Size(800, 600)
            };

            _form.Controls.Add(WebView);

            var options = new CoreWebView2EnvironmentOptions();

            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2\\UserDataFolder", _environmentCounter++.ToString());

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder, options: options);

            WebView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    _appFunc = DevToolsContextFixture.AppFunc; ;
                    WebView.CoreWebView2.WebResourceRequested += WebViewWebResourceRequestedAsync;
                    WebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                }
            };

            _form.Load += async (s, e) =>
            {
                await WebView.EnsureCoreWebView2Async(environment);

                EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;

                handler = async (sender, args) =>
                {
                    if (args.IsSuccess)
                    {
                        DevToolsContext = await WebView.CoreWebView2.CreateDevToolsContextAsync();
                        await DevToolsContext.IgnoreCertificateErrorsAsync(_ignoreHTTPSerrors).ConfigureAwait(true);

                        DevToolsContext.DefaultTimeout = System.Diagnostics.Debugger.IsAttached ? TestConstants.DebuggerAttachedTestTimeout : TestConstants.DefaultDevToolsTimeout;

                        _taskCompletionSource.TrySetResult(true);
                    }
                    else
                    {
                        var ex = new WebView2DevToolsContextException("Load Error:" + args.WebErrorStatus.ToString());

                        _taskCompletionSource.TrySetException(ex);
                    }

                    WebView.CoreWebView2.NavigationCompleted -= handler;
                };

                WebView.CoreWebView2.NavigationCompleted += handler;

                WebView.CoreWebView2.Navigate(TestConstants.ServerDefaultUrl);
            };

            _form.Show();

            await _taskCompletionSource.Task;
        }

        private async void WebViewWebResourceRequestedAsync(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var request = new ResourceRequest(e.Request.Uri, e.Request.Method, e.Request.Headers, e.Request.Content);

            var response = await RequestInterceptor.ProcessRequest(_appFunc, request);

            foreach(var header in _additionalHeaders)
            {
                response.Headers.Add(header.Key, new string[] { header.Value });
            }

            var coreWebView2 = (CoreWebView2)sender;

            e.Response = coreWebView2.Environment.CreateWebResourceResponse(response.Stream, response.StatusCode, response.ReasonPhrase, response.GetHeaderString());

            deferral.Complete();
        }

        public virtual Task DisposeAsync()
        {
            _form.Close();

            return Task.CompletedTask;            
        }

        protected Task WaitForError()
        {
            var wrapper = new TaskCompletionSource<bool>(TaskContinuationOptions.RunContinuationsAsynchronously);

            void errorEvent(object sender, WebView2.DevTools.Dom.ErrorEventArgs e)
            {
                wrapper.TrySetResult(true);
                DevToolsContext.Error -= errorEvent;
            }

            DevToolsContext.Error += errorEvent;

            return wrapper.Task;
        }
    }
}
