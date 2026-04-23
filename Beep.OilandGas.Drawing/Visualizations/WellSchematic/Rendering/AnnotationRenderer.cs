using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Renders annotations, depth scale, and grid for well schematics.
    /// </summary>
    public class AnnotationRenderer
    {
        private readonly DepthTransform depthSystem;
        private readonly WellSchematicConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationRenderer"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth transform.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public AnnotationRenderer(DepthTransform depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Renders the depth scale.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="canvasWidth">The canvas width.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="depthSystem">The depth transform.</param>
        public void RenderDepthScale(SKCanvas canvas, float canvasWidth, float canvasHeight, DepthTransform depthSystem)
        {
            float scaleWidth = configuration.DepthScaleWidth;
            float x = canvasWidth - scaleWidth;

            var paint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = 12,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            };

            // Draw scale line
            canvas.DrawLine(x, 0, x, canvasHeight, paint);

            // Draw tick marks and labels
            float minDepth = (float)depthSystem.MinimumDepth;
            float maxDepth = (float)depthSystem.MaximumDepth;
            float depthRange = maxDepth - minDepth;
            int numTicks = 10;

            for (int i = 0; i <= numTicks; i++)
            {
                float depth = minDepth + (depthRange * i / numTicks);
                float y = depthSystem.ToScreenY(depth, canvasHeight);

                // Draw tick mark
                canvas.DrawLine(x - 5, y, x, y, paint);

                // Draw label
                string label = depth.ToString("F0");
                canvas.DrawText(label, x - 10, y + 5, textPaint);
            }
        }

        /// <summary>
        /// Renders the grid.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="canvasWidth">The canvas width.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="depthSystem">The depth transform.</param>
        /// <param name="gridColor">The grid color.</param>
        public void RenderGrid(SKCanvas canvas, float canvasWidth, float canvasHeight, DepthTransform depthSystem, SKColor gridColor)
        {
            var paint = new SKPaint
            {
                Color = gridColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.5f,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            };

            float minDepth = (float)depthSystem.MinimumDepth;
            float maxDepth = (float)depthSystem.MaximumDepth;
            float depthRange = maxDepth - minDepth;
            int numLines = 20;

            // Draw horizontal grid lines
            for (int i = 0; i <= numLines; i++)
            {
                float depth = minDepth + (depthRange * i / numLines);
                float y = depthSystem.ToScreenY(depth, canvasHeight);
                canvas.DrawLine(0, y, canvasWidth, y, paint);
            }

            // Draw vertical grid lines
            int numVerticalLines = 10;
            for (int i = 0; i <= numVerticalLines; i++)
            {
                float x = canvasWidth * i / numVerticalLines;
                canvas.DrawLine(x, 0, x, canvasHeight, paint);
            }
        }

        /// <summary>
        /// Renders borehole labels.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="borehole">The borehole data.</param>
        /// <param name="index">The borehole index.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="depthSystem">The depth transform.</param>
        public void RenderBoreholeLabels(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, DepthTransform depthSystem)
        {
            var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            // Render borehole name/label if available
            // Use UBHI or generate label from index
            string boreholeLabel = !string.IsNullOrEmpty(borehole.UBHI) ? borehole.UBHI : $"Borehole {index + 1}";
            float labelY = depthSystem.ToScreenY(borehole.TopDepth, canvas.DeviceClipBounds.Height) - 10;
            canvas.DrawText(boreholeLabel, centerX, labelY, textPaint);
        }

        /// <summary>
        /// Renders survey markers for kickoff and landing points.
        /// </summary>
        public void RenderSurveyMarkers(SKCanvas canvas, DeviationSurvey survey, IReadOnlyList<SKPoint> centerLine, float labelBoundaryX)
        {
            if (survey?.SurveyPoints == null || survey.SurveyPoints.Count < 2 || centerLine == null || centerLine.Count < 2)
                return;

            var orderedPoints = survey.SurveyPoints.OrderBy(point => point.MD).ToList();
            var kickoffIndex = FindMarkerIndex(orderedPoints, configuration.KickoffDeviationThresholdDegrees);
            var landingIndex = FindMarkerIndex(orderedPoints, configuration.LandingDeviationThresholdDegrees);

            if (kickoffIndex.HasValue)
            {
                DrawSurveyMarker(canvas, centerLine, kickoffIndex.Value, labelBoundaryX, "KOP", orderedPoints[kickoffIndex.Value].MD, SKColors.OrangeRed, false);
            }

            if (landingIndex.HasValue && landingIndex != kickoffIndex)
            {
                DrawSurveyMarker(canvas, centerLine, landingIndex.Value, labelBoundaryX, "Landing", orderedPoints[landingIndex.Value].MD, SKColors.DodgerBlue, true);
            }
        }

        /// <summary>
        /// Renders callouts for perforation intervals.
        /// </summary>
        public void RenderPerforationCallouts(SKCanvas canvas, IReadOnlyList<WellData_Perf> perforations, List<SKPoint> centerLine, float calloutStartX, float canvasHeight)
        {
            var layouts = PreparePerforationCallouts(perforations, centerLine, calloutStartX, canvasHeight);
            if (layouts.Count == 0)
                return;

            using var linePaint = new SKPaint
            {
                Color = SKColors.IndianRed,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.4f,
                IsAntialias = true
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = configuration.GetEffectiveCalloutTextSize(),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };

            foreach (var layout in layouts)
            {
                var callout = layout.Callout;
                float bracketX = Math.Min(calloutStartX - 24.0f, callout.BoundaryX + 12.0f);
                float lineEndX = calloutStartX - 6.0f;

                canvas.DrawLine(bracketX, callout.TopY, bracketX, callout.BottomY, linePaint);
                canvas.DrawLine(bracketX - 6.0f, callout.TopY, bracketX, callout.TopY, linePaint);
                canvas.DrawLine(bracketX - 6.0f, callout.BottomY, bracketX, callout.BottomY, linePaint);
                canvas.DrawLine(bracketX, callout.Anchor.Y, lineEndX, layout.LabelY, linePaint);
                canvas.DrawText(layout.LabelText, calloutStartX, layout.TextBaselineY, textPaint);
            }
        }

        /// <summary>
        /// Renders callouts for equipment items.
        /// </summary>
        internal void RenderEquipmentCallouts(SKCanvas canvas, IReadOnlyList<EquipmentCalloutTarget> targets, float calloutStartX, float canvasHeight)
        {
            var layouts = PrepareEquipmentCallouts(targets, calloutStartX, canvasHeight);
            if (layouts.Count == 0)
                return;

            using var linePaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor.WithAlpha(200),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.2f,
                IsAntialias = true
            };

            using var pointPaint = new SKPaint
            {
                Color = configuration.Theme.GetColor("Equipment", SKColors.DarkGreen),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = configuration.GetEffectiveCalloutTextSize(),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };

            foreach (var layout in layouts)
            {
                var callout = layout.Callout;
                float elbowX = Math.Min(calloutStartX - 20.0f, callout.Anchor.X + 16.0f);
                float lineEndX = calloutStartX - 6.0f;

                canvas.DrawCircle(callout.Anchor.X, callout.Anchor.Y, 3.5f, pointPaint);
                canvas.DrawLine(callout.Anchor.X, callout.Anchor.Y, elbowX, callout.Anchor.Y, linePaint);
                canvas.DrawLine(elbowX, callout.Anchor.Y, lineEndX, layout.LabelY, linePaint);
                canvas.DrawText(layout.LabelText, calloutStartX, layout.TextBaselineY, textPaint);
            }
        }

        internal IReadOnlyList<PerforationCalloutLayout> PreparePerforationCallouts(IReadOnlyList<WellData_Perf> perforations, List<SKPoint> centerLine, float calloutStartX, float canvasHeight)
        {
            if (!configuration.ShouldRenderPerforationCallouts() || perforations == null || perforations.Count == 0 || centerLine == null || centerLine.Count < 2)
                return Array.Empty<PerforationCalloutLayout>();

            var prepared = perforations
                .Select(perforation => CreatePerforationCallout(perforation, centerLine, canvasHeight))
                .Where(callout => callout != null)
                .OrderBy(callout => callout.Anchor.Y)
                .ToList();

            if (prepared.Count == 0)
                return Array.Empty<PerforationCalloutLayout>();

            var labelYs = LayoutCalloutYPositions(prepared.Select(callout => callout.Anchor.Y).ToList(), canvasHeight);
            using var textPaint = CreateCalloutTextPaint();
            var layouts = new List<PerforationCalloutLayout>(prepared.Count);

            for (int index = 0; index < prepared.Count; index++)
            {
                var callout = prepared[index];
                float labelY = labelYs[index];
                float textBaselineY = labelY + 4.0f;
                string labelText = BuildPerforationCalloutLabel(callout.Perforation);
                layouts.Add(new PerforationCalloutLayout(
                    callout,
                    labelY,
                    textBaselineY,
                    labelText,
                    BuildTextBounds(calloutStartX, textBaselineY, labelText, textPaint)));
            }

            return layouts;
        }

        internal IReadOnlyList<EquipmentCalloutLayout> PrepareEquipmentCallouts(IReadOnlyList<EquipmentCalloutTarget> targets, float calloutStartX, float canvasHeight)
        {
            if (!configuration.ShouldRenderEquipmentCallouts() || targets == null || targets.Count == 0)
                return Array.Empty<EquipmentCalloutLayout>();

            var prepared = targets
                .Where(target => target.Equipment != null && target.Path != null && target.Path.Count >= 2)
                .Select(target => CreateEquipmentCallout(target, canvasHeight))
                .Where(callout => callout != null)
                .OrderBy(callout => callout.Anchor.Y)
                .ToList();

            if (prepared.Count == 0)
                return Array.Empty<EquipmentCalloutLayout>();

            var labelYs = LayoutCalloutYPositions(prepared.Select(callout => callout.Anchor.Y).ToList(), canvasHeight);
            using var textPaint = CreateCalloutTextPaint();
            var layouts = new List<EquipmentCalloutLayout>(prepared.Count);

            for (int index = 0; index < prepared.Count; index++)
            {
                var callout = prepared[index];
                float labelY = labelYs[index];
                float textBaselineY = labelY + 4.0f;
                string labelText = BuildEquipmentCalloutLabel(callout.Target);
                layouts.Add(new EquipmentCalloutLayout(
                    callout,
                    labelY,
                    textBaselineY,
                    labelText,
                    BuildTextBounds(calloutStartX, textBaselineY, labelText, textPaint)));
            }

            return layouts;
        }

        private static int? FindMarkerIndex(IReadOnlyList<DeviationSurveyPoint> surveyPoints, float threshold)
        {
            for (int index = 0; index < surveyPoints.Count; index++)
            {
                if (surveyPoints[index].DEVIATION_ANGLE >= threshold)
                    return index;
            }

            return null;
        }

        private void DrawSurveyMarker(SKCanvas canvas, IReadOnlyList<SKPoint> centerLine, int surveyIndex, float labelBoundaryX, string label, double measuredDepth, SKColor color, bool drawDiamond)
        {
            var index = Math.Min(surveyIndex, centerLine.Count - 1);
            var anchor = centerLine[index];
            const float markerSize = 6.0f;
            const float leaderLength = 18.0f;

            using var leaderPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            using var fillPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var textPaint = new SKPaint
            {
                Color = color,
                TextSize = configuration.GetEffectiveMarkerTextSize(),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                FakeBoldText = true
            };

            float lineEndX = Math.Min(labelBoundaryX - 4, anchor.X + leaderLength);
            float textX = Math.Min(labelBoundaryX, lineEndX + 6);
            float textY = anchor.Y - 4;

            canvas.DrawLine(anchor.X, anchor.Y, lineEndX, anchor.Y, leaderPaint);

            if (drawDiamond)
            {
                using var diamond = new SKPath();
                diamond.MoveTo(anchor.X, anchor.Y - markerSize);
                diamond.LineTo(anchor.X + markerSize, anchor.Y);
                diamond.LineTo(anchor.X, anchor.Y + markerSize);
                diamond.LineTo(anchor.X - markerSize, anchor.Y);
                diamond.Close();
                canvas.DrawPath(diamond, fillPaint);
            }
            else
            {
                canvas.DrawCircle(anchor.X, anchor.Y, markerSize, fillPaint);
            }

            canvas.DrawText($"{label} {measuredDepth:F0} MD", textX, textY, textPaint);
        }

        private PerforationCallout CreatePerforationCallout(WellData_Perf perforation, List<SKPoint> centerLine, float canvasHeight)
        {
            float topY = depthSystem.ToScreenY(perforation.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(perforation.BottomDepth, canvasHeight);
            var segment = PathHelper.GetPathSegment(centerLine, topY, bottomY);
            if (segment.Count < 2)
                return null;

            return new PerforationCallout(
                perforation,
                PathHelper.GetPointOnPathNormalized(segment, 0.5f),
                topY,
                bottomY,
                segment.Max(point => point.X));
        }

        private EquipmentCallout CreateEquipmentCallout(EquipmentCalloutTarget target, float canvasHeight)
        {
            float anchorDepth = (target.Equipment.TopDepth + target.Equipment.BottomDepth) / 2.0f;
            float anchorY = depthSystem.ToScreenY(anchorDepth, canvasHeight);
            return new EquipmentCallout(target, PathHelper.GetPointOnPath(target.Path, anchorY));
        }

        private List<float> LayoutCalloutYPositions(IReadOnlyList<float> preferredYs, float canvasHeight)
        {
            var labelYs = new List<float>(preferredYs.Count);
            float minimumY = 18.0f;
            float maximumY = Math.Max(minimumY, canvasHeight - 18.0f);
            float spacing = Math.Max(8.0f, configuration.GetEffectiveAnnotationSpacing());

            foreach (var preferredY in preferredYs)
            {
                float candidate = Math.Max(preferredY, labelYs.Count == 0 ? minimumY : labelYs[^1] + spacing);
                labelYs.Add(Math.Min(candidate, maximumY));
            }

            for (int index = labelYs.Count - 2; index >= 0; index--)
            {
                float permitted = labelYs[index + 1] - spacing;
                if (labelYs[index] > permitted)
                {
                    labelYs[index] = Math.Max(minimumY, permitted);
                }
            }

            return labelYs;
        }

        private SKPaint CreateCalloutTextPaint()
        {
            return new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = configuration.GetEffectiveCalloutTextSize(),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };
        }

        private static SKRect BuildTextBounds(float x, float baselineY, string labelText, SKPaint textPaint)
        {
            float width = textPaint.MeasureText(labelText ?? string.Empty);
            var metrics = textPaint.FontMetrics;
            return new SKRect(
                x,
                baselineY + metrics.Ascent,
                x + width,
                baselineY + metrics.Descent);
        }

        private static string BuildPerforationCalloutLabel(WellData_Perf perforation)
        {
            string interval = $"{perforation.TopDepth:F0}-{perforation.BottomDepth:F0} MD";

            if (!string.IsNullOrWhiteSpace(perforation.CompletionCode))
                return $"Perf {interval} [{perforation.CompletionCode}]";

            if (!string.IsNullOrWhiteSpace(perforation.PerfType))
                return $"Perf {interval} {perforation.PerfType}";

            return $"Perf {interval}";
        }

        private static string BuildEquipmentCalloutLabel(EquipmentCalloutTarget target)
        {
            var equipment = target.Equipment;
            string name = FirstNonEmpty(equipment.EquipmentName, equipment.EquipmentType, equipment.EquipmentDescription, equipment.ToolTipText, "Equipment");
            string depth = $"{((equipment.TopDepth + equipment.BottomDepth) / 2.0f):F0} MD";
            string prefix = string.IsNullOrWhiteSpace(target.Prefix) ? string.Empty : target.Prefix + ": ";
            return prefix + name + " @ " + depth;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        internal sealed record PerforationCallout(WellData_Perf Perforation, SKPoint Anchor, float TopY, float BottomY, float BoundaryX);

        internal sealed record EquipmentCallout(EquipmentCalloutTarget Target, SKPoint Anchor);

        internal sealed record PerforationCalloutLayout(PerforationCallout Callout, float LabelY, float TextBaselineY, string LabelText, SKRect LabelBounds);

        internal sealed record EquipmentCalloutLayout(EquipmentCallout Callout, float LabelY, float TextBaselineY, string LabelText, SKRect LabelBounds);
    }

    internal sealed record EquipmentCalloutTarget(WellData_Equip Equipment, List<SKPoint> Path, string Prefix = null);
}

