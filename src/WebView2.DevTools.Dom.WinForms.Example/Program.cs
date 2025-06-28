using System;
using System.Windows.Forms;

namespace WebView2.DevTools.Dom.WinForms.Example.Example
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new BrowserForm());

            return 0;
        }
    }
}
