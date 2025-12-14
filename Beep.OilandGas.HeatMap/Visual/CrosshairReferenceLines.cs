using System;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Visual
{
    /// <summary>
    /// Manages crosshair and reference lines for heatmap visualization.
    /// </summary>
    public class CrosshairReferenceLines
    {
        /// <summary>
        /// Gets or sets whether to show crosshair.
        /// </summary>
        public bool ShowCrosshair { get; set; } = false;

        /// <summary>
        /// Gets or sets the crosshair position (screen coordinates).
        /// </summary>
        public SKPoint CrosshairPosition { get; set; }

        /// <summary>
        /// Gets or sets the crosshair color.
        /// </summary>
        public SKColor CrosshairColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the crosshair line width.
        /// </summary>
        public float CrosshairLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the crosshair line style.
        /// </summary>
        public SKStrokeCap CrosshairLineStyle { get; set; } = SKStrokeCap.Round;

        /// <summary>
        /// Gets or sets whether to show vertical reference line.
        /// </summary>
        public bool ShowVerticalReference { get; set; } = false;

        /// <summary>
        /// Gets or sets the vertical reference line X position (data coordinates).
        /// </summary>
        public double VerticalReferenceX { get; set; }

        /// <summary>
        /// Gets or sets whether to show horizontal reference line.
        /// </summary>
        public bool ShowHorizontalReference { get; set; } = false;

        /// <summary>
        /// Gets or sets the horizontal reference line Y position (data coordinates).
        /// </summary>
        public double HorizontalReferenceY { get; set; }

        /// <summary>
        /// Gets or sets the reference line color.
        /// </summary>
        public SKColor ReferenceLineColor { get; set; } = SKColors.Blue;

        /// <summary>
        /// Gets or sets the reference line width.
        /// </summary>
        public float ReferenceLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the reference line style (dashed, solid, etc.).
        /// </summary>
        public float[] ReferenceLineDashPattern { get; set; } = new float[] { 5, 5 };

        /// <summary>
        /// Gets or sets whether to show grid snap.
        /// </summary>
        public bool EnableGridSnap { get; set; } = false;

        /// <summary>
        /// Gets or sets the grid snap spacing.
        /// </summary>
        public double GridSnapSpacing { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets whether to show coordinate display.
        /// </summary>
        public bool ShowCoordinates { get; set; } = true;

        /// <summary>
        /// Gets or sets the coordinate display font size.
        /// </summary>
        public float CoordinateFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the coordinate display color.
        /// </summary>
        public SKColor CoordinateColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the coordinate transformation function (screen to data).
        /// </summary>
        public Func<float, float, (double dataX, double dataY)> ScreenToDataTransform { get; set; }

        /// <summary>
        /// Gets or sets the coordinate transformation function (data to screen).
        /// </summary>
        public Func<double, double, (float screenX, float screenY)> DataToScreenTransform { get; set; }

        /// <summary>
        /// Updates the crosshair position.
        /// </summary>
        public void UpdateCrosshair(float screenX, float screenY)
        {
            CrosshairPosition = new SKPoint(screenX, screenY);
        }

        /// <summary>
        /// Snaps a coordinate to the grid if grid snap is enabled.
        /// </summary>
        public double SnapToGrid(double value)
        {
            if (!EnableGridSnap)
                return value;

            return Math.Round(value / GridSnapSpacing) * GridSnapSpacing;
        }

        /// <summary>
        /// Renders crosshair and reference lines on the canvas.
        /// </summary>
        public void Render(SKCanvas canvas, float width, float height)
        {
            // Render reference lines first (behind crosshair)
            if (ShowVerticalReference && DataToScreenTransform != null)
            {
                var (screenX, _) = DataToScreenTransform(VerticalReferenceX, 0);
                RenderVerticalLine(canvas, screenX, height);
            }

            if (ShowHorizontalReference && DataToScreenTransform != null)
            {
                var (_, screenY) = DataToScreenTransform(0, HorizontalReferenceY);
                RenderHorizontalLine(canvas, screenY, width);
            }

            // Render crosshair
            if (ShowCrosshair)
            {
                RenderCrosshair(canvas, CrosshairPosition.X, CrosshairPosition.Y, width, height);
            }
        }

        private void RenderCrosshair(SKCanvas canvas, float x, float y, float width, float height)
        {
            using (var paint = new SKPaint
            {
                Color = CrosshairColor,
                StrokeWidth = CrosshairLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeCap = CrosshairLineStyle
            })
            {
                // Vertical line
                canvas.DrawLine(x, 0, x, height, paint);

                // Horizontal line
                canvas.DrawLine(0, y, width, y, paint);
            }

            // Draw coordinate display
            if (ShowCoordinates && ScreenToDataTransform != null)
            {
                var (dataX, dataY) = ScreenToDataTransform(x, y);
                if (EnableGridSnap)
                {
                    dataX = SnapToGrid(dataX);
                    dataY = SnapToGrid(dataY);
                }

                string coordText = $"({dataX:F2}, {dataY:F2})";

                using (var textPaint = new SKPaint
                {
                    Color = CoordinateColor,
                    TextSize = CoordinateFontSize,
                    IsAntialias = true
                })
                using (var bgPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, 220),
                    Style = SKPaintStyle.Fill
                })
                {
                    var bounds = new SKRect();
                    textPaint.MeasureText(coordText, ref bounds);
                    bounds.Offset(x + 5, y - 5);
                    bounds.Inflate(3, 2);

                    canvas.DrawRect(bounds, bgPaint);
                    canvas.DrawText(coordText, x + 5, y - 5, textPaint);
                }
            }
        }

        private void RenderVerticalLine(SKCanvas canvas, float x, float height)
        {
            using (var paint = new SKPaint
            {
                Color = ReferenceLineColor,
                StrokeWidth = ReferenceLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                if (ReferenceLineDashPattern != null && ReferenceLineDashPattern.Length > 0)
                {
                    paint.PathEffect = SKPathEffect.CreateDash(ReferenceLineDashPattern, 0);
                }

                canvas.DrawLine(x, 0, x, height, paint);
            }
        }

        private void RenderHorizontalLine(SKCanvas canvas, float y, float width)
        {
            using (var paint = new SKPaint
            {
                Color = ReferenceLineColor,
                StrokeWidth = ReferenceLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                if (ReferenceLineDashPattern != null && ReferenceLineDashPattern.Length > 0)
                {
                    paint.PathEffect = SKPathEffect.CreateDash(ReferenceLineDashPattern, 0);
                }

                canvas.DrawLine(0, y, width, y, paint);
            }
        }
    }
}

