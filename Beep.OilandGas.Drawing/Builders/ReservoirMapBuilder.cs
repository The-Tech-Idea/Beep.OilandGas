using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for reservoir map scenes that combine contours with well and fault overlays.
    /// </summary>
    public sealed class ReservoirMapBuilder
    {
        private ReservoirData reservoirData;
        private ReservoirSurfaceData surfaceData;
        private IReservoirLoader reservoirLoader;
        private ReservoirLoadConfiguration loadConfiguration;
        private string reservoirIdentifier;
        private string dataSource;
        private DataLoaderType? loaderType;
        private string surfaceIdentifier;
        private readonly List<ReservoirWellMapPoint> wells = new List<ReservoirWellMapPoint>();
        private readonly List<ReservoirSurfaceData> explicitFaultSurfaces = new List<ReservoirSurfaceData>();
        private ReservoirMapConfiguration configuration = new ReservoirMapConfiguration();
        private float zoom = 1.0f;
        private int width = 1800;
        private int height = 1200;
        private bool useExplicitFaultSurfaces;

        /// <summary>
        /// Sets the reservoir data directly.
        /// </summary>
        public ReservoirMapBuilder WithReservoirData(ReservoirData data)
        {
            reservoirData = data;
            surfaceData = null;
            return this;
        }

        /// <summary>
        /// Sets the contour surface directly.
        /// </summary>
        public ReservoirMapBuilder WithSurface(ReservoirSurfaceData surface)
        {
            surfaceData = surface;
            return this;
        }

        /// <summary>
        /// Selects a specific contour surface by identifier or name.
        /// </summary>
        public ReservoirMapBuilder SelectSurface(string identifierOrName)
        {
            surfaceIdentifier = identifierOrName;
            return this;
        }

        /// <summary>
        /// Sets a reservoir loader instance.
        /// </summary>
        public ReservoirMapBuilder WithLoader(IReservoirLoader loader, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
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
        public ReservoirMapBuilder WithDataSource(string dataSource, DataLoaderType loaderType, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.loaderType = loaderType;
            this.reservoirIdentifier = reservoirIdentifier;
            loadConfiguration = configuration;
            reservoirLoader = null;
            return this;
        }

        /// <summary>
        /// Adds a well overlay point.
        /// </summary>
        public ReservoirMapBuilder AddWell(ReservoirWellMapPoint well)
        {
            if (well != null)
            {
                wells.Add(well);
            }

            return this;
        }

        /// <summary>
        /// Replaces the well overlay collection.
        /// </summary>
        public ReservoirMapBuilder WithWells(IEnumerable<ReservoirWellMapPoint> wells)
        {
            this.wells.Clear();
            if (wells != null)
            {
                this.wells.AddRange(wells);
            }

            return this;
        }

        /// <summary>
        /// Adds an explicit fault overlay surface.
        /// </summary>
        public ReservoirMapBuilder AddFaultSurface(ReservoirSurfaceData faultSurface)
        {
            if (faultSurface != null)
            {
                explicitFaultSurfaces.Add(faultSurface);
                useExplicitFaultSurfaces = true;
            }

            return this;
        }

        /// <summary>
        /// Replaces the explicit fault overlay surfaces.
        /// </summary>
        public ReservoirMapBuilder WithFaultSurfaces(IEnumerable<ReservoirSurfaceData> faultSurfaces)
        {
            explicitFaultSurfaces.Clear();
            useExplicitFaultSurfaces = true;
            if (faultSurfaces != null)
            {
                explicitFaultSurfaces.AddRange(faultSurfaces);
            }

            return this;
        }

        /// <summary>
        /// Sets the map configuration.
        /// </summary>
        public ReservoirMapBuilder WithConfiguration(ReservoirMapConfiguration configuration)
        {
            this.configuration = configuration ?? new ReservoirMapConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the zoom multiplier applied after zoom-to-fit.
        /// </summary>
        public ReservoirMapBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public ReservoirMapBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the reservoir map drawing engine.
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

            var baseSurface = ReservoirVisualizationResolver.ResolveSurface(reservoir, surfaceData, surfaceIdentifier);
            var faultSurfaces = ReservoirVisualizationResolver.ResolveFaultSurfaces(
                reservoir,
                baseSurface,
                useExplicitFaultSurfaces ? explicitFaultSurfaces : null);

            var engine = new DrawingEngine(width, height);
            engine.UseScene(CreateScene(reservoir, baseSurface, faultSurfaces, wells));

            if (configuration.ShowContours)
            {
                if (baseSurface == null)
                    throw new InvalidOperationException("A contour-capable reservoir surface must be set or loadable before building a reservoir map.");

                engine.AddLayer(new ReservoirContourLayer(baseSurface, configuration.ContourConfiguration)
                {
                    Name = "Reservoir Map Contours",
                    IsVisible = true,
                    ZOrder = 0
                });
            }

            if ((configuration.ShowFaults && faultSurfaces.Count > 0) || (configuration.ShowWells && wells.Count > 0))
            {
                engine.AddLayer(new ReservoirMapOverlayLayer(faultSurfaces, wells, configuration)
                {
                    Name = "Reservoir Map Overlays",
                    IsVisible = true,
                    ZOrder = 10
                });
            }

            if (engine.Layers.Count == 0)
                throw new InvalidOperationException("The reservoir map configuration produced no visible layers to render.");

            engine.ZoomToFit();
            engine.SetZoom(engine.Viewport.Zoom * zoom);
            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static ReservoirMapBuilder Create()
        {
            return new ReservoirMapBuilder();
        }

        private static DrawingScene CreateScene(
            ReservoirData reservoir,
            ReservoirSurfaceData baseSurface,
            IReadOnlyCollection<ReservoirSurfaceData> faultSurfaces,
            IReadOnlyCollection<ReservoirWellMapPoint> wells)
        {
            var bounds = ResolveBounds(reservoir, baseSurface, faultSurfaces, wells);
            if (bounds == null)
                throw new InvalidOperationException("A reservoir map scene requires at least one valid map-space geometry source.");

            var sceneName = baseSurface?.SurfaceName ?? reservoir?.ReservoirName ?? "Reservoir Map";
            var coordinateReferenceSystem = reservoir?.CoordinateReferenceSystem
                ?? CoordinateReferenceSystem.CreateProjected("LOCAL:RESERVOIR-MAP", "Local Reservoir Map", "m", CoordinateAuthority.Custom);

            var scene = DrawingScene.CreateMapScene(sceneName, coordinateReferenceSystem);
            scene.SourceSystem = reservoir?.ReservoirId ?? baseSurface?.SurfaceId ?? sceneName;
            scene.WorldBounds = CreateWorldBounds(bounds);
            scene.SetMetadata("SurfaceId", baseSurface?.SurfaceId);
            scene.SetMetadata("SurfaceKind", baseSurface?.SurfaceKind.ToString());
            scene.SetMetadata("FaultSurfaceCount", faultSurfaces.Count.ToString());
            scene.SetMetadata("WellOverlayCount", wells.Count.ToString());
            return scene;
        }

        private static BoundingBox ResolveBounds(
            ReservoirData reservoir,
            ReservoirSurfaceData baseSurface,
            IReadOnlyCollection<ReservoirSurfaceData> faultSurfaces,
            IReadOnlyCollection<ReservoirWellMapPoint> wells)
        {
            if (baseSurface?.BoundingBox != null && baseSurface.BoundingBox.MaxX > baseSurface.BoundingBox.MinX && baseSurface.BoundingBox.MaxY > baseSurface.BoundingBox.MinY)
                return baseSurface.BoundingBox;

            var points = new List<Point3D>();
            points.AddRange((baseSurface?.Points ?? Enumerable.Empty<Point3D>()).Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z)));

            foreach (var faultSurface in faultSurfaces)
                points.AddRange((faultSurface?.Points ?? Enumerable.Empty<Point3D>()).Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z)));

            foreach (var well in wells)
            {
                if (well?.SurfaceLocation != null && double.IsFinite(well.SurfaceLocation.X) && double.IsFinite(well.SurfaceLocation.Y) && double.IsFinite(well.SurfaceLocation.Z))
                    points.Add(well.SurfaceLocation);

                points.AddRange((well?.TrajectoryPoints ?? Enumerable.Empty<Point3D>()).Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z)));
            }

            if (points.Count > 0)
            {
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

            return reservoir?.BoundingBox;
        }

        private static SKRect CreateWorldBounds(BoundingBox bounds)
        {
            var width = Math.Max(1.0, bounds.MaxX - bounds.MinX);
            var height = Math.Max(1.0, bounds.MaxY - bounds.MinY);
            var paddingX = width * 0.05;
            var paddingY = height * 0.05;
            return new SKRect(
                (float)(bounds.MinX - paddingX),
                (float)(bounds.MinY - paddingY),
                (float)(bounds.MaxX + paddingX),
                (float)(bounds.MaxY + paddingY));
        }
    }
}