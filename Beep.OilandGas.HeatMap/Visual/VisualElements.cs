using SkiaSharp;
using System;

namespace Beep.OilandGas.HeatMap.Visual
{
    /// <summary>
    /// Provides visual elements for heat maps including grid lines, scale bar, and north arrow.
    /// </summary>
    public static class VisualElements
    {
        /// <summary>
        /// Draws grid lines on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <param name="spacing">Grid line spacing in pixels.</param>
        /// <param name="color">Grid line color.</param>
        /// <param name="strokeWidth">Grid line stroke width.</param>
        public static void DrawGrid(
            SKCanvas canvas,
            float width,
            float height,
            float spacing,
            SKColor color,
            float strokeWidth = 1f)
        {
            var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true
            };

            // Draw vertical lines
            for (float x = 0; x <= width; x += spacing)
            {
                canvas.DrawLine(x, 0, x, height, paint);
            }

            // Draw horizontal lines
            for (float y = 0; y <= height; y += spacing)
            {
                canvas.DrawLine(0, y, width, y, paint);
            }
        }

        /// <summary>
        /// Draws axis labels on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <param name="xLabel">X-axis label.</param>
        /// <param name="yLabel">Y-axis label.</param>
        /// <param name="fontSize">Font size for labels.</param>
        /// <param name="color">Text color.</param>
        public static void DrawAxisLabels(
            SKCanvas canvas,
            float width,
            float height,
            string xLabel,
            string yLabel,
            float fontSize = 14f,
            SKColor color = default)
        {
            if (color == default)
                color = SKColors.Black;

            var paint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                TextSize = fontSize,
                TextAlign = SKTextAlign.Center
            };

            // Draw X-axis label (centered at bottom)
            if (!string.IsNullOrEmpty(xLabel))
            {
                canvas.DrawText(xLabel, width / 2, height - 10, paint);
            }

            // Draw Y-axis label (rotated, centered on left)
            if (!string.IsNullOrEmpty(yLabel))
            {
                canvas.Save();
                canvas.Translate(20, height / 2);
                canvas.RotateDegrees(-90);
                paint.TextAlign = SKTextAlign.Center;
                canvas.DrawText(yLabel, 0, 0, paint);
                canvas.Restore();
            }
        }

        /// <summary>
        /// Draws a scale bar on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="x">X position of the scale bar.</param>
        /// <param name="y">Y position of the scale bar.</param>
        /// <param name="length">Length of the scale bar in pixels.</param>
        /// <param name="realWorldDistance">Real-world distance represented by the scale bar.</param>
        /// <param name="unit">Unit of measurement (e.g., "m", "km", "ft").</param>
        /// <param name="fontSize">Font size for labels.</param>
        /// <param name="color">Color of the scale bar and text.</param>
        public static void DrawScaleBar(
            SKCanvas canvas,
            float x,
            float y,
            float length,
            double realWorldDistance,
            string unit = "m",
            float fontSize = 12f,
            SKColor color = default)
        {
            if (color == default)
                color = SKColors.Black;

            var linePaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                TextSize = fontSize,
                TextAlign = SKTextAlign.Center
            };

            // Draw the scale bar line
            canvas.DrawLine(x, y, x + length, y, linePaint);

            // Draw tick marks at ends
            float tickLength = 8f;
            canvas.DrawLine(x, y - tickLength / 2, x, y + tickLength / 2, linePaint);
            canvas.DrawLine(x + length, y - tickLength / 2, x + length, y + tickLength / 2, linePaint);

            // Draw label
            string label = $"{realWorldDistance:F0} {unit}";
            canvas.DrawText(label, x + length / 2, y - 10, textPaint);
        }

        /// <summary>
        /// Draws a north arrow on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="x">X position of the arrow center.</param>
        /// <param name="y">Y position of the arrow center.</param>
        /// <param name="size">Size of the arrow in pixels.</param>
        /// <param name="color">Color of the arrow.</param>
        /// <param name="showLabel">Whether to show "N" label.</param>
        public static void DrawNorthArrow(
            SKCanvas canvas,
            float x,
            float y,
            float size = 30f,
            SKColor color = default,
            bool showLabel = true)
        {
            if (color == default)
                color = SKColors.Black;

            var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var strokePaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            // Draw arrow pointing up (north)
            var path = new SKPath();
            
            // Arrow head (triangle pointing up)
            float arrowHeadSize = size * 0.4f;
            path.MoveTo(x, y - size / 2);
            path.LineTo(x - arrowHeadSize / 2, y - size / 2 + arrowHeadSize);
            path.LineTo(x + arrowHeadSize / 2, y - size / 2 + arrowHeadSize);
            path.Close();

            canvas.DrawPath(path, paint);
            canvas.DrawPath(path, strokePaint);

            // Draw arrow shaft
            float shaftWidth = size * 0.15f;
            float shaftLength = size * 0.6f;
            var shaftRect = new SKRect(
                x - shaftWidth / 2,
                y - size / 2 + arrowHeadSize,
                x + shaftWidth / 2,
                y - size / 2 + arrowHeadSize + shaftLength);

            canvas.DrawRect(shaftRect, paint);
            canvas.DrawRect(shaftRect, strokePaint);

            // Draw "N" label
            if (showLabel)
            {
                var textPaint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true,
                    TextSize = size * 0.3f,
                    TextAlign = SKTextAlign.Center,
                    FakeBoldText = true
                };

                canvas.DrawText("N", x, y - size / 2 - 5, textPaint);
            }
        }

        /// <summary>
        /// Draws coordinate system information on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="x">X position for the coordinate display.</param>
        /// <param name="y">Y position for the coordinate display.</param>
        /// <param name="coordinateSystem">Coordinate system name (e.g., "UTM Zone 15N", "WGS84").</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="color">Text color.</param>
        public static void DrawCoordinateSystem(
            SKCanvas canvas,
            float x,
            float y,
            string coordinateSystem,
            float fontSize = 10f,
            SKColor color = default)
        {
            if (color == default)
                color = SKColors.Black;

            if (string.IsNullOrEmpty(coordinateSystem))
                return;

            var paint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                TextSize = fontSize,
                TextAlign = SKTextAlign.Left
            };

            canvas.DrawText($"CRS: {coordinateSystem}", x, y, paint);
        }
    }
}

