using System;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Maps depth-domain values into screen coordinates using a typed depth CRS.
    /// </summary>
    public sealed class DepthTransform
    {
        /// <summary>
        /// Gets the shallowest depth in the transform.
        /// </summary>
        public double MinimumDepth { get; }

        /// <summary>
        /// Gets the deepest depth in the transform.
        /// </summary>
        public double MaximumDepth { get; }

        /// <summary>
        /// Gets the default canvas height used when an explicit height is not provided.
        /// </summary>
        public float CanvasHeight { get; }

        /// <summary>
        /// Gets the linear unit used by the depth axis.
        /// </summary>
        public MeasurementUnit Unit { get; }

        /// <summary>
        /// Gets the typed CRS that describes this depth axis.
        /// </summary>
        public CoordinateReferenceSystem CoordinateReferenceSystem { get; }

        public DepthTransform(double minimumDepth, double maximumDepth, float canvasHeight = 0, string unitCode = "ft")
        {
            if (maximumDepth < minimumDepth)
            {
                (minimumDepth, maximumDepth) = (maximumDepth, minimumDepth);
            }

            MinimumDepth = minimumDepth;
            MaximumDepth = maximumDepth;
            CanvasHeight = Math.Max(0, canvasHeight);
            Unit = MeasurementUnit.FromCode(unitCode);
            CoordinateReferenceSystem = CoordinateReferenceSystem.CreateDepth(Unit.Code);
        }

        /// <summary>
        /// Creates a copy of the transform using a different canvas height.
        /// </summary>
        public DepthTransform WithCanvasHeight(float canvasHeight)
        {
            return new DepthTransform(MinimumDepth, MaximumDepth, canvasHeight, Unit.Code);
        }

        /// <summary>
        /// Converts a depth value into a screen-space Y coordinate.
        /// </summary>
        public float ToScreenY(double depth, float? canvasHeight = null)
        {
            var height = ResolveCanvasHeight(canvasHeight);
            if (MaximumDepth <= MinimumDepth || height <= 0)
                return 0;

            var normalizedDepth = (depth - MinimumDepth) / (MaximumDepth - MinimumDepth);
            var clamped = Math.Clamp(normalizedDepth, 0d, 1d);
            return (float)(height * (1.0 - clamped));
        }

        /// <summary>
        /// Converts a screen-space Y coordinate into a depth value.
        /// </summary>
        public double ToDepth(float screenY, float? canvasHeight = null)
        {
            var height = ResolveCanvasHeight(canvasHeight);
            if (MaximumDepth <= MinimumDepth || height <= 0)
                return MinimumDepth;

            var normalizedY = 1.0 - (screenY / height);
            var clamped = Math.Clamp(normalizedY, 0d, 1d);
            return MinimumDepth + (clamped * (MaximumDepth - MinimumDepth));
        }

        private float ResolveCanvasHeight(float? canvasHeight)
        {
            if (canvasHeight.HasValue && canvasHeight.Value > 0)
                return canvasHeight.Value;

            return CanvasHeight;
        }
    }
}