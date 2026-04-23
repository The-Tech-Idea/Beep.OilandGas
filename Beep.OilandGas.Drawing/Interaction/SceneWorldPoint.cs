using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Represents a persisted world-space coordinate for interaction annotations.
    /// </summary>
    public sealed class SceneWorldPoint
    {
        /// <summary>
        /// Gets or sets the world-space X coordinate.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the world-space Y coordinate.
        /// </summary>
        public double Y { get; set; }

        public static SceneWorldPoint From(SKPoint point)
        {
            return new SceneWorldPoint { X = point.X, Y = point.Y };
        }
    }
}