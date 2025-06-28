namespace WebView2.DevTools.Dom.Input
{
    /// <summary>
    /// options to use <see cref="Mouse.MoveAsync(double, double, MoveOptions)"/>
    /// </summary>
    public class MoveOptions
    {
        /// <summary>
        /// Default number of mouse Steps
        /// </summary>
        public const int DefaultSteps = 1;

        /// <summary>
        /// Sends intermediate <c>mousemove</c> events. Defaults to <see cref="DefaultSteps"/>
        /// </summary>
        public int Steps { get; set; } = DefaultSteps;
    }
}
