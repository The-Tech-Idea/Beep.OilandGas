using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Extracts section-view profiles from typed reservoir surfaces along a straight cut line.
    /// </summary>
    public static class ReservoirCrossSectionExtractor
    {
        /// <summary>
        /// Extracts a reservoir cross-section from the supplied reservoir data and section line.
        /// </summary>
        public static ReservoirCrossSectionData Extract(
            ReservoirData reservoir,
            ReservoirSectionLine sectionLine,
            ReservoirCrossSectionConfiguration configuration,
            IEnumerable<ReservoirSurfaceData> surfaces = null,
            IEnumerable<ReservoirWellMapPoint> wells = null)
        {
            if (sectionLine == null)
                throw new ArgumentNullException(nameof(sectionLine));

            configuration ??= new ReservoirCrossSectionConfiguration();

            if (sectionLine.Start == null || sectionLine.End == null || sectionLine.Length <= 0)
                throw new InvalidOperationException("A valid section line with distinct start and end points is required.");

            var selectedSurfaces = ResolveSurfaces(reservoir, surfaces).ToList();
            if (selectedSurfaces.Count == 0)
                throw new InvalidOperationException("At least one sampled reservoir surface is required to extract a cross-section.");

            var data = new ReservoirCrossSectionData
            {
                SectionLine = sectionLine,
                FluidContacts = reservoir?.FluidContacts
            };

            foreach (var surface in selectedSurfaces)
            {
                var profile = ExtractProfile(surface, sectionLine, configuration);
                if (profile.Samples.Count > 0)
                {
                    data.Profiles.Add(profile);
                }
            }

            if (data.Profiles.Count == 0)
                throw new InvalidOperationException("The selected section line did not produce any valid surface samples.");

            var profileDepths = data.Profiles.SelectMany(profile => profile.Samples).Select(sample => sample.Depth).ToList();
            double minDepth = profileDepths.Min();
            double maxDepth = profileDepths.Max();

            data.WellMarkers = ExtractWellMarkers(wells, sectionLine, configuration, minDepth).ToList();

            if (data.WellMarkers.Count > 0)
            {
                minDepth = Math.Min(minDepth, data.WellMarkers.Min(marker => marker.Depth));
                maxDepth = Math.Max(maxDepth, data.WellMarkers.Max(marker => marker.Depth));
            }

            foreach (var contactDepth in EnumerateFluidContactDepths(data.FluidContacts))
            {
                minDepth = Math.Min(minDepth, contactDepth);
                maxDepth = Math.Max(maxDepth, contactDepth);
            }

            if (Math.Abs(maxDepth - minDepth) < 1e-6)
            {
                maxDepth = minDepth + 1;
            }

            data.Bounds = new BoundingBox
            {
                MinX = 0,
                MaxX = sectionLine.Length,
                MinY = 0,
                MaxY = 0,
                MinZ = minDepth,
                MaxZ = maxDepth
            };

            return data;
        }

        private static IEnumerable<ReservoirSurfaceData> ResolveSurfaces(ReservoirData reservoir, IEnumerable<ReservoirSurfaceData> surfaces)
        {
            var explicitSurfaces = (surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null && surface.Points.Count >= 3)
                .ToList();

            if (explicitSurfaces.Count > 0)
                return explicitSurfaces;

            var preferredSurfaces = (reservoir?.Surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null && surface.Points.Count >= 3)
                .Where(surface => surface.SurfaceKind != ReservoirSurfaceKind.Fault)
                .ToList();

            if (preferredSurfaces.Count > 0)
                return preferredSurfaces;

            return (reservoir?.Surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null && surface.Points.Count >= 3)
                .ToList();
        }

        private static ReservoirSectionProfile ExtractProfile(
            ReservoirSurfaceData surface,
            ReservoirSectionLine sectionLine,
            ReservoirCrossSectionConfiguration configuration)
        {
            var profile = new ReservoirSectionProfile
            {
                SurfaceId = surface.SurfaceId,
                SurfaceName = surface.SurfaceName,
                SurfaceKind = surface.SurfaceKind,
                ValueUnit = surface.ValueUnit
            };

            int sampleCount = Math.Max(2, configuration.SampleCount);
            for (int index = 0; index < sampleCount; index++)
            {
                double t = sampleCount == 1 ? 0 : (double)index / (sampleCount - 1);
                double x = sectionLine.Start.X + ((sectionLine.End.X - sectionLine.Start.X) * t);
                double y = sectionLine.Start.Y + ((sectionLine.End.Y - sectionLine.Start.Y) * t);
                double depth = ReservoirSurfaceInterpolation.InterpolateZ(surface.Points, x, y, configuration.MaxInfluencePoints, configuration.InterpolationPower);

                profile.Samples.Add(new ReservoirSectionSample
                {
                    Distance = sectionLine.Length * t,
                    Depth = depth
                });
            }

            return profile;
        }

        private static IEnumerable<ReservoirWellSectionMarker> ExtractWellMarkers(
            IEnumerable<ReservoirWellMapPoint> wells,
            ReservoirSectionLine sectionLine,
            ReservoirCrossSectionConfiguration configuration,
            double fallbackDepth)
        {
            if (wells == null)
                yield break;

            foreach (var well in wells)
            {
                var projection = ResolveBestProjection(well, sectionLine);
                if (projection == null || projection.Offset > configuration.MaximumWellOffsetFromSection)
                    continue;

                yield return new ReservoirWellSectionMarker
                {
                    WellId = well.WellId,
                    WellName = well.WellName,
                    Uwi = well.Uwi,
                    DistanceAlongSection = projection.DistanceAlongSection,
                    Depth = projection.Depth,
                    OffsetFromSection = projection.Offset,
                    ColorCode = well.ColorCode
                };
            }
        }

        private static WellProjection ResolveBestProjection(ReservoirWellMapPoint well, ReservoirSectionLine sectionLine)
        {
            if (well == null)
                return null;

            var candidates = new List<Point3D>();
            if (well.SurfaceLocation != null)
            {
                candidates.Add(well.SurfaceLocation);
            }

            if (well.TrajectoryPoints != null)
            {
                candidates.AddRange(well.TrajectoryPoints.Where(point => point != null));
            }

            if (candidates.Count == 0)
                return null;

            var projections = candidates
                .Where(point => double.IsFinite(point.X) && double.IsFinite(point.Y))
                .Select(point => ProjectPoint(sectionLine, point))
                .OrderBy(projection => projection.Offset)
                .ToList();

            return projections.FirstOrDefault();
        }

        private static WellProjection ProjectPoint(ReservoirSectionLine sectionLine, Point3D point)
        {
            double dx = sectionLine.End.X - sectionLine.Start.X;
            double dy = sectionLine.End.Y - sectionLine.Start.Y;
            double lengthSquared = (dx * dx) + (dy * dy);
            double projection = (((point.X - sectionLine.Start.X) * dx) + ((point.Y - sectionLine.Start.Y) * dy)) / lengthSquared;
            projection = Math.Max(0, Math.Min(1, projection));

            double projectedX = sectionLine.Start.X + (dx * projection);
            double projectedY = sectionLine.Start.Y + (dy * projection);
            double offsetX = point.X - projectedX;
            double offsetY = point.Y - projectedY;

            return new WellProjection
            {
                DistanceAlongSection = sectionLine.Length * projection,
                Offset = Math.Sqrt((offsetX * offsetX) + (offsetY * offsetY)),
                Depth = double.IsFinite(point.Z) ? point.Z : 0
            };
        }

        private static IEnumerable<double> EnumerateFluidContactDepths(FluidContacts fluidContacts)
        {
            if (fluidContacts == null)
                yield break;

            if (fluidContacts.FreeWaterLevel.HasValue)
                yield return fluidContacts.FreeWaterLevel.Value;

            if (fluidContacts.OilWaterContact.HasValue)
                yield return fluidContacts.OilWaterContact.Value;

            if (fluidContacts.GasOilContact.HasValue)
                yield return fluidContacts.GasOilContact.Value;

            if (fluidContacts.GasWaterContact.HasValue)
                yield return fluidContacts.GasWaterContact.Value;
        }

        private sealed class WellProjection
        {
            public double DistanceAlongSection { get; set; }

            public double Offset { get; set; }

            public double Depth { get; set; }
        }
    }
}