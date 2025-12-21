using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Accounting.Operational.Allocation;

namespace Beep.OilandGas.Accounting.Rendering
{
    /// <summary>
    /// Renders allocation charts using SkiaSharp.
    /// </summary>
    public class AllocationChartRenderer
    {
        private readonly ProductionChartRendererConfiguration configuration;
        private List<AllocationResult> allocationData;

        private float plotAreaX;
        private float plotAreaY;
        private float plotAreaWidth;
        private float plotAreaHeight;

        public AllocationChartRenderer(ProductionChartRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new ProductionChartRendererConfiguration();
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

            if (allocationData == null || allocationData.Count == 0)
                return;

            DrawTitle(canvas, width, "Allocation Breakdown");

            // Draw pie chart for latest allocation
            var latestAllocation = allocationData.LastOrDefault();
            if (latestAllocation != null)
            {
                DrawAllocationPieChart(canvas, latestAllocation);
            }

            // Draw bar chart comparing allocations
            DrawAllocationBarChart(canvas);
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

        private void DrawAllocationPieChart(SKCanvas canvas, AllocationResult allocation)
        {
            if (allocation.Details.Count == 0)
                return;

            float centerX = plotAreaX + plotAreaWidth / 2f;
            float centerY = plotAreaY + plotAreaHeight / 2f;
            float radius = Math.Min(plotAreaWidth, plotAreaHeight) / 3f;

            decimal total = allocation.Details.Sum(d => d.AllocatedVolume);
            if (total == 0)
                return;

            float startAngle = 0;
            var colors = GenerateColors(allocation.Details.Count);

            for (int i = 0; i < allocation.Details.Count; i++)
            {
                var detail = allocation.Details[i];
                float sweepAngle = (float)((double)detail.AllocatedVolume / (double)total * 360);

                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, sweepAngle, colors[i], detail.EntityName);

                startAngle += sweepAngle;
            }
        }

        private void DrawAllocationBarChart(SKCanvas canvas)
        {
            if (allocationData == null || allocationData.Count == 0)
                return;

            // Group allocations by entity
            var entityAllocations = new Dictionary<string, List<decimal>>();
            var entityNames = new HashSet<string>();

            foreach (var allocation in allocationData)
            {
                foreach (var detail in allocation.Details)
                {
                    if (!entityAllocations.ContainsKey(detail.EntityId))
                    {
                        entityAllocations[detail.EntityId] = new List<decimal>();
                        entityNames.Add(detail.EntityName);
                    }
                    entityAllocations[detail.EntityId].Add(detail.AllocatedVolume);
                }
            }

            if (entityAllocations.Count == 0)
                return;

            // Draw stacked bar chart
            int barCount = entityAllocations.Count;
            float barWidth = plotAreaWidth / (barCount + 1);
            float maxValue = (float)entityAllocations.Values.SelectMany(v => v).Max();
            if (maxValue == 0) maxValue = 1;

            int index = 0;
            var colors = GenerateColors(entityAllocations.Count);

            foreach (var entity in entityAllocations.Keys)
            {
                float x = plotAreaX + (index + 1) * barWidth;
                float totalHeight = (float)((double)entityAllocations[entity].Sum() / (double)maxValue * plotAreaHeight);

                using (var paint = new SKPaint
                {
                    Color = colors[index % colors.Length],
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(x - barWidth / 2, plotAreaY + plotAreaHeight - totalHeight, barWidth, totalHeight, paint);
                }

                // Draw label
                using (var textPaint = new SKPaint
                {
                    Color = configuration.TextColor,
                    TextSize = configuration.TickLabelFontSize,
                    IsAntialias = true
                })
                {
                    textPaint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText(entity, x, plotAreaY + plotAreaHeight + 20, textPaint);
                }

                index++;
            }
        }

        private void DrawPieSlice(SKCanvas canvas, float centerX, float centerY, float radius, float startAngle, float sweepAngle, SKColor color, string label)
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

        private SKColor[] GenerateColors(int count)
        {
            var colors = new List<SKColor>
            {
                new SKColor(33, 150, 243),   // Blue
                new SKColor(76, 175, 80),   // Green
                new SKColor(244, 67, 54),   // Red
                new SKColor(156, 39, 176),  // Purple
                new SKColor(255, 152, 0),   // Orange
                new SKColor(0, 188, 212),   // Cyan
                new SKColor(255, 87, 34),   // Deep Orange
                new SKColor(63, 81, 181)    // Indigo
            };

            var result = new SKColor[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = colors[i % colors.Count];
            }
            return result;
        }
    }
}

