using System;
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
    /// Builder for creating contour-map visualizations from reservoir surfaces.
    /// </summary>
    public sealed class ReservoirContourBuilder
    {
        private ReservoirData reservoirData;
        private ReservoirSurfaceData surfaceData;
        private IReservoirLoader reservoirLoader;
        private ReservoirLoadConfiguration loadConfiguration;
        private string reservoirIdentifier;
        private string dataSource;
        private DataLoaderType? loaderType;
        private string surfaceIdentifier;
        private ReservoirContourConfiguration configuration = new ReservoirContourConfiguration();
        private float zoom = 1.0f;
        private int width = 1800;
        private int height = 1200;

        /// <summary>
        /// Sets the reservoir data directly.
        /// </summary>
        public ReservoirContourBuilder WithReservoirData(ReservoirData data)
        {
            reservoirData = data;
            surfaceData = null;
            return this;
        }

        /// <summary>
        /// Sets the surface data directly.
        /// </summary>
        public ReservoirContourBuilder WithSurface(ReservoirSurfaceData surface)
        {
            surfaceData = surface;
            return this;
        }

        /// <summary>
        /// Selects a specific surface by identifier or name when using reservoir data.
        /// </summary>
        public ReservoirContourBuilder SelectSurface(string identifierOrName)
        {
            surfaceIdentifier = identifierOrName;
            return this;
        }

        /// <summary>
        /// Sets a reservoir loader instance.
        /// </summary>
        public ReservoirContourBuilder WithLoader( IReservoirLoader loader, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
        {
            reservoirLoader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.reservoirIdentifier = reservoirIdentifier;
            loadConfiguration = configuration;
            dataSource = null;
            loaderType = null;
            return this;
        }

        /// <summary>
        /// Sets a data source description for loading reservoir data through the loader factory.
        /// </summary>
        public ReservoirContourBuilder WithDataSource(string dataSource, DataLoaderType loaderType, string reservoirIdentifier = null, ReservoirLoadConfiguration configuration = null)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.loaderType = loaderType;
            this.reservoirIdentifier = reservoirIdentifier;
            loadConfiguration = configuration;
            reservoirLoader = null;
            return this;
        }

        /// <summary>
        /// Sets the contour rendering configuration.
        /// </summary>
        public ReservoirContourBuilder WithConfiguration(ReservoirContourConfiguration configuration)
        {
            this.configuration = configuration ?? new ReservoirContourConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the zoom multiplier applied after zoom-to-fit.
        /// </summary>
        public ReservoirContourBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public ReservoirContourBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the drawing engine with a contour layer.
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

            var surface = ReservoirVisualizationResolver.ResolveSurface(reservoir, surfaceData, surfaceIdentifier);
            if (surface == null)
                throw new InvalidOperationException("A contour-capable reservoir surface must be set or loadable before building.");

            var engine = new DrawingEngine(width, height);
            engine.UseScene(CreateScene(reservoir, surface));
            engine.AddLayer(new ReservoirContourLayer(surface, configuration)
            {
                Name = "Reservoir Contours",
                IsVisible = true,
                ZOrder = 0
            });

            engine.ZoomToFit();
            engine.SetZoom(engine.Viewport.Zoom * zoom);
            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static ReservoirContourBuilder Create()
        {
            return new ReservoirContourBuilder();
        }

        private static DrawingScene CreateScene(ReservoirData reservoir, ReservoirSurfaceData surface)
        {
            var bounds = ResolveBounds(reservoir, surface);
            if (bounds == null)
                throw new InvalidOperationException("A reservoir contour scene requires a surface with valid map-space geometry.");

            var sceneName = surface.SurfaceName ?? reservoir?.ReservoirName ?? "Reservoir Contours";
            var coordinateReferenceSystem = reservoir?.CoordinateReferenceSystem
                ?? CoordinateReferenceSystem.CreateProjected("LOCAL:RESERVOIR-MAP", "Local Reservoir Map", "m", CoordinateAuthority.Custom);

            var scene = DrawingScene.CreateMapScene(sceneName, coordinateReferenceSystem);
            scene.SourceSystem = reservoir?.ReservoirId ?? surface.SurfaceId ?? sceneName;
            scene.WorldBounds = CreateWorldBounds(bounds);
            scene.SetMetadata("SurfaceId", surface.SurfaceId);
            scene.SetMetadata("SurfaceKind", surface.SurfaceKind.ToString());
            scene.SetMetadata("PointCount", (surface.Points?.Count ?? 0).ToString());
            return scene;
        }

        private static BoundingBox ResolveBounds(ReservoirData reservoir, ReservoirSurfaceData surface)
        {
            if (surface?.BoundingBox != null && surface.BoundingBox.MaxX > surface.BoundingBox.MinX && surface.BoundingBox.MaxY > surface.BoundingBox.MinY)
                return surface.BoundingBox;

            var points = (surface?.Points ?? new System.Collections.Generic.List<Point3D>())
                .Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z))
                .ToList();

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