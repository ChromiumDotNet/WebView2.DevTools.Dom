using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using NodaTime;
using NodaTime.Extensions;
using WebView2.DevTools.Dom;

namespace WebView2.DevTools.Dom.Wpf.Example
{
    /// <summary>
    /// WebView2 DevTools Context Example Main Window
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.DOMContentLoaded += async (s, e) =>
            {
                var devToolsContext = await webView.CoreWebView2.CreateDevToolsContextAsync();

                //webView.CoreWebView2.OpenDevToolsWindow();

                await devToolsContext.ExposeFunctionAsync("jsAlertButtonClick", () =>
                {
                    _ = devToolsContext.EvaluateExpressionAsync("window.alert('Hello! You invoked window.alert()');");
                });

                await devToolsContext.ExposeFunctionAsync("csAlertButtonClick", () =>
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        WindowState = WindowState switch
                        {
                            WindowState.Maximized => WindowState.Normal,
                            WindowState.Normal => WindowState.Maximized,
                            _ => WindowState.Minimized,
                        };
                    });
                });

                var jsAlertButton = await devToolsContext.QuerySelectorAsync("#jsAlertButton");
                var csAlertButton = await devToolsContext.QuerySelectorAsync("#csAlertButton");

                _ = jsAlertButton.AddEventListenerAsync("click", "jsAlertButtonClick");
                _ = csAlertButton.AddEventListenerAsync("click", "csAlertButtonClick");

                var innerText = await jsAlertButton.GetPropertyValueAsync<string>("innerText");

                var currentTimeSpan = await devToolsContext.QuerySelectorAsync("#current-time");
                var fpsSpan = await devToolsContext.QuerySelectorAsync("#fps");

                await devToolsContext.ExposeFunctionAsync<double, bool>("requestAnimationFrameCallback", (highResTime) =>
                {
                    var duration = NodaTime.Duration.FromNanoseconds(Math.Round(highResTime * 1000) * 1000);

                    callback(duration);

                    return false;
                });

                callback(NodaTime.Duration.Zero);

                void callback(NodaTime.Duration timestamp)
                {
                    _ = currentTimeSpan.SetInnerText(GetCurrentDateTime());
                    _ = fpsSpan.SetInnerText(CalculateFps(timestamp).ToString());

                    _ = devToolsContext.EvaluateExpressionAsync(@"window.requestAnimationFrame((x) => { window.requestAnimationFrameCallback(x)});");
                }
            };
            webView.NavigateToString(@"
				<h1>WebView2 DevTools Context</h1>
				<p>The current time is <span id='current-time'></span></p>
				<p>requestAnimationFrame FPS (called from C#) is <span id='fps'></span></p>
				<p>
					<button id='jsAlertButton'>(JS) Show alert</button>
					<br />
					<button id='csAlertButton'>(C#) Change window size</button>
				</p>
			");
        }

        private static string GetCurrentDateTime() =>
            SystemClock.Instance
            .InTzdbSystemDefaultZone()
            .GetCurrentLocalDateTime()
            .ToString("o", null);

        private static readonly Queue<NodaTime.Duration> timestamps =
            new Queue<NodaTime.Duration>();

        private static int CalculateFps(NodaTime.Duration timestamp)
        {
            while (timestamps.TryPeek(out var first) && first <= timestamp - NodaTime.Duration.FromSeconds(1))
            {
                _ = timestamps.Dequeue();
            }

            timestamps.Enqueue(timestamp);

            return timestamps.Count;
        }
    }
}
