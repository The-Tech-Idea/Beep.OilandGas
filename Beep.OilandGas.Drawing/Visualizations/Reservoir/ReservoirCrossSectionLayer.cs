using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Layers;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Renders extracted reservoir cross-section profiles, wells, and fluid contacts.
    /// </summary>
    public sealed class ReservoirCrossSectionLayer : LayerBase
    {
        private readonly ReservoirCrossSectionData sectionData;
        private readonly ReservoirCrossSectionConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservoirCrossSectionLayer"/> class.
        /// </summary>
        public ReservoirCrossSectionLayer(ReservoirCrossSectionData sectionData, ReservoirCrossSectionConfiguration configuration = null)
            : base("Reservoir Cross Section")
        {
            this.sectionData = sectionData ?? throw new ArgumentNullException(nameof(sectionData));
            this.configuration = configuration ?? new ReservoirCrossSectionConfiguration();
        }

        /// <inheritdoc />
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            canvas.DrawColor(configuration.BackgroundColor);
            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                DrawProfiles(canvas);

                if (configuration.ShowFluidContacts)
                {
                    DrawFluidContacts(canvas);
                }

                if (configuration.ShowWellMarkers)
                {
                    DrawWellMarkers(canvas);
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
            if (sectionData.Bounds == null)
                return SKRect.Empty;

            return new SKRect(
                (float)sectionData.Bounds.MinX,
                (float)sectionData.Bounds.MinZ,
                (float)sectionData.Bounds.MaxX,
                (float)sectionData.Bounds.MaxZ);
        }

        private void DrawProfiles(SKCanvas canvas)
        {
            foreach (var profile in sectionData.Profiles.OrderBy(profile => profile.Samples.Average(sample => sample.Depth)))
            {
                using var paint = new SKPaint
                {
                    Color = ResolveProfileColor(profile.SurfaceKind),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.SurfaceLineWidth,
                    IsAntialias = true
                };

                using var path = new SKPath();
                var samples = profile.Samples.Where(sample => double.IsFinite(sample.Distance) && double.IsFinite(sample.Depth)).ToList();
                if (samples.Count < 2)
                    continue;

                path.MoveTo((float)samples[0].Distance, (float)samples[0].Depth);
                for (int index = 1; index < samples.Count; index++)
                {
                    path.LineTo((float)samples[index].Distance, (float)samples[index].Depth);
                }

                canvas.DrawPath(path, paint);

                if (configuration.ShowSurfaceLabels)
                {
                    DrawSurfaceLabel(canvas, profile, paint.Color);
                }
            }
        }

        private void DrawSurfaceLabel(SKCanvas canvas, ReservoirSectionProfile profile, SKColor color)
        {
            var label = string.IsNullOrWhiteSpace(profile.SurfaceName) ? profile.SurfaceId : profile.SurfaceName;
            if (string.IsNullOrWhiteSpace(label) || profile.Samples.Count == 0)
                return;

            var anchor = profile.Samples[^1];

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.SurfaceLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var textPaint = new SKPaint
            {
                Color = color,
                TextSize = configuration.SurfaceLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right,
                Style = SKPaintStyle.Fill
            };

            float x = (float)Math.Max(sectionData.Bounds.MinX + 1, anchor.Distance - 5);
            float y = (float)anchor.Depth - 3f;
            canvas.DrawText(label, x, y, haloPaint);
            canvas.DrawText(label, x, y, textPaint);
        }

        private void DrawFluidContacts(SKCanvas canvas)
        {
            var contacts = new List<(string Label, double Depth)>
            {
                ("FWL", sectionData.FluidContacts?.FreeWaterLevel ?? double.NaN),
                ("OWC", sectionData.FluidContacts?.OilWaterContact ?? double.NaN),
                ("GOC", sectionData.FluidContacts?.GasOilContact ?? double.NaN),
                ("GWC", sectionData.FluidContacts?.GasWaterContact ?? double.NaN)
            }
            .Where(contact => double.IsFinite(contact.Depth))
            .ToList();

            if (contacts.Count == 0)
                return;

            using var linePaint = new SKPaint
            {
                Color = configuration.ContactColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = configuration.ContactLineWidth,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 12f, 8f }, 0)
            };

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.SurfaceLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.ContactColor,
                TextSize = configuration.SurfaceLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Style = SKPaintStyle.Fill
            };

            foreach (var contact in contacts)
            {
                float y = (float)contact.Depth;
                canvas.DrawLine((float)sectionData.Bounds.MinX, y, (float)sectionData.Bounds.MaxX, y, linePaint);
                canvas.DrawText(contact.Label, (float)sectionData.Bounds.MinX + 6f, y - 4f, haloPaint);
                canvas.DrawText(contact.Label, (float)sectionData.Bounds.MinX + 6f, y - 4f, textPaint);
            }
        }

        private void DrawWellMarkers(SKCanvas canvas)
        {
            if (sectionData.WellMarkers == null || sectionData.WellMarkers.Count == 0)
                return;

            float depthRange = (float)Math.Max(1.0, sectionData.Bounds.MaxZ - sectionData.Bounds.MinZ);
            float labelY = (float)sectionData.Bounds.MinZ + (depthRange * 0.05f);

            using var linePaint = new SKPaint
            {
                Color = configuration.WellColor.WithAlpha(110),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.2f,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 8f, 6f }, 0)
            };

            using var outlinePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            using var haloPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = configuration.WellLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.WellColor,
                TextSize = configuration.WellLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            foreach (var marker in sectionData.WellMarkers.OrderBy(marker => marker.DistanceAlongSection))
            {
                float x = (float)marker.DistanceAlongSection;
                float y = (float)marker.Depth;
                var color = ResolveWellColor(marker);

                linePaint.Color = color.WithAlpha(110);
                textPaint.Color = color;

                using var fillPaint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                canvas.DrawLine(x, (float)sectionData.Bounds.MinZ, x, (float)sectionData.Bounds.MaxZ, linePaint);
                canvas.DrawCircle(x, y, configuration.WellMarkerRadius + 1f, outlinePaint);
                canvas.DrawCircle(x, y, configuration.WellMarkerRadius, fillPaint);

                var label = string.IsNullOrWhiteSpace(marker.WellName) ? marker.Uwi : marker.WellName;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    canvas.DrawText(label, x, labelY, haloPaint);
                    canvas.DrawText(label, x, labelY, textPaint);
                }
            }
        }

        private static SKColor ResolveProfileColor(ReservoirSurfaceKind kind)
        {
            return kind switch
            {
                ReservoirSurfaceKind.Horizon => new SKColor(32, 74, 135),
                ReservoirSurfaceKind.Structure => new SKColor(46, 125, 50),
                ReservoirSurfaceKind.Property => new SKColor(2, 136, 209),
                ReservoirSurfaceKind.Isochore => new SKColor(245, 124, 0),
                ReservoirSurfaceKind.Fault => new SKColor(132, 35, 27),
                ReservoirSurfaceKind.GridDerived => new SKColor(96, 125, 139),
                _ => new SKColor(84, 110, 122)
            };
        }

        private SKColor ResolveWellColor(ReservoirWellSectionMarker marker)
        {
            if (!string.IsNullOrWhiteSpace(marker?.ColorCode) && marker.ColorCode.StartsWith("#", StringComparison.Ordinal))
            {
                try
                {
                    return SKColor.Parse(marker.ColorCode);
                }
                catch
                {
                }
            }

            return configuration.WellColor;
        }
    }
}