using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Filtering
{
    /// <summary>
    /// Enumeration of filter types.
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Filter by value range.
        /// </summary>
        ValueRange,

        /// <summary>
        /// Filter by spatial bounds.
        /// </summary>
        SpatialBounds,

        /// <summary>
        /// Filter by label pattern.
        /// </summary>
        LabelPattern,

        /// <summary>
        /// Custom filter function.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents a filter for heat map data points.
    /// </summary>
    public class DataFilter
    {
        /// <summary>
        /// Gets or sets the filter type.
        /// </summary>
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Gets or sets the minimum value (for ValueRange filter).
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value (for ValueRange filter).
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum X coordinate (for SpatialBounds filter).
        /// </summary>
        public double? MinX { get; set; }

        /// <summary>
        /// Gets or sets the maximum X coordinate (for SpatialBounds filter).
        /// </summary>
        public double? MaxX { get; set; }

        /// <summary>
        /// Gets or sets the minimum Y coordinate (for SpatialBounds filter).
        /// </summary>
        public double? MinY { get; set; }

        /// <summary>
        /// Gets or sets the maximum Y coordinate (for SpatialBounds filter).
        /// </summary>
        public double? MaxY { get; set; }

        /// <summary>
        /// Gets or sets the label pattern (for LabelPattern filter).
        /// </summary>
        public string LabelPattern { get; set; }

        /// <summary>
        /// Gets or sets whether label pattern matching is case-sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets the custom filter function (for Custom filter).
        /// </summary>
        public Func<HEAT_MAP_DATA_POINT, bool> CustomFilter { get; set; }

        /// <summary>
        /// Applies the filter to a list of data points.
        /// </summary>
        /// <param name="dataPoints">List of data points to filter.</param>
        /// <returns>Filtered list of data points.</returns>
        public List<HEAT_MAP_DATA_POINT> Apply(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (dataPoints == null)
                return new List<HEAT_MAP_DATA_POINT>();

            return FilterType switch
            {
                FilterType.ValueRange => ApplyValueRangeFilter(dataPoints),
                FilterType.SpatialBounds => ApplySpatialBoundsFilter(dataPoints),
                FilterType.LabelPattern => ApplyLabelPatternFilter(dataPoints),
                FilterType.Custom => ApplyCustomFilter(dataPoints),
                _ => dataPoints
            };
        }

        /// <summary>
        /// Applies value range filter.
        /// </summary>
        private List<HEAT_MAP_DATA_POINT> ApplyValueRangeFilter(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            return dataPoints.Where(p =>
            {
                bool matches = true;
                if (MinValue.HasValue)
                    matches = matches && p.Value >= MinValue.Value;
                if (MaxValue.HasValue)
                    matches = matches && p.Value <= MaxValue.Value;
                return matches;
            }).ToList();
        }

        /// <summary>
        /// Applies spatial bounds filter.
        /// </summary>
        private List<HEAT_MAP_DATA_POINT> ApplySpatialBoundsFilter(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            return dataPoints.Where(p =>
            {
                bool matches = true;
                if (MinX.HasValue)
                    matches = matches && p.X >= MinX.Value;
                if (MaxX.HasValue)
                    matches = matches && p.X <= MaxX.Value;
                if (MinY.HasValue)
                    matches = matches && p.Y >= MinY.Value;
                if (MaxY.HasValue)
                    matches = matches && p.Y <= MaxY.Value;
                return matches;
            }).ToList();
        }

        /// <summary>
        /// Applies label pattern filter.
        /// </summary>
        private List<HEAT_MAP_DATA_POINT> ApplyLabelPatternFilter(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (string.IsNullOrEmpty(LabelPattern))
                return dataPoints;

            StringComparison comparison = CaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            return dataPoints.Where(p =>
                !string.IsNullOrEmpty(p.LABEL) &&
                p.LABEL.Contains(LabelPattern, comparison)).ToList();
        }

        /// <summary>
        /// Applies custom filter.
        /// </summary>
        private List<HEAT_MAP_DATA_POINT> ApplyCustomFilter(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (CustomFilter == null)
                return dataPoints;

            return dataPoints.Where(CustomFilter).ToList();
        }
    }

    /// <summary>
    /// Manages multiple filters and applies them in combination.
    /// </summary>
    public class FilterManager
    {
        /// <summary>
        /// Gets the list of active filters.
        /// </summary>
        public List<DataFilter> Filters { get; }

        /// <summary>
        /// Gets or sets whether filters are combined with AND (all must match) or OR (any must match).
        /// </summary>
        public bool UseAndLogic { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterManager"/> class.
        /// </summary>
        public FilterManager()
        {
            Filters = new List<DataFilter>();
        }

        /// <summary>
        /// Adds a filter to the manager.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        public void AddFilter(DataFilter filter)
        {
            if (filter != null)
                Filters.Add(filter);
        }

        /// <summary>
        /// Removes a filter from the manager.
        /// </summary>
        /// <param name="filter">The filter to remove.</param>
        public void RemoveFilter(DataFilter filter)
        {
            Filters.Remove(filter);
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        public void ClearFilters()
        {
            Filters.Clear();
        }

        /// <summary>
        /// Applies all filters to a list of data points.
        /// </summary>
        /// <param name="dataPoints">List of data points to filter.</param>
        /// <returns>Filtered list of data points.</returns>
        public List<HEAT_MAP_DATA_POINT> ApplyFilters(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (dataPoints == null || Filters.Count == 0)
                return dataPoints ?? new List<HEAT_MAP_DATA_POINT>();

            if (UseAndLogic)
            {
                // All filters must match
                var result = dataPoints;
                foreach (var filter in Filters)
                {
                    result = filter.Apply(result);
                }
                return result;
            }
            else
            {
                // Any filter can match (OR logic)
                var resultSet = new HashSet<HEAT_MAP_DATA_POINT>();
                foreach (var filter in Filters)
                {
                    var filtered = filter.Apply(dataPoints);
                    foreach (var point in filtered)
                    {
                        resultSet.Add(point);
                    }
                }
                return resultSet.ToList();
            }
        }

        /// <summary>
        /// Creates a value range filter.
        /// </summary>
        public static DataFilter CreateValueRangeFilter(double? minValue, double? maxValue)
        {
            return new DataFilter
            {
                FilterType = FilterType.ValueRange,
                MinValue = minValue,
                MaxValue = maxValue
            };
        }

        /// <summary>
        /// Creates a spatial bounds filter.
        /// </summary>
        public static DataFilter CreateSpatialBoundsFilter(
            double? minX, double? maxX, double? minY, double? maxY)
        {
            return new DataFilter
            {
                FilterType = FilterType.SpatialBounds,
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY
            };
        }

        /// <summary>
        /// Creates a label pattern filter.
        /// </summary>
        public static DataFilter CreateLabelPatternFilter(string pattern, bool caseSensitive = false)
        {
            return new DataFilter
            {
                FilterType = FilterType.LabelPattern,
                LabelPattern = pattern,
                CaseSensitive = caseSensitive
            };
        }

        /// <summary>
        /// Creates a custom filter.
        /// </summary>
        public static DataFilter CreateCustomFilter(Func<HEAT_MAP_DATA_POINT, bool> filterFunction)
        {
            return new DataFilter
            {
                FilterType = FilterType.Custom,
                CustomFilter = filterFunction
            };
        }
    }
}

