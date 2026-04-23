using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.FieldMap;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for plan-view field maps covering fields, pools, leases, wells, facilities, and surface networks.
    /// </summary>
    public sealed class FieldMapBuilder
    {
        private FieldMapData fieldMapData;
        private readonly List<FieldMapAreaAsset> areaAssets = new List<FieldMapAreaAsset>();
        private readonly List<FieldMapPointAsset> pointAssets = new List<FieldMapPointAsset>();
        private readonly List<FieldMapConnectionAsset> connectionAssets = new List<FieldMapConnectionAsset>();
        private FieldMapConfiguration configuration = new FieldMapConfiguration();
        private CoordinateReferenceSystem coordinateReferenceSystem;
        private float zoom = 1.0f;
        private int width = 1800;
        private int height = 1200;

        /// <summary>
        /// Sets the field map data directly.
        /// </summary>
        public FieldMapBuilder WithFieldMapData(FieldMapData data)
        {
            fieldMapData = data;
            return this;
        }

        /// <summary>
        /// Sets an explicit coordinate reference system.
        /// </summary>
        public FieldMapBuilder WithCoordinateReferenceSystem(CoordinateReferenceSystem coordinateReferenceSystem)
        {
            this.coordinateReferenceSystem = coordinateReferenceSystem;
            return this;
        }

        /// <summary>
        /// Adds a polygon or boundary asset.
        /// </summary>
        public FieldMapBuilder AddAreaAsset(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a point or path asset.
        /// </summary>
        public FieldMapBuilder AddPointAsset(FieldMapPointAsset asset)
        {
            if (asset != null)
            {
                pointAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds an explicit surface connection asset.
        /// </summary>
        public FieldMapBuilder AddConnection(FieldMapConnectionAsset asset)
        {
            if (asset != null)
            {
                connectionAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a field boundary.
        /// </summary>
        public FieldMapBuilder AddField(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Field;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a pool boundary.
        /// </summary>
        public FieldMapBuilder AddPool(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Pool;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a lease boundary.
        /// </summary>
        public FieldMapBuilder AddLease(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Lease;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a land-right overlay.
        /// </summary>
        public FieldMapBuilder AddLandRight(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.LandRight;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a protected-area overlay.
        /// </summary>
        public FieldMapBuilder AddProtectedArea(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.ProtectedArea;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds an HSE hazard or exclusion overlay.
        /// </summary>
        public FieldMapBuilder AddHazardZone(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.HazardZone;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a facility marker.
        /// </summary>
        public FieldMapBuilder AddFacility(FieldMapPointAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Facility;
                pointAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a well marker or trajectory.
        /// </summary>
        public FieldMapBuilder AddWell(FieldMapPointAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Well;
                pointAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a surface-system point asset.
        /// </summary>
        public FieldMapBuilder AddSurfaceSystem(FieldMapPointAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.SurfaceSystem;
                pointAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a 3D seismic survey footprint.
        /// </summary>
        public FieldMapBuilder AddSeismicFootprint(FieldMapAreaAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Seismic3D;
                areaAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a 2D seismic line or swath.
        /// </summary>
        public FieldMapBuilder AddSeismicLine(FieldMapPointAsset asset)
        {
            if (asset != null)
            {
                asset.AssetKind = FieldMapAssetKind.Seismic2D;
                pointAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a flowline connection.
        /// </summary>
        public FieldMapBuilder AddFlowline(FieldMapConnectionAsset asset)
        {
            if (asset != null)
            {
                asset.ConnectionKind = FieldMapConnectionKind.Flowline;
                connectionAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Adds a gathering-line connection.
        /// </summary>
        public FieldMapBuilder AddGatheringLine(FieldMapConnectionAsset asset)
        {
            if (asset != null)
            {
                asset.ConnectionKind = FieldMapConnectionKind.GatheringLine;
                connectionAssets.Add(asset);
            }

            return this;
        }

        /// <summary>
        /// Sets the field map configuration.
        /// </summary>
        public FieldMapBuilder WithConfiguration(FieldMapConfiguration configuration)
        {
            this.configuration = configuration ?? new FieldMapConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the zoom multiplier applied after zoom-to-fit.
        /// </summary>
        public FieldMapBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public FieldMapBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the field map drawing engine.
        /// </summary>
        public DrawingEngine Build()
        {
            var data = ResolveFieldMapData();
            var bounds = ResolveBounds(data);
            if (bounds == null)
                throw new InvalidOperationException("At least one valid field map asset with map-space geometry is required before building.");

            var crs = coordinateReferenceSystem
                ?? data.CoordinateReferenceSystem
                ?? CoordinateReferenceSystem.CreateProjected("LOCAL:FIELD-MAP", "Local Field Map", "m", CoordinateAuthority.Custom);

            var scene = DrawingScene.CreateMapScene(data.MapName ?? "Field Map", crs);
            scene.SourceSystem = data.MapName ?? "Field Map";
            scene.WorldBounds = CreateWorldBounds(bounds);
            scene.SetMetadata("AreaAssetCount", data.AreaAssets.Count.ToString());
            scene.SetMetadata("PointAssetCount", data.PointAssets.Count.ToString());
            scene.SetMetadata("ConnectionAssetCount", data.ConnectionAssets.Count.ToString());

            var engine = new DrawingEngine(width, height)
            {
                BackgroundColor = configuration.BackgroundColor
            };

            engine.UseScene(scene);
            engine.AddLayer(new FieldMapLayer(data, configuration)
            {
                Name = "Field Map",
                IsVisible = true,
                ZOrder = 0
            });

            engine.SetZoom(engine.Viewport.Zoom * zoom);
            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static FieldMapBuilder Create()
        {
            return new FieldMapBuilder();
        }

        private FieldMapData ResolveFieldMapData()
        {
            var data = fieldMapData ?? new FieldMapData();

            var resolved = new FieldMapData
            {
                MapName = data.MapName,
                CoordinateReferenceSystem = data.CoordinateReferenceSystem,
                BoundingBox = data.BoundingBox,
                Metadata = new Dictionary<string, string>(data.Metadata)
            };

            resolved.AreaAssets.AddRange(data.AreaAssets ?? Enumerable.Empty<FieldMapAreaAsset>());
            resolved.PointAssets.AddRange(data.PointAssets ?? Enumerable.Empty<FieldMapPointAsset>());
            resolved.ConnectionAssets.AddRange(data.ConnectionAssets ?? Enumerable.Empty<FieldMapConnectionAsset>());
            resolved.AreaAssets.AddRange(areaAssets);
            resolved.PointAssets.AddRange(pointAssets);
            resolved.ConnectionAssets.AddRange(connectionAssets);
            return resolved;
        }

        private static BoundingBox ResolveBounds(FieldMapData data)
        {
            if (data.BoundingBox != null && data.BoundingBox.MaxX > data.BoundingBox.MinX && data.BoundingBox.MaxY > data.BoundingBox.MinY)
                return data.BoundingBox;

            var points = new List<Point3D>();
            points.AddRange((data.AreaAssets ?? Enumerable.Empty<FieldMapAreaAsset>()).SelectMany(asset => asset?.BoundaryPoints ?? Enumerable.Empty<Point3D>()));
            points.AddRange((data.PointAssets ?? Enumerable.Empty<FieldMapPointAsset>()).SelectMany(asset => EnumeratePointAssetGeometry(asset)));
            points.AddRange((data.ConnectionAssets ?? Enumerable.Empty<FieldMapConnectionAsset>()).SelectMany(asset => asset?.Vertices ?? Enumerable.Empty<Point3D>()));

            var validPoints = points
                .Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y))
                .ToList();

            if (validPoints.Count == 0)
                return null;

            return new BoundingBox
            {
                MinX = validPoints.Min(point => point.X),
                MaxX = validPoints.Max(point => point.X),
                MinY = validPoints.Min(point => point.Y),
                MaxY = validPoints.Max(point => point.Y),
                MinZ = validPoints.Min(point => point.Z),
                MaxZ = validPoints.Max(point => point.Z)
            };
        }

        private static IEnumerable<Point3D> EnumeratePointAssetGeometry(FieldMapPointAsset asset)
        {
            if (asset?.Location != null)
                yield return asset.Location;

            if (asset?.PathPoints == null)
                yield break;

            foreach (var point in asset.PathPoints)
            {
                if (point != null)
                    yield return point;
            }
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