using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Performance
{
    /// <summary>
    /// Provides level-of-detail (LOD) rendering to optimize performance for large datasets.
    /// </summary>
    public static class LevelOfDetail
    {
        /// <summary>
        /// Reduces the number of points based on zoom level and point density.
        /// </summary>
        /// <param name="dataPoints">List of all data points.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="maxPoints">Maximum number of points to render (default: 1000).</param>
        /// <param name="minZoomForFullDetail">Minimum zoom level to show full detail (default: 1.0).</param>
        /// <returns>Filtered list of points to render.</returns>
        public static List<HEAT_MAP_DATA_POINT> ApplyLOD(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double zoom,
            int maxPoints = 1000,
            double minZoomForFullDetail = 1.0)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<HEAT_MAP_DATA_POINT>();

            // If zoomed in enough and points are within limit, return all
            if (zoom >= minZoomForFullDetail && dataPoints.Count <= maxPoints)
            {
                return dataPoints;
            }

            // Calculate target number of points based on zoom
            int targetPoints = CalculateTargetPoints(dataPoints.Count, zoom, maxPoints, minZoomForFullDetail);

            if (targetPoints >= dataPoints.Count)
            {
                return dataPoints;
            }

            // Use spatial sampling to reduce points
            return SpatialSample(dataPoints, targetPoints);
        }

        /// <summary>
        /// Calculates the target number of points based on zoom level.
        /// </summary>
        private static int CalculateTargetPoints(
            int totalPoints,
            double zoom,
            int maxPoints,
            double minZoomForFullDetail)
        {
            if (zoom >= minZoomForFullDetail)
            {
                return Math.Min(totalPoints, maxPoints);
            }

            // Scale down points proportionally to zoom
            double zoomRatio = zoom / minZoomForFullDetail;
            int target = (int)(maxPoints * zoomRatio);

            return Math.Max(1, Math.Min(target, totalPoints));
        }

        /// <summary>
        /// Performs spatial sampling to reduce point count while maintaining distribution.
        /// </summary>
        private static List<HEAT_MAP_DATA_POINT> SpatialSample(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int targetCount)
        {
            if (targetCount >= dataPoints.Count)
                return dataPoints;

            // Calculate grid size for spatial sampling
            double minX = dataPoints.Min(p => p.X);
            double maxX = dataPoints.Max(p => p.X);
            double minY = dataPoints.Min(p => p.Y);
            double maxY = dataPoints.Max(p => p.Y);

            double width = maxX - minX;
            double height = maxY - minY;

            if (width <= 0 || height <= 0)
                return dataPoints.Take(targetCount).ToList();

            // Calculate grid dimensions (aim for roughly square cells)
            int gridSize = (int)Math.Ceiling(Math.Sqrt(targetCount));
            double cellWidth = width / gridSize;
            double cellHeight = height / gridSize;

            // Sample one point per cell (highest value or first encountered)
            var sampled = new List<HEAT_MAP_DATA_POINT>();
            var grid = new Dictionary<(int, int), HEAT_MAP_DATA_POINT>();

            foreach (var point in dataPoints)
            {
                int gridX = (int)((point.X - minX) / cellWidth);
                int gridY = (int)((point.Y - minY) / cellHeight);

                gridX = Math.Max(0, Math.Min(gridSize - 1, gridX));
                gridY = Math.Max(0, Math.Min(gridSize - 1, gridY));

                var key = (gridX, gridY);

                if (!grid.ContainsKey(key))
                {
                    grid[key] = point;
                }
                else
                {
                    // Keep point with higher value
                    if (point.Value > grid[key].Value)
                    {
                        grid[key] = point;
                    }
                }
            }

            sampled.AddRange(grid.Values);

            // If we still have too many points, randomly sample
            if (sampled.Count > targetCount)
            {
                var random = new Random();
                sampled = sampled.OrderBy(x => random.Next()).Take(targetCount).ToList();
            }

            return sampled;
        }

        /// <summary>
        /// Applies LOD based on point density in viewport.
        /// </summary>
        /// <param name="dataPoints">List of data points in viewport.</param>
        /// <param name="viewportArea">Area of the viewport in data coordinates squared.</param>
        /// <param name="maxDensity">Maximum points per unit area (default: 10).</param>
        /// <returns>Filtered list of points.</returns>
        public static List<HEAT_MAP_DATA_POINT> ApplyDensityBasedLOD(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double viewportArea,
            double maxDensity = 10.0)
        {
            if (dataPoints == null || dataPoints.Count == 0 || viewportArea <= 0)
                return dataPoints;

            double currentDensity = dataPoints.Count / viewportArea;

            if (currentDensity <= maxDensity)
            {
                return dataPoints;
            }

            // Calculate target count
            int targetCount = (int)(maxDensity * viewportArea);

            return SpatialSample(dataPoints, targetCount);
        }

        /// <summary>
        /// Applies adaptive LOD that adjusts based on both zoom and density.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="viewportArea">Area of the viewport in data coordinates squared.</param>
        /// <param name="maxPoints">Maximum number of points (default: 1000).</param>
        /// <param name="maxDensity">Maximum points per unit area (default: 10).</param>
        /// <returns>Filtered list of points.</returns>
        public static List<HEAT_MAP_DATA_POINT> ApplyAdaptiveLOD(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double zoom,
            double viewportArea,
            int maxPoints = 1000,
            double maxDensity = 10.0)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<HEAT_MAP_DATA_POINT>();

            // Apply zoom-based LOD
            var zoomFiltered = ApplyLOD(dataPoints, zoom, maxPoints);

            // Apply density-based LOD if viewport area is known
            if (viewportArea > 0)
            {
                return ApplyDensityBasedLOD(zoomFiltered, viewportArea, maxDensity);
            }

            return zoomFiltered;
        }
    }
}

