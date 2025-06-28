namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// <see cref="WebView2DevToolsContext.FrameAttached"/>, <see cref="WebView2DevToolsContext.FrameDetached"/> and <see cref="WebView2DevToolsContext.FrameNavigated"/> arguments.
    /// </summary>
    public class FrameEventArgs
    {
        /// <summary>
        /// Gets or sets the frame.
        /// </summary>
        /// <value>The frame.</value>
        public Frame Frame { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameEventArgs"/> class.
        /// </summary>
        /// <param name="frame">Frame.</param>
        public FrameEventArgs(Frame frame)
        {
            Frame = frame;
        }
    }
}