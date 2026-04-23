using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Scenes;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Renders persisted scene interaction artifacts through the core drawing pipeline.
    /// </summary>
    internal static class SceneInteractionRenderer
    {
        public static void Render(SKCanvas canvas, Viewport viewport, DrawingScene scene)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));
            if (viewport == null)
                throw new ArgumentNullException(nameof(viewport));
            if (scene?.InteractionState == null)
                return;

            var style = scene.InteractionState.RenderStyle;
            if (style == null || !style.IsVisible)
                return;

            if (scene.InteractionState.Measurements.Count == 0 && scene.InteractionState.Selections.Count == 0)
                return;

            canvas.Save();
            try
            {
                RenderMeasurements(canvas, viewport, scene.InteractionState.Measurements, style);
                RenderSelections(canvas, viewport, scene.InteractionState.Selections, style);
            }
            finally
            {
                canvas.Restore();
            }
        }

        private static void RenderMeasurements(SKCanvas canvas, Viewport viewport, IReadOnlyList<SceneMeasurementAnnotation> measurements, SceneInteractionRenderStyle style)
        {
            if (measurements == null || measurements.Count == 0)
                return;

            using var linePaint = new SKPaint
            {
                Color = style.MeasurementLineColor,
                StrokeWidth = style.MeasurementStrokeWidth,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            using var areaFillPaint = new SKPaint
            {
                Color = new SKColor(style.MeasurementAreaColor.Red, style.MeasurementAreaColor.Green, style.MeasurementAreaColor.Blue, style.MeasurementAreaFillAlpha),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var areaStrokePaint = new SKPaint
            {
                Color = style.MeasurementAreaColor,
                StrokeWidth = style.MeasurementStrokeWidth,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            foreach (var measurement in measurements)
            {
                if (measurement == null || measurement.Vertices.Count == 0)
                    continue;

                var screenPoints = measurement.Vertices
                    .Select(vertex => ToScreenPoint(vertex, viewport))
                    .ToList();

                if (screenPoints.Count == 0)
                    continue;

                bool isPolygon = measurement.GeometryKind == SceneMeasurementGeometryKind.Polygon && screenPoints.Count >= 3;
                using var path = new SKPath();
                path.MoveTo(screenPoints[0]);
                for (int index = 1; index < screenPoints.Count; index++)
                {
                    path.LineTo(screenPoints[index]);
                }

                if (isPolygon)
                {
                    path.Close();
                    canvas.DrawPath(path, areaFillPaint);
                    canvas.DrawPath(path, areaStrokePaint);
                }
                else
                {
                    canvas.DrawPath(path, linePaint);
                }

                RenderMeasurementVertices(canvas, screenPoints, isPolygon ? style.MeasurementAreaColor : style.MeasurementLineColor, style);
                if (style.ShowMeasurementLabels)
                {
                    RenderLabel(canvas, GetLabelAnchor(screenPoints, isPolygon, style), BuildMeasurementLabel(measurement), style);
                }
            }
        }

        private static void RenderSelections(SKCanvas canvas, Viewport viewport, IReadOnlyList<SceneSelectionAnnotation> selections, SceneInteractionRenderStyle style)
        {
            if (selections == null || selections.Count == 0)
                return;

            using var ringPaint = new SKPaint
            {
                Color = style.SelectionColor,
                StrokeWidth = style.SelectionStrokeWidth,
                Style = SKPaintStyle.Stroke,
                PathEffect = style.SelectionDashPattern is { Length: > 0 }
                    ? SKPathEffect.CreateDash(style.SelectionDashPattern, 0f)
                    : null,
                IsAntialias = true
            };

            using var corePaint = new SKPaint
            {
                Color = style.SelectionColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            foreach (var selection in selections)
            {
                if (selection?.Anchor == null)
                    continue;

                var screenPoint = ToScreenPoint(selection.Anchor, viewport);
                canvas.DrawCircle(screenPoint, style.SelectionRingRadius, ringPaint);
                canvas.DrawCircle(screenPoint, style.SelectionCoreRadius, corePaint);

                if (style.ShowSelectionLabels)
                {
                    string label = string.IsNullOrWhiteSpace(selection.FeatureLabel)
                        ? selection.FeatureKind
                        : selection.FeatureLabel;
                    RenderLabel(
                        canvas,
                        new SKPoint(screenPoint.X + style.SelectionLabelOffsetX, screenPoint.Y + style.SelectionLabelOffsetY),
                        label,
                        style);
                }
            }
        }

        private static void RenderMeasurementVertices(SKCanvas canvas, IReadOnlyList<SKPoint> screenPoints, SKColor color, SceneInteractionRenderStyle style)
        {
            using var pointPaint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var haloPaint = new SKPaint
            {
                Color = style.MeasurementVertexHaloColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = style.MeasurementVertexHaloStrokeWidth,
                IsAntialias = true
            };

            foreach (var point in screenPoints)
            {
                canvas.DrawCircle(point, style.MeasurementVertexRadius, pointPaint);
                canvas.DrawCircle(point, style.MeasurementVertexRadius, haloPaint);
            }
        }

        private static void RenderLabel(SKCanvas canvas, SKPoint anchor, string text, SceneInteractionRenderStyle style)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            using var strokePaint = new SKPaint
            {
                Color = style.LabelStrokeColor,
                TextSize = style.LabelTextSize,
                FakeBoldText = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = style.LabelStrokeWidth,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            using var fillPaint = new SKPaint
            {
                Color = style.LabelFillColor,
                TextSize = style.LabelTextSize,
                FakeBoldText = true,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawText(text, anchor.X, anchor.Y, strokePaint);
            canvas.DrawText(text, anchor.X, anchor.Y, fillPaint);
        }

        private static SKPoint ToScreenPoint(SceneWorldPoint point, Viewport viewport)
        {
            return viewport.WorldToScreen((float)point.X, (float)point.Y);
        }

        private static SKPoint GetLabelAnchor(IReadOnlyList<SKPoint> points, bool polygon, SceneInteractionRenderStyle style)
        {
            if (points == null || points.Count == 0)
                return SKPoint.Empty;

            var anchor = polygon
                ? HitTestGeometry.ComputeCentroid(points)
                : HitTestGeometry.ComputeMidpoint(points);

            return new SKPoint(anchor.X + style.MeasurementLabelOffsetX, anchor.Y + style.MeasurementLabelOffsetY);
        }

        private static string BuildMeasurementLabel(SceneMeasurementAnnotation measurement)
        {
            if (measurement == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(measurement.Label))
                return measurement.DisplayText ?? string.Empty;

            if (string.IsNullOrWhiteSpace(measurement.DisplayText))
                return measurement.Label;

            return string.Equals(measurement.Label, measurement.DisplayText, StringComparison.OrdinalIgnoreCase)
                ? measurement.Label
                : $"{measurement.Label}: {measurement.DisplayText}";
        }
    }
}