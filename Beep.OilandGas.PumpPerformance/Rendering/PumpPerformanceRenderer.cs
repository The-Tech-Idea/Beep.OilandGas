using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.PumpPerformance.Calculations;

namespace Beep.OilandGas.PumpPerformance.Rendering
{
    /// <summary>
    /// Renders pump performance curves using SkiaSharp.
    /// Supports H-Q, P-Q, Efficiency curves, system curves, operating points, and multi-pump configurations.
    /// </summary>
    public class PumpPerformanceRenderer
    {
        private readonly PumpPerformanceRendererConfiguration configuration;
        
        // Viewport state
        private double zoom = 1.0;
        private SKPoint panOffset = SKPoint.Empty;
        
        // Data bounds
        private double minFlowRate;
        private double maxFlowRate;
        private double minHead;
        private double maxHead;
        private double minPower;
        private double maxPower;
        private double minEfficiency;
        private double maxEfficiency;
        
        // Plot area
        private float plotAreaX;
        private float plotAreaY;
        private float plotAreaWidth;
        private float plotAreaHeight;
        
        // Data to render
        private List<HeadQuantityPoint> hqCurve;
        private List<SystemCurvePoint> systemCurve;
        private (double flowRate, double head)? operatingPoint;
        private HeadQuantityPoint bepPoint;
        private List<List<HeadQuantityPoint>> multiPumpCurves;
        private List<string> multiPumpLabels;
        private List<HeadQuantityPoint> affinityLawCurves;
        private string affinityLawLabel;
        private bool showPQCurve;
        private bool showEfficiencyCurve;
        private bool showNPSHCurves;
        private List<(double flowRate, double npsh)> npshRequired;
        private List<(double flowRate, double npsh)> npshAvailable;

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
        /// Initializes a new instance of the <see cref="PumpPerformanceRenderer"/> class.
        /// </summary>
        /// <param name="configuration">Rendering configuration.</param>
        public PumpPerformanceRenderer(PumpPerformanceRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new PumpPerformanceRendererConfiguration();
            this.zoom = this.configuration.DefaultZoom;
        }

        /// <summary>
        /// Sets the H-Q curve data to render.
        /// </summary>
        public void SetHQCurve(List<HeadQuantityPoint> curve)
        {
            this.hqCurve = curve;
            UpdateBounds();
        }

        /// <summary>
        /// Sets the system curve data to render.
        /// </summary>
        public void SetSystemCurve(List<SystemCurvePoint> curve)
        {
            this.systemCurve = curve;
            UpdateBounds();
        }

        /// <summary>
        /// Sets the operating point.
        /// </summary>
        public void SetOperatingPoint(double flowRate, double head)
        {
            this.operatingPoint = (flowRate, head);
        }

        /// <summary>
        /// Sets the Best Efficiency Point (BEP).
        /// </summary>
        public void SetBEP(HeadQuantityPoint bep)
        {
            this.bepPoint = bep;
        }

        /// <summary>
        /// Sets multiple pump curves for series/parallel configurations.
        /// </summary>
        public void SetMultiPumpCurves(List<List<HeadQuantityPoint>> curves, List<string> labels = null)
        {
            this.multiPumpCurves = curves;
            this.multiPumpLabels = labels ?? curves.Select((_, i) => $"Pump {i + 1}").ToList();
            UpdateBounds();
        }

        /// <summary>
        /// Sets affinity law curve (speed variation).
        /// </summary>
        public void SetAffinityLawCurve(List<HeadQuantityPoint> curve, string label = null)
        {
            this.affinityLawCurves = curve;
            this.affinityLawLabel = label ?? "Affinity Law";
        }

        /// <summary>
        /// Sets whether to show P-Q curve.
        /// </summary>
        public void ShowPQCurve(bool show)
        {
            this.showPQCurve = show;
            UpdateBounds();
        }

        /// <summary>
        /// Sets whether to show efficiency curve.
        /// </summary>
        public void ShowEfficiencyCurve(bool show)
        {
            this.showEfficiencyCurve = show;
            UpdateBounds();
        }

        /// <summary>
        /// Sets NPSH curves.
        /// </summary>
        public void SetNPSHCurves(List<(double flowRate, double npsh)> required, List<(double flowRate, double npsh)> available)
        {
            this.npshRequired = required;
            this.npshAvailable = available;
            this.showNPSHCurves = true;
        }

