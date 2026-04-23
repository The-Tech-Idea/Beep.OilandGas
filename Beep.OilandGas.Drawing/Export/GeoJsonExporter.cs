using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;

namespace Beep.OilandGas.Drawing.Export
{
    /// <summary>
    /// Exports typed map and reservoir geometries to GeoJSON feature collections.
    /// </summary>
    public static class GeoJsonExporter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Exports field-map geometry to a GeoJSON file.
        /// </summary>
        public static void ExportFieldMap(FieldMapData fieldMapData, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A target file path is required.", nameof(filePath));

            File.WriteAllText(filePath, ExportFieldMapToString(fieldMapData));
        }

        /// <summary>
        /// Exports field-map geometry to a GeoJSON string.
        /// </summary>
        public static string ExportFieldMapToString(FieldMapData fieldMapData)
        {
            if (fieldMapData == null)
                throw new ArgumentNullException(nameof(fieldMapData));

            var features = new List<object>();

            foreach (var asset in fieldMapData.AreaAssets ?? Enumerable.Empty<FieldMapAreaAsset>())
            {
                var geometry = CreateAreaGeometry(asset);
                if (geometry == null)
                    continue;

                features.Add(CreateFeature(
                    asset.AssetId,
                    geometry,
                    CreateProperties(
                        ("featureType", "area-asset"),
                        ("assetId", asset.AssetId),
                        ("assetName", asset.AssetName),
                        ("assetKind", asset.AssetKind.ToString()),
                        ("fillColorCode", asset.FillColorCode),
                        ("strokeColorCode", asset.StrokeColorCode),
                        ("metadata", asset.Metadata))));
            }

            foreach (var asset in fieldMapData.PointAssets ?? Enumerable.Empty<FieldMapPointAsset>())
            {
                var geometry = CreatePointAssetGeometry(asset);
                if (geometry == null)
                    continue;

                features.Add(CreateFeature(
                    asset.AssetId,
                    geometry,
                    CreateProperties(
                        ("featureType", "point-asset"),
                        ("assetId", asset.AssetId),
                        ("assetName", asset.AssetName),
                        ("assetKind", asset.AssetKind.ToString()),
                        ("status", asset.Status),
                        ("colorCode", asset.ColorCode),
                        ("metadata", asset.Metadata))));
            }

            foreach (var asset in fieldMapData.ConnectionAssets ?? Enumerable.Empty<FieldMapConnectionAsset>())
            {
                var geometry = CreateConnectionGeometry(asset);
                if (geometry == null)
                    continue;

                features.Add(CreateFeature(
                    asset.ConnectionId,
                    geometry,
                    CreateProperties(
                        ("featureType", "connection-asset"),
                        ("connectionId", asset.ConnectionId),
                        ("connectionName", asset.ConnectionName),
                        ("connectionKind", asset.ConnectionKind.ToString()),
                        ("fromAssetId", asset.FromAssetId),
                        ("toAssetId", asset.ToAssetId),
                        ("colorCode", asset.ColorCode),
                        ("metadata", asset.Metadata))));
            }

            return Serialize(CreateFeatureCollection(
                features,
                fieldMapData.CoordinateReferenceSystem,
                fieldMapData.BoundingBox,
                CreateProperties(
                    ("name", fieldMapData.MapName),
                    ("metadata", fieldMapData.Metadata))));
        }

        /// <summary>
        /// Exports reservoir contours, faults, and wells to a GeoJSON file.
        /// </summary>
        public static void ExportReservoirMap(
            ReservoirSurfaceData contourSurface,
            string filePath,
            CoordinateReferenceSystem coordinateReferenceSystem = null,
            IEnumerable<ReservoirSurfaceData> faultSurfaces = null,
            IEnumerable<ReservoirWellMapPoint> wells = null,
            ReservoirContourConfiguration contourConfiguration = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A target file path is required.", nameof(filePath));

            File.WriteAllText(filePath, ExportReservoirMapToString(contourSurface, coordinateReferenceSystem, faultSurfaces, wells, contourConfiguration));
        }

