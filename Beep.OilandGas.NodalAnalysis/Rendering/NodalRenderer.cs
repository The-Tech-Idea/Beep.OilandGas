using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Models.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Rendering
{
    /// <summary>
    /// Renders nodal analysis plots using SkiaSharp.
    /// </summary>
    public class NodalRenderer
    {
        private readonly NodalRendererConfiguration configuration;
        private List<IPRPoint> iprCurve;
        private List<VLPPoint> vlpCurve;
        private OperatingPoint operatingPoint;
        
        private double zoom = 1.0;
        private SKPoint panOffset = SKPoint.Empty;
        
        private double minFlowRate;
        private double maxFlowRate;
        private double minPressure;
        private double maxPressure;
        
        private float plotAreaX;
        private float plotAreaY;
        private float plotAreaWidth;
        private float plotAreaHeight;

        public double Zoom
        {
            get => zoom;
            set => zoom = Math.Max(configuration.MinZoom, Math.Min(configuration.MaxZoom, value));
        }

        public SKPoint PanOffset
        {
            get => panOffset;
            set => panOffset = value;
        }

        public NodalRenderer(NodalRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new NodalRendererConfiguration();
            this.zoom = 1.0;
        }

        public void SetIPRCurve(List<IPRPoint> curve)
        {
            this.iprCurve = curve?.OrderBy(p => p.FlowRate).ToList();
            UpdateBounds();
        }

        public void SetVLPCurve(List<VLPPoint> curve)
        {
            this.vlpCurve = curve?.OrderBy(p => p.FlowRate).ToList();
            UpdateBounds();
        }

        public void SetOperatingPoint(OperatingPoint point)
        {
            this.operatingPoint = point;
        }

        private void UpdateBounds()
        {
            var allFlowRates = new List<double>();
            var allPressures = new List<double>();

            if (iprCurve != null && iprCurve.Count > 0)
            {
                allFlowRates.AddRange(iprCurve.Select(p => p.FlowRate));
                allPressures.AddRange(iprCurve.Select(p => p.FlowingBottomholePressure));
            }

            if (vlpCurve != null && vlpCurve.Count > 0)
            {
                allFlowRates.AddRange(vlpCurve.Select(p => p.FlowRate));
                allPressures.AddRange(vlpCurve.Select(p => p.RequiredBottomholePressure));
            }

            if (operatingPoint != null)
            {
                allFlowRates.Add(operatingPoint.FlowRate);
                allPressures.Add(operatingPoint.BottomholePressure);
            }

            if (allFlowRates.Count > 0)
            {
                minFlowRate = allFlowRates.Min();
                maxFlowRate = allFlowRates.Max();
                double range = maxFlowRate - minFlowRate;
                minFlowRate -= range * 0.05;
                maxFlowRate += range * 0.05;
            }
            else
            {
                minFlowRate = 0;
                maxFlowRate = 5000;
            }

            if (allPressures.Count > 0)
            {
                minPressure = allPressures.Min();
                maxPressure = allPressures.Max();
                double range = maxPressure - minPressure;
                minPressure -= range * 0.05;
                maxPressure += range * 0.05;
            }
            else
            {
                minPressure = 0;
                maxPressure = 5000;
            }
        }

        public void Render(SKCanvas canvas, float width, float height)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            canvas.Clear(configuration.BackgroundColor);

            plotAreaX = configuration.LeftMargin;
            plotAreaY = configuration.TopMargin;
            plotAreaWidth = width - configuration.LeftMargin - configuration.RightMargin;
            plotAreaHeight = height - configuration.TopMargin - configuration.BottomMargin;

            DrawTitle(canvas, width);

            if (configuration.ShowGrid)
            {
                DrawGrid(canvas);
            }

            DrawAxes(canvas);

            if (iprCurve != null && iprCurve.Count > 0)
            {
                DrawIPRCurve(canvas);
            }

            if (vlpCurve != null && vlpCurve.Count > 0)
            {
                DrawVLPCurve(canvas);
            }

            if (operatingPoint != null)
            {
                DrawOperatingPoint(canvas);
            }

            if (configuration.ShowLegend)
            {
                DrawLegend(canvas, width, height);
            }
        }

        private float FlowRateToScreenX(double flowRate)
        {
            double normalized = (flowRate - minFlowRate) / (maxFlowRate - minFlowRate);
            return plotAreaX + (float)(normalized * plotAreaWidth) + panOffset.X;
        }

        private float PressureToScreenY(double pressure)
        {
            double normalized = (pressure - minPressure) / (maxPressure - minPressure);
            return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
        }

        private void DrawTitle(SKCanvas canvas, float width)
        {
            if (string.IsNullOrEmpty(configuration.Title))
                return;

            using (var paint = new SKPaint
            {
                Color = configuration.TitleColor,
                TextSize = configuration.TitleFontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                TextAlign = SKTextAlign.Center
            })
            {
                float x = width / 2f;
                float y = configuration.TopMargin / 2f;
                canvas.DrawText(configuration.Title, x, y, paint);
            }
        }

        private void DrawGrid(SKCanvas canvas)
        {
            using (var paint = new SKPaint
            {
                Color = configuration.GridColor,
                StrokeWidth = configuration.GridLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double flowRate = minFlowRate + (maxFlowRate - minFlowRate) * i / xTicks;
                    float x = FlowRateToScreenX(flowRate);
                    canvas.DrawLine(x, plotAreaY, x, plotAreaY + plotAreaHeight, paint);
                }

                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    double pressure = minPressure + (maxPressure - minPressure) * i / yTicks;
                    float y = PressureToScreenY(pressure);
                    canvas.DrawLine(plotAreaX, y, plotAreaX + plotAreaWidth, y, paint);
                }
            }
        }

        private void DrawAxes(SKCanvas canvas)
        {
            using (var axisPaint = new SKPaint
            {
                Color = configuration.TextColor,
                StrokeWidth = 2f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawLine(plotAreaX, plotAreaY + plotAreaHeight, plotAreaX + plotAreaWidth, plotAreaY + plotAreaHeight, axisPaint);
                canvas.DrawLine(plotAreaX, plotAreaY, plotAreaX, plotAreaY + plotAreaHeight, axisPaint);
            }

            using (var labelPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.AxisLabelFontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            })
            {
                labelPaint.TextAlign = SKTextAlign.Center;
                float xLabelY = plotAreaY + plotAreaHeight + configuration.BottomMargin / 2f;
                canvas.DrawText(configuration.XAxisLabel, plotAreaX + plotAreaWidth / 2f, xLabelY, labelPaint);

                labelPaint.TextAlign = SKTextAlign.Center;
                float yLabelX = configuration.LeftMargin / 2f;
                canvas.Save();
                canvas.Translate(yLabelX, plotAreaY + plotAreaHeight / 2f);
                canvas.RotateDegrees(-90);
                canvas.DrawText(configuration.YAxisLabel, 0, 0, labelPaint);
                canvas.Restore();
            }

            using (var tickPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.TickLabelFontSize,
                IsAntialias = true
            })
            {
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double flowRate = minFlowRate + (maxFlowRate - minFlowRate) * i / xTicks;
                    float x = FlowRateToScreenX(flowRate);
                    string label = FormatFlowRate(flowRate);
                    tickPaint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText(label, x, plotAreaY + plotAreaHeight + 20f, tickPaint);
                }

                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    double pressure = minPressure + (maxPressure - minPressure) * i / yTicks;
                    float y = PressureToScreenY(pressure);
                    string label = FormatPressure(pressure);
                    tickPaint.TextAlign = SKTextAlign.Right;
                    canvas.DrawText(label, plotAreaX - 10f, y + 5f, tickPaint);
                }
            }
        }

        private void DrawIPRCurve(SKCanvas canvas)
        {
            if (iprCurve == null || iprCurve.Count == 0)
                return;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in iprCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = PressureToScreenY(point.FlowingBottomholePressure);

                    if (first)
                    {
                        path.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        path.LineTo(x, y);
                    }
                }

                using (var paint = new SKPaint
                {
                    Color = configuration.IPRCurveColor,
                    StrokeWidth = configuration.IPRCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawVLPCurve(SKCanvas canvas)
        {
            if (vlpCurve == null || vlpCurve.Count == 0)
                return;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in vlpCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = PressureToScreenY(point.RequiredBottomholePressure);

                    if (first)
                    {
                        path.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        path.LineTo(x, y);
                    }
                }

                using (var paint = new SKPaint
                {
                    Color = configuration.VLPCurveColor,
                    StrokeWidth = configuration.VLPCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawOperatingPoint(SKCanvas canvas)
        {
            if (operatingPoint == null)
                return;

            float x = FlowRateToScreenX(operatingPoint.FlowRate);
            float y = PressureToScreenY(operatingPoint.BottomholePressure);

            using (var paint = new SKPaint
            {
                Color = configuration.OperatingPointColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(x, y, configuration.OperatingPointSize, paint);
            }

            using (var paint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(x, y, configuration.OperatingPointSize, paint);
            }

            if (configuration.ShowOperatingPointLabel)
            {
                using (var paint = new SKPaint
                {
                    Color = configuration.OperatingPointColor,
                    TextSize = configuration.FontSize,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                })
                {
                    string label = $"OP: {FormatFlowRate(operatingPoint.FlowRate)}, {FormatPressure(operatingPoint.BottomholePressure)}";
                    paint.TextAlign = SKTextAlign.Left;
                    canvas.DrawText(label, x + configuration.OperatingPointSize + 5f, y - 5f, paint);
                }
            }
        }

        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var items = new List<(string label, SKColor color)>();

            if (iprCurve != null && iprCurve.Count > 0)
                items.Add(("IPR", configuration.IPRCurveColor));

            if (vlpCurve != null && vlpCurve.Count > 0)
                items.Add(("VLP", configuration.VLPCurveColor));

            if (items.Count == 0)
                return;

            float itemHeight = 20f;
            float padding = 10f;
            float legendWidth = 100f;
            float legendHeight = items.Count * itemHeight + padding * 2;

            float legendX = plotAreaX + plotAreaWidth - legendWidth - 10f;
            float legendY = plotAreaY + 10f;

            using (var bgPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 230),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawRect(legendX, legendY, legendWidth, legendHeight, bgPaint);
            }

            using (var borderPaint = new SKPaint
            {
                Color = new SKColor(200, 200, 200),
                StrokeWidth = 1f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawRect(legendX, legendY, legendWidth, legendHeight, borderPaint);
            }

            using (var textPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.FontSize - 2f,
                IsAntialias = true
            })
            {
                for (int i = 0; i < items.Count; i++)
                {
                    float y = legendY + padding + i * itemHeight + itemHeight / 2f;

                    using (var boxPaint = new SKPaint
                    {
                        Color = items[i].color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(legendX + padding, y - 6f, 12f, 12f, boxPaint);
                    }

                    textPaint.TextAlign = SKTextAlign.Left;
                    canvas.DrawText(items[i].label, legendX + padding + 18f, y + 4f, textPaint);
                }
            }
        }

        private string FormatFlowRate(double flowRate)
        {
            if (flowRate >= 1000)
                return $"{flowRate / 1000:F1}K";
            return $"{flowRate:F0}";
        }

        private string FormatPressure(double pressure)
        {
            if (pressure >= 1000)
                return $"{pressure / 1000:F1}K";
            return $"{pressure:F0}";
        }

        public void ExportToPng(string filePath, float width, float height)
        {
            int pixelWidth = (int)(width * configuration.ExportDPI / 96f);
            int pixelHeight = (int)(height * configuration.ExportDPI / 96f);

            var imageInfo = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

            using (var surface = SKSurface.Create(imageInfo))
            {
                var canvas = surface.Canvas;
                canvas.Scale(configuration.ExportDPI / 96f);
                Render(canvas, width, height);

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = System.IO.File.Create(filePath))
                {
                    data.SaveTo(stream);
                }
            }
        }
    }
}

