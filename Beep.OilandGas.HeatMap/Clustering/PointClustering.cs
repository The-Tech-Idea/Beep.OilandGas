using Beep.OilandGas.Models.HeatMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Clustering
{
    /// <summary>
    /// Represents a cluster of data points.
    /// </summary>
    public class Cluster
    {
        /// <summary>
        /// Gets or sets the center X coordinate of the cluster.
        /// </summary>
        public double CenterX { get; set; }

        /// <summary>
        /// Gets or sets the center Y coordinate of the cluster.
        /// </summary>
        public double CenterY { get; set; }

        /// <summary>
        /// Gets or sets the points in this cluster.
        /// </summary>
        public List<HEAT_MAP_DATA_POINT> Points { get; set; }

        /// <summary>
        /// Gets or sets the aggregated value (e.g., average, sum, max).
        /// </summary>
        public double AggregatedValue { get; set; }

        /// <summary>
        /// Gets or sets the cluster radius.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Gets the number of points in the cluster.
        /// </summary>
        public int Count => Points?.Count ?? 0;

        public Cluster()
        {
            Points = new List<HEAT_MAP_DATA_POINT>();
        }
    }

    /// <summary>
    /// Enumeration of clustering algorithms.
    /// </summary>
    public enum ClusteringAlgorithm
    {
        /// <summary>
        /// Grid-based clustering (fast, simple).
        /// </summary>
        GridBased,

        /// <summary>
        /// Distance-based clustering (DBSCAN-like).
        /// </summary>
        DistanceBased,

        /// <summary>
        /// K-means clustering (requires number of clusters).
        /// </summary>
        KMeans
    }

    /// <summary>
    /// Enumeration of aggregation methods for cluster values.
    /// </summary>
    public enum AggregationMethod
    {
        /// <summary>
        /// Average of all point values.
        /// </summary>
        Average,

        /// <summary>
        /// Sum of all point values.
        /// </summary>
        Sum,

        /// <summary>
        /// Maximum value in cluster.
        /// </summary>
        Max,

        /// <summary>
        /// Minimum value in cluster.
        /// </summary>
        Min,

        /// <summary>
        /// Median value in cluster.
        /// </summary>
        Median
    }

    /// <summary>
    /// Provides clustering functionality for heat map data points.
    /// </summary>
    public static class PointClustering
    {
        /// <summary>
        /// Performs grid-based clustering.
        /// </summary>
        /// <param name="dataPoints">List of data points to cluster.</param>
        /// <param name="gridSize">Size of each grid cell.</param>
        /// <param name="aggregationMethod">Method to aggregate values in each cell.</param>
        /// <returns>List of clusters.</returns>
        public static List<Cluster> GridBasedClustering(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double gridSize,
            AggregationMethod aggregationMethod = AggregationMethod.Average)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<Cluster>();

            // Calculate bounds
            double minX = dataPoints.Min(p => p.X);
            double maxX = dataPoints.Max(p => p.X);
            double minY = dataPoints.Min(p => p.Y);
            double maxY = dataPoints.Max(p => p.Y);

            // Create grid
            int cols = (int)Math.Ceiling((maxX - minX) / gridSize);
            int rows = (int)Math.Ceiling((maxY - minY) / gridSize);

            var clusters = new Dictionary<(int, int), Cluster>();

            // Assign points to grid cells
            foreach (var point in dataPoints)
            {
                int col = (int)((point.X - minX) / gridSize);
                int row = (int)((point.Y - minY) / gridSize);

                col = Math.Max(0, Math.Min(cols - 1, col));
                row = Math.Max(0, Math.Min(rows - 1, row));

                var key = (col, row);

                if (!clusters.ContainsKey(key))
                {
                    clusters[key] = new Cluster
                    {
                        CenterX = minX + (col + 0.5) * gridSize,
                        CenterY = minY + (row + 0.5) * gridSize,
                        Radius = gridSize / 2.0
                    };
                }

                clusters[key].Points.Add(point);
            }

            // Aggregate values
            foreach (var cluster in clusters.Values)
            {
                cluster.AggregatedValue = AggregateValues(cluster.Points, aggregationMethod);
            }

            return clusters.Values.ToList();
        }

        /// <summary>
        /// Performs distance-based clustering (DBSCAN-like).
        /// </summary>
        /// <param name="dataPoints">List of data points to cluster.</param>
        /// <param name="epsilon">Maximum distance between points in the same cluster.</param>
        /// <param name="minPoints">Minimum number of points required to form a cluster.</param>
        /// <param name="aggregationMethod">Method to aggregate values in each cluster.</param>
        /// <returns>List of clusters.</returns>
        public static List<Cluster> DistanceBasedClustering(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double epsilon,
            int minPoints = 3,
            AggregationMethod aggregationMethod = AggregationMethod.Average)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<Cluster>();

            var clusters = new List<Cluster>();
            var visited = new HashSet<HEAT_MAP_DATA_POINT>();
            var noise = new HashSet<HEAT_MAP_DATA_POINT>();

            foreach (var point in dataPoints)
            {
                if (visited.Contains(point))
                    continue;

                visited.Add(point);

                var neighbors = GetNeighbors(point, dataPoints, epsilon);

                if (neighbors.Count < minPoints)
                {
                    noise.Add(point);
                    continue;
                }

                // Create new cluster
                var cluster = new Cluster
                {
                    Points = new List<HEAT_MAP_DATA_POINT> { point }
                };
                cluster.Points.AddRange(neighbors);

                // Expand cluster
                var seedSet = new Queue<HEAT_MAP_DATA_POINT>(neighbors);
                while (seedSet.Count > 0)
                {
                    var currentPoint = seedSet.Dequeue();
                    if (visited.Contains(currentPoint))
                        continue;

                    visited.Add(currentPoint);
                    var currentNeighbors = GetNeighbors(currentPoint, dataPoints, epsilon);

                    if (currentNeighbors.Count >= minPoints)
                    {
                        foreach (var neighbor in currentNeighbors)
                        {
                            if (!cluster.Points.Contains(neighbor))
                            {
                                cluster.Points.Add(neighbor);
                                seedSet.Enqueue(neighbor);
                            }
                        }
                    }
                }

                // Calculate cluster center and aggregate value
                cluster.CenterX = cluster.Points.Average(p => p.X);
                cluster.CenterY = cluster.Points.Average(p => p.Y);
                cluster.AggregatedValue = AggregateValues(cluster.Points, aggregationMethod);

                // Calculate radius as maximum distance from center
                double maxDistance = 0;
                foreach (var p in cluster.Points)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(p.X - cluster.CenterX, 2) +
                        Math.Pow(p.Y - cluster.CenterY, 2));
                    maxDistance = Math.Max(maxDistance, distance);
                }
                cluster.Radius = maxDistance;

                clusters.Add(cluster);
            }

            return clusters;
        }

        /// <summary>
        /// Performs K-means clustering.
        /// </summary>
        /// <param name="dataPoints">List of data points to cluster.</param>
        /// <param name="k">Number of clusters.</param>
        /// <param name="maxIterations">Maximum number of iterations.</param>
        /// <param name="aggregationMethod">Method to aggregate values in each cluster.</param>
        /// <returns>List of clusters.</returns>
        public static List<Cluster> KMeansClustering(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int k,
            int maxIterations = 100,
            AggregationMethod aggregationMethod = AggregationMethod.Average)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<Cluster>();

            if (k <= 0 || k > dataPoints.Count)
                k = Math.Max(1, Math.Min(k, dataPoints.Count));

            // Initialize centroids randomly
            var random = new Random();
            var centroids = new List<(double x, double y)>();
            var usedIndices = new HashSet<int>();

            for (int i = 0; i < k; i++)
            {
                int index;
                do
                {
                    index = random.Next(dataPoints.Count);
                } while (usedIndices.Contains(index));

                usedIndices.Add(index);
                centroids.Add((dataPoints[index].X, dataPoints[index].Y));
            }

            var clusters = new List<Cluster>();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Assign points to nearest centroid
                clusters = new List<Cluster>();
                for (int i = 0; i < k; i++)
                {
                    clusters.Add(new Cluster
                    {
                        CenterX = centroids[i].x,
                        CenterY = centroids[i].y
                    });
                }

                foreach (var point in dataPoints)
                {
                    int nearestCluster = 0;
                    double minDistance = double.MaxValue;

                    for (int i = 0; i < k; i++)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(point.X - centroids[i].x, 2) +
                            Math.Pow(point.Y - centroids[i].y, 2));

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestCluster = i;
                        }
                    }

                    clusters[nearestCluster].Points.Add(point);
                }

                // Update centroids
                bool converged = true;
                for (int i = 0; i < k; i++)
                {
                    if (clusters[i].Points.Count > 0)
                    {
                        double newX = clusters[i].Points.Average(p => p.X);
                        double newY = clusters[i].Points.Average(p => p.Y);

                        if (Math.Abs(newX - centroids[i].x) > 0.001 ||
                            Math.Abs(newY - centroids[i].y) > 0.001)
                        {
                            converged = false;
                        }

                        centroids[i] = (newX, newY);
                        clusters[i].CenterX = newX;
                        clusters[i].CenterY = newY;
                    }
                }

                if (converged)
                    break;
            }

            // Calculate aggregated values and radii
            foreach (var cluster in clusters)
            {
                if (cluster.Points.Count > 0)
                {
                    cluster.AggregatedValue = AggregateValues(cluster.Points, aggregationMethod);

                    double maxDistance = 0;
                    foreach (var p in cluster.Points)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(p.X - cluster.CenterX, 2) +
                            Math.Pow(p.Y - cluster.CenterY, 2));
                        maxDistance = Math.Max(maxDistance, distance);
                    }
                    cluster.Radius = maxDistance;
                }
            }

            return clusters.Where(c => c.Points.Count > 0).ToList();
        }

        /// <summary>
        /// Aggregates values from a list of points using the specified method.
        /// </summary>
        private static double AggregateValues(List<HEAT_MAP_DATA_POINT> points, AggregationMethod method)
        {
            if (points == null || points.Count == 0)
                return 0;

            return method switch
            {
                AggregationMethod.Average => points.Average(p => p.Value),
                AggregationMethod.Sum => points.Sum(p => p.Value),
                AggregationMethod.Max => points.Max(p => p.Value),
                AggregationMethod.Min => points.Min(p => p.Value),
                AggregationMethod.Median => CalculateMedian(points.Select(p => p.Value).ToList()),
                _ => points.Average(p => p.Value)
            };
        }

        /// <summary>
        /// Calculates the median of a list of values.
        /// </summary>
        private static double CalculateMedian(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
            {
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
            else
            {
                return sorted[mid];
            }
        }

        /// <summary>
        /// Gets neighbors of a point within epsilon distance.
        /// </summary>
        private static List<HEAT_MAP_DATA_POINT> GetNeighbors(
            HEAT_MAP_DATA_POINT point,
            List<HEAT_MAP_DATA_POINT> allPoints,
            double epsilon)
        {
            var neighbors = new List<HEAT_MAP_DATA_POINT>();
            double epsilonSquared = epsilon * epsilon;

            foreach (var other in allPoints)
            {
                if (other == point)
                    continue;

                double dx = other.X - point.X;
                double dy = other.Y - point.Y;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= epsilonSquared)
                {
                    neighbors.Add(other);
                }
            }

            return neighbors;
        }
    }
}

