using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;

namespace Beep.OilandGas.WellTestAnalysis.Rendering
{
    /// <summary>
    /// Renders well test analysis plots using SkiaSharp.
    /// </summary>
    public class WellTestRenderer
    {
        private readonly WellTestRendererConfiguration configuration;
        private List<PressureTimePoint> pressureData;
        private List<PressureTimePoint> derivativeData;
        private WellTestAnalysisResult analysisResult;
        
        private double zoom = 1.0;
        private SKPoint panOffset = SKPoint.Empty;
        
        private double minTime;
        private double maxTime;
        private double minPressure;
        private double maxPressure;
        private double minDerivative;
        private double maxDerivative;
        
        private float plotAreaX;
        private float plotAreaY;
        private float plotAreaWidth;
        private float plotAreaHeight;

        /// <summary>
        /// Gets or sets the current zoom level.
        /// </summary>
        public double Zoom
        {
            get => zoom;
            set => zoom = Math.Max(configuration.MinZoom, Math.Min(configuration.MaxZoom, value));
        }

        /// <summary>
        /// Gets or sets the current pan offset.
        /// </summary>
        public SKPoint PanOffset
        {
            get => panOffset;
            set => panOffset = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WellTestRenderer"/> class.
        /// </summary>
        public WellTestRenderer(WellTestRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new WellTestRendererConfiguration();
            this.zoom = 1.0;
        }

        /// <summary>
        /// Sets the pressure-time data to render.
        /// </summary>
        public void SetPressureData(List<PressureTimePoint> data)
        {
            this.pressureData = data?.OrderBy(p => p.Time).ToList();
            UpdateBounds();
        }

        /// <summary>
        /// Sets the derivative data to render.
        /// </summary>
        public void SetDerivativeData(List<PressureTimePoint> data)
        {
            this.derivativeData = data?.OrderBy(p => p.Time).ToList();
            UpdateBounds();
        }

        /// <summary>
        /// Sets the analysis result to display.
        /// </summary>
        public void SetAnalysisResult(WellTestAnalysisResult result)
        {
            this.analysisResult = result;
        }

        /// <summary>
        /// Updates the data bounds.
        /// </summary>
        private void UpdateBounds()
        {
            var allTimes = new List<double>();
            var allPressures = new List<double>();
            var allDerivatives = new List<double>();

            if (pressureData != null && pressureData.Count > 0)
            {
                allTimes.AddRange(pressureData.Select(p => p.Time));
                allPressures.AddRange(pressureData.Select(p => p.Pressure));
            }

            if (derivativeData != null && derivativeData.Count > 0)
            {
                allTimes.AddRange(derivativeData.Select(p => p.Time));
                allDerivatives.AddRange(derivativeData
                    .Where(p => p.PressureDerivative.HasValue)
                    .Select(p => p.PressureDerivative.Value));
            }

            if (allTimes.Count > 0)
            {
                minTime = allTimes.Min();
                maxTime = allTimes.Max();
                double timeRange = maxTime - minTime;
                minTime = Math.Max(0.001, minTime - timeRange * 0.05);
                maxTime += timeRange * 0.05;
            }
            else
            {
                minTime = 0.001;
                maxTime = 1000;
            }

            if (allPressures.Count > 0)
            {
                minPressure = allPressures.Min();
                maxPressure = allPressures.Max();
                double pressureRange = maxPressure - minPressure;
                minPressure -= pressureRange * 0.05;
                maxPressure += pressureRange * 0.05;
            }
            else
            {
                minPressure = 0;
                maxPressure = 5000;
            }

            if (allDerivatives.Count > 0)
            {
                minDerivative = allDerivatives.Min();
                maxDerivative = allDerivatives.Max();
                double derivativeRange = maxDerivative - minDerivative;
                minDerivative -= derivativeRange * 0.05;
                maxDerivative += derivativeRange * 0.05;
            }
            else
            {
                minDerivative = 0;
                maxDerivative = 1000;
            }
        }

        /// <summary>
        /// Renders the well test plot to the canvas.
        /// </summary>
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

            if (pressureData != null && pressureData.Count > 0)
            {
                DrawPressureCurve(canvas);
            }

            if (configuration.ShowDerivative && derivativeData != null && derivativeData.Count > 0)
            {
                DrawDerivativeCurve(canvas);
            }

            if (configuration.ShowLegend)
            {
                DrawLegend(canvas, width, height);
            }

            if (analysisResult != null)
            {
                DrawAnalysisResults(canvas, width, height);
            }
        }

        /// <summary>
        /// Converts time to screen X coordinate.
        /// </summary>
        private float TimeToScreenX(double time)
        {
            if (configuration.UseLogScaleX)
            {
                double logMin = Math.Log10(minTime);
                double logMax = Math.Log10(maxTime);
                double logTime = Math.Log10(time);
                double normalized = (logTime - logMin) / (logMax - logMin);
                return plotAreaX + (float)(normalized * plotAreaWidth) + panOffset.X;
            }
            else
            {
                double normalized = (time - minTime) / (maxTime - minTime);
                return plotAreaX + (float)(normalized * plotAreaWidth) + panOffset.X;
            }
        }

