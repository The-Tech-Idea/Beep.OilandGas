using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Interpolation
{
    /// <summary>
    /// Enumeration of interpolation methods for heat map generation.
    /// </summary>
    public enum InterpolationMethodType
    {
        /// <summary>
        /// No interpolation - use raw data points only.
        /// </summary>
        None,

        /// <summary>
        /// Inverse Distance Weighting interpolation.
        /// </summary>
        InverseDistanceWeighting,

        /// <summary>
        /// Kriging interpolation (simplified implementation).
        /// </summary>
        Kriging,

        /// <summary>
        /// Bilinear interpolation on a grid.
        /// </summary>
        Bilinear,

        /// <summary>
        /// Optimized IDW with k-nearest neighbors.
        /// </summary>
        OptimizedIdw,

        /// <summary>
        /// Radial Basis Function interpolation (Thin Plate Spline).
        /// </summary>
        RadialBasisFunction,

        /// <summary>
        /// Natural Neighbor interpolation.
        /// </summary>
        NaturalNeighbor,

        /// <summary>
        /// Spline interpolation (Catmull-Rom).
        /// </summary>
        Spline
    }

    /// <summary>
    /// Provides interpolation methods for generating smooth heat maps from discrete data points.
    /// </summary>
    public static class InterpolationMethod
    {
        /// <summary>
        /// Performs Inverse Distance Weighting (IDW) interpolation.
        /// </summary>
        /// <param name="dataPoints">List of data points with known values.</param>
        /// <param name="targetX">X coordinate of the target point.</param>
        /// <param name="targetY">Y coordinate of the target point.</param>
        /// <param name="power">Power parameter (default: 2.0). Higher values give more weight to nearby points.</param>
        /// <param name="maxDistance">Maximum distance to consider for interpolation (default: infinity).</param>
        /// <returns>Interpolated value at the target point.</returns>
        public static double InverseDistanceWeighting(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double targetX,
            double targetY,
            double power = 2.0,
            double maxDistance = double.MaxValue)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            double numerator = 0.0;
            double denominator = 0.0;

            foreach (var point in dataPoints)
            {
                double distance = Math.Sqrt(
                    Math.Pow(point.X - targetX, 2) +
                    Math.Pow(point.Y - targetY, 2));

                // Skip if beyond maximum distance
                if (distance > maxDistance)
                    continue;

                // Avoid division by zero
                if (distance < 1e-10)
                    return point.Value;

                double weight = 1.0 / Math.Pow(distance, power);
                numerator += weight * point.Value;
                denominator += weight;
            }

            if (denominator < 1e-10)
                return 0.0; // No nearby points

            return numerator / denominator;
        }

        /// <summary>
        /// Generates a grid of interpolated values using IDW.
        /// </summary>
        /// <param name="dataPoints">List of data points with known values.</param>
        /// <param name="gridWidth">Width of the grid in pixels.</param>
        /// <param name="gridHeight">Height of the grid in pixels.</param>
        /// <param name="cellSize">Size of each grid cell in pixels.</param>
        /// <param name="power">Power parameter for IDW (default: 2.0).</param>
        /// <returns>2D array of interpolated values.</returns>
        public static double[,] GenerateGridIdw(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int gridWidth,
            int gridHeight,
            double cellSize = 10.0,
            double power = 2.0)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            int cols = (int)(gridWidth / cellSize);
            int rows = (int)(gridHeight / cellSize);
            var grid = new double[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    double x = col * cellSize;
                    double y = row * cellSize;
                    grid[row, col] = InverseDistanceWeighting(dataPoints, x, y, power);
                }
            }

            return grid;
        }

        /// <summary>
        /// Performs simplified Kriging interpolation (using exponential variogram model).
        /// </summary>
        /// <param name="dataPoints">List of data points with known values.</param>
        /// <param name="targetX">X coordinate of the target point.</param>
        /// <param name="targetY">Y coordinate of the target point.</param>
        /// <param name="range">Range parameter for the variogram (default: 100.0).</param>
        /// <param name="sill">Sill parameter for the variogram (default: 1.0).</param>
        /// <param name="nugget">Nugget parameter for the variogram (default: 0.0).</param>
        /// <returns>Interpolated value at the target point.</returns>
        public static double Kriging(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double targetX,
            double targetY,
            double range = 100.0,
            double sill = 1.0,
            double nugget = 0.0)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // Simplified Kriging using exponential variogram
            // This is a basic implementation - full Kriging requires solving a system of equations

            int n = dataPoints.Count;
            if (n == 1)
                return dataPoints[0].Value;

            // Calculate distances and variogram values
            var distances = new double[n];
            var variogramValues = new double[n];

            for (int i = 0; i < n; i++)
            {
                double distance = Math.Sqrt(
                    Math.Pow(dataPoints[i].X - targetX, 2) +
                    Math.Pow(dataPoints[i].Y - targetY, 2));

                distances[i] = distance;

                // Exponential variogram model
                if (distance < 1e-10)
                {
                    variogramValues[i] = nugget;
                }
                else
                {
                    variogramValues[i] = nugget + sill * (1 - Math.Exp(-distance / range));
                }
            }

            // Use inverse distance weighting as approximation for weights
            // Full Kriging would solve: C * w = c, where C is covariance matrix
            double sumWeights = 0.0;
            double sumWeightedValues = 0.0;

            for (int i = 0; i < n; i++)
            {
                // Use inverse of variogram value as weight (simplified)
                double weight = 1.0 / (variogramValues[i] + 1e-10);
                sumWeights += weight;
                sumWeightedValues += weight * dataPoints[i].Value;
            }

            if (sumWeights < 1e-10)
                return dataPoints[0].Value;

            return sumWeightedValues / sumWeights;
        }

        /// <summary>
        /// Generates contour lines from interpolated grid data.
        /// </summary>
        /// <param name="grid">2D array of interpolated values.</param>
        /// <param name="contourLevels">Array of contour levels to generate.</param>
        /// <returns>List of contour line segments, each represented as (x1, y1, x2, y2).</returns>
        public static List<(double x1, double y1, double x2, double y2)> GenerateContours(
            double[,] grid,
            double[] contourLevels)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (contourLevels == null || contourLevels.Length == 0)
                throw new ArgumentException("Contour levels cannot be null or empty.");

            var contours = new List<(double, double, double, double)>();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            foreach (var level in contourLevels)
            {
                // Marching squares algorithm (simplified)
                for (int row = 0; row < rows - 1; row++)
                {
                    for (int col = 0; col < cols - 1; col++)
                    {
                        double tl = grid[row, col];         // Top-left
                        double tr = grid[row, col + 1];     // Top-right
                        double bl = grid[row + 1, col];     // Bottom-left
                        double br = grid[row + 1, col + 1]; // Bottom-right

                        // Check each edge of the square
                        // Top edge
                        if ((tl < level && tr >= level) || (tl >= level && tr < level))
                        {
                            double t = (level - tl) / (tr - tl + 1e-10);
                            double x1 = col + t;
                            double y1 = row;
                            double x2 = col + t;
                            double y2 = row;
                            contours.Add((x1, y1, x2, y2));
                        }

                        // Right edge
                        if ((tr < level && br >= level) || (tr >= level && br < level))
                        {
                            double t = (level - tr) / (br - tr + 1e-10);
                            double x1 = col + 1;
                            double y1 = row + t;
                            double x2 = col + 1;
                            double y2 = row + t;
                            contours.Add((x1, y1, x2, y2));
                        }

                        // Bottom edge
                        if ((bl < level && br >= level) || (bl >= level && br < level))
                        {
                            double t = (level - bl) / (br - bl + 1e-10);
                            double x1 = col + t;
                            double y1 = row + 1;
                            double x2 = col + t;
                            double y2 = row + 1;
                            contours.Add((x1, y1, x2, y2));
                        }

                        // Left edge
                        if ((tl < level && bl >= level) || (tl >= level && bl < level))
                        {
                            double t = (level - tl) / (bl - tl + 1e-10);
                            double x1 = col;
                            double y1 = row + t;
                            double x2 = col;
                            double y2 = row + t;
                            contours.Add((x1, y1, x2, y2));
                        }
                    }
                }
            }

            return contours;
        }

        /// <summary>
        /// Performs interpolation using the specified method type, with support for enhanced methods.
        /// </summary>
        /// <param name="methodType">The interpolation method to use.</param>
        /// <param name="dataPoints">List of data points with known values.</param>
        /// <param name="targetX">X coordinate of the target point.</param>
        /// <param name="targetY">Y coordinate of the target point.</param>
        /// <param name="power">Power parameter for IDW (default: 2.0).</param>
        /// <param name="maxDistance">Maximum distance for IDW (default: infinity).</param>
        /// <param name="kNearestNeighbors">Number of nearest neighbors for optimized IDW (0 = use all).</param>
        /// <param name="rbfType">Type of RBF function (default: ThinPlateSpline).</param>
        /// <param name="rbfShapeParameter">Shape parameter for RBF (default: 1.0).</param>
        /// <param name="splineTension">Tension parameter for spline (default: 0.5).</param>
        /// <returns>Interpolated value at the target point.</returns>
        public static double Interpolate(
            InterpolationMethodType methodType,
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double targetX,
            double targetY,
            double power = 2.0,
            double maxDistance = double.MaxValue,
            int kNearestNeighbors = 0,
            RbfType rbfType = RbfType.ThinPlateSpline,
            double rbfShapeParameter = 1.0,
            double splineTension = 0.5)
        {
            return methodType switch
            {
                InterpolationMethodType.None => throw new ArgumentException("Cannot interpolate with None method."),
                InterpolationMethodType.InverseDistanceWeighting => 
                    InverseDistanceWeighting(dataPoints, targetX, targetY, power, maxDistance),
                InterpolationMethodType.OptimizedIdw => 
                    EnhancedInterpolation.OptimizedIdw(dataPoints, targetX, targetY, power, kNearestNeighbors, maxDistance),
                InterpolationMethodType.Kriging => 
                    Kriging(dataPoints, targetX, targetY),
                InterpolationMethodType.RadialBasisFunction => 
                    EnhancedInterpolation.RadialBasisFunction(dataPoints, targetX, targetY, rbfType, rbfShapeParameter),
                InterpolationMethodType.NaturalNeighbor => 
                    EnhancedInterpolation.NaturalNeighbor(dataPoints, targetX, targetY),
                InterpolationMethodType.Spline => 
                    EnhancedInterpolation.SplineInterpolation(dataPoints, targetX, targetY, splineTension),
                InterpolationMethodType.Bilinear => 
                    throw new NotImplementedException("Bilinear interpolation not yet implemented."),
                _ => InverseDistanceWeighting(dataPoints, targetX, targetY, power, maxDistance)
            };
        }

        /// <summary>
        /// Generates a grid of interpolated values using the specified method.
        /// </summary>
        /// <param name="methodType">The interpolation method to use.</param>
        /// <param name="dataPoints">List of data points with known values.</param>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <param name="cellSize">Size of each grid cell.</param>
        /// <param name="power">Power parameter for IDW (default: 2.0).</param>
        /// <param name="maxDistance">Maximum distance for IDW (default: infinity).</param>
        /// <param name="kNearestNeighbors">Number of nearest neighbors for optimized IDW (0 = use all).</param>
        /// <param name="rbfType">Type of RBF function (default: ThinPlateSpline).</param>
        /// <param name="rbfShapeParameter">Shape parameter for RBF (default: 1.0).</param>
        /// <param name="splineTension">Tension parameter for spline (default: 0.5).</param>
        /// <returns>2D array of interpolated values.</returns>
        public static double[,] GenerateGrid(
            InterpolationMethodType methodType,
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double minX,
            double maxX,
            double minY,
            double maxY,
            double cellSize = 10.0,
            double power = 2.0,
            double maxDistance = double.MaxValue,
            int kNearestNeighbors = 0,
            RbfType rbfType = RbfType.ThinPlateSpline,
            double rbfShapeParameter = 1.0,
            double splineTension = 0.5)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            int gridWidth = (int)((maxX - minX) / cellSize) + 1;
            int gridHeight = (int)((maxY - minY) / cellSize) + 1;
            var grid = new double[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    double targetX = minX + x * cellSize;
                    double targetY = minY + y * cellSize;

                    grid[x, y] = Interpolate(
                        methodType,
                        dataPoints,
                        targetX,
                        targetY,
                        power,
                        maxDistance,
                        kNearestNeighbors,
                        rbfType,
                        rbfShapeParameter,
                        splineTension);
                }
            }

            return grid;
        }
    }
}