        /// <summary>
        /// Updates the data bounds based on current data.
        /// </summary>
        private void UpdateBounds()
        {
            var allFlowRates = new List<double>();
            var allHeads = new List<double>();
            var allPowers = new List<double>();
            var allEfficiencies = new List<double>();

            if (hqCurve != null && hqCurve.Count > 0)
            {
                allFlowRates.AddRange(hqCurve.Select(p => p.FlowRate));
                allHeads.AddRange(hqCurve.Select(p => p.Head));
                allPowers.AddRange(hqCurve.Select(p => p.Power));
                allEfficiencies.AddRange(hqCurve.Select(p => p.Efficiency));
            }

            if (systemCurve != null && systemCurve.Count > 0)
            {
                allFlowRates.AddRange(systemCurve.Select(p => p.FlowRate));
                allHeads.AddRange(systemCurve.Select(p => p.RequiredHead));
            }

            if (multiPumpCurves != null)
            {
                foreach (var curve in multiPumpCurves)
                {
                    if (curve != null && curve.Count > 0)
                    {
                        allFlowRates.AddRange(curve.Select(p => p.FlowRate));
                        allHeads.AddRange(curve.Select(p => p.Head));
                        allPowers.AddRange(curve.Select(p => p.Power));
                        allEfficiencies.AddRange(curve.Select(p => p.Efficiency));
                    }
                }
            }

            if (affinityLawCurves != null && affinityLawCurves.Count > 0)
            {
                allFlowRates.AddRange(affinityLawCurves.Select(p => p.FlowRate));
                allHeads.AddRange(affinityLawCurves.Select(p => p.Head));
            }

            if (operatingPoint.HasValue)
            {
                allFlowRates.Add(operatingPoint.Value.flowRate);
                allHeads.Add(operatingPoint.Value.head);
            }

            if (bepPoint != null)
            {
                allFlowRates.Add(bepPoint.FlowRate);
                allHeads.Add(bepPoint.Head);
            }

            if (allFlowRates.Count > 0)
            {
                minFlowRate = allFlowRates.Min();
                maxFlowRate = allFlowRates.Max();
                double flowRange = maxFlowRate - minFlowRate;
                minFlowRate -= flowRange * 0.05;
                maxFlowRate += flowRange * 0.05;
            }
            else
            {
                minFlowRate = 0;
                maxFlowRate = 1000;
            }

            if (allHeads.Count > 0)
            {
                minHead = allHeads.Min();
                maxHead = allHeads.Max();
                double headRange = maxHead - minHead;
                minHead -= headRange * 0.05;
                maxHead += headRange * 0.05;
            }
            else
            {
                minHead = 0;
                maxHead = 500;
            }

            if (allPowers.Count > 0)
            {
                minPower = allPowers.Min();
                maxPower = allPowers.Max();
            }
            else
            {
                minPower = 0;
                maxPower = 1000;
            }

            if (allEfficiencies.Count > 0)
            {
                minEfficiency = allEfficiencies.Min();
                maxEfficiency = allEfficiencies.Max();
            }
            else
            {
                minEfficiency = 0;
                maxEfficiency = 1.0;
            }
        }

        /// <summary>
        /// Renders the pump performance curves to the canvas.
        /// </summary>
        public void Render(SKCanvas canvas, float width, float height)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            // Clear canvas
            canvas.Clear(configuration.BackgroundColor);

            // Calculate plot area
            plotAreaX = configuration.LeftMargin;
            plotAreaY = configuration.TopMargin;
            plotAreaWidth = width - configuration.LeftMargin - configuration.RightMargin;
            plotAreaHeight = height - configuration.TopMargin - configuration.BottomMargin;

            // Draw title
            DrawTitle(canvas, width);

            // Draw grid
            if (configuration.ShowGrid)
            {
                DrawGrid(canvas);
            }

            // Draw axes
            DrawAxes(canvas);

            // Draw curves
            if (multiPumpCurves != null && multiPumpCurves.Count > 0)
            {
                DrawMultiPumpCurves(canvas);
            }
            else
            {
                if (hqCurve != null && hqCurve.Count > 0)
                {
                    DrawHQCurve(canvas);
                }

                if (affinityLawCurves != null && affinityLawCurves.Count > 0)
                {
                    DrawAffinityLawCurve(canvas);
                }
            }

            if (systemCurve != null && systemCurve.Count > 0)
            {
                DrawSystemCurve(canvas);
            }

