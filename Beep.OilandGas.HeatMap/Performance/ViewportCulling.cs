using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Performance
{
    /// <summary>
    /// Provides viewport culling to optimize rendering by only processing visible points.
    /// </summary>
    public static class ViewportCulling
    {
        /// <summary>
        /// Filters data points to only those visible in the current viewport.
        /// </summary>
        /// <param name="dataPoints">List of all data points.</param>
        /// <param name="viewportBounds">The viewport bounds in data coordinates.</param>
        /// <param name="padding">Padding around viewport in data coordinates (default: 0).</param>
        /// <returns>List of points visible in the viewport.</returns>
        public static List<HeatMapDataPoint> CullToViewport(
            List<HeatMapDataPoint> dataPoints,
            BoundingBox viewportBounds,
            double padding = 0)
        {
            if (dataPoints == null)
                return new List<HeatMapDataPoint>();

            // Expand viewport bounds by padding
            var expandedBounds = new BoundingBox(
                viewportBounds.MinX - padding,
                viewportBounds.MinY - padding,
                viewportBounds.MaxX + padding,
                viewportBounds.MaxY + padding);

            return dataPoints.Where(p =>
                p.X >= expandedBounds.MinX &&
                p.X <= expandedBounds.MaxX &&
                p.Y >= expandedBounds.MinY &&
                p.Y <= expandedBounds.MaxY).ToList();
        }

        /// <summary>
        /// Calculates the viewport bounds in data coordinates from screen coordinates.
        /// </summary>
        /// <param name="canvasWidth">Canvas width in pixels.</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        /// <returns>Viewport bounds in data coordinates.</returns>
        public static BoundingBox CalculateViewportBounds(
            double canvasWidth,
            double canvasHeight,
            double zoom,
            SKPoint panOffset)
        {
            // Convert screen bounds to data coordinates
            double minX = -panOffset.X / zoom;
            double maxX = (canvasWidth - panOffset.X) / zoom;
            double minY = -panOffset.Y / zoom;
            double maxY = (canvasHeight - panOffset.Y) / zoom;

            return new BoundingBox(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Filters data points using spatial index for efficient viewport culling.
        /// </summary>
        /// <param name="spatialIndex">The spatial index containing the data points.</param>
        /// <param name="viewportBounds">The viewport bounds in data coordinates.</param>
        /// <param name="padding">Padding around viewport in data coordinates (default: 0).</param>
        /// <returns>List of points visible in the viewport.</returns>
        public static List<HeatMapDataPoint> CullToViewportWithIndex(
            SpatialIndex spatialIndex,
            BoundingBox viewportBounds,
            double padding = 0)
        {
            if (spatialIndex == null)
                return new List<HeatMapDataPoint>();

            // Expand viewport bounds by padding
            var expandedBounds = new BoundingBox(
                viewportBounds.MinX - padding,
                viewportBounds.MinY - padding,
                viewportBounds.MaxX + padding,
                viewportBounds.MaxY + padding);

            return spatialIndex.QueryBounds(
                expandedBounds.MinX,
                expandedBounds.MinY,
                expandedBounds.MaxX,
                expandedBounds.MaxY);
        }
    }
}

