using System;
using System.Windows.Forms;

namespace WebView2.DevTools.Dom.WinForms.Example
{
    public partial class BrowserForm : Form
    {
#if DEBUG
        private const string Build = "Debug";
#else
        private const string Build = "Release";
#endif
        private readonly string _windowTitle = "WebView2.DevTools.Dom.WinForms.Example (" + Build + ")";
        private readonly Microsoft.Web.WebView2.WinForms.WebView2 _webView2;

        public BrowserForm()
        {
            InitializeComponent();

            Text = _windowTitle;
            WindowState = FormWindowState.Maximized;

            _webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            _webView2.Dock = DockStyle.Fill;
            toolStripContainer.ContentPanel.Controls.Add(_webView2);

            Load += BrowserFormLoad;
        }

        private async void BrowserFormLoad(object? sender, EventArgs e)
        {
            await _webView2.EnsureCoreWebView2Async();

            var coreWebView2 = _webView2.CoreWebView2;

            coreWebView2.NavigationCompleted += OnWebView2NavigationCompleted;
            coreWebView2.DocumentTitleChanged += OnWebView2DocumentTitleChanged;
            coreWebView2.SourceChanged += OnWebView2SourceChanged;

            var version = _webView2.CoreWebView2.Environment.BrowserVersionString;

            var environment = string.Format("Environment: {0}, Runtime: {1}",
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
                System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

            DisplayOutput(string.Format("{0}, {1}", version, environment));

            _webView2.CoreWebView2.Navigate("https://google.com");
        }

        private void OnWebView2SourceChanged(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = _webView2.Source.ToString());
        }

        private void OnWebView2DocumentTitleChanged(object? sender, object e)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = _windowTitle + " - " + _webView2.CoreWebView2.DocumentTitle);
        }

        private void OnWebView2NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            
            SetCanGoBack(_webView2.CoreWebView2.CanGoBack);
            SetCanGoForward(_webView2.CoreWebView2.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(false));
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            if(_webView2.CanGoBack)
            {
                _webView2.GoBack();
            }
            
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            if (_webView2.CanGoForward)
            {
                _webView2.GoForward();
            }
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                //TODO: Better url parsing
                _webView2.CoreWebView2.Navigate(url.StartsWith("http") ? url : "https://" + url);
            }
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            _webView2.CoreWebView2.OpenDevToolsWindow();
        }

        private async void HighlightLinksToolStripMenuItemClick(object sender, EventArgs e)
        {
            var devToolsContext = await _webView2.CoreWebView2.CreateDevToolsContextAsync();

            var links = await devToolsContext.QuerySelectorAllAsync("a");

            foreach (var link in links)
            {
                _ = await link.EvaluateFunctionAsync("e => e.style.backgroundColor = 'yellow'");
            }

            await devToolsContext.DisposeAsync();
        }

        private void InvokeOnUiThreadIfRequired(Action action)
        {
            //If you are planning on using a similar function in your own code then please be sure to
            //have a quick read over https://stackoverflow.com/questions/1874728/avoid-calling-invoke-when-the-control-is-disposed
            //No action
            if (Disposing || IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        private async void ScrollLastElementIntoViewToolStripMenuItemClick(object sender, EventArgs e)
        {
            var devToolsContext = await _webView2.CoreWebView2.CreateDevToolsContextAsync();

            var body = await devToolsContext.QuerySelectorAsync("body");

            var lastElement = await body.EvaluateFunctionHandleAsync<HtmlElement>("e => e.lastElementChild");

            await lastElement.ScrollIntoViewIfNeededAsync();

            await devToolsContext.DisposeAsync();
        }
    }
}
