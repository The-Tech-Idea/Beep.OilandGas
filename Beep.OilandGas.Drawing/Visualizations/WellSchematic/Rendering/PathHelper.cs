using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Helper class for path operations.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Gets a point on a path at a specific Y coordinate (depth).
        /// </summary>
        /// <param name="pathPoints">The path points.</param>
        /// <param name="y">The Y coordinate (depth).</param>
        /// <returns>The point on the path.</returns>
        public static SKPoint GetPointOnPath(List<SKPoint> pathPoints, float y)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                throw new ArgumentException("Path points cannot be null or empty.", nameof(pathPoints));

            // Find the segment that contains this Y coordinate
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float y1 = pathPoints[i].Y;
                float y2 = pathPoints[i + 1].Y;

                // Check if Y is between these two points
                if ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))
                {
                    // Interpolate
                    float t = (y - y1) / (y2 - y1);
                    if (float.IsNaN(t) || float.IsInfinity(t))
                        t = 0;

                    float x = pathPoints[i].X + t * (pathPoints[i + 1].X - pathPoints[i].X);
                    return new SKPoint(x, y);
                }
            }

            // If not found, return the closest point
            return pathPoints.OrderBy(p => Math.Abs(p.Y - y)).First();
        }

        /// <summary>
        /// Gets the tangent vector for a path at a specific Y coordinate.
        /// </summary>
        /// <param name="pathPoints">The path points.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The tangent vector.</returns>
        public static SKPoint GetTangentOnPath(List<SKPoint> pathPoints, float y)
        {
            if (pathPoints == null || pathPoints.Count < 2)
                throw new ArgumentException("Path points must contain at least two points.", nameof(pathPoints));

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float y1 = pathPoints[i].Y;
                float y2 = pathPoints[i + 1].Y;

                if ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))
                {
                    return new SKPoint(pathPoints[i + 1].X - pathPoints[i].X, pathPoints[i + 1].Y - pathPoints[i].Y);
                }
            }

            var previous = pathPoints[^2];
            var last = pathPoints[^1];
            return new SKPoint(last.X - previous.X, last.Y - previous.Y);
        }

        /// <summary>
        /// Gets the normal vector for a path at a specific Y coordinate.
        /// </summary>
        /// <param name="pathPoints">The path points.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The normalized perpendicular vector.</returns>
        public static SKPoint GetNormalOnPath(List<SKPoint> pathPoints, float y)
        {
            var tangent = GetTangentOnPath(pathPoints, y);
            float length = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
            if (length <= 0)
                return new SKPoint(1, 0);

            return new SKPoint(-tangent.Y / length, tangent.X / length);
        }

        /// <summary>
        /// Extracts the portion of a path between two Y coordinates, including interpolated endpoints.
        /// </summary>
        /// <param name="pathPoints">The full path points.</param>
        /// <param name="startY">The starting Y coordinate.</param>
        /// <param name="endY">The ending Y coordinate.</param>
        /// <returns>The subpath between the supplied Y values.</returns>
        public static List<SKPoint> GetPathSegment(List<SKPoint> pathPoints, float startY, float endY)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                throw new ArgumentException("Path points cannot be null or empty.", nameof(pathPoints));

            var minY = Math.Min(startY, endY);
            var maxY = Math.Max(startY, endY);
            var segment = new List<SKPoint>
            {
                GetPointOnPath(pathPoints, startY)
            };

            foreach (var point in pathPoints)
            {
                if (point.Y > minY && point.Y < maxY)
                {
                    segment.Add(point);
                }
            }

            var endPoint = GetPointOnPath(pathPoints, endY);
            if (segment.Count == 0 || !ArePointsNear(segment[^1], endPoint))
            {
                segment.Add(endPoint);
            }

            return RemoveSequentialDuplicates(segment);
        }

        /// <summary>
        /// Creates a parallel path offset from the supplied centerline.
        /// </summary>
        /// <param name="pathPoints">The source path.</param>
        /// <param name="offset">The perpendicular offset distance.</param>
        /// <returns>The offset path.</returns>
        public static List<SKPoint> CreateParallelPath(List<SKPoint> pathPoints, float offset)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                throw new ArgumentException("Path points cannot be null or empty.", nameof(pathPoints));

            if (Math.Abs(offset) < 0.001f)
                return new List<SKPoint>(pathPoints);

            var parallelPath = new List<SKPoint>(pathPoints.Count);
            for (int index = 0; index < pathPoints.Count; index++)
            {
                var tangent = GetTangentAtIndex(pathPoints, index);
                float length = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
                if (length <= 0)
                {
                    parallelPath.Add(pathPoints[index]);
                    continue;
                }

                float normalX = -tangent.Y / length;
                float normalY = tangent.X / length;
                parallelPath.Add(new SKPoint(
                    pathPoints[index].X + normalX * offset,
                    pathPoints[index].Y + normalY * offset));
            }

            return parallelPath;
        }

        /// <summary>
        /// Gets a point on a path at a normalized position (0 to 1).
        /// </summary>
        /// <param name="pathPoints">The path points.</param>
        /// <param name="t">The normalized position (0 to 1).</param>
        /// <returns>The point on the path.</returns>
        public static SKPoint GetPointOnPathNormalized(List<SKPoint> pathPoints, float t)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                throw new ArgumentException("Path points cannot be null or empty.", nameof(pathPoints));

            t = Math.Max(0, Math.Min(1, t));
            var pathLengths = CalculatePathLengths(pathPoints);
            float totalLength = pathLengths.Last();
            float desiredLength = t * totalLength;

            for (int i = 1; i < pathLengths.Count; i++)
            {
                if (pathLengths[i] >= desiredLength)
                {
                    float segmentLength = pathLengths[i] - pathLengths[i - 1];
                    float remainder = desiredLength - pathLengths[i - 1];
                    float segmentT = remainder / segmentLength;

                    float x = pathPoints[i - 1].X + segmentT * (pathPoints[i].X - pathPoints[i - 1].X);
                    float y = pathPoints[i - 1].Y + segmentT * (pathPoints[i].Y - pathPoints[i - 1].Y);

                    return new SKPoint(x, y);
                }
            }

            return pathPoints.Last();
        }

        /// <summary>
        /// Calculates the cumulative lengths along a path.
        /// </summary>
        /// <param name="pathPoints">The path points.</param>
        /// <returns>The cumulative lengths.</returns>
        public static List<float> CalculatePathLengths(List<SKPoint> pathPoints)
        {
            var lengths = new List<float>();
            float totalLength = 0;
            lengths.Add(totalLength);

            for (int i = 1; i < pathPoints.Count; i++)
            {
                float dx = pathPoints[i].X - pathPoints[i - 1].X;
                float dy = pathPoints[i].Y - pathPoints[i - 1].Y;
                totalLength += (float)Math.Sqrt(dx * dx + dy * dy);
                lengths.Add(totalLength);
            }

            return lengths;
        }

        private static SKPoint GetTangentAtIndex(IReadOnlyList<SKPoint> pathPoints, int index)
        {
            var previous = index == 0 ? pathPoints[index] : pathPoints[index - 1];
            var next = index == pathPoints.Count - 1 ? pathPoints[index] : pathPoints[index + 1];
            return new SKPoint(next.X - previous.X, next.Y - previous.Y);
        }

        private static List<SKPoint> RemoveSequentialDuplicates(List<SKPoint> points)
        {
            var deduped = new List<SKPoint>();
            foreach (var point in points)
            {
                if (deduped.Count == 0 || !ArePointsNear(deduped[^1], point))
                {
                    deduped.Add(point);
                }
            }

            return deduped;
        }

        private static bool ArePointsNear(SKPoint left, SKPoint right)
        {
            return Math.Abs(left.X - right.X) < 0.01f && Math.Abs(left.Y - right.Y) < 0.01f;
        }
    }
}

