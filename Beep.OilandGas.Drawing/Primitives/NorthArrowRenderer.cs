using SkiaSharp;
using System;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.Primitives
{
    /// <summary>
    /// Configuration for rendering a north arrow overlay.
    /// </summary>
    public sealed class NorthArrowOptions
    {
        public OverlayAnchor Anchor { get; set; } = OverlayAnchor.TopLeft;
        public float Margin { get; set; } = 16f;
        public float Size { get; set; } = 42f;
        public float RotationDegrees { get; set; }
        public float FontSize { get; set; } = 14f;
        public bool ShowBackground { get; set; } = true;
    }

    /// <summary>
    /// Renders a reusable north arrow for map scenes.
    /// </summary>
    public sealed class NorthArrowRenderer
    {
        public void Render(SKCanvas canvas, Theme theme = null, NorthArrowOptions options = null)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            theme ??= Theme.Standard;
            options ??= new NorthArrowOptions();

            var totalWidth = options.Size + 18f;
            var totalHeight = options.Size + options.FontSize + 20f;
            var bounds = OverlayLayout.ResolveBounds(canvas.DeviceClipBounds, totalWidth, totalHeight, options.Anchor, options.Margin);
            var center = new SKPoint(bounds.MidX, bounds.Top + options.FontSize + options.Size / 2 + 4f);

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

            using var textPaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                TextSize = options.FontSize,
                IsAntialias = true,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Center
            };

            using var fillPaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var strokePaint = new SKPaint
            {
                Color = theme.ForegroundColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            canvas.DrawText("N", bounds.MidX, bounds.Top + options.FontSize, textPaint);

            canvas.Save();
            canvas.Translate(center.X, center.Y);
            canvas.RotateDegrees(options.RotationDegrees);

            var shaftTop = -options.Size / 2 + 4f;
            var shaftBottom = options.Size / 2 - 6f;
            canvas.DrawLine(0, shaftBottom, 0, shaftTop + 8f, strokePaint);

            using var path = new SKPath();
            path.MoveTo(0, shaftTop);
            path.LineTo(-options.Size / 6, shaftTop + options.Size / 3);
            path.LineTo(options.Size / 6, shaftTop + options.Size / 3);
            path.Close();
            canvas.DrawPath(path, fillPaint);

            canvas.Restore();
        }
    }
}