namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// Options to use when typing
    /// </summary>
    /// <seealso cref="WebView2DevToolsContext.TypeAsync(string, string, TypeOptions)"/>
    /// <seealso cref="HtmlElement.TypeAsync(string, TypeOptions)"/>
    /// <seealso cref="Keyboard.TypeAsync(string, TypeOptions)"/>
    public class TypeOptions
    {
        /// <summary>
        /// Time to wait between <c>keydown</c> and <c>keyup</c> in milliseconds. Defaults to 0.
        /// </summary>
        public int? Delay { get; set; }
    }
}
