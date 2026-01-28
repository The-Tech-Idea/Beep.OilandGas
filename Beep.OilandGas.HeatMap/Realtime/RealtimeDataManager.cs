using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.HeatMap.Realtime
{
    /// <summary>
    /// Manages real-time data updates for heat maps with change detection.
    /// </summary>
    public class RealtimeDataManager
    {
        private readonly List<HEAT_MAP_DATA_POINT> dataPoints;
        private readonly Dictionary<HEAT_MAP_DATA_POINT, DateTime> pointTimestamps;
        private readonly object lockObject = new object();

        /// <summary>
        /// Gets or sets the time window for considering data as "recent" (in seconds).
        /// </summary>
        public double RecentDataWindowSeconds { get; set; } = 60.0;

        /// <summary>
        /// Gets or sets whether to highlight newly added points.
        /// </summary>
        public bool HighlightNewPoints { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to highlight updated points.
        /// </summary>
        public bool HighlightUpdatedPoints { get; set; } = true;

        /// <summary>
        /// Gets or sets the highlight duration (in seconds).
        /// </summary>
        public double HighlightDurationSeconds { get; set; } = 5.0;

        /// <summary>
        /// Event raised when data points are added.
        /// </summary>
        public event EventHandler<List<HEAT_MAP_DATA_POINT>> PointsAdded;

        /// <summary>
        /// Event raised when data points are updated.
        /// </summary>
        public event EventHandler<List<HEAT_MAP_DATA_POINT>> PointsUpdated;

        /// <summary>
        /// Event raised when data points are removed.
        /// </summary>
        public event EventHandler<List<HEAT_MAP_DATA_POINT>> PointsRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeDataManager"/> class.
        /// </summary>
        /// <param name="initialDataPoints">Initial set of data points.</param>
        public RealtimeDataManager(List<HEAT_MAP_DATA_POINT> initialDataPoints = null)
        {
            dataPoints = initialDataPoints ?? new List<HEAT_MAP_DATA_POINT>();
            pointTimestamps = new Dictionary<HEAT_MAP_DATA_POINT, DateTime>();

            // Initialize timestamps for initial points
            var now = DateTime.Now;
            foreach (var point in dataPoints)
            {
                pointTimestamps[point] = now;
            }
        }

        /// <summary>
        /// Adds new data points.
        /// </summary>
        /// <param name="newPoints">List of new points to add.</param>
        public void AddPoints(List<HEAT_MAP_DATA_POINT> newPoints)
        {
            if (newPoints == null || newPoints.Count == 0)
                return;

            lock (lockObject)
            {
                var addedPoints = new List<HEAT_MAP_DATA_POINT>();
                var now = DateTime.Now;

                foreach (var point in newPoints)
                {
                    if (!dataPoints.Contains(point))
                    {
                        dataPoints.Add(point);
                        pointTimestamps[point] = now;
                        addedPoints.Add(point);
                    }
                }

                if (addedPoints.Count > 0)
                {
                    PointsAdded?.Invoke(this, addedPoints);
                }
            }
        }

        /// <summary>
        /// Updates existing data points.
        /// </summary>
        /// <param name="updatedPoints">List of points with updated values.</param>
        public void UpdatePoints(List<HEAT_MAP_DATA_POINT> updatedPoints)
        {
            if (updatedPoints == null || updatedPoints.Count == 0)
                return;

            lock (lockObject)
            {
                var changedPoints = new List<HEAT_MAP_DATA_POINT>();
                var now = DateTime.Now;

                foreach (var updatedPoint in updatedPoints)
                {
                    var existingPoint = dataPoints.FirstOrDefault(p =>
                        Math.Abs(p.X - updatedPoint.X) < 0.001 &&
                        Math.Abs(p.Y - updatedPoint.Y) < 0.001);

                    if (existingPoint != null)
                    {
                        // Check if value actually changed
                        if (Math.Abs(existingPoint.Value - updatedPoint.Value) > 0.001)
                        {
                            existingPoint.Value = updatedPoint.Value;
                            if (!string.IsNullOrEmpty(updatedPoint.LABEL))
                                existingPoint.LABEL = updatedPoint.LABEL;
                            pointTimestamps[existingPoint] = now;
                            changedPoints.Add(existingPoint);
                        }
                    }
                }

                if (changedPoints.Count > 0)
                {
                    PointsUpdated?.Invoke(this, changedPoints);
                }
            }
        }

        /// <summary>
        /// Removes data points.
        /// </summary>
        /// <param name="pointsToRemove">List of points to remove.</param>
        public void RemovePoints(List<HEAT_MAP_DATA_POINT> pointsToRemove)
        {
            if (pointsToRemove == null || pointsToRemove.Count == 0)
                return;

            lock (lockObject)
            {
                var removedPoints = new List<HEAT_MAP_DATA_POINT>();

                foreach (var point in pointsToRemove)
                {
                    if (dataPoints.Remove(point))
                    {
                        pointTimestamps.Remove(point);
                        removedPoints.Add(point);
                    }
                }

                if (removedPoints.Count > 0)
                {
                    PointsRemoved?.Invoke(this, removedPoints);
                }
            }
        }

        /// <summary>
        /// Gets all current data points.
        /// </summary>
        /// <returns>Copy of the current data points list.</returns>
        public List<HEAT_MAP_DATA_POINT> GetCurrentPoints()
        {
            lock (lockObject)
            {
                return new List<HEAT_MAP_DATA_POINT>(dataPoints);
            }
        }

        /// <summary>
        /// Gets points that have been added or updated recently.
        /// </summary>
        /// <returns>List of recently changed points.</returns>
        public List<HEAT_MAP_DATA_POINT> GetRecentPoints()
        {
            lock (lockObject)
            {
                var cutoffTime = DateTime.Now.AddSeconds(-RecentDataWindowSeconds);
                return dataPoints.Where(p =>
                    pointTimestamps.ContainsKey(p) &&
                    pointTimestamps[p] >= cutoffTime).ToList();
            }
        }

        /// <summary>
        /// Gets points that should be highlighted (newly added or recently updated).
        /// </summary>
        /// <returns>List of points to highlight.</returns>
        public List<HEAT_MAP_DATA_POINT> GetHighlightedPoints()
        {
            if (!HighlightNewPoints && !HighlightUpdatedPoints)
                return new List<HEAT_MAP_DATA_POINT>();

            lock (lockObject)
            {
                var cutoffTime = DateTime.Now.AddSeconds(-HighlightDurationSeconds);
                return dataPoints.Where(p =>
                    pointTimestamps.ContainsKey(p) &&
                    pointTimestamps[p] >= cutoffTime).ToList();
            }
        }

        /// <summary>
        /// Clears all data points.
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                var allPoints = new List<HEAT_MAP_DATA_POINT>(dataPoints);
                dataPoints.Clear();
                pointTimestamps.Clear();
                PointsRemoved?.Invoke(this, allPoints);
            }
        }

        /// <summary>
        /// Gets the count of data points.
        /// </summary>
        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return dataPoints.Count;
                }
            }
        }
    }
}

