namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// options to use with <see cref="Keyboard.DownAsync(string, DownOptions)"/>
    /// </summary>
    public class DownOptions
    {
        /// <summary>
        /// If specified, generates an input event with this text
        /// </summary>
        public string Text { get; set; }
    }
}