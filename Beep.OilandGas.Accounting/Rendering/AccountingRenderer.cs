using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.SuccessfulEfforts;

namespace Beep.OilandGas.Accounting.Rendering
{
    /// <summary>
    /// Renders accounting visualizations using SkiaSharp.
    /// </summary>
    public class AccountingRenderer
    {
        private readonly AccountingRendererConfiguration configuration;
        private SuccessfulEffortsAccounting accounting;
        private List<(DateTime period, decimal amount, CostType type)> costData;
        private List<(DateTime period, decimal amortization)> amortizationData;
        
        private double zoom = 1.0;
        private SKPoint panOffset = SKPoint.Empty;
        
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

        public AccountingRenderer(AccountingRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new AccountingRendererConfiguration();
            this.zoom = 1.0;
        }

        public void SetAccountingData(SuccessfulEffortsAccounting accounting)
        {
            this.accounting = accounting;
            BuildCostData();
        }

        public void SetAmortizationData(List<(DateTime period, decimal amortization)> data)
        {
            this.amortizationData = data;
        }

        private void BuildCostData()
        {
            if (accounting == null)
                return;

            costData = new List<(DateTime, decimal, CostType)>();

            // Add acquisition costs
            foreach (var property in accounting.GetUnprovedProperties())
            {
                costData.Add((property.AcquisitionDate, property.AcquisitionCost, CostType.Acquisition));
            }

            // Add exploration costs (would need to track dates from exploration costs)
            // This is simplified - full implementation would track dates
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

            switch (configuration.PlotType)
            {
                case AccountingPlotType.CostTrend:
                    DrawCostTrend(canvas);
                    break;
                case AccountingPlotType.AmortizationSchedule:
                    DrawAmortizationSchedule(canvas);
                    break;
                case AccountingPlotType.CostBreakdown:
                    DrawCostBreakdown(canvas);
                    break;
                case AccountingPlotType.ReserveVsCost:
                    DrawReserveVsCost(canvas);
                    break;
            }

            if (configuration.ShowLegend)
            {
                DrawLegend(canvas, width, height);
            }
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
                    float x = plotAreaX + (plotAreaWidth * i / xTicks);
                    canvas.DrawLine(x, plotAreaY, x, plotAreaY + plotAreaHeight, paint);
                }

                int yTicks = configuration.YAxisTickCount;
                for (int i = 0; i <= yTicks; i++)
                {
                    float y = plotAreaY + (plotAreaHeight * i / yTicks);
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
        }

        private void DrawCostTrend(SKCanvas canvas)
        {
            if (costData == null || costData.Count == 0)
                return;

            // Group by cost type and draw lines
            var acquisitionCosts = costData.Where(c => c.type == CostType.Acquisition).ToList();
            var explorationCosts = costData.Where(c => c.type == CostType.Exploration).ToList();
            var developmentCosts = costData.Where(c => c.type == CostType.Development).ToList();

            if (acquisitionCosts.Count > 0)
            {
                DrawCostLine(canvas, acquisitionCosts, configuration.AcquisitionCostColor, "Acquisition");
            }

            if (explorationCosts.Count > 0)
            {
                DrawCostLine(canvas, explorationCosts, configuration.ExplorationCostColor, "Exploration");
            }

            if (developmentCosts.Count > 0)
            {
                DrawCostLine(canvas, developmentCosts, configuration.DevelopmentCostColor, "Development");
            }
        }

        private void DrawCostLine(SKCanvas canvas, List<(DateTime period, decimal amount, CostType type)> data, SKColor color, string label)
        {
            if (data.Count == 0)
                return;

            var sorted = data.OrderBy(d => d.period).ToList();
            decimal maxAmount = Math.Max(sorted.Max(d => d.amount), 1);

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sorted)
                {
                    float x = plotAreaX + (float)((point.period.Ticks % 10000000000) / 10000000000.0 * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - (float)((double)point.amount / (double)maxAmount * plotAreaHeight);

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
                    StrokeWidth = 2.5f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawAmortizationSchedule(SKCanvas canvas)
        {
            if (amortizationData == null || amortizationData.Count == 0)
                return;

            var sorted = amortizationData.OrderBy(d => d.period).ToList();
            decimal maxAmortization = Math.Max(sorted.Max(d => d.amortization), 1);

            using (var path = new SKPath())
            {
                bool first = true;
                for (int i = 0; i < sorted.Count; i++)
                {
                    float x = plotAreaX + (i / (float)(sorted.Count - 1)) * plotAreaWidth;
                    float y = plotAreaY + plotAreaHeight - (float)((double)sorted[i].amortization / (double)maxAmortization * plotAreaHeight);

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
                    Color = configuration.AmortizationColor,
                    StrokeWidth = 2.5f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawCostBreakdown(SKCanvas canvas)
        {
            if (costData == null || costData.Count == 0)
                return;

            decimal total = costData.Sum(c => c.amount);
            if (total == 0)
                return;

            var acquisition = costData.Where(c => c.type == CostType.Acquisition).Sum(c => c.amount);
            var exploration = costData.Where(c => c.type == CostType.Exploration).Sum(c => c.amount);
            var development = costData.Where(c => c.type == CostType.Development).Sum(c => c.amount);

            float centerX = plotAreaX + plotAreaWidth / 2f;
            float centerY = plotAreaY + plotAreaHeight / 2f;
            float radius = Math.Min(plotAreaWidth, plotAreaHeight) / 3f;

            float startAngle = 0;

            // Draw acquisition slice
            if (acquisition > 0)
            {
                float sweepAngle = (float)((double)acquisition / (double)total * 360);
                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, sweepAngle, configuration.AcquisitionCostColor);
                startAngle += sweepAngle;
            }

            // Draw exploration slice
            if (exploration > 0)
            {
                float sweepAngle = (float)((double)exploration / (double)total * 360);
                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, sweepAngle, configuration.ExplorationCostColor);
                startAngle += sweepAngle;
            }

            // Draw development slice
            if (development > 0)
            {
                float sweepAngle = (float)((double)development / (double)total * 360);
                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, sweepAngle, configuration.DevelopmentCostColor);
            }
        }

        private void DrawPieSlice(SKCanvas canvas, float centerX, float centerY, float radius, float startAngle, float sweepAngle, SKColor color)
        {
            using (var path = new SKPath())
            {
                path.MoveTo(centerX, centerY);
                path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                    startAngle, sweepAngle, false);
                path.Close();

                using (var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }

                using (var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 1f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawReserveVsCost(SKCanvas canvas)
        {
            // Implementation for reserve vs cost chart
            // Would show reserves on one axis, costs on another
        }

        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var items = new List<(string label, SKColor color)>();

            if (costData != null && costData.Any(c => c.type == CostType.Acquisition))
                items.Add(("Acquisition", configuration.AcquisitionCostColor));

            if (costData != null && costData.Any(c => c.type == CostType.Exploration))
                items.Add(("Exploration", configuration.ExplorationCostColor));

            if (costData != null && costData.Any(c => c.type == CostType.Development))
                items.Add(("Development", configuration.DevelopmentCostColor));

            if (amortizationData != null && amortizationData.Count > 0)
                items.Add(("Amortization", configuration.AmortizationColor));

            if (items.Count == 0)
                return;

            float itemHeight = 20f;
            float padding = 10f;
            float legendWidth = 150f;
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

    /// <summary>
    /// Cost type enumeration.
    /// </summary>
    public enum CostType
    {
        Acquisition,
        Exploration,
        Development,
        Production
    }
}

