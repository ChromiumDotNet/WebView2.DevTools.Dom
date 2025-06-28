namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// options to use when pressing a key.
    /// </summary>
    /// <seealso cref="Keyboard.PressAsync(string, PressOptions)"/>
    /// <seealso cref="HtmlElement.PressAsync(string, PressOptions)"/>
    public class PressOptions : DownOptions
    {
        /// <summary>
        /// Time to wait between <c>keydown</c> and <c>keyup</c> in milliseconds. Defaults to 0.
        /// </summary>
        public int? Delay { get; set; }

        /// <summary>
        /// Create a new <see cref="PressOptions"/> instance with
        /// the specified delay
        /// </summary>
        /// <param name="delay">delay in ms</param>
        /// <returns>PressOptions</returns>
        public static PressOptions WithDelayInMs(int delay)
        {
            return new PressOptions { Delay = delay };
        }
    }
}
