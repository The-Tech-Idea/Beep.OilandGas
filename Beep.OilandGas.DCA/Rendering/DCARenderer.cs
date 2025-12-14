using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.DCA.Visualization;

namespace Beep.OilandGas.DCA.Rendering
{
    /// <summary>
    /// Renders DCA plots using SkiaSharp.
    /// </summary>
    public class DCARenderer
    {
        private readonly DeclineCurvePlot plot;
        private readonly PlotWithIntervals plotWithIntervals;
        private readonly DCARendererConfiguration configuration;
        
        private double zoom = 1.0;
        private SKPoint panOffset = SKPoint.Empty;
        
        private double minTime;
        private double maxTime;
        private double minRate;
        private double maxRate;
        
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
        /// Initializes a new instance of the <see cref="DCARenderer"/> class.
        /// </summary>
        /// <param name="plot">The decline curve plot to render.</param>
        /// <param name="configuration">Rendering configuration.</param>
        public DCARenderer(DeclineCurvePlot plot, DCARendererConfiguration configuration = null)
        {
            this.plot = plot ?? throw new ArgumentNullException(nameof(plot));
            this.configuration = configuration ?? new DCARendererConfiguration();
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCARenderer"/> class for plots with intervals.
        /// </summary>
        /// <param name="plotWithIntervals">The plot with intervals to render.</param>
        /// <param name="configuration">Rendering configuration.</param>
        public DCARenderer(PlotWithIntervals plotWithIntervals, DCARendererConfiguration configuration = null)
        {
            this.plotWithIntervals = plotWithIntervals ?? throw new ArgumentNullException(nameof(plotWithIntervals));
            this.configuration = configuration ?? new DCARendererConfiguration();
            this.configuration.ShowPredictionIntervals = true;
            Initialize();
        }

        /// <summary>
        /// Initializes the renderer with plot bounds.
        /// </summary>
        private void Initialize()
        {
            if (plot != null)
            {
                minTime = plot.MinTime;
                maxTime = plot.MaxTime;
                minRate = plot.MinProductionRate;
                maxRate = plot.MaxProductionRate;
            }
            else if (plotWithIntervals != null)
            {
                var allPoints = plotWithIntervals.ObservedPoints
                    .Concat(plotWithIntervals.PredictedPointsWithIntervals.Cast<PlotPoint>())
                    .Concat(plotWithIntervals.ForecastPointsWithIntervals.Cast<PlotPoint>());
                
                minTime = allPoints.Min(p => p.Time);
                maxTime = allPoints.Max(p => p.Time);
                minRate = allPoints.Min(p => p.ProductionRate);
                maxRate = allPoints.Max(p => p.ProductionRate);
            }

            // Add padding
            double timeRange = maxTime - minTime;
            double rateRange = maxRate - minRate;
            minTime -= timeRange * 0.05;
            maxTime += timeRange * 0.05;
            minRate -= rateRange * 0.05;
            maxRate += rateRange * 0.05;
        }

        /// <summary>
        /// Renders the plot to the canvas.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        public void Render(SKCanvas canvas, float width, float height)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            // Calculate plot area
            plotAreaX = configuration.LeftMargin;
            plotAreaY = configuration.TopMargin;
            plotAreaWidth = width - configuration.LeftMargin - configuration.RightMargin;
            plotAreaHeight = height - configuration.TopMargin - configuration.BottomMargin;

            // Clear background
            canvas.Clear(configuration.BackgroundColor);

            // Draw plot area background
            using (var paint = new SKPaint
            {
                Color = configuration.PlotAreaBackgroundColor,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(plotAreaX, plotAreaY, plotAreaWidth, plotAreaHeight, paint);
            }

            // Draw grid
            if (configuration.ShowGrid)
            {
                DrawGrid(canvas);
            }

            // Draw prediction intervals if enabled
            if (configuration.ShowPredictionIntervals && plotWithIntervals != null)
            {
                DrawPredictionIntervals(canvas);
            }

            // Draw curves
            if (plot != null)
            {
                if (configuration.ShowPredictedCurve)
                    DrawCurve(canvas, plot.PredictedPoints, configuration.PredictedColor, false);
                
                if (configuration.ShowForecastCurve)
                    DrawCurve(canvas, plot.ForecastPoints, configuration.ForecastColor, false);
                
                if (configuration.ShowObservedPoints)
                    DrawPoints(canvas, plot.ObservedPoints, configuration.ObservedColor);
            }
            else if (plotWithIntervals != null)
            {
                if (configuration.ShowPredictedCurve)
                    DrawCurveWithIntervals(canvas, plotWithIntervals.PredictedPointsWithIntervals, configuration.PredictedColor);
                
                if (configuration.ShowForecastCurve)
                    DrawCurveWithIntervals(canvas, plotWithIntervals.ForecastPointsWithIntervals, configuration.ForecastColor);
                
                if (configuration.ShowObservedPoints)
                    DrawPoints(canvas, plotWithIntervals.ObservedPoints, configuration.ObservedColor);
            }

            // Draw axes
            if (configuration.ShowAxisLabels || configuration.ShowTicks)
            {
                DrawAxes(canvas, width, height);
            }

            // Draw title
            if (!string.IsNullOrEmpty(plot?.Title) || !string.IsNullOrEmpty(plotWithIntervals?.Title))
            {
                DrawTitle(canvas, width);
            }

            // Draw legend
            if (configuration.ShowLegend)
            {
                DrawLegend(canvas, width, height);
            }

            // Draw crosshair
            if (configuration.ShowCrosshair)
            {
                DrawCrosshair(canvas, width, height);
            }
        }

        /// <summary>
        /// Draws grid lines.
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
                // Vertical grid lines (time axis)
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double time = minTime + (maxTime - minTime) * i / xTicks;
                    float x = TimeToScreenX(time);
                    canvas.DrawLine(x, plotAreaY, x, plotAreaY + plotAreaHeight, paint);
                }