            if (showPQCurve && hqCurve != null && hqCurve.Count > 0)
            {
                DrawPQCurve(canvas);
            }

            if (showEfficiencyCurve && hqCurve != null && hqCurve.Count > 0)
            {
                DrawEfficiencyCurve(canvas);
            }

            if (showNPSHCurves)
            {
                DrawNPSHCurves(canvas);
            }

            // Draw operating point
            if (operatingPoint.HasValue)
            {
                DrawOperatingPoint(canvas);
            }

            // Draw BEP
            if (bepPoint != null)
            {
                DrawBEP(canvas);
            }

            // Draw legend
            if (configuration.ShowLegend)
            {
                DrawLegend(canvas, width, height);
            }
        }

        /// <summary>
        /// Converts flow rate to screen X coordinate.
        /// </summary>
        private float FlowRateToScreenX(double flowRate)
        {
            double normalized = (flowRate - minFlowRate) / (maxFlowRate - minFlowRate);
            return plotAreaX + (float)(normalized * plotAreaWidth) + panOffset.X;
        }

        /// <summary>
        /// Converts head to screen Y coordinate.
        /// </summary>
        private float HeadToScreenY(double head)
        {
            double normalized = (head - minHead) / (maxHead - minHead);
            return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
        }

        /// <summary>
        /// Converts power to screen Y coordinate (for P-Q curve on secondary axis).
        /// </summary>
        private float PowerToScreenY(double power)
        {
            double normalized = (power - minPower) / (maxPower - minPower);
            return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
        }

        /// <summary>
        /// Converts efficiency to screen Y coordinate (for efficiency curve on secondary axis).
        /// </summary>
        private float EfficiencyToScreenY(double efficiency)
        {
            double normalized = (efficiency - minEfficiency) / (maxEfficiency - minEfficiency);
            return plotAreaY + plotAreaHeight - (float)(normalized * plotAreaHeight) + panOffset.Y;
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
                // Vertical grid lines (flow rate axis)
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double flowRate = minFlowRate + (maxFlowRate - minFlowRate) * i / xTicks;
                    float x = FlowRateToScreenX(flowRate);
                    canvas.DrawLine(x, plotAreaY, x, plotAreaY + plotAreaHeight, paint);
                }

