using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Layers;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Renders map-space fault and well overlays on top of a reservoir contour base.
    /// </summary>
    public sealed class ReservoirMapOverlayLayer : LayerBase, IInteractiveLayer
    {
        private readonly IReadOnlyList<ReservoirSurfaceData> faultSurfaces;
        private readonly IReadOnlyList<ReservoirWellMapPoint> wells;
        private readonly ReservoirMapConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservoirMapOverlayLayer"/> class.
        /// </summary>
        public ReservoirMapOverlayLayer(
            IEnumerable<ReservoirSurfaceData> faultSurfaces = null,
            IEnumerable<ReservoirWellMapPoint> wells = null,
            ReservoirMapConfiguration configuration = null)
            : base("Reservoir Map Overlays")
        {
            this.faultSurfaces = (faultSurfaces ?? Enumerable.Empty<ReservoirSurfaceData>()).ToList();
            this.wells = (wells ?? Enumerable.Empty<ReservoirWellMapPoint>()).ToList();
            this.configuration = configuration ?? new ReservoirMapConfiguration();
        }

        /// <inheritdoc />
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            if ((!configuration.ShowFaults || faultSurfaces.Count == 0) && (!configuration.ShowWells || wells.Count == 0))
                return;

            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                if (configuration.ShowFaults)
                {
                    DrawFaultOverlays(canvas);
                }

                if (configuration.ShowWells)
                {
                    DrawWellOverlays(canvas);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <inheritdoc />
        public override SKRect GetBounds()
        {
            var points = new List<SKPoint>();

            points.AddRange(faultSurfaces.SelectMany(GetProjectedPoints));
            points.AddRange(wells.SelectMany(GetWellPoints));

            if (points.Count == 0)
                return SKRect.Empty;

            var minX = points.Min(point => point.X) - configuration.WellMarkerRadius;
            var maxX = points.Max(point => point.X) + configuration.WellMarkerRadius;
            var minY = points.Min(point => point.Y) - configuration.WellMarkerRadius;
            var maxY = points.Max(point => point.Y) + configuration.WellMarkerRadius;
            return new SKRect(minX, minY, maxX, maxY);
        }

        /// <inheritdoc />
        public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
        {
            if (configuration.ShowWells)
            {
                var wellHit = HitTestWells(worldPoint, worldTolerance);
                if (wellHit != null)
                    return wellHit;
            }

            if (configuration.ShowFaults)
            {
                var faultHit = HitTestFaults(worldPoint, worldTolerance);
                if (faultHit != null)
                    return faultHit;
            }

            return null;
        }

        private void DrawFaultOverlays(SKCanvas canvas)
        {
            using var linePaint = new SKPaint
            {
                Color = configuration.FaultColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = configuration.FaultLineWidth,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 14f, 8f, 3f, 8f }, 0)
            };

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.FaultLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.FaultLabelColor,
                TextSize = configuration.FaultLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            foreach (var faultSurface in faultSurfaces)
            {
                var trace = BuildFaultTrace(faultSurface);
                if (trace.Count < 2)
                    continue;

                DrawPolyline(canvas, trace, linePaint);

                if (configuration.ShowFaultLabels)
                {
                    var labelPoint = trace[trace.Count / 2];
                    var label = string.IsNullOrWhiteSpace(faultSurface.SurfaceName)
                        ? faultSurface.SurfaceId
                        : faultSurface.SurfaceName;

                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        canvas.DrawText(label, labelPoint.X, labelPoint.Y - 6f, haloPaint);
                        canvas.DrawText(label, labelPoint.X, labelPoint.Y - 6f, textPaint);
                    }
                }
            }
        }

        private void DrawWellOverlays(SKCanvas canvas)
        {
            using var trajectoryPaint = new SKPaint
            {
                Color = configuration.WellTrajectoryColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = configuration.WellTrajectoryLineWidth,
                IsAntialias = true
            };

            using var outlinePaint = new SKPaint
            {
                Color = configuration.WellOutlineColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.8f,
                IsAntialias = true
            };

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.WellLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.WellLabelColor,
                TextSize = configuration.WellLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Fill
            };

            foreach (var well in wells)
            {
                var mapPoints = GetWellPoints(well).ToList();
                if (mapPoints.Count == 0)
                    continue;

                var wellColor = ResolveWellColor(well);
                using var fillPaint = new SKPaint
                {
                    Color = wellColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                if (configuration.ShowWellTrajectories && mapPoints.Count > 1)
                {
                    trajectoryPaint.Color = wellColor.WithAlpha(configuration.WellTrajectoryColor.Alpha);
                    DrawPolyline(canvas, mapPoints, trajectoryPaint);
                }

                var anchor = mapPoints[0];
                canvas.DrawCircle(anchor, configuration.WellMarkerRadius + 1.5f, outlinePaint);
                canvas.DrawCircle(anchor, configuration.WellMarkerRadius, fillPaint);

                if (configuration.ShowWellLabels)
                {
                    var label = ResolveWellLabel(well);
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        float labelX = anchor.X + configuration.WellMarkerRadius + 6f;
                        float labelY = anchor.Y - configuration.WellMarkerRadius;
                        canvas.DrawText(label, labelX, labelY, haloPaint);
                        canvas.DrawText(label, labelX, labelY, textPaint);
                    }
                }
            }
        }

        private List<SKPoint> BuildFaultTrace(ReservoirSurfaceData surface)
        {
            var points = GetProjectedPoints(surface)
                .GroupBy(point => $"{Math.Round(point.X, 2)}|{Math.Round(point.Y, 2)}")
                .Select(group => group.First())
                .ToList();

            if (points.Count < 3)
                return points;

            float centerX = points.Average(point => point.X);
            float centerY = points.Average(point => point.Y);

            double xx = 0;
            double xy = 0;
            double yy = 0;

            foreach (var point in points)
            {
                double dx = point.X - centerX;
                double dy = point.Y - centerY;
                xx += dx * dx;
                xy += dx * dy;
                yy += dy * dy;
            }

            double angle = 0.5 * Math.Atan2(2 * xy, xx - yy);
            float axisX = (float)Math.Cos(angle);
            float axisY = (float)Math.Sin(angle);

            return points
                .OrderBy(point => ((point.X - centerX) * axisX) + ((point.Y - centerY) * axisY))
                .ToList();
        }

        private IEnumerable<SKPoint> GetProjectedPoints(ReservoirSurfaceData surface)
        {
            if (surface?.Points == null)
                yield break;

            foreach (var point in surface.Points)
            {
                if (point == null || !double.IsFinite(point.X) || !double.IsFinite(point.Y))
                    continue;

                yield return new SKPoint((float)point.X, (float)point.Y);
            }
        }

        private IEnumerable<SKPoint> GetWellPoints(ReservoirWellMapPoint well)
        {
            if (well == null)
                yield break;

            if (well.SurfaceLocation != null && double.IsFinite(well.SurfaceLocation.X) && double.IsFinite(well.SurfaceLocation.Y))
            {
                yield return new SKPoint((float)well.SurfaceLocation.X, (float)well.SurfaceLocation.Y);
            }

            if (well.TrajectoryPoints == null)
                yield break;

            foreach (var point in well.TrajectoryPoints)
            {
                if (point == null || !double.IsFinite(point.X) || !double.IsFinite(point.Y))
                    continue;

                yield return new SKPoint((float)point.X, (float)point.Y);
            }
        }

        private static void DrawPolyline(SKCanvas canvas, IReadOnlyList<SKPoint> points, SKPaint paint)
        {
            if (points == null || points.Count < 2)
                return;

            using var path = new SKPath();
            path.MoveTo(points[0]);
            for (int index = 1; index < points.Count; index++)
            {
                path.LineTo(points[index]);
            }

            canvas.DrawPath(path, paint);
        }

        private SKColor ResolveWellColor(ReservoirWellMapPoint well)
        {
            if (!string.IsNullOrWhiteSpace(well?.ColorCode) && well.ColorCode.StartsWith("#", StringComparison.Ordinal))
            {
                try
                {
                    return SKColor.Parse(well.ColorCode);
                }
                catch
                {
                }
            }

            return configuration.WellColor;
        }

        private static string ResolveWellLabel(ReservoirWellMapPoint well)
        {
            if (!string.IsNullOrWhiteSpace(well.WellName))
                return well.WellName;

            if (!string.IsNullOrWhiteSpace(well.Uwi))
                return well.Uwi;

            return well.WellId;
        }

        private LayerHitResult HitTestWells(SKPoint worldPoint, float worldTolerance)
        {
            foreach (var well in wells.Reverse())
            {
                var points = GetWellPoints(well).ToList();
                if (points.Count == 0)
                    continue;

                var anchor = points[0];
                float markerDistance = SKPoint.Distance(worldPoint, anchor);
                if (markerDistance <= configuration.WellMarkerRadius + worldTolerance)
                {
                    return CreateHitResult(
                        well.WellId ?? well.Uwi,
                        ResolveWellLabel(well),
                        "Well",
                        anchor,
                        markerDistance,
                        well.Metadata);
                }

                if (configuration.ShowWellTrajectories && points.Count > 1)
                {
                    float trajectoryDistance = HitTestGeometry.DistanceToPolyline(worldPoint, points);
                    if (trajectoryDistance <= worldTolerance + (configuration.WellTrajectoryLineWidth * 0.5f))
                    {
                        return CreateHitResult(
                            well.WellId ?? well.Uwi,
                            ResolveWellLabel(well),
                            "WellTrajectory",
                            HitTestGeometry.ComputeMidpoint(points),
                            trajectoryDistance,
                            well.Metadata);
                    }
                }
            }

            return null;
        }

        private LayerHitResult HitTestFaults(SKPoint worldPoint, float worldTolerance)
        {
            foreach (var faultSurface in faultSurfaces.Reverse())
            {
                var trace = BuildFaultTrace(faultSurface);
                if (trace.Count < 2)
                    continue;

                float distance = HitTestGeometry.DistanceToPolyline(worldPoint, trace);
                if (distance <= worldTolerance + (configuration.FaultLineWidth * 0.5f))
                {
                    return CreateHitResult(
                        faultSurface.SurfaceId,
                        string.IsNullOrWhiteSpace(faultSurface.SurfaceName) ? faultSurface.SurfaceId : faultSurface.SurfaceName,
                        "Fault",
                        HitTestGeometry.ComputeMidpoint(trace),
                        distance,
                        faultSurface.Metadata);
                }
            }

            return null;
        }

        private LayerHitResult CreateHitResult(
            string featureId,
            string featureLabel,
            string featureKind,
            SKPoint worldAnchor,
            float distance,
            IReadOnlyDictionary<string, string> metadata)
        {
            return new LayerHitResult(
                Name,
                featureId,
                featureLabel,
                featureKind,
                worldAnchor,
                distance,
                metadata != null ? new Dictionary<string, string>(metadata) : new Dictionary<string, string>());
        }
    }
}