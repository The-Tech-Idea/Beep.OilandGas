using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Scenes;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Measurements
{
    /// <summary>
    /// Provides scene-aware measurement calculations for map, section, and depth views.
    /// </summary>
    public static class SceneMeasurementService
    {
        private const double EarthRadiusMeters = 6378137.0;
        private const double DegreesToRadians = Math.PI / 180.0;

        /// <summary>
        /// Measures a distance or path length using world-space vertices.
        /// </summary>
        public static SceneMeasurementResult MeasureDistance(DrawingScene scene, IEnumerable<SKPoint> worldPoints)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            var points = NormalizeVertices(worldPoints, minimumCount: 2);
            if (scene.Kind == DrawingSceneKind.Depth)
            {
                var depthAxis = scene.CoordinateReferenceSystem.Axes
                    .FirstOrDefault(axis => axis.Kind == CoordinateAxisKind.Depth || axis.Kind == CoordinateAxisKind.MeasuredDepth);
                if (depthAxis == null)
                    throw new InvalidOperationException("Depth scenes require a depth axis before distance measurements can be computed.");

                double value = 0;
                for (int index = 1; index < points.Count; index++)
                {
                    value += Math.Abs(points[index].Y - points[index - 1].Y);
                }

                return new SceneMeasurementResult(
                    SceneMeasurementKind.Distance,
                    points.Count == 2 ? SceneMeasurementGeometryKind.Segment : SceneMeasurementGeometryKind.Polyline,
                    value,
                    depthAxis.Unit.Code,
                    depthAxis.Unit.Code,
                    "depth-axis",
                    isApproximate: false,
                    vertexCount: points.Count);
            }

            if (IsGeographic(scene.CoordinateReferenceSystem))
            {
                double value = 0;
                for (int index = 1; index < points.Count; index++)
                {
                    value += ComputeGeodesicDistanceMeters(points[index - 1], points[index]);
                }

                return new SceneMeasurementResult(
                    SceneMeasurementKind.Distance,
                    points.Count == 2 ? SceneMeasurementGeometryKind.Segment : SceneMeasurementGeometryKind.Polyline,
                    value,
                    "m",
                    "m",
                    "geodesic",
                    isApproximate: true,
                    vertexCount: points.Count);
            }

            var linearUnit = ResolvePrimaryMeasureUnit(scene.CoordinateReferenceSystem);
            double pathLength = 0;
            for (int index = 1; index < points.Count; index++)
            {
                pathLength += ComputePlanarDistance(points[index - 1], points[index]);
            }

            return new SceneMeasurementResult(
                SceneMeasurementKind.Distance,
                points.Count == 2 ? SceneMeasurementGeometryKind.Segment : SceneMeasurementGeometryKind.Polyline,
                pathLength,
                linearUnit.Code,
                linearUnit.Code,
                scene.Kind == DrawingSceneKind.Section ? "section-planar" : "planar",
                isApproximate: false,
                vertexCount: points.Count);
        }

        /// <summary>
        /// Measures a polygon area using world-space vertices.
        /// </summary>
        public static SceneMeasurementResult MeasureArea(DrawingScene scene, IEnumerable<SKPoint> worldPolygon)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));
            if (scene.Kind == DrawingSceneKind.Depth)
                throw new InvalidOperationException("Area measurements are not supported for one-dimensional depth scenes.");

            var vertices = NormalizePolygonVertices(worldPolygon);
            if (vertices.Count < 3)
                throw new ArgumentException("At least three distinct vertices are required for an area measurement.", nameof(worldPolygon));

            if (IsGeographic(scene.CoordinateReferenceSystem))
            {
                double squareMeters = ComputeApproximateGeographicAreaSquareMeters(vertices);
                return new SceneMeasurementResult(
                    SceneMeasurementKind.Area,
                    SceneMeasurementGeometryKind.Polygon,
                    squareMeters,
                    "m2",
                    "m^2",
                    "geodesic-area",
                    isApproximate: true,
                    vertexCount: vertices.Count);
            }

            var linearUnit = ResolvePrimaryMeasureUnit(scene.CoordinateReferenceSystem);
            double area = Math.Abs(ComputeSignedPolygonArea(vertices));
            return new SceneMeasurementResult(
                SceneMeasurementKind.Area,
                SceneMeasurementGeometryKind.Polygon,
                area,
                linearUnit.Code + "2",
                linearUnit.Code + "^2",
                scene.Kind == DrawingSceneKind.Section ? "section-area" : "planar-area",
                isApproximate: false,
                vertexCount: vertices.Count);
        }

        private static List<SKPoint> NormalizeVertices(IEnumerable<SKPoint> points, int minimumCount)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            var normalized = points
                .Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                .ToList();

            if (normalized.Count < minimumCount)
                throw new ArgumentException($"At least {minimumCount} valid vertices are required.", nameof(points));

            return normalized;
        }

        private static List<SKPoint> NormalizePolygonVertices(IEnumerable<SKPoint> points)
        {
            var normalized = NormalizeVertices(points, minimumCount: 3);
            if (normalized.Count > 1 && AreEquivalent(normalized[0], normalized[normalized.Count - 1]))
                normalized.RemoveAt(normalized.Count - 1);

            return normalized;
        }

        private static bool IsGeographic(CoordinateReferenceSystem coordinateReferenceSystem)
        {
            return coordinateReferenceSystem?.Kind == CoordinateReferenceSystemKind.Geographic;
        }

        private static MeasurementUnit ResolvePrimaryMeasureUnit(CoordinateReferenceSystem coordinateReferenceSystem)
        {
            if (coordinateReferenceSystem == null)
                throw new ArgumentNullException(nameof(coordinateReferenceSystem));

            var unit = coordinateReferenceSystem.Axes
                .FirstOrDefault(axis => axis.Unit.Dimension == MeasurementDimension.Length || axis.Unit.Dimension == MeasurementDimension.Time)
                ?.Unit;

            if (unit == null)
                throw new InvalidOperationException("The active scene does not expose a measurable linear or time axis.");

            return unit;
        }

        private static double ComputePlanarDistance(SKPoint a, SKPoint b)
        {
            double deltaX = b.X - a.X;
            double deltaY = b.Y - a.Y;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private static double ComputeGeodesicDistanceMeters(SKPoint a, SKPoint b)
        {
            double latitude1 = a.Y * DegreesToRadians;
            double latitude2 = b.Y * DegreesToRadians;
            double deltaLatitude = (b.Y - a.Y) * DegreesToRadians;
            double deltaLongitude = (b.X - a.X) * DegreesToRadians;

            double sinLatitude = Math.Sin(deltaLatitude / 2.0);
            double sinLongitude = Math.Sin(deltaLongitude / 2.0);
            double haversine = (sinLatitude * sinLatitude)
                + (Math.Cos(latitude1) * Math.Cos(latitude2) * sinLongitude * sinLongitude);

            double angularDistance = 2.0 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(Math.Max(0.0, 1.0 - haversine)));
            return EarthRadiusMeters * angularDistance;
        }

        private static double ComputeApproximateGeographicAreaSquareMeters(IReadOnlyList<SKPoint> vertices)
        {
            double originLongitude = vertices[0].X * DegreesToRadians;
            double originLatitude = vertices[0].Y * DegreesToRadians;
            double referenceLatitude = vertices.Average(point => point.Y) * DegreesToRadians;

            var projected = vertices.Select(point =>
            {
                double longitude = point.X * DegreesToRadians;
                double latitude = point.Y * DegreesToRadians;
                double x = EarthRadiusMeters * (longitude - originLongitude) * Math.Cos(referenceLatitude);
                double y = EarthRadiusMeters * (latitude - originLatitude);
                return new SKPoint((float)x, (float)y);
            }).ToList();

            return Math.Abs(ComputeSignedPolygonArea(projected));
        }

        private static double ComputeSignedPolygonArea(IReadOnlyList<SKPoint> vertices)
        {
            double area = 0;
            for (int index = 0; index < vertices.Count; index++)
            {
                var current = vertices[index];
                var next = vertices[(index + 1) % vertices.Count];
                area += (current.X * next.Y) - (next.X * current.Y);
            }

            return area * 0.5;
        }

        private static bool AreEquivalent(SKPoint a, SKPoint b)
        {
            return Math.Abs(a.X - b.X) < 0.001f && Math.Abs(a.Y - b.Y) < 0.001f;
        }
    }
}