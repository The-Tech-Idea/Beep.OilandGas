using Beep.OilandGas.Models.HeatMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Density
{
    /// <summary>
    /// Types of kernel functions for density estimation.
    /// </summary>
    public enum KernelType
    {
        /// <summary>
        /// Gaussian kernel (normal distribution).
        /// </summary>
        Gaussian,

        /// <summary>
        /// Epanechnikov kernel (parabolic).
        /// </summary>
        Epanechnikov,

        /// <summary>
        /// Uniform kernel (box).
        /// </summary>
        Uniform,

        /// <summary>
        /// Triangular kernel.
        /// </summary>
        Triangular
    }

    /// <summary>
    /// Provides kernel density estimation (KDE) for heatmaps.
    /// </summary>
    public static class KernelDensityEstimation
    {
        /// <summary>
        /// Calculates kernel density estimation for a grid of points.
        /// </summary>
        /// <param name="dataPoints">The data points.</param>
        /// <param name="gridWidth">Width of the output grid.</param>
        /// <param name="gridHeight">Height of the output grid.</param>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <param name="bandwidth">Bandwidth parameter (default: auto-calculated).</param>
        /// <param name="kernelType">Type of kernel to use.</param>
        /// <returns>2D array of density values.</returns>
        public static double[,] CalculateDensity(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int gridWidth,
            int gridHeight,
            double minX,
            double maxX,
            double minY,
            double maxY,
            double? bandwidth = null,
            KernelType kernelType = KernelType.Gaussian)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new double[gridWidth, gridHeight];

            // Auto-calculate bandwidth if not provided (Silverman's rule of thumb)
            if (!bandwidth.HasValue)
            {
                bandwidth = CalculateOptimalBandwidth(dataPoints);
            }

            var density = new double[gridWidth, gridHeight];
            double cellWidth = (maxX - minX) / gridWidth;
            double cellHeight = (maxY - minY) / gridHeight;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    double targetX = minX + (x + 0.5) * cellWidth;
                    double targetY = minY + (y + 0.5) * cellHeight;

                    double densityValue = 0;

                    foreach (var point in dataPoints)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(point.X - targetX, 2) +
                            Math.Pow(point.Y - targetY, 2));

                        densityValue += KernelFunction(distance / bandwidth.Value, kernelType);
                    }

                    density[x, y] = densityValue / (dataPoints.Count * bandwidth.Value);
                }
            }

            return density;
        }

        /// <summary>
        /// Calculates optimal bandwidth using Silverman's rule of thumb.
        /// </summary>
        private static double CalculateOptimalBandwidth(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (dataPoints.Count < 2)
                return 1.0;

            // Calculate standard deviation of distances
            var distances = new List<double>();
            for (int i = 0; i < Math.Min(100, dataPoints.Count); i++) // Sample for performance
            {
                for (int j = i + 1; j < Math.Min(100, dataPoints.Count); j++)
                {
                    double dist = Math.Sqrt(
                        Math.Pow(dataPoints[i].X - dataPoints[j].X, 2) +
                        Math.Pow(dataPoints[i].Y - dataPoints[j].Y, 2));
                    distances.Add(dist);
                }
            }

            if (distances.Count == 0)
                return 1.0;

            double mean = distances.Average();
            double variance = distances.Sum(d => Math.Pow(d - mean, 2)) / distances.Count;
            double stdDev = Math.Sqrt(variance);

            // Silverman's rule of thumb: h = 1.06 * Ïƒ * n^(-1/5)
            double n = dataPoints.Count;
            return 1.06 * stdDev * Math.Pow(n, -0.2);
        }

        /// <summary>
        /// Evaluates a kernel function at the given distance.
        /// </summary>
        private static double KernelFunction(double u, KernelType kernelType)
        {
            double absU = Math.Abs(u);

            return kernelType switch
            {
                KernelType.Gaussian => Math.Exp(-0.5 * u * u) / Math.Sqrt(2 * Math.PI),
                KernelType.Epanechnikov => absU <= 1 ? 0.75 * (1 - u * u) : 0,
                KernelType.Uniform => absU <= 1 ? 0.5 : 0,
                KernelType.Triangular => absU <= 1 ? (1 - absU) : 0,
                _ => Math.Exp(-0.5 * u * u) / Math.Sqrt(2 * Math.PI)
            };
        }

        /// <summary>
        /// Generates contour levels from density grid.
        /// </summary>
        /// <param name="density">The density grid.</param>
        /// <param name="numLevels">Number of contour levels.</param>
        /// <returns>Array of contour level values.</returns>
        public static double[] GenerateContourLevels(double[,] density, int numLevels = 10)
        {
            if (density == null)
                return new double[0];

            var values = new List<double>();
            int width = density.GetLength(0);
            int height = density.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    values.Add(density[x, y]);
                }
            }

            if (values.Count == 0)
                return new double[0];

            double min = values.Min();
            double max = values.Max();

            var levels = new double[numLevels];
            for (int i = 0; i < numLevels; i++)
            {
                levels[i] = min + (max - min) * (i + 1) / (numLevels + 1);
            }

            return levels;
        }
    }
}

