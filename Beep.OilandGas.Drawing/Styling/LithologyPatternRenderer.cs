using SkiaSharp;
using System;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Renders lithology patterns for reservoir layer visualization.
    /// Supports both programmatically generated patterns and SVG-based patterns.
    /// </summary>
    public static class LithologyPatternRenderer
    {
        private static SvgLithologyPatternLoader svgLoader;

        /// <summary>
        /// Gets or sets the SVG pattern loader instance.
        /// </summary>
        public static SvgLithologyPatternLoader SvgLoader
        {
            get
            {
                if (svgLoader == null)
                {
                    svgLoader = new SvgLithologyPatternLoader();
                }
                return svgLoader;
            }
            set => svgLoader = value;
        }
        /// <summary>
        /// Creates a paint with a lithology pattern.
        /// </summary>
        /// <param name="baseColor">The base color for the pattern.</param>
        /// <param name="pattern">The pattern type.</param>
        /// <param name="patternColor">The pattern line/dot color (defaults to darker version of base color).</param>
        /// <param name="patternSize">The size of the pattern elements (default 4).</param>
        /// <param name="lithologyName">Optional lithology name for SVG pattern lookup.</param>
        /// <param name="useSvgPattern">Whether to try using SVG patterns first (default: true).</param>
        /// <returns>A paint configured with the pattern.</returns>
        public static SKPaint CreatePatternPaint(
            SKColor baseColor,
            LithologyPattern pattern,
            SKColor? patternColor = null,
            float patternSize = 4.0f,
            string lithologyName = null,
            bool useSvgPattern = true)
        {
            var paint = new SKPaint
            {
                Color = baseColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            if (pattern == LithologyPattern.Solid)
            {
                return paint;
            }

            // Try to use SVG pattern if available and requested
            if (useSvgPattern && !string.IsNullOrEmpty(lithologyName))
            {
                string svgPatternName = SvgLithologyPatternLoader.GetSvgPatternName(lithologyName);
                if (!string.IsNullOrEmpty(svgPatternName))
                {
                    // Use darker color for stroke (pattern lines)
                    SKColor strokeColor = new SKColor(
                        (byte)(baseColor.Red * 0.7),
                        (byte)(baseColor.Green * 0.7),
                        (byte)(baseColor.Blue * 0.7));
                    
                    var svgPaint = SvgLoader.CreateSvgPatternPaint(svgPatternName, baseColor, (int)patternSize, strokeColor);
                    if (svgPaint != null)
                    {
                        return svgPaint;
                    }
                }
            }

            // Fall back to programmatically generated pattern
            var patternPaintColor = patternColor ?? DarkenColor(baseColor, 0.3f);
            var shader = CreatePatternShader(baseColor, patternPaintColor, pattern, patternSize);
            paint.Shader = shader;

            return paint;
        }

        /// <summary>
        /// Creates a pattern shader.
        /// </summary>
        private static SKShader CreatePatternShader(
            SKColor baseColor,
            SKColor patternColor,
            LithologyPattern pattern,
            float patternSize)
        {
            // Create a small bitmap for the pattern
            int size = (int)Math.Max(16, patternSize * 4);
            using (var bitmap = new SKBitmap(size, size))
            using (var canvas = new SKCanvas(bitmap))
            {
                // Fill with base color
                canvas.Clear(baseColor);

                var patternPaint = new SKPaint
                {
                    Color = patternColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.0f,
                    IsAntialias = true
                };

                switch (pattern)
                {
                    case LithologyPattern.HorizontalLines:
                        DrawHorizontalLines(canvas, size, patternPaint);
                        break;

                    case LithologyPattern.VerticalLines:
                        DrawVerticalLines(canvas, size, patternPaint);
                        break;

                    case LithologyPattern.DiagonalLines:
                        DrawDiagonalLines(canvas, size, patternPaint, false);
                        break;

                    case LithologyPattern.DiagonalCrossHatch:
                        DrawDiagonalLines(canvas, size, patternPaint, false);
                        DrawDiagonalLines(canvas, size, patternPaint, true);
                        break;

                    case LithologyPattern.Dots:
                        DrawDots(canvas, size, patternPaint, patternSize);
                        break;

                    case LithologyPattern.CrossHatch:
                        DrawHorizontalLines(canvas, size, patternPaint);
                        DrawVerticalLines(canvas, size, patternPaint);
                        break;

                    case LithologyPattern.Brick:
                        DrawBrickPattern(canvas, size, patternPaint);
                        break;

                    case LithologyPattern.Zigzag:
                        DrawZigzagPattern(canvas, size, patternPaint);
                        break;
                }

                // Create shader from bitmap
                return SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            }
        }

        private static void DrawHorizontalLines(SKCanvas canvas, int size, SKPaint paint)
        {
            float spacing = size / 4.0f;
            for (float y = spacing; y < size; y += spacing)
            {
                canvas.DrawLine(0, y, size, y, paint);
            }
        }

        private static void DrawVerticalLines(SKCanvas canvas, int size, SKPaint paint)
        {
            float spacing = size / 4.0f;
            for (float x = spacing; x < size; x += spacing)
            {
                canvas.DrawLine(x, 0, x, size, paint);
            }
        }

        private static void DrawDiagonalLines(SKCanvas canvas, int size, SKPaint paint, bool reverse)
        {
            float spacing = size / 4.0f;
            if (reverse)
            {
                for (float offset = -size; offset < size * 2; offset += spacing)
                {
                    canvas.DrawLine(offset, 0, offset + size, size, paint);
                }
            }
            else
            {
                for (float offset = 0; offset < size * 2; offset += spacing)
                {
                    canvas.DrawLine(offset, 0, offset - size, size, paint);
                }
            }
        }

        private static void DrawDots(SKCanvas canvas, int size, SKPaint paint, float dotSize)
        {
            paint.Style = SKPaintStyle.Fill;
            float spacing = size / 3.0f;
            float radius = Math.Min(dotSize, spacing / 3.0f);

            for (float y = spacing; y < size; y += spacing)
            {
                for (float x = spacing; x < size; x += spacing)
                {
                    canvas.DrawCircle(x, y, radius, paint);
                }
            }
        }

        private static void DrawBrickPattern(SKCanvas canvas, int size, SKPaint paint)
        {
            float brickWidth = size / 2.0f;
            float brickHeight = size / 4.0f;

            for (int row = 0; row < 4; row++)
            {
                float y = row * brickHeight;
                float offset = (row % 2 == 0) ? 0 : brickWidth / 2.0f;

                for (float x = offset; x < size; x += brickWidth)
                {
                    var rect = new SKRect(x, y, x + brickWidth, y + brickHeight);
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        private static void DrawZigzagPattern(SKCanvas canvas, int size, SKPaint paint)
        {
            float spacing = size / 4.0f;
            var path = new SKPath();

            for (float y = 0; y < size; y += spacing)
            {
                bool isUp = ((int)(y / spacing) % 2) == 0;
                float x = isUp ? 0 : size / 2.0f;
                path.MoveTo(x, y);
                path.LineTo(x + size / 2.0f, y + spacing / 2.0f);
                path.LineTo(x, y + spacing);
            }

            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Darkens a color by a specified factor.
        /// </summary>
        private static SKColor DarkenColor(SKColor color, float factor)
        {
            byte r = (byte)Math.Max(0, color.Red - (color.Red * factor));
            byte g = (byte)Math.Max(0, color.Green - (color.Green * factor));
            byte b = (byte)Math.Max(0, color.Blue - (color.Blue * factor));
            return new SKColor(r, g, b, color.Alpha);
        }
    }
}

