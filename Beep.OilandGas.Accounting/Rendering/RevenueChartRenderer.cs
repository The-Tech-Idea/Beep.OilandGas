using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Accounting.Operational.Revenue;

namespace Beep.OilandGas.Accounting.Rendering
{
    /// <summary>
    /// Renders revenue and profitability charts using SkiaSharp.
    /// </summary>
    public class RevenueChartRenderer
    {
        private readonly ProductionChartRendererConfiguration configuration;
        private List<SalesTransaction> transactions;
        private List<(DateTime date, decimal revenue, decimal costs, decimal profit)> profitabilityData;

        private float plotAreaX;
        private float plotAreaY;
        private float plotAreaWidth;
        private float plotAreaHeight;

        public RevenueChartRenderer(ProductionChartRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new ProductionChartRendererConfiguration();
        }

        public void SetTransactions(List<SalesTransaction> transactions)
        {
            this.transactions = transactions;
            BuildProfitabilityData();
        }

        private void BuildProfitabilityData()
        {
            if (transactions == null)
                return;

            profitabilityData = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.TotalValue),
                    Costs = g.Sum(t => t.Costs.TotalCosts + t.Taxes.Sum(tax => tax.Amount)),
                    Profit = g.Sum(t => t.NetRevenue)
                })
                .Select(x => (x.Date, x.Revenue, x.Costs, x.Profit))
                .OrderBy(x => x.Date)
                .ToList();
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

            DrawTitle(canvas, width, "Revenue and Profitability Analysis");

            if (configuration.ShowGrid)
            {
                DrawGrid(canvas);
            }

            DrawAxes(canvas);
            DrawProfitabilityChart(canvas);
            DrawLegend(canvas, width, height);
        }

        private void DrawTitle(SKCanvas canvas, float width, string title)
        {
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
                canvas.DrawText(title, x, y, paint);
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
        }

        private void DrawProfitabilityChart(SKCanvas canvas)
        {
            if (profitabilityData == null || profitabilityData.Count == 0)
                return;

            decimal maxValue = Math.Max(
                profitabilityData.Max(d => d.revenue),
                Math.Max(profitabilityData.Max(d => d.costs), profitabilityData.Max(d => d.profit)));

            if (maxValue <= 0)
                maxValue = 1;

            DateTime minDate = profitabilityData.Min(d => d.date);
            DateTime maxDate = profitabilityData.Max(d => d.date);
            double dateRange = (maxDate - minDate).TotalDays;
            if (dateRange == 0) dateRange = 1;

            // Draw revenue line
            DrawLine(canvas, profitabilityData, d => d.revenue, maxValue, minDate, dateRange, configuration.RevenueColor);

            // Draw costs line
            DrawLine(canvas, profitabilityData, d => d.costs, maxValue, minDate, dateRange, configuration.CostColor);

            // Draw profit line
            DrawLine(canvas, profitabilityData, d => d.profit, maxValue, minDate, dateRange, new SKColor(76, 175, 80));
        }

        private void DrawLine(SKCanvas canvas, List<(DateTime date, decimal revenue, decimal costs, decimal profit)> data,
            Func<(DateTime date, decimal revenue, decimal costs, decimal profit), decimal> valueSelector,
            decimal maxValue, DateTime minDate, double dateRange, SKColor color)
        {
            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in data)
                {
                    double daysFromStart = (point.date - minDate).TotalDays;
                    float x = plotAreaX + (float)(daysFromStart / dateRange * plotAreaWidth);
                    decimal value = valueSelector(point);
                    float y = plotAreaY + plotAreaHeight - (float)((double)value / (double)maxValue * plotAreaHeight);

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

        private void DrawLegend(SKCanvas canvas, float width, float height)
        {
            var items = new List<(string label, SKColor color)>
            {
                ("Revenue", configuration.RevenueColor),
                ("Costs", configuration.CostColor),
                ("Profit", new SKColor(76, 175, 80))
            };

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
    }
}

