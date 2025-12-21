using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Accounting.Operational.Production;
using Beep.OilandGas.Accounting.Operational.Revenue;
using Beep.OilandGas.Accounting.Operational.Allocation;

namespace Beep.OilandGas.Accounting.Rendering
{
    /// <summary>
    /// Renders production charts using SkiaSharp.
    /// </summary>
    public class ProductionChartRenderer
    {
        private readonly ProductionChartRendererConfiguration configuration;
        private List<(DateTime date, decimal value)> productionData;
        private List<(DateTime date, decimal value)> revenueData;
        private List<(DateTime date, decimal value)> costData;
        private List<AllocationResult> allocationData;

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

        public ProductionChartRenderer(ProductionChartRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new ProductionChartRendererConfiguration();
        }

        public void SetProductionData(List<(DateTime date, decimal value)> data)
        {
            this.productionData = data;
        }

        public void SetRevenueData(List<(DateTime date, decimal value)> data)
        {
            this.revenueData = data;
        }

        public void SetCostData(List<(DateTime date, decimal value)> data)
        {
            this.costData = data;
        }

        public void SetAllocationData(List<AllocationResult> data)
        {
            this.allocationData = data;
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

            switch (configuration.ChartType)
            {
                case ProductionChartType.ProductionTrend:
                    DrawProductionTrend(canvas);
                    break;
                case ProductionChartType.RevenueTrend:
                    DrawRevenueTrend(canvas);
                    break;
                case ProductionChartType.CostTrend:
                    DrawCostTrend(canvas);
                    break;
                case ProductionChartType.Inventory:
                    DrawInventory(canvas);
                    break;
                case ProductionChartType.Allocation:
                    DrawAllocation(canvas);
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

        private void DrawProductionTrend(SKCanvas canvas)
        {
            if (productionData == null || productionData.Count == 0)
                return;

            var sorted = productionData.OrderBy(d => d.date).ToList();
            decimal maxValue = Math.Max(sorted.Max(d => d.value), 1);
            DateTime minDate = sorted.Min(d => d.date);
            DateTime maxDate = sorted.Max(d => d.date);
            double dateRange = (maxDate - minDate).TotalDays;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sorted)
                {
                    double daysFromStart = (point.date - minDate).TotalDays;
                    float x = plotAreaX + (float)(daysFromStart / dateRange * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - (float)((double)point.value / (double)maxValue * plotAreaHeight);

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
                    Color = configuration.ProductionColor,
                    StrokeWidth = 2.5f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawRevenueTrend(SKCanvas canvas)
        {
            if (revenueData == null || revenueData.Count == 0)
                return;

            var sorted = revenueData.OrderBy(d => d.date).ToList();
            decimal maxValue = Math.Max(sorted.Max(d => d.value), 1);
            DateTime minDate = sorted.Min(d => d.date);
            DateTime maxDate = sorted.Max(d => d.date);
            double dateRange = (maxDate - minDate).TotalDays;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sorted)
                {
                    double daysFromStart = (point.date - minDate).TotalDays;
                    float x = plotAreaX + (float)(daysFromStart / dateRange * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - (float)((double)point.value / (double)maxValue * plotAreaHeight);

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
                    Color = configuration.RevenueColor,
                    StrokeWidth = 2.5f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawCostTrend(SKCanvas canvas)
        {
            if (costData == null || costData.Count == 0)
                return;

            var sorted = costData.OrderBy(d => d.date).ToList();
            decimal maxValue = Math.Max(sorted.Max(d => d.value), 1);
            DateTime minDate = sorted.Min(d => d.date);
            DateTime maxDate = sorted.Max(d => d.date);
            double dateRange = (maxDate - minDate).TotalDays;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in sorted)
                {
                    double daysFromStart = (point.date - minDate).TotalDays;
                    float x = plotAreaX + (float)(daysFromStart / dateRange * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - (float)((double)point.value / (double)maxValue * plotAreaHeight);

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
                    Color = configuration.CostColor,
                    StrokeWidth = 2.5f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawInventory(SKCanvas canvas)
        {
            // Implementation for inventory chart
            // Would show inventory levels over time
        }

        private void DrawAllocation(SKCanvas canvas)
        {
            if (allocationData == null || allocationData.Count == 0)
                return;

            // Draw pie chart for allocation
            float centerX = plotAreaX + plotAreaWidth / 2f;
            float centerY = plotAreaY + plotAreaHeight / 2f;
            float radius = Math.Min(plotAreaWidth, plotAreaHeight) / 3f;

            var latestAllocation = allocationData.LastOrDefault();
            if (latestAllocation == null)
                return;

            decimal total = latestAllocation.Details.Sum(d => d.AllocatedVolume);
            if (total == 0)
                return;

            float startAngle = 0;
            var colors = new[] 
            { 
                configuration.ProductionColor, 
                configuration.RevenueColor, 
                configuration.CostColor, 
                configuration.InventoryColor,
                configuration.AllocationColor
            };
            int colorIndex = 0;

            foreach (var detail in latestAllocation.Details)
            {
                float sweepAngle = (float)((double)detail.AllocatedVolume / (double)total * 360);
                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, sweepAngle, colors[colorIndex % colors.Length]);
                startAngle += sweepAngle;
                colorIndex++;
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

        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var items = new List<(string label, SKColor color)>();

            if (productionData != null && productionData.Count > 0)
                items.Add(("Production", configuration.ProductionColor));

            if (revenueData != null && revenueData.Count > 0)
                items.Add(("Revenue", configuration.RevenueColor));

            if (costData != null && costData.Count > 0)
                items.Add(("Costs", configuration.CostColor));

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
}

