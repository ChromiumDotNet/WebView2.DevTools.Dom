namespace WebView2.DevTools.Dom.Media
{
    /// <summary>
    /// Clip data.
    /// </summary>
    /// <seealso cref="BoundingBox.ToClip"/>
    public class Clip
    {
        /// <summary>
        /// x-coordinate of top-left corner of clip area.
        /// </summary>
        /// <value>The x.</value>
        public double X { get; set; }
        /// <summary>
        /// y-coordinate of top-left corner of clip area.
        /// </summary>
        /// <value>The y.</value>
        public double Y { get; set; }
        /// <summary>
        /// Width of clipping area.
        /// </summary>
        /// <value>The width.</value>
        public double Width { get; set; }
        /// <summary>
        /// Height of clipping area.
        /// </summary>
        /// <value>The height.</value>
        public double Height { get; set; }
        /// <summary>
        /// Scale of the webpage rendering. Defaults to 1.
        /// </summary>
        /// <value>The scale.</value>
        public int Scale { get; set; } = 1;
    }
}
