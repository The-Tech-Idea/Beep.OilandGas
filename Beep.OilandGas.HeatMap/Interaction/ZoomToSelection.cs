using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Interaction
{
    /// <summary>
    /// Provides zoom-to-selection functionality for heat maps.
    /// </summary>
    public static class ZoomToSelection
    {
        /// <summary>
        /// Calculates zoom and pan settings to fit selected points in the viewport.
        /// </summary>
        /// <param name="selectedPoints">List of selected points.</param>
        /// <param name="canvasWidth">Canvas width in pixels.</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        /// <param name="padding">Padding around the selection in pixels (default: 20).</param>
        /// <param name="maxZoom">Maximum zoom level (default: 10.0).</param>
        /// <returns>Tuple containing (zoom, panOffset), or null if no valid selection.</returns>
        public static (double zoom, SKPoint panOffset)? CalculateZoomToSelection(
            List<SelectedPoint> selectedPoints,
            double canvasWidth,
            double canvasHeight,
            float padding = 20f,
            double maxZoom = 10.0)
        {
            if (selectedPoints == null || selectedPoints.Count == 0)
                return null;

            // Get bounding box of selected points
            double minX = selectedPoints.Min(sp => sp.DataPoint.X);
            double maxX = selectedPoints.Max(sp => sp.DataPoint.X);
            double minY = selectedPoints.Min(sp => sp.DataPoint.Y);
            double maxY = selectedPoints.Max(sp => sp.DataPoint.Y);

            // Calculate selection dimensions
            double selectionWidth = maxX - minX;
            double selectionHeight = maxY - minY;

            // Avoid division by zero
            if (selectionWidth <= 0 || selectionHeight <= 0)
                return null;

            // Calculate available canvas space (with padding)
            double availableWidth = canvasWidth - (padding * 2);
            double availableHeight = canvasHeight - (padding * 2);

            // Calculate zoom levels for width and height
            double zoomX = availableWidth / selectionWidth;
            double zoomY = availableHeight / selectionHeight;

            // Use the smaller zoom to ensure everything fits
            double zoom = Math.Min(zoomX, zoomY);
            zoom = Math.Min(zoom, maxZoom); // Cap at maximum zoom

            // Calculate center of selection
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            // Calculate pan offset to center the selection
            float panX = (float)(canvasWidth / 2.0 - centerX * zoom);
            float panY = (float)(canvasHeight / 2.0 - centerY * zoom);

            return (zoom, new SKPoint(panX, panY));
        }

        /// <summary>
        /// Calculates zoom and pan settings to fit all data points in the viewport.
        /// </summary>
        /// <param name="dataPoints">List of all data points.</param>
        /// <param name="canvasWidth">Canvas width in pixels.</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        /// <param name="padding">Padding around the data in pixels (default: 20).</param>
        /// <returns>Tuple containing (zoom, panOffset), or null if no valid data.</returns>
        public static (double zoom, SKPoint panOffset)? CalculateZoomToFit(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double canvasWidth,
            double canvasHeight,
            float padding = 20f)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return null;

            // Get bounding box of all points
            double minX = dataPoints.Min(p => p.X);
            double maxX = dataPoints.Max(p => p.X);
            double minY = dataPoints.Min(p => p.Y);
            double maxY = dataPoints.Max(p => p.Y);

            // Calculate data dimensions
            double dataWidth = maxX - minX;
            double dataHeight = maxY - minY;

            // Avoid division by zero
            if (dataWidth <= 0 || dataHeight <= 0)
                return null;

            // Calculate available canvas space (with padding)
            double availableWidth = canvasWidth - (padding * 2);
            double availableHeight = canvasHeight - (padding * 2);

            // Calculate zoom levels for width and height
            double zoomX = availableWidth / dataWidth;
            double zoomY = availableHeight / dataHeight;

            // Use the smaller zoom to ensure everything fits
            double zoom = Math.Min(zoomX, zoomY);

            // Calculate center of data
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            // Calculate pan offset to center the data
            float panX = (float)(canvasWidth / 2.0 - centerX * zoom);
            float panY = (float)(canvasHeight / 2.0 - centerY * zoom);

            return (zoom, new SKPoint(panX, panY));
        }

        /// <summary>
        /// Calculates zoom and pan settings to fit a bounding box in the viewport.
        /// </summary>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <param name="canvasWidth">Canvas width in pixels.</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        /// <param name="padding">Padding around the bounding box in pixels (default: 20).</param>
        /// <param name="maxZoom">Maximum zoom level (default: 10.0).</param>
        /// <returns>Tuple containing (zoom, panOffset), or null if invalid bounds.</returns>
        public static (double zoom, SKPoint panOffset)? CalculateZoomToBounds(
            double minX,
            double minY,
            double maxX,
            double maxY,
            double canvasWidth,
            double canvasHeight,
            float padding = 20f,
            double maxZoom = 10.0)
        {
            // Validate bounds
            if (maxX <= minX || maxY <= minY)
                return null;

            // Calculate dimensions
            double width = maxX - minX;
            double height = maxY - minY;

            // Calculate available canvas space (with padding)
            double availableWidth = canvasWidth - (padding * 2);
            double availableHeight = canvasHeight - (padding * 2);

            // Calculate zoom levels
            double zoomX = availableWidth / width;
            double zoomY = availableHeight / height;

            // Use the smaller zoom to ensure everything fits
            double zoom = Math.Min(zoomX, zoomY);
            zoom = Math.Min(zoom, maxZoom); // Cap at maximum zoom

            // Calculate center
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            // Calculate pan offset
            float panX = (float)(canvasWidth / 2.0 - centerX * zoom);
            float panY = (float)(canvasHeight / 2.0 - centerY * zoom);

            return (zoom, new SKPoint(panX, panY));
        }
    }
}