        /// <summary>
        /// Exports reservoir contours, faults, and wells to a GeoJSON string.
        /// </summary>
        public static string ExportReservoirMapToString(
            ReservoirSurfaceData contourSurface,
            CoordinateReferenceSystem coordinateReferenceSystem = null,
            IEnumerable<ReservoirSurfaceData> faultSurfaces = null,
            IEnumerable<ReservoirWellMapPoint> wells = null,
            ReservoirContourConfiguration contourConfiguration = null)
        {
            var normalizedFaultSurfaces = (faultSurfaces ?? Enumerable.Empty<ReservoirSurfaceData>()).Where(surface => surface != null).ToList();
            var normalizedWells = (wells ?? Enumerable.Empty<ReservoirWellMapPoint>()).Where(well => well != null).ToList();

            if (contourSurface == null && normalizedFaultSurfaces.Count == 0 && normalizedWells.Count == 0)
                throw new InvalidOperationException("At least one reservoir surface, fault trace, or well is required to export GeoJSON.");

            var features = new List<object>();

            if (contourSurface != null)
                features.AddRange(CreateContourFeatures(contourSurface, contourConfiguration ?? new ReservoirContourConfiguration()));

            foreach (var faultSurface in normalizedFaultSurfaces)
            {
                var geometry = CreateFaultGeometry(faultSurface);
                if (geometry == null)
                    continue;

                features.Add(CreateFeature(
                    faultSurface.SurfaceId,
                    geometry,
                    CreateProperties(
                        ("featureType", "fault-trace"),
                        ("surfaceId", faultSurface.SurfaceId),
                        ("surfaceName", faultSurface.SurfaceName),
                        ("surfaceKind", faultSurface.SurfaceKind.ToString()),
                        ("sourceRepresentationType", faultSurface.SourceRepresentationType),
                        ("sourceGridId", faultSurface.SourceGridId),
                        ("propertyName", faultSurface.PropertyName),
                        ("valueUnit", faultSurface.ValueUnit),
                        ("metadata", faultSurface.Metadata))));
            }

            foreach (var well in normalizedWells)
            {
                var geometry = CreateWellGeometry(well);
                if (geometry == null)
                    continue;

                features.Add(CreateFeature(
                    well.WellId,
                    geometry,
                    CreateProperties(
                        ("featureType", "well"),
                        ("wellId", well.WellId),
                        ("wellName", well.WellName),
                        ("uwi", well.Uwi),
                        ("status", well.Status),
                        ("colorCode", well.ColorCode),
                        ("metadata", well.Metadata))));
            }

            return Serialize(CreateFeatureCollection(
                features,
                coordinateReferenceSystem,
                ResolveBounds(contourSurface, normalizedFaultSurfaces, normalizedWells),
                CreateProperties(
                    ("name", contourSurface?.SurfaceName ?? "Reservoir Map"),
                    ("surfaceId", contourSurface?.SurfaceId),
                    ("surfaceKind", contourSurface?.SurfaceKind.ToString()),
                    ("valueUnit", contourSurface?.ValueUnit),
                    ("metadata", contourSurface?.Metadata))));
        }

        private static object CreateFeatureCollection(
            IReadOnlyCollection<object> features,
            CoordinateReferenceSystem coordinateReferenceSystem,
            BoundingBox bounds,
            Dictionary<string, object> properties)
        {
            var collection = new Dictionary<string, object>
            {
                ["type"] = "FeatureCollection",
                ["features"] = features.ToList()
            };

            var bbox = CreateBoundingBox(bounds);
            if (bbox != null)
                collection["bbox"] = bbox;

            var crsMetadata = CoordinateReferenceSystemExportMetadata.Create(coordinateReferenceSystem);
            if (crsMetadata != null)
                collection["beepCrs"] = crsMetadata;

            if (properties != null && properties.Count > 0)
                collection["properties"] = properties;

            return collection;
        }

        private static IEnumerable<object> CreateContourFeatures(ReservoirSurfaceData contourSurface, ReservoirContourConfiguration contourConfiguration)
        {
            foreach (var segment in ContourGenerator.Generate(contourSurface, contourConfiguration))
            {
                yield return CreateFeature(
                    $"{contourSurface.SurfaceId}:{segment.Level}:{segment.Start.X}:{segment.Start.Y}:{segment.End.X}:{segment.End.Y}",
                    CreateLineStringGeometry(new[]
                    {
                        new Point3D { X = segment.Start.X, Y = segment.Start.Y, Z = segment.Level },
                        new Point3D { X = segment.End.X, Y = segment.End.Y, Z = segment.Level }
                    }),
                    CreateProperties(
                        ("featureType", "contour-segment"),
                        ("surfaceId", contourSurface.SurfaceId),
                        ("surfaceName", contourSurface.SurfaceName),
                        ("surfaceKind", contourSurface.SurfaceKind.ToString()),
                        ("level", segment.Level),
                        ("isMajor", segment.IsMajor),
                        ("valueUnit", contourSurface.ValueUnit),
                        ("metadata", contourSurface.Metadata)));
            }
        }

        private static object CreateAreaGeometry(FieldMapAreaAsset asset)
        {
            var points = GetValidPoints(asset?.BoundaryPoints).ToList();
            if (points.Count == 0)
                return null;

            if (points.Count == 1)
                return CreatePointGeometry(points[0]);

            if (points.Count == 2)
                return CreateLineStringGeometry(points);

            var ring = points.Select(CreateCoordinate).ToList();
            var first = ring[0];
            var last = ring[ring.Count - 1];
            if (first[0] != last[0] || first[1] != last[1])
                ring.Add(new[] { first[0], first[1] });

            return new Dictionary<string, object>
            {
                ["type"] = "Polygon",
                ["coordinates"] = new[] { ring }
            };
        }

        private static object CreatePointAssetGeometry(FieldMapPointAsset asset)
        {
            var pathPoints = GetValidPoints(asset?.PathPoints).ToList();
            if (pathPoints.Count > 1)
                return CreateLineStringGeometry(pathPoints);

            if (asset?.Location != null && IsFinite(asset.Location))
                return CreatePointGeometry(asset.Location);

