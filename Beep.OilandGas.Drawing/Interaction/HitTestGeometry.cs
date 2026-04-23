using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    internal static class HitTestGeometry
    {
        public static float DistanceToPolyline(SKPoint point, IReadOnlyList<SKPoint> polyline)
        {
            if (polyline == null || polyline.Count == 0)
                return float.PositiveInfinity;

            if (polyline.Count == 1)
                return SKPoint.Distance(point, polyline[0]);

            float minimumDistance = float.PositiveInfinity;
            for (int index = 1; index < polyline.Count; index++)
            {
                minimumDistance = Math.Min(minimumDistance, DistanceToSegment(point, polyline[index - 1], polyline[index]));
            }

            return minimumDistance;
        }

        public static float DistanceToPolygonEdge(SKPoint point, IReadOnlyList<SKPoint> polygon)
        {
            if (polygon == null || polygon.Count < 2)
                return float.PositiveInfinity;

            float minimumDistance = float.PositiveInfinity;
            for (int index = 0; index < polygon.Count; index++)
            {
                var start = polygon[index];
                var end = polygon[(index + 1) % polygon.Count];
                minimumDistance = Math.Min(minimumDistance, DistanceToSegment(point, start, end));
            }

            return minimumDistance;
        }

        public static bool ContainsPolygon(SKPoint point, IReadOnlyList<SKPoint> polygon)
        {
            if (polygon == null || polygon.Count < 3)
                return false;

            bool contains = false;
            for (int index = 0, previous = polygon.Count - 1; index < polygon.Count; previous = index++)
            {
                var currentPoint = polygon[index];
                var previousPoint = polygon[previous];

                bool intersects = ((currentPoint.Y > point.Y) != (previousPoint.Y > point.Y))
                    && (point.X < ((previousPoint.X - currentPoint.X) * (point.Y - currentPoint.Y) / ((previousPoint.Y - currentPoint.Y) + float.Epsilon)) + currentPoint.X);

                if (intersects)
                    contains = !contains;
            }

            return contains;
        }

        public static SKPoint ComputeCentroid(IReadOnlyList<SKPoint> points)
        {
            if (points == null || points.Count == 0)
                return SKPoint.Empty;

            return new SKPoint(points.Average(point => point.X), points.Average(point => point.Y));
        }

        public static SKPoint ComputeMidpoint(IReadOnlyList<SKPoint> points)
        {
            if (points == null || points.Count == 0)
                return SKPoint.Empty;

            if (points.Count == 1)
                return points[0];

            return points[points.Count / 2];
        }

        private static float DistanceToSegment(SKPoint point, SKPoint start, SKPoint end)
        {
            float deltaX = end.X - start.X;
            float deltaY = end.Y - start.Y;
            float lengthSquared = (deltaX * deltaX) + (deltaY * deltaY);

            if (lengthSquared <= float.Epsilon)
                return SKPoint.Distance(point, start);

            float t = ((point.X - start.X) * deltaX + (point.Y - start.Y) * deltaY) / lengthSquared;
            t = Math.Max(0f, Math.Min(1f, t));

            var projection = new SKPoint(start.X + (deltaX * t), start.Y + (deltaY * t));
            return SKPoint.Distance(point, projection);
        }
    }
}