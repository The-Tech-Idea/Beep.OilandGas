using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Rendering;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Renders plan-view contour lines and labels for a reservoir surface.
    /// </summary>
    public sealed class ReservoirContourLayer : LayerBase, IInteractiveLayer
    {
        private readonly ReservoirSurfaceData surface;
        private readonly ReservoirContourConfiguration configuration;
        private IReadOnlyList<ContourLineSegment> contourSegments;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservoirContourLayer"/> class.
        /// </summary>
        public ReservoirContourLayer(ReservoirSurfaceData surface, ReservoirContourConfiguration configuration = null)
            : base("Reservoir Contours")
        {
            this.surface = surface ?? throw new ArgumentNullException(nameof(surface));
            this.configuration = configuration ?? new ReservoirContourConfiguration();
        }

        /// <inheritdoc />
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            if (surface.Points == null || surface.Points.Count < 3)
                return;

            EnsureContoursGenerated();
            if (contourSegments == null || contourSegments.Count == 0)
                return;

            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                using var minorPaint = new SKPaint
                {
                    Color = configuration.MinorContourColor,
                    StrokeWidth = configuration.MinorContourLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                };

                using var majorPaint = new SKPaint
                {
                    Color = configuration.MajorContourColor,
                    StrokeWidth = configuration.MajorContourLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                };

                foreach (var segment in contourSegments)
                {
                    canvas.DrawLine(segment.Start, segment.End, segment.IsMajor ? majorPaint : minorPaint);
                }

                if (configuration.ShowContourLabels)
                {
                    DrawLabels(canvas);
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
            var points = surface.Points ?? new List<Point3D>();
            if (points.Count == 0)
                return SKRect.Empty;

            var boundingBox = surface.BoundingBox ?? new BoundingBox
            {
                MinX = points.Min(point => point.X),
                MaxX = points.Max(point => point.X),
                MinY = points.Min(point => point.Y),
                MaxY = points.Max(point => point.Y)
            };

            return new SKRect(
                (float)boundingBox.MinX,
                (float)boundingBox.MinY,
                (float)Math.Max(boundingBox.MaxX, boundingBox.MinX + 1),
                (float)Math.Max(boundingBox.MaxY, boundingBox.MinY + 1));
        }

        /// <inheritdoc />
        public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
        {
            EnsureContoursGenerated();
            if (contourSegments == null || contourSegments.Count == 0)
                return null;

            var hit = contourSegments
                .Select((segment, index) => new
                {
                    Segment = segment,
                    Index = index,
                    Distance = HitTestGeometry.DistanceToPolyline(worldPoint, new[] { segment.Start, segment.End })
                })
                .Where(candidate => candidate.Distance <= worldTolerance)
                .OrderBy(candidate => candidate.Distance)
                .ThenByDescending(candidate => candidate.Segment.IsMajor)
                .FirstOrDefault();

            if (hit == null)
                return null;

            string formattedLevel = hit.Segment.Level.ToString(configuration.LabelFormat);
            return new LayerHitResult(
                layerName: Name,
                featureId: $"contour:{formattedLevel}:{hit.Index}",
                featureLabel: $"Contour {formattedLevel}",
                featureKind: "Contour",
                worldAnchor: hit.Segment.Midpoint,
                distance: hit.Distance,
                metadata: new Dictionary<string, string>
                {
                    ["Level"] = formattedLevel,
                    ["IsMajor"] = hit.Segment.IsMajor ? "Y" : "N",
                    ["Length"] = hit.Segment.Length.ToString("0.###")
                });
        }

        private void EnsureContoursGenerated()
        {
            contourSegments ??= ContourGenerator.Generate(surface, configuration);
        }

        private void DrawLabels(SKCanvas canvas)
        {
            var candidateSegments = contourSegments
                .Where(segment => segment.Length >= configuration.MinimumLabeledSegmentLength)
                .Where(segment => !configuration.LabelMajorContoursOnly || segment.IsMajor)
                .GroupBy(segment => segment.Level)
                .Select(group => group.OrderByDescending(segment => segment.Length).First())
                .ToList();

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.LabelColor,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            foreach (var segment in candidateSegments)
            {
                string label = segment.Level.ToString(configuration.LabelFormat);
                float x = segment.Midpoint.X;
                float y = segment.Midpoint.Y - 2f;
                canvas.DrawText(label, x, y, haloPaint);
                canvas.DrawText(label, x, y, textPaint);
            }
        }
    }
}