                // Horizontal grid lines (rate axis)
                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    double rate = minRate + (maxRate - minRate) * i / yTicks;
                    float y = RateToScreenY(rate);
                    canvas.DrawLine(plotAreaX, y, plotAreaX + plotAreaWidth, y, paint);
                }
            }
        }

        /// <summary>
        /// Draws a curve from points.
        /// </summary>
        private void DrawCurve(SKCanvas canvas, List<PlotPoint> points, SKColor color, bool showPoints)
        {
            if (points == null || points.Count == 0)
                return;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in points)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = RateToScreenY(point.ProductionRate);
                    
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
                    Color = color,
                    StrokeWidth = configuration.LineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            if (showPoints)
            {
                DrawPoints(canvas, points, color);
            }
        }

        /// <summary>
        /// Draws a curve with prediction intervals.
        /// </summary>
        private void DrawCurveWithIntervals(SKCanvas canvas, List<IntervalPlotPoint> points, SKColor color)
        {
            if (points == null || points.Count == 0)
                return;

            // Draw confidence interval bands
            using (var path = new SKPath())
            {
                // Upper bound
                foreach (var point in points)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = RateToScreenY(point.UpperBound);
                    if (path.PointCount == 0)
                        path.MoveTo(x, y);
                    else
                        path.LineTo(x, y);
                }

                // Lower bound (reverse)
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    float x = TimeToScreenX(points[i].Time);
                    float y = RateToScreenY(points[i].LowerBound);
                    path.LineTo(x, y);
                }
                path.Close();

                using (var paint = new SKPaint
                {
                    Color = configuration.ConfidenceIntervalColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }

                // Draw interval borders
                using (var borderPath = new SKPath())
                {
                    foreach (var point in points)
                    {
                        float x = TimeToScreenX(point.Time);
                        float yUpper = RateToScreenY(point.UpperBound);
                        float yLower = RateToScreenY(point.LowerBound);
                        
                        if (borderPath.PointCount == 0)
                            borderPath.MoveTo(x, yUpper);
                        else
                            borderPath.LineTo(x, yUpper);
                    }
                    
                    for (int i = points.Count - 1; i >= 0; i--)
                    {
                        float x = TimeToScreenX(points[i].Time);
                        float y = RateToScreenY(points[i].LowerBound);
                        borderPath.LineTo(x, y);
                    }
                    borderPath.Close();

                    using (var borderPaint = new SKPaint
                    {
                        Color = configuration.ConfidenceIntervalBorderColor,
                        StrokeWidth = 1f,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(borderPath, borderPaint);
                    }
                }
            }

            // Draw main curve
            DrawCurve(canvas, points.Cast<PlotPoint>().ToList(), color, false);
        }

        /// <summary>
        /// Draws prediction intervals as filled regions.
        /// </summary>
        private void DrawPredictionIntervals(SKCanvas canvas)
        {
            if (plotWithIntervals == null)
                return;

            // Draw intervals for predicted points
            if (plotWithIntervals.PredictedPointsWithIntervals.Count > 0)
            {
                DrawIntervalBand(canvas, plotWithIntervals.PredictedPointsWithIntervals);
            }

            // Draw intervals for forecast points
            if (plotWithIntervals.ForecastPointsWithIntervals.Count > 0)
            {
                DrawIntervalBand(canvas, plotWithIntervals.ForecastPointsWithIntervals);
            }
        }

        /// <summary>
        /// Draws an interval band.
        /// </summary>
        private void DrawIntervalBand(SKCanvas canvas, List<IntervalPlotPoint> points)
        {
            if (points == null || points.Count == 0)
                return;

            using (var path = new SKPath())
            {
                // Upper bound
                foreach (var point in points)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = RateToScreenY(point.UpperBound);
                    if (path.PointCount == 0)
                        path.MoveTo(x, y);
                    else
                        path.LineTo(x, y);
                }

                // Lower bound (reverse)
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    float x = TimeToScreenX(points[i].Time);
                    float y = RateToScreenY(points[i].LowerBound);
                    path.LineTo(x, y);
                }
                path.Close();

                using (var paint = new SKPaint
                {
                    Color = configuration.ConfidenceIntervalColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Draws data points.
        /// </summary>
        private void DrawPoints(SKCanvas canvas, List<PlotPoint> points, SKColor color)
        {
            if (points == null || points.Count == 0)
                return;

            using (var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                foreach (var point in points)
                {
                    float x = TimeToScreenX(point.Time);
                    float y = RateToScreenY(point.ProductionRate);
                    canvas.DrawCircle(x, y, configuration.PointSize, paint);
                }
            }
        }

        /// <summary>
        /// Draws axes with labels and ticks.
        /// </summary>
        private void DrawAxes(SKCanvas canvas, float width, float height)
        {
            using (var axisPaint = new SKPaint
            {
                Color = configuration.TextColor,
                StrokeWidth = 2f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                // X-axis
                canvas.DrawLine(plotAreaX, plotAreaY + plotAreaHeight, plotAreaX + plotAreaWidth, plotAreaY + plotAreaHeight, axisPaint);
                
                // Y-axis
                canvas.DrawLine(plotAreaX, plotAreaY, plotAreaX, plotAreaY + plotAreaHeight, axisPaint);
            }

            if (configuration.ShowTicks)
            {
                DrawTicks(canvas);
            }

            if (configuration.ShowAxisLabels)
            {
                DrawAxisLabels(canvas, width, height);
            }
        }

        /// <summary>
        /// Draws tick marks and labels.
        /// </summary>
        private void DrawTicks(SKCanvas canvas)
        {
            using (var tickPaint = new SKPaint
            {
                Color = configuration.TextColor,
                StrokeWidth = 1f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            using (var labelPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.TickLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                // X-axis ticks
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double time = minTime + (maxTime - minTime) * i / xTicks;
                    float x = TimeToScreenX(time);
                    
                    // Tick mark
                    canvas.DrawLine(x, plotAreaY + plotAreaHeight, x, plotAreaY + plotAreaHeight + 5, tickPaint);
                    
                    // Label
                    string label = FormatTime(time);
                    canvas.DrawText(label, x, plotAreaY + plotAreaHeight + 20, labelPaint);
                }

                // Y-axis ticks
                int yTicks = configuration.YAxisTickCount;
                labelPaint.TextAlign = SKTextAlign.Right;
                for (int i = 0; i <= yTicks; i++)
                {
                    double rate = minRate + (maxRate - minRate) * i / yTicks;
                    float y = RateToScreenY(rate);
                    
                    // Tick mark
                    canvas.DrawLine(plotAreaX, y, plotAreaX - 5, y, tickPaint);
                    
                    // Label
                    string label = FormatRate(rate);
                    canvas.DrawText(label, plotAreaX - 10, y + configuration.TickLabelFontSize / 3, labelPaint);
                }
            }
        }

        /// <summary>
        /// Draws axis labels.
        /// </summary>
        private void DrawAxisLabels(SKCanvas canvas, float width, float height)
        {
            using (var labelPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.AxisLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                // X-axis label
                string xLabel = plot?.XAxisLabel ?? plotWithIntervals?.XAxisLabel ?? "Time";
                canvas.DrawText(xLabel, plotAreaX + plotAreaWidth / 2, height - 10, labelPaint);

                // Y-axis label (rotated)
                canvas.Save();
                canvas.Translate(20, plotAreaY + plotAreaHeight / 2);
                canvas.RotateDegrees(-90);
                labelPaint.TextAlign = SKTextAlign.Center;
                string yLabel = plot?.YAxisLabel ?? plotWithIntervals?.YAxisLabel ?? "Production Rate";
                canvas.DrawText(yLabel, 0, 0, labelPaint);
                canvas.Restore();
            }
        }

        /// <summary>
        /// Draws the plot title.
        /// </summary>
        private void DrawTitle(SKCanvas canvas, float width)
        {
            string title = plot?.Title ?? plotWithIntervals?.Title;
            if (string.IsNullOrEmpty(title))
                return;

            using (var paint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.TitleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            })
            {
                canvas.DrawText(title, width / 2, 25, paint);
            }
        }

        /// <summary>
        /// Draws the legend.
        /// </summary>
        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            float legendX, legendY;
            
            switch (configuration.LegendPosition)
            {
                case LegendPosition.TopLeft:
                    legendX = plotAreaX + 10;
                    legendY = plotAreaY + 10;
                    break;
                case LegendPosition.TopRight:
                    legendX = plotAreaX + plotAreaWidth - 120;
                    legendY = plotAreaY + 10;
                    break;
                case LegendPosition.BottomLeft:
                    legendX = plotAreaX + 10;
                    legendY = plotAreaY + plotAreaHeight - 80;
                    break;
                case LegendPosition.BottomRight:
                    legendX = plotAreaX + plotAreaWidth - 120;
                    legendY = plotAreaY + plotAreaHeight - 80;
                    break;
                default:
                    legendX = plotAreaX + plotAreaWidth - 120;
                    legendY = plotAreaY + 10;
                    break;
            }

            using (var textPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.FontSize,
                IsAntialias = true
            })
            {
                float yOffset = 0;
                
                if (configuration.ShowObservedPoints)
                {
                    DrawLegendItem(canvas, legendX, legendY + yOffset, "Observed", configuration.ObservedColor, textPaint);
                    yOffset += 20;
                }
                
                if (configuration.ShowPredictedCurve)
                {
                    DrawLegendItem(canvas, legendX, legendY + yOffset, "Predicted", configuration.PredictedColor, textPaint);
                    yOffset += 20;
                }
                
                if (configuration.ShowForecastCurve)
                {
                    DrawLegendItem(canvas, legendX, legendY + yOffset, "Forecast", configuration.ForecastColor, textPaint);
                    yOffset += 20;
                }
            }
        }

        /// <summary>
        /// Draws a single legend item.
        /// </summary>
        private void DrawLegendItem(SKCanvas canvas, float x, float y, string label, SKColor color, SKPaint textPaint)
        {
            // Color box
            using (var boxPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(x, y - 8, 15, 15, boxPaint);
            }

            // Label
            textPaint.TextAlign = SKTextAlign.Left;
            canvas.DrawText(label, x + 20, y + 5, textPaint);
        }

        /// <summary>
        /// Draws crosshair.
        /// </summary>
        private void DrawCrosshair(SKCanvas canvas, float width, float height)
        {
            // This will be updated by InteractionHandler
            // Placeholder for now
        }

        /// <summary>
        /// Converts time value to screen X coordinate.
        /// </summary>
        private float TimeToScreenX(double time)
        {
            double normalized = (time - minTime) / (maxTime - minTime);
            return plotAreaX + (float)(normalized * plotAreaWidth);
        }

        /// <summary>
        /// Converts production rate to screen Y coordinate.
        /// </summary>
        private float RateToScreenY(double rate)
        {
            double normalized = (rate - minRate) / (maxRate - minRate);
            if (configuration.UseLogScale)
            {
                double logMin = Math.Log10(Math.Max(minRate, 0.001));
                double logMax = Math.Log10(Math.Max(maxRate, 0.001));
                double logRate = Math.Log10(Math.Max(rate, 0.001));
                normalized = (logRate - logMin) / (logMax - logMin);
            }
            return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight);
        }

        /// <summary>
        /// Formats time value for display.
        /// </summary>
        private string FormatTime(double time)
        {
            return time.ToString("F0");
        }

        /// <summary>
        /// Formats production rate for display.
        /// </summary>
        private string FormatRate(double rate)
        {
            return rate.ToString("F1");
        }

        /// <summary>
        /// Zooms to fit all data.
        /// </summary>
        public void ZoomToFit(float width, float height)
        {
            zoom = 1.0;
            panOffset = SKPoint.Empty;
        }

        /// <summary>
        /// Gets the plot bounds.
        /// </summary>
        public (double minTime, double maxTime, double minRate, double maxRate) GetBounds()
        {
            return (minTime, maxTime, minRate, maxRate);
        }

        /// <summary>
        /// Gets the renderer configuration.
        /// </summary>
        public DCARendererConfiguration GetConfiguration()
        {
            return configuration;
        }
    }
}

