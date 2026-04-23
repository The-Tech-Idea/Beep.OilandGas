using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.Rendering
{
    internal static class ReservoirSurfaceInterpolation
    {
        public static double InterpolateZ(IReadOnlyList<Point3D> points, double x, double y, int maxInfluencePoints, double power)
        {
            if (points == null || points.Count == 0)
                return 0;

            int influenceCount = Math.Max(3, maxInfluencePoints);
            var nearest = points
                .Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z))
                .Select(point => new
                {
                    Point = point,
                    DistanceSquared = ((point.X - x) * (point.X - x)) + ((point.Y - y) * (point.Y - y))
                })
                .OrderBy(sample => sample.DistanceSquared)
                .Take(influenceCount)
                .ToList();

            if (nearest.Count == 0)
                return 0;

            if (nearest[0].DistanceSquared < 1e-12)
                return nearest[0].Point.Z;

            double numerator = 0;
            double denominator = 0;

            foreach (var sample in nearest)
            {
                double distance = Math.Sqrt(sample.DistanceSquared);
                double weight = 1.0 / Math.Pow(Math.Max(distance, 1e-6), power);
                numerator += sample.Point.Z * weight;
                denominator += weight;
            }

            return denominator <= 0 ? nearest[0].Point.Z : numerator / denominator;
        }
    }
}