using System;
using WebView2.DevTools.Dom.Media;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Bounding box data returned by <see cref="HtmlElement.BoundingBoxAsync"/>.
    /// </summary>
    public class BoundingBox : IEquatable<BoundingBox>
    {
        /// <summary>
        /// The x coordinate of the element in pixels.
        /// </summary>
        /// <value>The x.</value>
        public double X { get; set; }
        /// <summary>
        /// The y coordinate of the element in pixels.
        /// </summary>
        /// <value>The y.</value>
        public double Y { get; set; }
        /// <summary>
        /// The width of the element in pixels.
        /// </summary>
        /// <value>The width.</value>
        public double Width { get; set; }
        /// <summary>
        /// The height of the element in pixels.
        /// </summary>
        /// <value>The height.</value>
        public double Height { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> class.
        /// </summary>
        public BoundingBox()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public BoundingBox(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal Clip ToClip()
        {
            return new Clip
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height
            };
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((BoundingBox)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="BoundingBox"/> is equal to the current <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="obj">The <see cref="BoundingBox"/> to compare with the current <see cref="BoundingBox"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="BoundingBox"/> is equal to the current
        /// <see cref="BoundingBox"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(BoundingBox obj)
            => obj != null &&
                obj.X == X &&
                obj.Y == Y &&
                obj.Height == Height &&
                obj.Width == Width;

        /// <inheritdoc/>
        public override int GetHashCode()
            => X.GetHashCode() * 397
                ^ Y.GetHashCode() * 397
                ^ Width.GetHashCode() * 397
                ^ Height.GetHashCode() * 397;
    }
}
