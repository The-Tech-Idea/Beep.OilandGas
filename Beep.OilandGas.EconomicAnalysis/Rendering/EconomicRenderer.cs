using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.EconomicAnalysis.Calculations;

namespace Beep.OilandGas.EconomicAnalysis.Rendering
{
    public class EconomicRenderer
    {
        private readonly EconomicRendererConfiguration configuration;
        private CashFlow[] cashFlows;
        private List<NPVProfilePoint> npvProfile;
        private EconomicResult economicResult;
        
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

        public EconomicRenderer(EconomicRendererConfiguration configuration = null)
        {
            this.configuration = configuration ?? new EconomicRendererConfiguration();
            this.zoom = 1.0;
        }

        public void SetCashFlows(CashFlow[] cashFlows)
        {
            this.cashFlows = cashFlows;
        }

        public void SetNPVProfile(List<NPVProfilePoint> profile)
        {
            this.npvProfile = profile;
        }

        public void SetEconomicResult(EconomicResult result)
        {
            this.economicResult = result;
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
                case EconomicPlotType.CashFlow:
                    if (cashFlows != null && cashFlows.Length > 0)
                        DrawCashFlowChart(canvas);
                    break;
                case EconomicPlotType.NPVProfile:
                    if (npvProfile != null && npvProfile.Count > 0)
                        DrawNPVProfile(canvas);
                    break;
                case EconomicPlotType.CumulativeCashFlow:
                    if (cashFlows != null && cashFlows.Length > 0)
                        DrawCumulativeCashFlow(canvas);
                    break;
            }

            if (economicResult != null)
            {
                DrawEconomicResults(canvas, width, height);
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
                // Vertical grid lines
                int xTicks = configuration.XAxisTickCount;
                for (int i = 0; i <= xTicks; i++)
                {
                    float x = plotAreaX + (plotAreaWidth * i / xTicks);
                    canvas.DrawLine(x, plotAreaY, x, plotAreaY + plotAreaHeight, paint);
                }

                // Horizontal grid lines
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

        private void DrawCashFlowChart(SKCanvas canvas)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                return;

            var sorted = cashFlows.OrderBy(cf => cf.Period).ToArray();
            double maxAmount = Math.Max(Math.Abs(sorted.Max(cf => cf.Amount)), Math.Abs(sorted.Min(cf => cf.Amount)));
            double minAmount = -maxAmount;
            float zeroY = plotAreaY + plotAreaHeight / 2f;

            float barWidth = plotAreaWidth / sorted.Length * configuration.CashFlowBarWidth;
            float spacing = plotAreaWidth / sorted.Length * (1 - configuration.CashFlowBarWidth) / 2f;

            for (int i = 0; i < sorted.Length; i++)
            {
                float x = plotAreaX + spacing + i * (plotAreaWidth / sorted.Length);
                float height = (float)(Math.Abs(sorted[i].Amount) / maxAmount * plotAreaHeight / 2f);
                float y = sorted[i].Amount >= 0 ? zeroY - height : zeroY;

                using (var paint = new SKPaint
                {
                    Color = sorted[i].Amount >= 0 ? configuration.PositiveCashFlowColor : configuration.NegativeCashFlowColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(x, y, barWidth, height, paint);
                }
            }

            // Draw zero line
            using (var paint = new SKPaint
            {
                Color = configuration.ZeroLineColor,
                StrokeWidth = 1f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawLine(plotAreaX, zeroY, plotAreaX + plotAreaWidth, zeroY, paint);
            }
        }

        private void DrawNPVProfile(SKCanvas canvas)
        {
            if (npvProfile == null || npvProfile.Count == 0)
                return;

            double minRate = npvProfile.Min(p => p.DiscountRate);
            double maxRate = npvProfile.Max(p => p.DiscountRate);
            double minNPV = npvProfile.Min(p => p.NPV);
            double maxNPV = npvProfile.Max(p => p.NPV);
            double npvRange = maxNPV - minNPV;
            if (npvRange == 0) npvRange = 1;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in npvProfile.OrderBy(p => p.DiscountRate))
                {
                    float x = plotAreaX + (float)((point.DiscountRate - minRate) / (maxRate - minRate) * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - (float)((point.NPV - minNPV) / npvRange * plotAreaHeight);

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
                    Color = configuration.NPVCurveColor,
                    StrokeWidth = configuration.NPVCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            // Draw zero line
            if (minNPV < 0 && maxNPV > 0)
            {
                float zeroY = plotAreaY + plotAreaHeight - (float)(-minNPV / npvRange * plotAreaHeight);
                using (var paint = new SKPaint
                {
                    Color = configuration.ZeroLineColor,
                    StrokeWidth = 1f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawLine(plotAreaX, zeroY, plotAreaX + plotAreaWidth, zeroY, paint);
                }
            }
        }

        private void DrawCumulativeCashFlow(SKCanvas canvas)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                return;

            var sorted = cashFlows.OrderBy(cf => cf.Period).ToArray();
            double cumulative = 0;
            var cumulativePoints = new List<(double period, double cumulative)>();

            foreach (var cf in sorted)
            {
                cumulative += cf.Amount;
                cumulativePoints.Add((cf.Period, cumulative));
            }

            double maxCumulative = Math.Max(Math.Abs(cumulativePoints.Max(p => p.cumulative)), 
                Math.Abs(cumulativePoints.Min(p => p.cumulative)));
            double minCumulative = -maxCumulative;

            using (var path = new SKPath())
            {
                bool first = true;
                foreach (var point in cumulativePoints)
                {
                    float x = plotAreaX + (float)(point.period / sorted.Last().Period * plotAreaWidth);
                    float y = plotAreaY + plotAreaHeight - 
                        (float)((point.cumulative - minCumulative) / (maxCumulative - minCumulative) * plotAreaHeight);

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
                    Color = configuration.NPVCurveColor,
                    StrokeWidth = configuration.NPVCurveLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawEconomicResults(SKCanvas canvas, float width, float height)
        {
            if (economicResult == null)
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
                canvas.DrawText("Economic Results:", x, y, paint);
                y += lineHeight * 1.5f;

                paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
                canvas.DrawText($"NPV: ${economicResult.NPV:N2}", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"IRR: {economicResult.IRR:P2}", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"MIRR: {economicResult.MIRR:P2}", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"PI: {economicResult.ProfitabilityIndex:F2}", x, y, paint);
                y += lineHeight;
                canvas.DrawText($"Payback: {economicResult.PaybackPeriod:F2} years", x, y, paint);
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