        /// <summary>
        /// Converts pressure to screen Y coordinate.
        /// </summary>
        private float PressureToScreenY(double pressure)
        {
            if (configuration.UseLogScaleY)
            {
                double logMin = Math.Log10(minPressure);
                double logMax = Math.Log10(maxPressure);
                double logPressure = Math.Log10(pressure);
                double normalized = (logPressure - logMin) / (logMax - logMin);
                return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
            }
            else
            {
                double normalized = (pressure - minPressure) / (maxPressure - minPressure);
                return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
            }
        }

        /// <summary>
        /// Converts derivative to screen Y coordinate.
        /// </summary>
        private float DerivativeToScreenY(double derivative)
        {
            if (configuration.UseLogScaleY)
            {
                double logMin = Math.Log10(minDerivative);
                double logMax = Math.Log10(maxDerivative);
                double logDerivative = Math.Log10(derivative);
                double normalized = (logDerivative - logMin) / (logMax - logMin);
                return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
            }
            else
            {
                double normalized = (derivative - minDerivative) / (maxDerivative - minDerivative);
                return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
            }
        }

        /// <summary>
        /// Draws the title.
        /// </summary>
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

        /// <summary>
        /// Draws the grid.
        /// </summary>
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
                    double time;
                    if (configuration.UseLogScaleX)
                    {
                        double logMin = Math.Log10(minTime);
                        double logMax = Math.Log10(maxTime);
                        double logTime = logMin + (logMax - logMin) * i / xTicks;
                        time = Math.Pow(10, logTime);
                    }
                    else
                    {
                        time = minTime + (maxTime - minTime) * i / xTicks;
                    }
                    float x = TimeToScreenX(time);
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

        /// <summary>
        /// Draws the axes.
        /// </summary>
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
                    double time;
                    if (configuration.UseLogScaleX)
                    {
                        double logMin = Math.Log10(minTime);
                        double logMax = Math.Log10(maxTime);
                        double logTime = logMin + (logMax - logMin) * i / xTicks;
                        time = Math.Pow(10, logTime);
                    }
                    else
                    {
                        time = minTime + (maxTime - minTime) * i / xTicks;
                    }
                    float x = TimeToScreenX(time);
                    string label = FormatTime(time);
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

        /// <summary>
        /// Draws the pressure curve.
        /// </summary>
        private void DrawPressureCurve(SKCanvas canvas)
        {
            if (pressureData == null || pressureData.Count == 0)
                return;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in pressureData)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = PressureToScreenY(point.Pressure);

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
                    Color = configuration.PressureCurveColor,
                    StrokeWidth = configuration.PressureCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            if (configuration.ShowPressurePoints)
            {
                using (var paint = new SKPaint
                {
                    Color = configuration.PressureCurveColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    foreach (var point in pressureData)
                    {
                        float x = TimeToScreenX(point.Time);
                        float y = PressureToScreenY(point.Pressure);
                        canvas.DrawCircle(x, y, configuration.PointSize, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the derivative curve.
        /// </summary>
        private void DrawDerivativeCurve(SKCanvas canvas)
        {
            if (derivativeData == null || derivativeData.Count == 0)
                return;

            var validData = derivativeData.Where(p => p.PressureDerivative.HasValue).ToList();
            if (validData.Count == 0)
                return;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in validData)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = DerivativeToScreenY(point.PressureDerivative.Value);

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
                    Color = configuration.DerivativeCurveColor,
                    StrokeWidth = configuration.DerivativeCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Draws the legend.
        /// </summary>
        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var items = new List<(string label, SKColor color)>();

            if (pressureData != null && pressureData.Count > 0)
                items.Add(("Pressure", configuration.PressureCurveColor));

            if (configuration.ShowDerivative && derivativeData != null && derivativeData.Count > 0)
                items.Add(("Derivative", configuration.DerivativeCurveColor));

            if (items.Count == 0)
                return;

            float itemHeight = 20f;
            float padding = 10f;
            float legendWidth = 120f;
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

        /// <summary>
        /// Draws analysis results as text.
        /// </summary>
        private void DrawAnalysisResults(SKCanvas canvas, float width, float height)
        {
            if (analysisResult == null)
                return;

            float x = plotAreaX + 10f;
            float y = plotAreaY + 30f;
            float lineHeight = 18f;

            using (var paint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.FontSize - 1f,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            })
            {
                canvas.DrawText("Analysis Results:", x, y, paint);
                y += lineHeight * 1.5f;

                paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
                canvas.DrawText($"Permeability: {analysisResult.Permeability:F2} md", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"Skin Factor: {analysisResult.SkinFactor:F2}", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"Reservoir Pressure: {analysisResult.ReservoirPressure:F0} psi", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"PI: {analysisResult.ProductivityIndex:F2} BPD/psi", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"Flow Efficiency: {analysisResult.FlowEfficiency:P2}", x, y, paint);
            }
        }

        /// <summary>
        /// Formats time for display.
        /// </summary>
        private string FormatTime(double time)
        {
            if (time < 0.01)
                return $"{time * 1000:F2}m";
            if (time < 1)
                return $"{time * 60:F1}min";
            if (time < 1000)
                return $"{time:F1}h";
            return $"{time / 24:F1}d";
        }

        /// <summary>
        /// Formats pressure for display.
        /// </summary>
        private string FormatPressure(double pressure)
        {
            if (pressure >= 1000)
                return $"{pressure / 1000:F1}K";
            return $"{pressure:F0}";
        }

        /// <summary>
        /// Exports the plot to PNG.
        /// </summary>
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

