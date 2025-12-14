using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Performance
{
    /// <summary>
    /// Represents a bounding box for spatial indexing.
    /// </summary>
    public struct BoundingBox
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }

        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public bool Contains(double x, double y)
        {
            return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
        }

        public bool Intersects(BoundingBox other)
        {
            return !(MaxX < other.MinX || MinX > other.MaxX ||
                     MaxY < other.MinY || MinY > other.MaxY);
        }

        public double Width => MaxX - MinX;
        public double Height => MaxY - MinY;
    }

    /// <summary>
    /// A simple QuadTree implementation for spatial indexing of heat map data points.
    /// </summary>
    public class QuadTree
    {
        private const int MaxPointsPerNode = 10;
        private const int MaxDepth = 10;

        private readonly BoundingBox bounds;
        private readonly List<HeatMapDataPoint> points;
        private readonly int depth;
        private QuadTree[] children;
        private bool isLeaf;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadTree"/> class.
        /// </summary>
        /// <param name="bounds">The bounding box for this node.</param>
        /// <param name="depth">The depth of this node in the tree.</param>
        public QuadTree(BoundingBox bounds, int depth = 0)
        {
            this.bounds = bounds;
            this.depth = depth;
            this.points = new List<HeatMapDataPoint>();
            this.isLeaf = true;
        }

        /// <summary>
        /// Inserts a point into the QuadTree.
        /// </summary>
        /// <param name="point">The point to insert.</param>
        public void Insert(HeatMapDataPoint point)
        {
            if (!bounds.Contains(point.X, point.Y))
                return;

            if (isLeaf)
            {
                points.Add(point);

                // Split if we exceed the maximum points and haven't reached max depth
                if (points.Count > MaxPointsPerNode && depth < MaxDepth)
                {
                    Split();
                }
            }
            else
            {
                // Insert into appropriate child
                InsertIntoChild(point);
            }
        }

        /// <summary>
        /// Queries all points within the specified bounding box.
        /// </summary>
        /// <param name="queryBounds">The bounding box to query.</param>
        /// <returns>List of points within the query bounds.</returns>
        public List<HeatMapDataPoint> Query(BoundingBox queryBounds)
        {
            var results = new List<HeatMapDataPoint>();

            if (!bounds.Intersects(queryBounds))
                return results;

            if (isLeaf)
            {
                // Check all points in this leaf
                foreach (var point in points)
                {
                    if (queryBounds.Contains(point.X, point.Y))
                    {
                        results.Add(point);
                    }
                }
            }
            else
            {
                // Query children
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        results.AddRange(child.Query(queryBounds));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Queries all points within a radius of the specified point.
        /// </summary>
        /// <param name="centerX">X coordinate of the center point.</param>
        /// <param name="centerY">Y coordinate of the center point.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>List of points within the radius.</returns>
        public List<HeatMapDataPoint> QueryRadius(double centerX, double centerY, double radius)
        {
            var queryBounds = new BoundingBox(
                centerX - radius,
                centerY - radius,
                centerX + radius,
                centerY + radius);

            var candidates = Query(queryBounds);

            // Filter to exact radius (query gives bounding box, need to check actual distance)
            var results = new List<HeatMapDataPoint>();
            double radiusSquared = radius * radius;

            foreach (var point in candidates)
            {
                double dx = point.X - centerX;
                double dy = point.Y - centerY;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= radiusSquared)
                {
                    results.Add(point);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all points in the tree.
        /// </summary>
        /// <returns>List of all points.</returns>
        public List<HeatMapDataPoint> GetAllPoints()
        {
            var results = new List<HeatMapDataPoint>();

            if (isLeaf)
            {
                results.AddRange(points);
            }
            else
            {
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        results.AddRange(child.GetAllPoints());
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Splits this node into four children.
        /// </summary>
        private void Split()
        {
            double halfWidth = bounds.Width / 2.0;
            double halfHeight = bounds.Height / 2.0;
            double midX = bounds.MinX + halfWidth;
            double midY = bounds.MinY + halfHeight;

            children = new QuadTree[4];
            children[0] = new QuadTree(new BoundingBox(bounds.MinX, bounds.MinY, midX, midY), depth + 1); // NW
            children[1] = new QuadTree(new BoundingBox(midX, bounds.MinY, bounds.MaxX, midY), depth + 1); // NE
            children[2] = new QuadTree(new BoundingBox(bounds.MinX, midY, midX, bounds.MaxY), depth + 1); // SW
            children[3] = new QuadTree(new BoundingBox(midX, midY, bounds.MaxX, bounds.MaxY), depth + 1); // SE

            isLeaf = false;

            // Redistribute points to children
            foreach (var point in points)
            {
                InsertIntoChild(point);
            }

            points.Clear();
        }

        /// <summary>
        /// Inserts a point into the appropriate child node.
        /// </summary>
        private void InsertIntoChild(HeatMapDataPoint point)
        {
            double halfWidth = bounds.Width / 2.0;
            double halfHeight = bounds.Height / 2.0;
            double midX = bounds.MinX + halfWidth;
            double midY = bounds.MinY + halfHeight;

            int index = 0;
            if (point.X >= midX) index += 1; // East
            if (point.Y >= midY) index += 2; // South

            children[index].Insert(point);
        }
    }

    /// <summary>
    /// Provides spatial indexing for heat map data points using QuadTree.
    /// </summary>
    public class SpatialIndex
    {
        private QuadTree quadTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialIndex"/> class.
        /// </summary>
        /// <param name="dataPoints">List of data points to index.</param>
        public SpatialIndex(List<HeatMapDataPoint> dataPoints)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points list cannot be null or empty.");

            // Calculate bounding box
            double minX = dataPoints.Min(p => p.X);
            double maxX = dataPoints.Max(p => p.X);
            double minY = dataPoints.Min(p => p.Y);
            double maxY = dataPoints.Max(p => p.Y);

            var bounds = new BoundingBox(minX, minY, maxX, maxY);
            quadTree = new QuadTree(bounds);

            // Insert all points
            foreach (var point in dataPoints)
            {
                quadTree.Insert(point);
            }
        }

        /// <summary>
        /// Queries points within the specified bounding box.
        /// </summary>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <returns>List of points within the bounds.</returns>
        public List<HeatMapDataPoint> QueryBounds(double minX, double minY, double maxX, double maxY)
        {
            var queryBounds = new BoundingBox(minX, minY, maxX, maxY);
            return quadTree.Query(queryBounds);
        }

        /// <summary>
        /// Queries points within a radius of the specified point.
        /// </summary>
        /// <param name="x">X coordinate of the center point.</param>
        /// <param name="y">Y coordinate of the center point.</param>
        /// <param name="radius">Search radius.</param>
        /// <returns>List of points within the radius.</returns>
        public List<HeatMapDataPoint> QueryRadius(double x, double y, double radius)
        {
            return quadTree.QueryRadius(x, y, radius);
        }

        /// <summary>
        /// Finds the nearest point to the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="maxDistance">Maximum distance to search (default: infinity).</param>
        /// <returns>The nearest point, or null if none found within maxDistance.</returns>
        public HeatMapDataPoint FindNearest(double x, double y, double maxDistance = double.MaxValue)
        {
            // Start with a small radius and expand if needed
            double searchRadius = Math.Min(100, maxDistance);
            List<HeatMapDataPoint> candidates;

            do
            {
                candidates = quadTree.QueryRadius(x, y, searchRadius);
                if (candidates.Count > 0)
                    break;

                searchRadius *= 2;
            } while (searchRadius < maxDistance);

            if (candidates.Count == 0)
                return null;

            // Find the actual nearest point
            HeatMapDataPoint nearest = null;
            double minDistanceSquared = maxDistance * maxDistance;

            foreach (var point in candidates)
            {
                double dx = point.X - x;
                double dy = point.Y - y;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearest = point;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Gets all points in the index.
        /// </summary>
        /// <returns>List of all points.</returns>
        public List<HeatMapDataPoint> GetAllPoints()
        {
            return quadTree.GetAllPoints();
        }
    }
}

