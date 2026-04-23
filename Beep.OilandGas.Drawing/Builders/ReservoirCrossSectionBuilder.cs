using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for reservoir cross-sections extracted from typed reservoir surfaces.
    /// </summary>
    public sealed class ReservoirCrossSectionBuilder
    {
        private ReservoirData reservoirData;
        private IReservoirLoader reservoirLoader;
        private ReservoirLoadConfiguration loadConfiguration;
        private string reservoirIdentifier;
        private string dataSource;
        private DataLoaderType? loaderType;
        private ReservoirSectionLine sectionLine;
        private readonly List<ReservoirSurfaceData> surfaces = new List<ReservoirSurfaceData>();
        private readonly List<ReservoirWellMapPoint> wells = new List<ReservoirWellMapPoint>();
        private ReservoirCrossSectionConfiguration configuration = new ReservoirCrossSectionConfiguration();
        private float zoom = 1.0f;
        private int width = 1800;
        private int height = 1200;

        /// <summary>
        /// Sets the reservoir data directly.
        /// </summary>
        public ReservoirCrossSectionBuilder WithReservoirData(ReservoirData data)
        {
            reservoirData = data;
            return this;
        }

        /// <summary>
        /// Sets a reservoir loader instance.
        /// </summary>
        public ReservoirCrossSectionBuilder WithLoader(IReservoirLoader loader, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
        {
            reservoirLoader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.reservoirIdentifier = reservoirIdentifier;
            loadConfiguration = configuration;
            dataSource = null;
            loaderType = null;
            return this;
        }

        /// <summary>
        /// Sets a data source for loading a reservoir through the factory.
        /// </summary>
        public ReservoirCrossSectionBuilder WithDataSource(string dataSource, DataLoaderType loaderType, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.loaderType = loaderType;
            this.reservoirIdentifier = reservoirIdentifier;
            loadConfiguration = configuration;
            reservoirLoader = null;
            return this;
        }

        /// <summary>
        /// Sets the section line.
        /// </summary>
        public ReservoirCrossSectionBuilder WithSectionLine(ReservoirSectionLine sectionLine)
        {
            this.sectionLine = sectionLine;
            return this;
        }

        /// <summary>
        /// Replaces the surface collection used during extraction.
        /// </summary>
        public ReservoirCrossSectionBuilder WithSurfaces(IEnumerable<ReservoirSurfaceData> surfaces)
        {
            this.surfaces.Clear();
            if (surfaces != null)
            {
                this.surfaces.AddRange(surfaces);
            }

            return this;
        }

        /// <summary>
        /// Adds a single surface used during extraction.
        /// </summary>
        public ReservoirCrossSectionBuilder AddSurface(ReservoirSurfaceData surface)
        {
            if (surface != null)
            {
                surfaces.Add(surface);
            }

            return this;
        }

        /// <summary>
        /// Replaces the well overlay collection.
        /// </summary>
        public ReservoirCrossSectionBuilder WithWells(IEnumerable<ReservoirWellMapPoint> wells)
        {
            this.wells.Clear();
            if (wells != null)
            {
                this.wells.AddRange(wells);
            }

            return this;
        }

        /// <summary>
        /// Adds a well used during section projection.
        /// </summary>
        public ReservoirCrossSectionBuilder AddWell(ReservoirWellMapPoint well)
        {
            if (well != null)
            {
                wells.Add(well);
            }

            return this;
        }

        /// <summary>
        /// Sets the cross-section configuration.
        /// </summary>
        public ReservoirCrossSectionBuilder WithConfiguration(ReservoirCrossSectionConfiguration configuration)
        {
            this.configuration = configuration ?? new ReservoirCrossSectionConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the zoom multiplier applied after scene fit.
        /// </summary>
        public ReservoirCrossSectionBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public ReservoirCrossSectionBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the reservoir cross-section drawing engine.
        /// </summary>
        public DrawingEngine Build()
        {
            var reservoir = ReservoirVisualizationResolver.ResolveReservoirData(
                ref reservoirData,
                reservoirLoader,
                dataSource,
                loaderType,
                reservoirIdentifier,
                loadConfiguration);

            var resolvedSectionLine = sectionLine ?? CreateDefaultSectionLine(reservoir);
            var sectionData = ReservoirCrossSectionExtractor.Extract(
                reservoir,
                resolvedSectionLine,
                configuration,
                surfaces.Count > 0 ? surfaces : null,
                wells);

            var engine = new DrawingEngine(width, height)
            {
                BackgroundColor = configuration.BackgroundColor
            };

            var scene = DrawingScene.CreateSectionScene(
                resolvedSectionLine.SectionName ?? "Reservoir Cross Section",
                ResolveDistanceUnitCode(reservoir),
                ResolveDepthUnitCode(reservoir));

            scene.SourceSystem = reservoir?.ReservoirName ?? reservoir?.ReservoirId ?? "Reservoir";
            scene.WorldBounds = CreateWorldBounds(sectionData.Bounds);
            if (resolvedSectionLine.Start != null && resolvedSectionLine.End != null)
            {
                scene.SetMetadata("SectionStart", $"{resolvedSectionLine.Start.X:0.##},{resolvedSectionLine.Start.Y:0.##}");
                scene.SetMetadata("SectionEnd", $"{resolvedSectionLine.End.X:0.##},{resolvedSectionLine.End.Y:0.##}");
            }

            engine.UseScene(scene);
            engine.AddLayer(new ReservoirCrossSectionLayer(sectionData, configuration)
            {
                Name = "Reservoir Cross Section",
                IsVisible = true,
                ZOrder = 0
            });

            engine.SetZoom(engine.Viewport.Zoom * zoom);
            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static ReservoirCrossSectionBuilder Create()
        {
            return new ReservoirCrossSectionBuilder();
        }

        private static ReservoirSectionLine CreateDefaultSectionLine(ReservoirData reservoir)
        {
            var bounds = reservoir?.BoundingBox;
            if (bounds == null || bounds.MaxX <= bounds.MinX || bounds.MaxY <= bounds.MinY)
            {
                bounds = BuildBoundsFromSurfaces(reservoir?.Surfaces);
            }

            if (bounds == null || bounds.MaxX <= bounds.MinX || bounds.MaxY <= bounds.MinY)
                throw new InvalidOperationException("A section line must be supplied when the reservoir does not provide usable map bounds.");

            double midY = (bounds.MinY + bounds.MaxY) * 0.5;
            return new ReservoirSectionLine
            {
                SectionName = "Centerline Section",
                Start = new Point3D { X = bounds.MinX, Y = midY, Z = 0 },
                End = new Point3D { X = bounds.MaxX, Y = midY, Z = 0 }
            };
        }

        private static BoundingBox BuildBoundsFromSurfaces(IEnumerable<ReservoirSurfaceData> surfaces)
        {
            var points = (surfaces ?? Enumerable.Empty<ReservoirSurfaceData>())
                .Where(surface => surface?.Points != null)
                .SelectMany(surface => surface.Points)
                .Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y))
                .ToList();

            if (points.Count == 0)
                return null;

            return new BoundingBox
            {
                MinX = points.Min(point => point.X),
                MaxX = points.Max(point => point.X),
                MinY = points.Min(point => point.Y),
                MaxY = points.Max(point => point.Y),
                MinZ = points.Min(point => point.Z),
                MaxZ = points.Max(point => point.Z)
            };
        }

        private static string ResolveDistanceUnitCode(ReservoirData reservoir)
        {
            return reservoir?.CoordinateReferenceSystem?.Axes
                .FirstOrDefault(axis => axis.Unit.Dimension == MeasurementDimension.Length)?.Unit.Code
                ?? "m";
        }

        private static string ResolveDepthUnitCode(ReservoirData reservoir)
        {
            return reservoir?.CoordinateReferenceSystem?.Axes
                .FirstOrDefault(axis => axis.Kind == CoordinateAxisKind.Depth || axis.Kind == CoordinateAxisKind.MeasuredDepth)?.Unit.Code
                ?? ResolveDistanceUnitCode(reservoir);
        }

        private static SKRect CreateWorldBounds(BoundingBox bounds)
        {
            var depthRange = Math.Max(1.0, bounds.MaxZ - bounds.MinZ);
            var distanceRange = Math.Max(1.0, bounds.MaxX - bounds.MinX);
            var depthPadding = depthRange * 0.05;
            var distancePadding = distanceRange * 0.02;

            return new SKRect(
                (float)(bounds.MinX - distancePadding),
                (float)(bounds.MinZ - depthPadding),
                (float)(bounds.MaxX + distancePadding),
                (float)(bounds.MaxZ + depthPadding));
        }
    }
}