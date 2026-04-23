using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.Primitives
{
    /// <summary>
    /// Represents the symbol style shown in a legend row.
    /// </summary>
    public enum LegendSymbolKind
    {
        Fill,
        Line,
        Marker
    }

    /// <summary>
    /// Represents a single legend row.
    /// </summary>
    public sealed class LegendItem
    {
        public string Label { get; set; }
        public LegendSymbolKind SymbolKind { get; set; } = LegendSymbolKind.Fill;
        public SKColor FillColor { get; set; } = SKColors.LightGray;
        public SKColor StrokeColor { get; set; } = SKColors.Black;
        public float StrokeWidth { get; set; } = 2.0f;
    }

    /// <summary>
    /// Configuration for rendering a legend overlay.
    /// </summary>
    public sealed class LegendOptions
    {
        public OverlayAnchor Anchor { get; set; } = OverlayAnchor.TopRight;
        public float Margin { get; set; } = 16f;
        public float Padding { get; set; } = 10f;
        public float RowHeight { get; set; } = 22f;
        public float SymbolSize { get; set; } = 14f;
        public float FontSize { get; set; } = 12f;
        public bool ShowBackground { get; set; } = true;
    }

    /// <summary>
    /// Renders a reusable legend box for map, section, and reservoir scenes.
    /// </summary>
    public sealed class LegendRenderer
    {
        public void Render(SKCanvas canvas, IReadOnlyList<LegendItem> items, Theme theme = null, LegendOptions options = null, string title = null)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return;

            theme ??= Theme.Standard;
            options ??= new LegendOptions();

            using var textPaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                TextSize = options.FontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };

            using var titlePaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                TextSize = options.FontSize + 1,
                IsAntialias = true,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Left
            };

            var labelWidths = items.Select(item => textPaint.MeasureText(item.Label ?? string.Empty)).ToList();
            var maxLabelWidth = labelWidths.Count == 0 ? 0 : labelWidths.Max();
            var titleHeight = string.IsNullOrWhiteSpace(title) ? 0 : options.RowHeight;
            var legendWidth = options.Padding * 3 + options.SymbolSize + maxLabelWidth;
            var legendHeight = options.Padding * 2 + titleHeight + (items.Count * options.RowHeight);
            var bounds = OverlayLayout.ResolveBounds(canvas.DeviceClipBounds, legendWidth, legendHeight, options.Anchor, options.Margin);

            if (options.ShowBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = theme.BackgroundColor.WithAlpha(230),
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

            var currentY = bounds.Top + options.Padding + options.FontSize;
            if (!string.IsNullOrWhiteSpace(title))
            {
                canvas.DrawText(title, bounds.Left + options.Padding, currentY, titlePaint);
                currentY += options.RowHeight;
            }

            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                var symbolRect = new SKRect(
                    bounds.Left + options.Padding,
                    currentY - options.SymbolSize + 2,
                    bounds.Left + options.Padding + options.SymbolSize,
                    currentY + 2);

                DrawSymbol(canvas, item, symbolRect);
                canvas.DrawText(item.Label ?? string.Empty, symbolRect.Right + options.Padding, currentY, textPaint);
                currentY += options.RowHeight;
            }
        }

        private static void DrawSymbol(SKCanvas canvas, LegendItem item, SKRect rect)
        {
            using var fillPaint = new SKPaint
            {
                Color = item.FillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var strokePaint = new SKPaint
            {
                Color = item.StrokeColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = item.StrokeWidth,
                IsAntialias = true
            };

            switch (item.SymbolKind)
            {
                case LegendSymbolKind.Line:
                    var centerY = (rect.Top + rect.Bottom) / 2;
                    canvas.DrawLine(rect.Left, centerY, rect.Right, centerY, strokePaint);
                    break;

                case LegendSymbolKind.Marker:
                    canvas.DrawCircle(rect.MidX, rect.MidY, Math.Min(rect.Width, rect.Height) / 3, fillPaint);
                    canvas.DrawCircle(rect.MidX, rect.MidY, Math.Min(rect.Width, rect.Height) / 3, strokePaint);
                    break;

                default:
                    canvas.DrawRect(rect, fillPaint);
                    canvas.DrawRect(rect, strokePaint);
                    break;
            }
        }
    }
}