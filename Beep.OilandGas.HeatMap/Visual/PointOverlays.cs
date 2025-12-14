using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Visual
{
    /// <summary>
    /// Represents an SVG image overlay at a specific point location.
    /// </summary>
    public class SvgImageOverlay
    {
        /// <summary>
        /// Gets or sets the X coordinate (in data space).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (in data space).
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the SVG file path or SVG content string.
        /// </summary>
        public string SvgPathOrContent { get; set; }

        /// <summary>
        /// Gets or sets the width of the rendered SVG.
        /// </summary>
        public float Width { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the height of the rendered SVG.
        /// </summary>
        public float Height { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the offset from the point (in pixels).
        /// </summary>
        public SKPoint Offset { get; set; } = new SKPoint(0, 0);

        /// <summary>
        /// Gets or sets the rotation angle in degrees.
        /// </summary>
        public float Rotation { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the opacity (0.0 to 1.0).
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets whether the overlay is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the SVG path is content (true) or file path (false).
        /// </summary>
        public bool IsSvgContent { get; set; } = false;
    }

    /// <summary>
    /// Represents a pie chart overlay at a specific point location.
    /// </summary>
    public class PieChartOverlay
    {
        /// <summary>
        /// Gets or sets the X coordinate (in data space).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (in data space).
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the data segments for the pie chart (value, color pairs).
        /// </summary>
        public List<(double Value, SKColor Color)> Segments { get; set; }

        /// <summary>
        /// Gets or sets the radius of the pie chart.
        /// </summary>
        public float Radius { get; set; } = 20f;

        /// <summary>
        /// Gets or sets the offset from the point (in pixels).
        /// </summary>
        public SKPoint Offset { get; set; } = new SKPoint(0, 0);

        /// <summary>
        /// Gets or sets the rotation angle in degrees.
        /// </summary>
        public float StartAngle { get; set; } = -90f; // Start at top

        /// <summary>
        /// Gets or sets whether to show segment labels.
        /// </summary>
        public bool ShowLabels { get; set; } = false;

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 8f;

        /// <summary>
        /// Gets or sets whether to show a border.
        /// </summary>
        public bool ShowBorder { get; set; } = true;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public SKColor BorderColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        public float BorderWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether the overlay is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        public PieChartOverlay()
        {
            Segments = new List<(double, SKColor)>();
        }
    }

    /// <summary>
    /// Represents a bar chart overlay at a specific point location.
    /// </summary>
    public class BarChartOverlay
    {
        /// <summary>
        /// Gets or sets the X coordinate (in data space).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (in data space).
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the data bars for the chart (value, color pairs).
        /// </summary>
        public List<(double Value, SKColor Color, string Label)> Bars { get; set; }

        /// <summary>
        /// Gets or sets the width of the bar chart.
        /// </summary>
        public float Width { get; set; } = 40f;

        /// <summary>
        /// Gets or sets the height of the bar chart.
        /// </summary>
        public float Height { get; set; } = 40f;

        /// <summary>
        /// Gets or sets the offset from the point (in pixels).
        /// </summary>
        public SKPoint Offset { get; set; } = new SKPoint(0, 0);

        /// <summary>
        /// Gets or sets the orientation (Horizontal or Vertical).
        /// </summary>
        public BarChartOrientation Orientation { get; set; } = BarChartOrientation.Vertical;

        /// <summary>
        /// Gets or sets whether to show bar labels.
        /// </summary>
        public bool ShowLabels { get; set; } = false;

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 8f;

        /// <summary>
        /// Gets or sets whether to show a border.
        /// </summary>
        public bool ShowBorder { get; set; } = true;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public SKColor BorderColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        public float BorderWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets whether the overlay is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        public BarChartOverlay()
        {
            Bars = new List<(double, SKColor, string)>();
        }
    }

    /// <summary>
    /// Orientation for bar charts.
    /// </summary>
    public enum BarChartOrientation
    {
        /// <summary>
        /// Bars extend vertically (upward).
        /// </summary>
        Vertical,

        /// <summary>
        /// Bars extend horizontally (rightward).
        /// </summary>
        Horizontal
    }

    /// <summary>
    /// Provides rendering methods for point overlays (SVG images, charts).
    /// </summary>
    public static class PointOverlayRenderer
    {
        /// <summary>
        /// Renders an SVG image overlay.
        /// </summary>
        public static void RenderSvgOverlay(SKCanvas canvas, SvgImageOverlay overlay,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            if (overlay == null || !overlay.IsVisible || string.IsNullOrEmpty(overlay.SvgPathOrContent))
                return;

            try
            {
                // Try to load SVG using SkiaSharp.Svg (if available)
                SKPicture svgPicture = null;

                // Use reflection to avoid direct dependency issues
                var svgType = Type.GetType("SkiaSharp.Svg.SKSvg, SkiaSharp.Svg");
                if (svgType != null)
                {
                    object svgInstance = Activator.CreateInstance(svgType);

                    if (overlay.IsSvgContent)
                    {
                        // Load from SVG content string
                        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(overlay.SvgPathOrContent)))
                        {
                            var loadMethod = svgType.GetMethod("Load", new[] { typeof(Stream) });
                            loadMethod?.Invoke(svgInstance, new object[] { stream });
                            var pictureProperty = svgType.GetProperty("Picture");
                            svgPicture = pictureProperty?.GetValue(svgInstance) as SKPicture;
                        }
                    }
                    else
                    {
                        // Load from file path
                        using (var stream = File.OpenRead(overlay.SvgPathOrContent))
                        {
                            var loadMethod = svgType.GetMethod("Load", new[] { typeof(Stream) });
                            loadMethod?.Invoke(svgInstance, new object[] { stream });
                            var pictureProperty = svgType.GetProperty("Picture");
                            svgPicture = pictureProperty?.GetValue(svgInstance) as SKPicture;
                        }
                    }
                }

                if (svgPicture == null)
                    return;

                float x = (float)(overlay.X * scaleX + offsetX) + overlay.Offset.X;
                float y = (float)(overlay.Y * scaleY + offsetY) + overlay.Offset.Y;

                canvas.Save();

                // Apply transformations
                canvas.Translate(x, y);
                if (overlay.Rotation != 0)
                {
                    canvas.RotateDegrees(overlay.Rotation);
                }

                // Apply opacity
                if (overlay.Opacity < 1.0f)
                {
                    var paint = new SKPaint
                    {
                        Color = new SKColor(255, 255, 255, (byte)(255 * overlay.Opacity))
                    };
                    canvas.SaveLayer(paint);
                }

                // Calculate scaling to fit desired size
                var bounds = svgPicture.CullRect;
                float scaleX_svg = overlay.Width / bounds.Width;
                float scaleY_svg = overlay.Height / bounds.Height;
                canvas.Scale(scaleX_svg, scaleY_svg);

                // Center the SVG
                canvas.Translate(-bounds.MidX, -bounds.MidY);

                // Draw SVG
                canvas.DrawPicture(svgPicture);

                canvas.Restore();
            }
            catch (Exception)
            {
                // Silently fail if SVG cannot be loaded
            }
        }

        /// <summary>
        /// Renders a pie chart overlay.
        /// </summary>
        public static void RenderPieChart(SKCanvas canvas, PieChartOverlay chart,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            if (chart == null || !chart.IsVisible || chart.Segments == null || chart.Segments.Count == 0)
                return;

            float centerX = (float)(chart.X * scaleX + offsetX) + chart.Offset.X;
            float centerY = (float)(chart.Y * scaleY + offsetY) + chart.Offset.Y;

            // Calculate total value
            double total = 0;
            foreach (var segment in chart.Segments)
            {
                total += segment.Value;
            }

            if (total <= 0)
                return;

            // Draw pie segments
            float currentAngle = chart.StartAngle;
            var rect = new SKRect(
                centerX - chart.Radius,
                centerY - chart.Radius,
                centerX + chart.Radius,
                centerY + chart.Radius);

            foreach (var segment in chart.Segments)
            {
                if (segment.Value <= 0)
                    continue;

                float sweepAngle = (float)(360.0 * segment.Value / total);

                using (var paint = new SKPaint
                {
                    Color = segment.Color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    using (var path = new SKPath())
                    {
                        path.AddArc(rect, currentAngle, sweepAngle);
                        path.LineTo(centerX, centerY);
                        path.Close();
                        canvas.DrawPath(path, paint);
                    }
                }

                // Draw border if enabled
                if (chart.ShowBorder)
                {
                    using (var borderPaint = new SKPaint
                    {
                        Color = chart.BorderColor,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = chart.BorderWidth,
                        IsAntialias = true
                    })
                    {
                        using (var path = new SKPath())
                        {
                            path.AddArc(rect, currentAngle, sweepAngle);
                            path.LineTo(centerX, centerY);
                            path.Close();
                            canvas.DrawPath(path, borderPaint);
                        }
                    }
                }

                // Draw label if enabled
                if (chart.ShowLabels && sweepAngle > 10) // Only show label if segment is large enough
                {
                    float labelAngle = currentAngle + sweepAngle / 2;
                    float labelRadius = chart.Radius * 0.7f;
                    float labelX = centerX + labelRadius * (float)Math.Cos(labelAngle * Math.PI / 180);
                    float labelY = centerY + labelRadius * (float)Math.Sin(labelAngle * Math.PI / 180);

                    var labelPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        TextSize = chart.LabelFontSize,
                        TextAlign = SKTextAlign.Center,
                        IsAntialias = true,
                        FakeBoldText = true
                    };

                    string label = $"{segment.Value:F1}";
                    canvas.DrawText(label, labelX, labelY + chart.LabelFontSize / 3, labelPaint);
                }

                currentAngle += sweepAngle;
            }
        }

        /// <summary>
        /// Renders a bar chart overlay.
        /// </summary>
        public static void RenderBarChart(SKCanvas canvas, BarChartOverlay chart,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            if (chart == null || !chart.IsVisible || chart.Bars == null || chart.Bars.Count == 0)
                return;

            float x = (float)(chart.X * scaleX + offsetX) + chart.Offset.X;
            float y = (float)(chart.Y * scaleY + offsetY) + chart.Offset.Y;

            // Calculate max value for scaling
            double maxValue = 0;
            foreach (var bar in chart.Bars)
            {
                if (bar.Value > maxValue)
                    maxValue = bar.Value;
            }

            if (maxValue <= 0)
                return;

            int barCount = chart.Bars.Count;
            float barSpacing = 2f;

            if (chart.Orientation == BarChartOrientation.Vertical)
            {
                float barWidth = (chart.Width - (barCount - 1) * barSpacing) / barCount;
                float baseY = y + chart.Height;

                for (int i = 0; i < barCount; i++)
                {
                    var bar = chart.Bars[i];
                    float barHeight = (float)(chart.Height * bar.Value / maxValue);
                    float barX = x + i * (barWidth + barSpacing);
                    float barY = baseY - barHeight;

                    var barRect = new SKRect(barX, barY, barX + barWidth, baseY);

                    // Draw bar
                    using (var paint = new SKPaint
                    {
                        Color = bar.Color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(barRect, paint);
                    }

                    // Draw border
                    if (chart.ShowBorder)
                    {
                        using (var borderPaint = new SKPaint
                        {
                            Color = chart.BorderColor,
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = chart.BorderWidth,
                            IsAntialias = true
                        })
                        {
                            canvas.DrawRect(barRect, borderPaint);
                        }
                    }

                    // Draw label
                    if (chart.ShowLabels && !string.IsNullOrEmpty(bar.Label))
                    {
                        var labelPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = chart.LabelFontSize,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true
                        };

                        canvas.DrawText(bar.Label, barX + barWidth / 2, baseY + chart.LabelFontSize + 2, labelPaint);
                    }
                }
            }
            else // Horizontal
            {
                float barHeight = (chart.Height - (barCount - 1) * barSpacing) / barCount;
                float baseX = x;

                for (int i = 0; i < barCount; i++)
                {
                    var bar = chart.Bars[i];
                    float barWidth = (float)(chart.Width * bar.Value / maxValue);
                    float barY = y + i * (barHeight + barSpacing);
                    float barX = baseX;

                    var barRect = new SKRect(barX, barY, barX + barWidth, barY + barHeight);

                    // Draw bar
                    using (var paint = new SKPaint
                    {
                        Color = bar.Color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(barRect, paint);
                    }

                    // Draw border
                    if (chart.ShowBorder)
                    {
                        using (var borderPaint = new SKPaint
                        {
                            Color = chart.BorderColor,
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = chart.BorderWidth,
                            IsAntialias = true
                        })
                        {
                            canvas.DrawRect(barRect, borderPaint);
                        }
                    }

                    // Draw label
                    if (chart.ShowLabels && !string.IsNullOrEmpty(bar.Label))
                    {
                        var labelPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = chart.LabelFontSize,
                            TextAlign = SKTextAlign.Left,
                            IsAntialias = true
                        };

                        canvas.DrawText(bar.Label, barX + barWidth + 3, barY + barHeight / 2 + chart.LabelFontSize / 3, labelPaint);
                    }
                }
            }
        }
    }
}

