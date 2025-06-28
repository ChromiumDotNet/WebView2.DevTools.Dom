namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// The type of button click to use with <see cref="Mouse.DownAsync(ClickOptions)"/>, <see cref="Mouse.UpAsync(ClickOptions)"/> and <see cref="Mouse.ClickAsync(double, double, ClickOptions)"/>
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// Non specified
        /// </summary>
        None,

        /// <summary>
        /// The left mouse button
        /// </summary>
        Left,

        /// <summary>
        /// The right mouse button
        /// </summary>
        Right,

        /// <summary>
        /// The middle mouse button
        /// </summary>
        Middle
    }
}