                // Horizontal grid lines (head axis)
                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    double head = minHead + (maxHead - minHead) * i / yTicks;
                    float y = HeadToScreenY(head);
                    canvas.DrawLine(plotAreaX, y, plotAreaX + plotAreaWidth, y, paint);
                }
            }
        }

        /// <summary>
        /// Draws the axes with labels and ticks.
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
                // X-axis (flow rate)
                canvas.DrawLine(plotAreaX, plotAreaY + plotAreaHeight, plotAreaX + plotAreaWidth, plotAreaY + plotAreaHeight, axisPaint);

                // Y-axis (head)
                canvas.DrawLine(plotAreaX, plotAreaY, plotAreaX, plotAreaY + plotAreaHeight, axisPaint);
            }

            // Draw axis labels
            using (var labelPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.AxisLabelFontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            })
            {
                // X-axis label
                labelPaint.TextAlign = SKTextAlign.Center;
                float xLabelY = plotAreaY + plotAreaHeight + configuration.BottomMargin / 2f;
                canvas.DrawText(configuration.XAxisLabel, plotAreaX + plotAreaWidth / 2f, xLabelY, labelPaint);

                // Y-axis label (rotated)
                labelPaint.TextAlign = SKTextAlign.Center;
                float yLabelX = configuration.LeftMargin / 2f;
                canvas.Save();
                canvas.Translate(yLabelX, plotAreaY + plotAreaHeight / 2f);
                canvas.RotateDegrees(-90);
                canvas.DrawText(configuration.YAxisLabel, 0, 0, labelPaint);
                canvas.Restore();
            }

            // Draw tick labels
            using (var tickPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.TickLabelFontSize,
                IsAntialias = true
            })
            {
                // X-axis ticks
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    double flowRate = minFlowRate + (maxFlowRate - minFlowRate) * i / xTicks;
                    float x = FlowRateToScreenX(flowRate);
                    string label = FormatFlowRate(flowRate);
                    tickPaint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText(label, x, plotAreaY + plotAreaHeight + 20f, tickPaint);
                }

                // Y-axis ticks
                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    double head = minHead + (maxHead - minHead) * i / yTicks;
                    float y = HeadToScreenY(head);
                    string label = FormatHead(head);
                    tickPaint.TextAlign = SKTextAlign.Right;
                    canvas.DrawText(label, plotAreaX - 10f, y + 5f, tickPaint);
                }
            }
        }

        /// <summary>
        /// Draws the H-Q curve.
        /// </summary>
        private void DrawHQCurve(SKCanvas canvas)
        {
            if (hqCurve == null || hqCurve.Count == 0)
                return;

            var sortedCurve = hqCurve.OrderBy(p => p.FlowRate).ToList();

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sortedCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = HeadToScreenY(point.Head);

                    if (first)
                    {
                        path.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        if (configuration.UseCurveSmoothing && path.PointCount > 0)
                        {
                            // Use quadratic bezier for smoothing
                            var lastPoint = path.LastPoint;
                            float controlX = (lastPoint.X + x) / 2f;
                            path.QuadTo(controlX, lastPoint.Y, x, y);
                        }
                        else
                        {
                            path.LineTo(x, y);
                        }
                    }
                }

                using (var paint = new SKPaint
                {
                    Color = configuration.HQCurveColor,
                    StrokeWidth = configuration.HQCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            // Draw points if enabled
            if (configuration.ShowHQPoints)
            {
                DrawPoints(canvas, sortedCurve.Select(p => (FlowRateToScreenX(p.FlowRate), HeadToScreenY(p.Head))).ToList(), 
                    configuration.HQCurveColor, configuration.PointSize);
            }
        }

        /// <summary>
        /// Draws the P-Q (Power-Quantity) curve.
        /// </summary>
        private void DrawPQCurve(SKCanvas canvas)
        {
            if (hqCurve == null || hqCurve.Count == 0)
                return;

            var sortedCurve = hqCurve.OrderBy(p => p.FlowRate).ToList();

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sortedCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = PowerToScreenY(point.Power);

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
                    Color = configuration.PQCurveColor,
                    StrokeWidth = configuration.PQCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            if (configuration.ShowPQPoints)
            {
                DrawPoints(canvas, sortedCurve.Select(p => (FlowRateToScreenX(p.FlowRate), PowerToScreenY(p.Power))).ToList(),
                    configuration.PQCurveColor, configuration.PointSize);
            }
        }

        /// <summary>
        /// Draws the efficiency curve.
        /// </summary>
        private void DrawEfficiencyCurve(SKCanvas canvas)
        {
            if (hqCurve == null || hqCurve.Count == 0)
                return;

            var sortedCurve = hqCurve.OrderBy(p => p.FlowRate).ToList();

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sortedCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = EfficiencyToScreenY(point.Efficiency);

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
                    Color = configuration.EfficiencyCurveColor,
                    StrokeWidth = configuration.EfficiencyCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            if (configuration.ShowEfficiencyPoints)
            {
                DrawPoints(canvas, sortedCurve.Select(p => (FlowRateToScreenX(p.FlowRate), EfficiencyToScreenY(p.Efficiency))).ToList(),
                    configuration.EfficiencyCurveColor, configuration.PointSize);
            }
        }

        /// <summary>
        /// Draws the system curve.
        /// </summary>
        private void DrawSystemCurve(SKCanvas canvas)
        {
            if (systemCurve == null || systemCurve.Count == 0)
                return;

            var sortedCurve = systemCurve.OrderBy(p => p.FlowRate).ToList();

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sortedCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = HeadToScreenY(point.RequiredHead);

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
                    Color = configuration.SystemCurveColor,
                    StrokeWidth = configuration.SystemCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    if (configuration.SystemCurveDashed)
                    {
                        paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10f, 5f }, 0);
                    }

                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Draws multiple pump curves.
        /// </summary>
        private void DrawMultiPumpCurves(SKCanvas canvas)
        {
            if (multiPumpCurves == null || multiPumpCurves.Count == 0)
                return;

            for (int i = 0; i < multiPumpCurves.Count; i++)
            {
                var curve = multiPumpCurves[i];
                if (curve == null || curve.Count == 0)
                    continue;

                var color = i < configuration.MultiPumpColors.Length 
                    ? configuration.MultiPumpColors[i] 
                    : configuration.HQCurveColor;

                var sortedCurve = curve.OrderBy(p => p.FlowRate).ToList();

                using (var path = new SKPath())
                {
                    bool first = true;
                    foreach (var point in sortedCurve)
                    {
                        float x = FlowRateToScreenX(point.FlowRate);
                        float y = HeadToScreenY(point.Head);

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
                        StrokeWidth = configuration.HQCurveLineWidth,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws affinity law curve.
        /// </summary>
        private void DrawAffinityLawCurve(SKCanvas canvas)
        {
            if (affinityLawCurves == null || affinityLawCurves.Count == 0)
                return;

            var sortedCurve = affinityLawCurves.OrderBy(p => p.FlowRate).ToList();

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sortedCurve)
                {
                    float x = FlowRateToScreenX(point.FlowRate);
                    float y = HeadToScreenY(point.Head);

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
                    Color = configuration.AffinityLawCurveColor,
                    StrokeWidth = configuration.AffinityLawCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    if (configuration.AffinityLawCurveDashed)
                    {
                        paint.PathEffect = SKPathEffect.CreateDash(new float[] { 8f, 4f }, 0);
                    }

                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Draws NPSH curves.
        /// </summary>
        private void DrawNPSHCurves(SKCanvas canvas)
        {
            // NPSH Required
            if (npshRequired != null && npshRequired.Count > 0)
            {
                var sorted = npshRequired.OrderBy(p => p.flowRate).ToList();
                using (var path = new SKPath())
                {
                    bool first = true;
                    foreach (var point in sorted)
                    {
                        float x = FlowRateToScreenX(point.flowRate);
                        float y = HeadToScreenY(point.npsh);

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
                        Color = configuration.NPSHRequiredColor,
                        StrokeWidth = 2f,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(path, paint);
                    }
                }
            }

            // NPSH Available
            if (npshAvailable != null && npshAvailable.Count > 0)
            {
                var sorted = npshAvailable.OrderBy(p => p.flowRate).ToList();
                using (var path = new SKPath())
                {
                    bool first = true;
                    foreach (var point in sorted)
                    {
                        float x = FlowRateToScreenX(point.flowRate);
                        float y = HeadToScreenY(point.npsh);

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
                        Color = configuration.NPSHAvailableColor,
                        StrokeWidth = 2f,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the operating point.
        /// </summary>
        private void DrawOperatingPoint(SKCanvas canvas)
        {
            if (!operatingPoint.HasValue)
                return;

            float x = FlowRateToScreenX(operatingPoint.Value.flowRate);
            float y = HeadToScreenY(operatingPoint.Value.head);

            using (var paint = new SKPaint
            {
                Color = configuration.OperatingPointColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(x, y, configuration.OperatingPointSize, paint);
            }

            // Draw border
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

            // Draw label
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
                    string label = $"OP: {FormatFlowRate(operatingPoint.Value.flowRate)}, {FormatHead(operatingPoint.Value.head)}";
                    paint.TextAlign = SKTextAlign.Left;
                    canvas.DrawText(label, x + configuration.OperatingPointSize + 5f, y - 5f, paint);
                }
            }
        }

        /// <summary>
        /// Draws the Best Efficiency Point (BEP).
        /// </summary>
        private void DrawBEP(SKCanvas canvas)
        {
            if (bepPoint == null)
                return;

            float x = FlowRateToScreenX(bepPoint.FlowRate);
            float y = HeadToScreenY(bepPoint.Head);

            using (var paint = new SKPaint
            {
                Color = configuration.BEPPointColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(x, y, configuration.BEPPointSize, paint);
            }

            // Draw border
            using (var paint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(x, y, configuration.BEPPointSize, paint);
            }

            // Draw label
            if (configuration.ShowBEPLabel)
            {
                using (var paint = new SKPaint
                {
                    Color = configuration.BEPPointColor,
                    TextSize = configuration.FontSize,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                })
                {
                    string label = $"BEP: {FormatFlowRate(bepPoint.FlowRate)}, {FormatHead(bepPoint.Head)}, η={bepPoint.Efficiency:P1}";
                    paint.TextAlign = SKTextAlign.Left;
                    canvas.DrawText(label, x + configuration.BEPPointSize + 5f, y + 15f, paint);
                }
            }
        }

        /// <summary>
        /// Draws points.
        /// </summary>
        private void DrawPoints(SKCanvas canvas, List<(float x, float y)> points, SKColor color, float size)
        {
            using (var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                foreach (var point in points)
                {
                    canvas.DrawCircle(point.x, point.y, size, paint);
                }
            }
        }

        /// <summary>
        /// Draws the legend.
        /// </summary>
        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var legendItems = new List<(string label, SKColor color)>();

            if (hqCurve != null && hqCurve.Count > 0)
                legendItems.Add(("H-Q Curve", configuration.HQCurveColor));

            if (showPQCurve && hqCurve != null && hqCurve.Count > 0)
                legendItems.Add(("P-Q Curve", configuration.PQCurveColor));

            if (showEfficiencyCurve && hqCurve != null && hqCurve.Count > 0)
                legendItems.Add(("Efficiency", configuration.EfficiencyCurveColor));

            if (systemCurve != null && systemCurve.Count > 0)
                legendItems.Add(("System Curve", configuration.SystemCurveColor));

            if (affinityLawCurves != null && affinityLawCurves.Count > 0)
                legendItems.Add((affinityLawLabel ?? "Affinity Law", configuration.AffinityLawCurveColor));

            if (multiPumpCurves != null && multiPumpCurves.Count > 0 && configuration.ShowPumpLabels)
            {
                for (int i = 0; i < multiPumpCurves.Count && i < multiPumpLabels.Count; i++)
                {
                    var color = i < configuration.MultiPumpColors.Length 
                        ? configuration.MultiPumpColors[i] 
                        : configuration.HQCurveColor;
                    legendItems.Add((multiPumpLabels[i], color));
                }
            }

            if (showNPSHCurves)
            {
                if (npshRequired != null && npshRequired.Count > 0)
                    legendItems.Add(("NPSH Required", configuration.NPSHRequiredColor));
                if (npshAvailable != null && npshAvailable.Count > 0)
                    legendItems.Add(("NPSH Available", configuration.NPSHAvailableColor));
            }

            if (legendItems.Count == 0)
                return;

            float legendX, legendY;
            float itemHeight = 20f;
            float padding = 10f;
            float legendWidth = 150f;
            float legendHeight = legendItems.Count * itemHeight + padding * 2;

            switch (configuration.LegendPosition)
            {
                case LegendPosition.TopLeft:
                    legendX = plotAreaX + 10f;
                    legendY = plotAreaY + 10f;
                    break;
                case LegendPosition.TopRight:
                    legendX = plotAreaX + plotAreaWidth - legendWidth - 10f;
                    legendY = plotAreaY + 10f;
                    break;
                case LegendPosition.BottomLeft:
                    legendX = plotAreaX + 10f;
                    legendY = plotAreaY + plotAreaHeight - legendHeight - 10f;
                    break;
                case LegendPosition.BottomRight:
                default:
                    legendX = plotAreaX + plotAreaWidth - legendWidth - 10f;
                    legendY = plotAreaY + plotAreaHeight - legendHeight - 10f;
                    break;
            }

            // Draw legend background
            using (var paint = new SKPaint
            {
                Color = configuration.LegendBackgroundColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawRect(legendX, legendY, legendWidth, legendHeight, paint);
            }

            // Draw legend border
            using (var paint = new SKPaint
            {
                Color = configuration.LegendBorderColor,
                StrokeWidth = 1f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawRect(legendX, legendY, legendWidth, legendHeight, paint);
            }

            // Draw legend items
            using (var textPaint = new SKPaint
            {
                Color = configuration.TextColor,
                TextSize = configuration.FontSize - 2f,
                IsAntialias = true
            })
            {
                for (int i = 0; i < legendItems.Count; i++)
                {
                    float y = legendY + padding + i * itemHeight + itemHeight / 2f;

                    // Draw color box
                    using (var boxPaint = new SKPaint
                    {
                        Color = legendItems[i].color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(legendX + padding, y - 6f, 12f, 12f, boxPaint);
                    }

                    // Draw label
                    textPaint.TextAlign = SKTextAlign.Left;
                    canvas.DrawText(legendItems[i].label, legendX + padding + 18f, y + 4f, textPaint);
                }
            }
        }

        /// <summary>
        /// Formats flow rate for display.
        /// </summary>
        private string FormatFlowRate(double flowRate)
        {
            if (flowRate >= 1000)
                return $"{flowRate / 1000:F1}K";
            return $"{flowRate:F0}";
        }

        /// <summary>
        /// Formats head for display.
        /// </summary>
        private string FormatHead(double head)
        {
            return $"{head:F0}";
        }

        /// <summary>
        /// Exports the plot to a PNG image.
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

