using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Interaction
{
    /// <summary>
    /// Represents a selected data point with selection information.
    /// </summary>
    public class SelectedPoint
    {
        /// <summary>
        /// Gets or sets the data point.
        /// </summary>
        public HeatMapDataPoint DataPoint { get; set; }

        /// <summary>
        /// Gets or sets the selection timestamp.
        /// </summary>
        public DateTime SelectedAt { get; set; }

        /// <summary>
        /// Gets or sets whether this point is part of a multi-selection.
        /// </summary>
        public bool IsMultiSelected { get; set; }

        public SelectedPoint(HeatMapDataPoint dataPoint)
        {
            DataPoint = dataPoint ?? throw new ArgumentNullException(nameof(dataPoint));
            SelectedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Provides interaction capabilities for heat maps including selection, tooltips, and zoom.
    /// </summary>
    public class HeatMapInteraction
    {
        /// <summary>
        /// Gets the list of selected points.
        /// </summary>
        public List<SelectedPoint> SelectedPoints { get; }

        /// <summary>
        /// Gets or sets whether multi-selection is enabled.
        /// </summary>
        public bool MultiSelectEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the selection radius in pixels.
        /// </summary>
        public float SelectionRadius { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the currently hovered point (for tooltips).
        /// </summary>
        public HeatMapDataPoint HoveredPoint { get; private set; }

        /// <summary>
        /// Gets or sets the hover position in screen coordinates.
        /// </summary>
        public SKPoint HoverPosition { get; private set; }

        /// <summary>
        /// Gets or sets whether tooltips are enabled.
        /// </summary>
        public bool TooltipsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the tooltip offset from the cursor.
        /// </summary>
        public SKPoint TooltipOffset { get; set; } = new SKPoint(10, 10);

        /// <summary>
        /// Gets or sets the tooltip background color.
        /// </summary>
        public SKColor TooltipBackgroundColor { get; set; } = new SKColor(0, 0, 0, 200);

        /// <summary>
        /// Gets or sets the tooltip text color.
        /// </summary>
        public SKColor TooltipTextColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the tooltip font size.
        /// </summary>
        public float TooltipFontSize { get; set; } = 12f;

        /// <summary>
        /// Event raised when a point is clicked.
        /// </summary>
        public event EventHandler<HeatMapDataPoint> PointClicked;

        /// <summary>
        /// Event raised when a point is selected.
        /// </summary>
        public event EventHandler<SelectedPoint> PointSelected;

        /// <summary>
        /// Event raised when selection is cleared.
        /// </summary>
        public event EventHandler SelectionCleared;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapInteraction"/> class.
        /// </summary>
        public HeatMapInteraction()
        {
            SelectedPoints = new List<SelectedPoint>();
        }

        /// <summary>
        /// Handles mouse click at the specified screen coordinates.
        /// </summary>
        /// <param name="screenX">Screen X coordinate.</param>
        /// <param name="screenY">Screen Y coordinate.</param>
        /// <param name="dataPoints">List of data points to search.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        /// <returns>The clicked data point, or null if none found.</returns>
        public HeatMapDataPoint HandleClick(
            float screenX,
            float screenY,
            List<HeatMapDataPoint> dataPoints,
            double zoom,
            SKPoint panOffset)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return null;

            // Convert screen coordinates to data coordinates
            float dataX = (float)((screenX - panOffset.X) / zoom);
            float dataY = (float)((screenY - panOffset.Y) / zoom);

            // Find nearest point within selection radius
            HeatMapDataPoint nearestPoint = FindNearestPoint(dataX, dataY, dataPoints, SelectionRadius);

            if (nearestPoint != null)
            {
                if (!MultiSelectEnabled)
                {
                    SelectedPoints.Clear();
                }

                var selectedPoint = new SelectedPoint(nearestPoint)
                {
                    IsMultiSelected = MultiSelectEnabled && SelectedPoints.Count > 0
                };

                // Avoid duplicate selections
                if (!SelectedPoints.Any(sp => sp.DataPoint == nearestPoint))
                {
                    SelectedPoints.Add(selectedPoint);
                    PointSelected?.Invoke(this, selectedPoint);
                }

                PointClicked?.Invoke(this, nearestPoint);
                return nearestPoint;
            }
            else if (!MultiSelectEnabled)
            {
                // Clear selection if clicking on empty space and multi-select is disabled
                ClearSelection();
            }

            return null;
        }

        /// <summary>
        /// Handles mouse hover at the specified screen coordinates.
        /// </summary>
        /// <param name="screenX">Screen X coordinate.</param>
        /// <param name="screenY">Screen Y coordinate.</param>
        /// <param name="dataPoints">List of data points to search.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        public void HandleHover(
            float screenX,
            float screenY,
            List<HeatMapDataPoint> dataPoints,
            double zoom,
            SKPoint panOffset)
        {
            if (!TooltipsEnabled || dataPoints == null || dataPoints.Count == 0)
            {
                HoveredPoint = null;
                return;
            }

            // Convert screen coordinates to data coordinates
            float dataX = (float)((screenX - panOffset.X) / zoom);
            float dataY = (float)((screenY - panOffset.Y) / zoom);

            // Find nearest point within hover radius
            float hoverRadius = SelectionRadius * 1.5f; // Slightly larger for hover
            HoveredPoint = FindNearestPoint(dataX, dataY, dataPoints, hoverRadius);
            HoverPosition = new SKPoint(screenX, screenY);
        }

        /// <summary>
        /// Draws tooltips on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        public void DrawTooltips(SKCanvas canvas)
        {
            if (!TooltipsEnabled || HoveredPoint == null)
                return;

            string tooltipText = FormatTooltipText(HoveredPoint);
            DrawTooltip(canvas, HoverPosition.X + TooltipOffset.X, HoverPosition.Y + TooltipOffset.Y, tooltipText);
        }

        /// <summary>
        /// Draws selection indicators on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        public void DrawSelection(SKCanvas canvas, double zoom, SKPoint panOffset)
        {
            if (SelectedPoints.Count == 0)
                return;

            var selectionPaint = new SKPaint
            {
                Color = SKColors.Yellow,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f,
                IsAntialias = true
            };

            foreach (var selected in SelectedPoints)
            {
                float x = (float)(selected.DataPoint.X * zoom + panOffset.X);
                float y = (float)(selected.DataPoint.Y * zoom + panOffset.Y);

                // Draw selection circle
                canvas.DrawCircle(x, y, SelectionRadius + 5, selectionPaint);
            }
        }

        /// <summary>
        /// Clears all selections.
        /// </summary>
        public void ClearSelection()
        {
            SelectedPoints.Clear();
            SelectionCleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the bounding box of all selected points.
        /// </summary>
        /// <returns>Bounding box as (minX, minY, maxX, maxY), or null if no selections.</returns>
        public (double minX, double minY, double maxX, double maxY)? GetSelectionBounds()
        {
            if (SelectedPoints.Count == 0)
                return null;

            double minX = SelectedPoints.Min(sp => sp.DataPoint.X);
            double minY = SelectedPoints.Min(sp => sp.DataPoint.Y);
            double maxX = SelectedPoints.Max(sp => sp.DataPoint.X);
            double maxY = SelectedPoints.Max(sp => sp.DataPoint.Y);

            return (minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Finds the nearest data point to the given coordinates.
        /// </summary>
        private HeatMapDataPoint FindNearestPoint(float x, float y, List<HeatMapDataPoint> dataPoints, float radius)
        {
            HeatMapDataPoint nearest = null;
            float minDistance = radius;

            foreach (var point in dataPoints)
            {
                float distance = (float)Math.Sqrt(
                    Math.Pow(point.X - x, 2) +
                    Math.Pow(point.Y - y, 2));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Formats tooltip text for a data point.
        /// </summary>
        private string FormatTooltipText(HeatMapDataPoint point)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(point.Label))
            {
                parts.Add($"Label: {point.Label}");
            }

            parts.Add($"X: {point.X:F2}");
            parts.Add($"Y: {point.Y:F2}");
            parts.Add($"Value: {point.Value:F4}");

            if (point.OriginalX != 0 || point.OriginalY != 0)
            {
                parts.Add($"UTM: ({point.OriginalX:F2}, {point.OriginalY:F2})");
            }

            return string.Join("\n", parts);
        }

        /// <summary>
        /// Draws a tooltip on the canvas.
        /// </summary>
        private void DrawTooltip(SKCanvas canvas, float x, float y, string text)
        {
            var textPaint = new SKPaint
            {
                Color = TooltipTextColor,
                IsAntialias = true,
                TextSize = TooltipFontSize
            };

            // Measure text
            var lines = text.Split('\n');
            float maxWidth = 0;
            float totalHeight = lines.Length * TooltipFontSize * 1.2f;

            foreach (var line in lines)
            {
                float width = textPaint.MeasureText(line);
                if (width > maxWidth)
                    maxWidth = width;
            }

            // Draw background
            float padding = 8f;
            var backgroundRect = new SKRect(
                x - padding,
                y - padding,
                x + maxWidth + padding,
                y + totalHeight + padding);

            var backgroundPaint = new SKPaint
            {
                Color = TooltipBackgroundColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawRoundRect(backgroundRect, 4f, 4f, backgroundPaint);

            // Draw text
            float lineY = y + TooltipFontSize;
            foreach (var line in lines)
            {
                canvas.DrawText(line, x, lineY, textPaint);
                lineY += TooltipFontSize * 1.2f;
            }
        }
    }
}

