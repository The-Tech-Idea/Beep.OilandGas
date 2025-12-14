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
    }
}

