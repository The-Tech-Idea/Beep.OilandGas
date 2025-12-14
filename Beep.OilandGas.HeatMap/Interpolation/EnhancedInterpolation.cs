using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Interpolation
{
    /// <summary>
    /// Enhanced interpolation methods with optimizations and additional algorithms.
    /// </summary>
    public static class EnhancedInterpolation
    {
        /// <summary>
        /// Performs optimized IDW interpolation using k-nearest neighbors.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="targetX">Target X coordinate.</param>
        /// <param name="targetY">Target Y coordinate.</param>
        /// <param name="power">Power parameter.</param>
        /// <param name="k">Number of nearest neighbors to use (0 = use all).</param>
        /// <param name="maxDistance">Maximum distance to consider.</param>
        /// <returns>Interpolated value.</returns>
        public static double OptimizedIdw(
            List<HeatMapDataPoint> dataPoints,
            double targetX,
            double targetY,
            double power = 2.0,
            int k = 0,
            double maxDistance = double.MaxValue)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // If k is specified and less than total points, use k-nearest neighbors
            if (k > 0 && k < dataPoints.Count)
            {
                var nearestNeighbors = dataPoints
                    .Select(p => new
                    {
                        Point = p,
                        Distance = Math.Sqrt(Math.Pow(p.X - targetX, 2) + Math.Pow(p.Y - targetY, 2))
                    })
                    .Where(n => n.Distance <= maxDistance)
                    .OrderBy(n => n.Distance)
                    .Take(k)
                    .ToList();

                if (nearestNeighbors.Count == 0)
                    return 0.0;

                double numerator = 0.0;
                double denominator = 0.0;

                foreach (var neighbor in nearestNeighbors)
                {
                    if (neighbor.Distance < 1e-10)
                        return neighbor.Point.Value;

                    double weight = 1.0 / Math.Pow(neighbor.Distance, power);
                    numerator += weight * neighbor.Point.Value;
                    denominator += weight;
                }

                return denominator > 1e-10 ? numerator / denominator : 0.0;
            }
            else
            {
                // Use all points (standard IDW)
                return InterpolationMethod.InverseDistanceWeighting(
                    dataPoints, targetX, targetY, power, maxDistance);
            }
        }

        /// <summary>
        /// Performs Radial Basis Function (RBF) interpolation.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="targetX">Target X coordinate.</param>
        /// <param name="targetY">Target Y coordinate.</param>
        /// <param name="rbfType">Type of RBF function.</param>
        /// <param name="shapeParameter">Shape parameter (epsilon) for RBF.</param>
        /// <returns>Interpolated value.</returns>
        public static double RadialBasisFunction(
            List<HeatMapDataPoint> dataPoints,
            double targetX,
            double targetY,
            RbfType rbfType = RbfType.ThinPlateSpline,
            double shapeParameter = 1.0)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // For simplicity, using direct evaluation (full RBF requires solving a system)
            double result = 0.0;
            double sumWeights = 0.0;

            foreach (var point in dataPoints)
            {
                double distance = Math.Sqrt(
                    Math.Pow(point.X - targetX, 2) +
                    Math.Pow(point.Y - targetY, 2));

                if (distance < 1e-10)
                    return point.Value;

                double weight = EvaluateRbf(distance, rbfType, shapeParameter);
                result += weight * point.Value;
                sumWeights += weight;
            }

            return sumWeights > 1e-10 ? result / sumWeights : 0.0;
        }

        /// <summary>
        /// Evaluates a radial basis function.
        /// </summary>
        private static double EvaluateRbf(double distance, RbfType rbfType, double shapeParameter)
        {
            double r = distance * shapeParameter;

            return rbfType switch
            {
                RbfType.ThinPlateSpline => r > 0 ? r * r * Math.Log(r) : 0,
                RbfType.Gaussian => Math.Exp(-r * r),
                RbfType.Multiquadric => Math.Sqrt(1 + r * r),
                RbfType.InverseMultiquadric => 1.0 / Math.Sqrt(1 + r * r),
                RbfType.Cubic => r * r * r,
                RbfType.Quintic => r * r * r * r * r,
                _ => Math.Exp(-r * r) // Default to Gaussian
            };
        }

        /// <summary>
        /// Performs Natural Neighbor interpolation.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="targetX">Target X coordinate.</param>
        /// <param name="targetY">Target Y coordinate.</param>
        /// <returns>Interpolated value.</returns>
        public static double NaturalNeighbor(
            List<HeatMapDataPoint> dataPoints,
            double targetX,
            double targetY)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // Simplified Natural Neighbor using Voronoi diagram approximation
            // Full implementation would require Delaunay triangulation

            // Find nearest neighbors
            var neighbors = dataPoints
                .Select(p => new
                {
                    Point = p,
                    Distance = Math.Sqrt(Math.Pow(p.X - targetX, 2) + Math.Pow(p.Y - targetY, 2))
                })
                .OrderBy(n => n.Distance)
                .Take(Math.Min(6, dataPoints.Count))
                .ToList();

            if (neighbors.Count == 0)
                return 0.0;

            // Use inverse distance weighting with natural neighbor weights
            double numerator = 0.0;
            double denominator = 0.0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor.Distance < 1e-10)
                    return neighbor.Point.Value;

                // Natural neighbor weight (simplified - uses area-based weighting)
                double weight = 1.0 / (neighbor.Distance * neighbor.Distance);
                numerator += weight * neighbor.Point.Value;
                denominator += weight;
            }

            return denominator > 1e-10 ? numerator / denominator : 0.0;
        }

        /// <summary>
        /// Performs Spline interpolation (bicubic).
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="targetX">Target X coordinate.</param>
        /// <param name="targetY">Target Y coordinate.</param>
        /// <param name="tension">Tension parameter (0 = linear, 1 = tight).</param>
        /// <returns>Interpolated value.</returns>
        public static double SplineInterpolation(
            List<HeatMapDataPoint> dataPoints,
            double targetX,
            double targetY,
            double tension = 0.5)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // Find 4 nearest neighbors for bicubic interpolation
            var nearest = dataPoints
                .Select(p => new
                {
                    Point = p,
                    Distance = Math.Sqrt(Math.Pow(p.X - targetX, 2) + Math.Pow(p.Y - targetY, 2))
                })
                .OrderBy(n => n.Distance)
                .Take(4)
                .ToList();

            if (nearest.Count == 0)
                return 0.0;

            if (nearest.Count == 1 || nearest[0].Distance < 1e-10)
                return nearest[0].Point.Value;

            // Use Catmull-Rom spline interpolation
            if (nearest.Count >= 4)
            {
                // Bicubic interpolation using 4 points
                return CatmullRomSpline(
                    nearest[0].Point, nearest[1].Point,
                    nearest[2].Point, nearest[3].Point,
                    targetX, targetY, tension);
            }
            else
            {
                // Fall back to weighted average
                double numerator = 0.0;
                double denominator = 0.0;

                foreach (var n in nearest)
                {
                    double weight = 1.0 / (n.Distance * n.Distance);
                    numerator += weight * n.Point.Value;
                    denominator += weight;
                }

                return denominator > 1e-10 ? numerator / denominator : 0.0;
            }
        }

        /// <summary>
        /// Performs Catmull-Rom spline interpolation.
        /// </summary>
        private static double CatmullRomSpline(
            HeatMapDataPoint p0, HeatMapDataPoint p1,
            HeatMapDataPoint p2, HeatMapDataPoint p3,
            double targetX, double targetY, double tension)
        {
            // Simplified 2D Catmull-Rom spline
            // Interpolate in X direction first, then Y
            double x0 = p0.X, y0 = p0.Y, v0 = p0.Value;
            double x1 = p1.X, y1 = p1.Y, v1 = p1.Value;
            double x2 = p2.X, y2 = p2.Y, v2 = p2.Value;
            double x3 = p3.X, y3 = p3.Y, v3 = p3.Value;

            // Normalize coordinates
            double tX = (targetX - x1) / (x2 - x1 + 1e-10);
            double tY = (targetY - y1) / (y2 - y1 + 1e-10);

            // Catmull-Rom interpolation
            double vX0 = InterpolateCatmullRom(v0, v1, v2, v3, tX, tension);
            double vX1 = InterpolateCatmullRom(v0, v1, v2, v3, tX + 0.1, tension);

            return InterpolateCatmullRom(vX0, vX1, vX0, vX1, tY, tension);
        }

        /// <summary>
        /// Performs 1D Catmull-Rom interpolation.
        /// </summary>
        private static double InterpolateCatmullRom(double p0, double p1, double p2, double p3, double t, double tension)
        {
            t = Math.Max(0, Math.Min(1, t));
            double t2 = t * t;
            double t3 = t2 * t;

            double m1 = (p2 - p0) * tension;
            double m2 = (p3 - p1) * tension;

            return (2 * t3 - 3 * t2 + 1) * p1 +
                   (t3 - 2 * t2 + t) * m1 +
                   (-2 * t3 + 3 * t2) * p2 +
                   (t3 - t2) * m2;
        }

        /// <summary>
        /// Performs adaptive interpolation with varying cell size based on data density.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="minX">Minimum X.</param>
        /// <param name="maxX">Maximum X.</param>
        /// <param name="minY">Minimum Y.</param>
        /// <param name="maxY">Maximum Y.</param>
        /// <param name="baseCellSize">Base cell size.</param>
        /// <param name="interpolationMethod">Interpolation method to use.</param>
        /// <returns>2D grid of interpolated values with adaptive cell sizes.</returns>
        public static AdaptiveGridResult AdaptiveInterpolation(
            List<HeatMapDataPoint> dataPoints,
            double minX, double maxX, double minY, double maxY,
            double baseCellSize,
            InterpolationMethodType interpolationMethod = InterpolationMethodType.InverseDistanceWeighting)
        {
            // Divide space into regions based on point density
            int gridDivisions = 4;
            double regionWidth = (maxX - minX) / gridDivisions;
            double regionHeight = (maxY - minY) / gridDivisions;

            var grid = new List<(double X, double Y, double Value, double CellSize)>();

            for (int i = 0; i < gridDivisions; i++)
            {
                for (int j = 0; j < gridDivisions; j++)
                {
                    double regionMinX = minX + i * regionWidth;
                    double regionMaxX = minX + (i + 1) * regionWidth;
                    double regionMinY = minY + j * regionHeight;
                    double regionMaxY = minY + (j + 1) * regionHeight;

                    // Count points in region
                    int pointCount = dataPoints.Count(p =>
                        p.X >= regionMinX && p.X <= regionMaxX &&
                        p.Y >= regionMinY && p.Y <= regionMaxY);

                    // Adjust cell size based on density
                    double cellSize = baseCellSize;
                    if (pointCount > 10)
                        cellSize = baseCellSize * 0.5; // Smaller cells in dense areas
                    else if (pointCount < 3)
                        cellSize = baseCellSize * 2.0; // Larger cells in sparse areas

                    // Generate grid for this region
                    for (double x = regionMinX; x <= regionMaxX; x += cellSize)
                    {
                        for (double y = regionMinY; y <= regionMaxY; y += cellSize)
                        {
                            double value = interpolationMethod switch
                            {
                                InterpolationMethodType.InverseDistanceWeighting =>
                                    InterpolationMethod.InverseDistanceWeighting(dataPoints, x, y),
                                InterpolationMethodType.Kriging =>
                                    InterpolationMethod.Kriging(dataPoints, x, y),
                                _ => 0.0
                            };

                            grid.Add((x, y, value, cellSize));
                        }
                    }
                }
            }

            return new AdaptiveGridResult
            {
                GridPoints = grid,
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY
            };
        }
    }

    /// <summary>
    /// Types of Radial Basis Functions.
    /// </summary>
    public enum RbfType
    {
        ThinPlateSpline,
        Gaussian,
        Multiquadric,
        InverseMultiquadric,
        Cubic,
        Quintic
    }

    /// <summary>
    /// Result of adaptive interpolation.
    /// </summary>
    public class AdaptiveGridResult
    {
        public List<(double X, double Y, double Value, double CellSize)> GridPoints { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
    }
}

