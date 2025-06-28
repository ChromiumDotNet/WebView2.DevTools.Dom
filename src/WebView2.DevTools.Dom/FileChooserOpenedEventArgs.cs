using System;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// FileChooserOpenedEventArgs used in conjunction with <see cref="WebView2DevToolsContext.FileChooserOpened"/>
    /// </summary>
    public class FileChooserOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="HtmlInputElement"/> that triggered the file chooser dialog.
        /// </summary>
        public HtmlInputElement Element { get; internal set; }

        /// <summary>
        /// Does the input element support multiple files
        /// </summary>
        public bool Multiple { get; internal set; }
    }
}
