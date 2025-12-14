using System;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Coordinate system for depth-based rendering (TVDSS).
    /// </summary>
    public class DepthCoordinateSystem : CoordinateSystem
    {
        private readonly double minDepth;
        private readonly double maxDepth;
        private readonly float canvasHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthCoordinateSystem"/> class.
        /// </summary>
        /// <param name="minDepth">Minimum depth (TVDSS).</param>
        /// <param name="maxDepth">Maximum depth (TVDSS).</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        public DepthCoordinateSystem(double minDepth, double maxDepth, float canvasHeight)
            : base(CoordinateSystemType.Depth, "feet", minDepth, maxDepth, isInverted: true)
        {
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            this.canvasHeight = canvasHeight;
        }

        /// <summary>
        /// Gets the minimum depth value.
        /// </summary>
        public double MinValue => minDepth;

        /// <summary>
        /// Gets the maximum depth value.
        /// </summary>
        public double MaxValue => maxDepth;

        /// <summary>
        /// Converts a depth value to screen Y coordinate.
        /// </summary>
        /// <param name="depth">The depth (TVDSS).</param>
        /// <param name="height">The canvas height (optional, uses constructor value if not provided).</param>
        /// <returns>The screen Y coordinate.</returns>
        public float ToScreenY(double depth, float? height = null)
        {
            float h = height ?? canvasHeight;
            double depthRange = maxDepth - minDepth;
            if (depthRange <= 0)
                return 0;

            double normalizedDepth = (depth - minDepth) / depthRange;
            return (float)(h * (1.0 - normalizedDepth)); // Invert Y (top = shallow, bottom = deep)
        }

        /// <summary>
        /// Converts a screen Y coordinate to depth.
        /// </summary>
        /// <param name="screenY">The screen Y coordinate.</param>
        /// <param name="height">The canvas height (optional).</param>
        /// <returns>The depth (TVDSS).</returns>
        public double ToDepth(float screenY, float? height = null)
        {
            float h = height ?? canvasHeight;
            double normalizedY = 1.0 - (screenY / h);
            double depthRange = maxDepth - minDepth;
            return minDepth + (normalizedY * depthRange);
        }
    }
}

