using SkiaSharp;
using System;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.Primitives
{
    /// <summary>
    /// Configuration for rendering a scale bar overlay.
    /// </summary>
    public sealed class ScaleBarOptions
    {
        public OverlayAnchor Anchor { get; set; } = OverlayAnchor.BottomLeft;
        public float Margin { get; set; } = 16f;
        public float MaxBarWidthPixels { get; set; } = 120f;
        public float BarHeight { get; set; } = 10f;
        public float FontSize { get; set; } = 12f;
        public int SegmentCount { get; set; } = 2;
        public bool ShowBackground { get; set; } = true;
    }

    /// <summary>
    /// Renders a scale bar using scene units and an explicit world-units-per-pixel ratio.
    /// </summary>
    public sealed class ScaleBarRenderer
    {
        public void Render(SKCanvas canvas, DrawingScene scene, double worldUnitsPerPixel, Theme theme = null, ScaleBarOptions options = null)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));
            if (worldUnitsPerPixel <= 0 || double.IsNaN(worldUnitsPerPixel) || double.IsInfinity(worldUnitsPerPixel))
                throw new ArgumentOutOfRangeException(nameof(worldUnitsPerPixel), "World units per pixel must be positive.");

            theme ??= Theme.Standard;
            options ??= new ScaleBarOptions();

            var unit = ResolvePrimaryLinearUnit(scene);
            var rawLength = options.MaxBarWidthPixels * worldUnitsPerPixel;
            var niceLength = SelectNiceLength(rawLength);
            var barWidth = (float)(niceLength / worldUnitsPerPixel);
            var label = FormatMeasurement(niceLength, unit);

            using var textPaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                TextSize = options.FontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            var labelWidth = textPaint.MeasureText(label);
            var contentWidth = Math.Max(barWidth, labelWidth);
            var totalWidth = contentWidth + 20f;
            var totalHeight = options.BarHeight + options.FontSize + 18f;
            var bounds = OverlayLayout.ResolveBounds(canvas.DeviceClipBounds, totalWidth, totalHeight, options.Anchor, options.Margin);
            var barLeft = bounds.Left + (bounds.Width - barWidth) / 2;
            var barTop = bounds.Top + 8f;

            if (options.ShowBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = theme.BackgroundColor.WithAlpha(220),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
                using var borderPaint = new SKPaint
                {
                    Color = theme.ForegroundColor.WithAlpha(160),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f
                };

                canvas.DrawRoundRect(bounds, 6, 6, backgroundPaint);
                canvas.DrawRoundRect(bounds, 6, 6, borderPaint);
            }

            var segmentCount = Math.Max(1, options.SegmentCount);
            var segmentWidth = barWidth / segmentCount;
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                using var segmentPaint = new SKPaint
                {
                    Color = segmentIndex % 2 == 0 ? theme.ForegroundColor : theme.BackgroundColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                using var segmentBorderPaint = new SKPaint
                {
                    Color = theme.ForegroundColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    IsAntialias = true
                };

                var segmentRect = new SKRect(
                    barLeft + segmentWidth * segmentIndex,
                    barTop,
                    barLeft + segmentWidth * (segmentIndex + 1),
                    barTop + options.BarHeight);
                canvas.DrawRect(segmentRect, segmentPaint);
                canvas.DrawRect(segmentRect, segmentBorderPaint);
            }

            canvas.DrawText(label, bounds.MidX, barTop + options.BarHeight + options.FontSize + 4f, textPaint);
        }

        private static MeasurementUnit ResolvePrimaryLinearUnit(DrawingScene scene)
        {
            return scene.CoordinateReferenceSystem.Axes
                .FirstOrDefault(axis => axis.Unit.Dimension == MeasurementDimension.Length)?.Unit
                ?? scene.CoordinateReferenceSystem.Axes.FirstOrDefault(axis => axis.Unit.Dimension == MeasurementDimension.Time)?.Unit
                ?? scene.CoordinateReferenceSystem.Axes.FirstOrDefault()?.Unit
                ?? MeasurementUnit.Unknown;
        }

        private static double SelectNiceLength(double rawLength)
        {
            if (rawLength <= 0)
                return 0;

            var exponent = Math.Floor(Math.Log10(rawLength));
            var fraction = rawLength / Math.Pow(10, exponent);
            var niceFraction = fraction switch
            {
                <= 1 => 1,
                <= 2 => 2,
                <= 5 => 5,
                _ => 10
            };

            return niceFraction * Math.Pow(10, exponent);
        }

        private static string FormatMeasurement(double value, MeasurementUnit unit)
        {
            if (value >= 100)
                return $"{Math.Round(value):0} {unit.Code}";
            if (value >= 10)
                return $"{value:0.#} {unit.Code}";

            return $"{value:0.##} {unit.Code}";
        }
    }
}