            return pathPoints.Count == 1 ? CreatePointGeometry(pathPoints[0]) : null;
        }

        private static object CreateConnectionGeometry(FieldMapConnectionAsset asset)
        {
            var vertices = GetValidPoints(asset?.Vertices).ToList();
            if (vertices.Count > 1)
                return CreateLineStringGeometry(vertices);

            return vertices.Count == 1 ? CreatePointGeometry(vertices[0]) : null;
        }

        private static object CreateFaultGeometry(ReservoirSurfaceData surface)
        {
            var trace = BuildFaultTrace(surface).ToList();
            if (trace.Count > 1)
                return CreateLineStringGeometry(trace);

            return trace.Count == 1 ? CreatePointGeometry(trace[0]) : null;
        }

        private static object CreateWellGeometry(ReservoirWellMapPoint well)
        {
            var points = GetValidPoints(well?.TrajectoryPoints).ToList();
            if (points.Count > 1)
                return CreateLineStringGeometry(points);

            if (well?.SurfaceLocation != null && IsFinite(well.SurfaceLocation))
                return CreatePointGeometry(well.SurfaceLocation);

            return points.Count == 1 ? CreatePointGeometry(points[0]) : null;
        }

        private static object CreatePointGeometry(Point3D point)
        {
            return new Dictionary<string, object>
            {
                ["type"] = "Point",
                ["coordinates"] = CreateCoordinate(point)
            };
        }

        private static object CreateLineStringGeometry(IEnumerable<Point3D> points)
        {
            var coordinates = GetValidPoints(points).Select(CreateCoordinate).ToList();
            if (coordinates.Count == 0)
                return null;

            return new Dictionary<string, object>
            {
                ["type"] = coordinates.Count == 1 ? "Point" : "LineString",
                ["coordinates"] = coordinates.Count == 1 ? coordinates[0] : coordinates
            };
        }

        private static double[] CreateCoordinate(Point3D point)
        {
            return new[] { point.X, point.Y };
        }

        private static Dictionary<string, object> CreateFeature(string id, object geometry, Dictionary<string, object> properties)
        {
            var feature = new Dictionary<string, object>
            {
                ["type"] = "Feature",
                ["geometry"] = geometry
            };

            if (!string.IsNullOrWhiteSpace(id))
                feature["id"] = id;

            if (properties != null && properties.Count > 0)
                feature["properties"] = properties;

            return feature;
        }

        private static Dictionary<string, object> CreateProperties(params (string Key, object Value)[] pairs)
        {
            var properties = new Dictionary<string, object>();

            foreach (var (key, value) in pairs)
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    continue;

                if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                    continue;

                if (value is IDictionary<string, string> metadata && metadata.Count == 0)
                    continue;

                properties[key] = value;
            }

            return properties;
        }

        private static double[] CreateBoundingBox(BoundingBox bounds)
        {
            if (bounds == null)
                return null;

            if (!double.IsFinite(bounds.MinX) || !double.IsFinite(bounds.MinY) || !double.IsFinite(bounds.MaxX) || !double.IsFinite(bounds.MaxY))
                return null;

            return new[] { bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY };
        }

        private static IEnumerable<Point3D> BuildFaultTrace(ReservoirSurfaceData surface)
        {
            var points = GetValidPoints(surface?.Points)
                .GroupBy(point => $"{Math.Round(point.X, 2)}|{Math.Round(point.Y, 2)}")
                .Select(group => group.First())
                .ToList();

            if (points.Count < 3)
                return points;

            double centerX = points.Average(point => point.X);
            double centerY = points.Average(point => point.Y);
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
            double axisX = Math.Cos(angle);
            double axisY = Math.Sin(angle);

            return points
                .OrderBy(point => ((point.X - centerX) * axisX) + ((point.Y - centerY) * axisY))
                .ToList();
        }

        private static BoundingBox ResolveBounds(
            ReservoirSurfaceData contourSurface,
            IReadOnlyCollection<ReservoirSurfaceData> faultSurfaces,
            IReadOnlyCollection<ReservoirWellMapPoint> wells)
        {
            var points = new List<Point3D>();

            if (contourSurface?.BoundingBox != null)
                return contourSurface.BoundingBox;

            points.AddRange(GetValidPoints(contourSurface?.Points));

            foreach (var faultSurface in faultSurfaces)
                points.AddRange(GetValidPoints(faultSurface?.Points));

            foreach (var well in wells)
            {
                if (well?.SurfaceLocation != null && IsFinite(well.SurfaceLocation))
                    points.Add(well.SurfaceLocation);

                points.AddRange(GetValidPoints(well?.TrajectoryPoints));
            }

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

        private static IEnumerable<Point3D> GetValidPoints(IEnumerable<Point3D> points)
        {
            return (points ?? Enumerable.Empty<Point3D>()).Where(IsFinite);
        }

        private static bool IsFinite(Point3D point)
        {
            return point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z);
        }

        private static string Serialize(object featureCollection)
        {
            return JsonSerializer.Serialize(featureCollection, SerializerOptions);
        }
    }
}