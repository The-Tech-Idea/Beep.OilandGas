using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.HeatMap;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Aggregation
{
    /// <summary>
    /// Aggregation methods for binning data.
    /// </summary>
    public enum AggregationMethod
    {
        /// <summary>
        /// Average of values in bin.
        /// </summary>
        Average,

        /// <summary>
        /// Sum of values in bin.
        /// </summary>
        Sum,

        /// <summary>
        /// Maximum value in bin.
        /// </summary>
        Max,

        /// <summary>
        /// Minimum value in bin.
        /// </summary>
        Min,

        /// <summary>
        /// Median value in bin.
        /// </summary>
        Median,

        /// <summary>
        /// Count of points in bin.
        /// </summary>
        Count
    }

    /// <summary>
    /// Represents a hexbin cell with aggregated data.
    /// </summary>
    public class HexbinCell
    {
        /// <summary>
        /// Gets or sets the center X coordinate.
        /// </summary>
        public double CenterX { get; set; }

        /// <summary>
        /// Gets or sets the center Y coordinate.
        /// </summary>
        public double CenterY { get; set; }

        /// <summary>
        /// Gets or sets the aggregated value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the count of points in this cell.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the hexagon radius.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Gets the points in this cell.
        /// </summary>
        public List<HEAT_MAP_DATA_POINT> Points { get; } = new List<HEAT_MAP_DATA_POINT>();
    }

    /// <summary>
    /// Represents a grid cell with aggregated data.
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// Gets or sets the cell X index.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the cell Y index.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the cell bounds (minX, minY, maxX, maxY).
        /// </summary>
        public (double minX, double minY, double maxX, double maxY) Bounds { get; set; }

        /// <summary>
        /// Gets or sets the aggregated value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the count of points in this cell.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets the points in this cell.
        /// </summary>
        public List<HEAT_MAP_DATA_POINT> Points { get; } = new List<HEAT_MAP_DATA_POINT>();
    }

    /// <summary>
    /// Provides data aggregation methods for heatmaps (Hexbin and Grid).
    /// </summary>
    public static class DataAggregation
    {
        /// <summary>
        /// Creates hexbin aggregation of data points.
        /// </summary>
        /// <param name="dataPoints">The data points to aggregate.</param>
        /// <param name="radius">The radius of each hexagon.</param>
        /// <param name="method">The aggregation method to use.</param>
        /// <returns>List of hexbin cells with aggregated values.</returns>
        public static List<HexbinCell> CreateHexbin(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double radius,
            AggregationMethod method = AggregationMethod.Average)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<HexbinCell>();

            var cells = new Dictionary<(int q, int r), HexbinCell>();

            foreach (var point in dataPoints)
            {
                // Convert to hex coordinates (axial/cube coordinates)
                var (q, r) = PixelToHex(point.X, point.Y, radius);

                if (!cells.ContainsKey((q, r)))
                {
                    var (centerX, centerY) = HexToPixel(q, r, radius);
                    cells[(q, r)] = new HexbinCell
                    {
                        CenterX = centerX,
                        CenterY = centerY,
                        Radius = radius
                    };
                }

                cells[(q, r)].Points.Add(point);
            }

            // Aggregate values
            var result = new List<HexbinCell>();
            foreach (var cell in cells.Values)
            {
                cell.Count = cell.Points.Count;
                cell.Value = AggregateValues(cell.Points, method);
                result.Add(cell);
            }

            return result;
        }

        /// <summary>
        /// Creates grid aggregation of data points.
        /// </summary>
        /// <param name="dataPoints">The data points to aggregate.</param>
        /// <param name="cellSize">The size of each grid cell.</param>
        /// <param name="method">The aggregation method to use.</param>
        /// <param name="minX">Minimum X coordinate (optional, auto-calculated if not provided).</param>
        /// <param name="maxX">Maximum X coordinate (optional, auto-calculated if not provided).</param>
        /// <param name="minY">Minimum Y coordinate (optional, auto-calculated if not provided).</param>
        /// <param name="maxY">Maximum Y coordinate (optional, auto-calculated if not provided).</param>
        /// <returns>List of grid cells with aggregated values.</returns>
        public static List<GridCell> CreateGrid(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            double cellSize,
            AggregationMethod method = AggregationMethod.Average,
            double? minX = null,
            double? maxX = null,
            double? minY = null,
            double? maxY = null)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<GridCell>();

            // Calculate bounds if not provided
            if (!minX.HasValue || !maxX.HasValue || !minY.HasValue || !maxY.HasValue)
            {
                minX = dataPoints.Min(p => p.X);
                maxX = dataPoints.Max(p => p.X);
                minY = dataPoints.Min(p => p.Y);
                maxY = dataPoints.Max(p => p.Y);
            }

            int cols = (int)Math.Ceiling((maxX.Value - minX.Value) / cellSize);
            int rows = (int)Math.Ceiling((maxY.Value - minY.Value) / cellSize);

            var cells = new Dictionary<(int x, int y), GridCell>();

            foreach (var point in dataPoints)
            {
                int col = (int)Math.Floor((point.X - minX.Value) / cellSize);
                int row = (int)Math.Floor((point.Y - minY.Value) / cellSize);

                col = Math.Max(0, Math.Min(cols - 1, col));
                row = Math.Max(0, Math.Min(rows - 1, row));

                if (!cells.ContainsKey((col, row)))
                {
                    cells[(col, row)] = new GridCell
                    {
                        X = col,
                        Y = row,
                        Bounds = (
                            minX.Value + col * cellSize,
                            minY.Value + row * cellSize,
                            minX.Value + (col + 1) * cellSize,
                            minY.Value + (row + 1) * cellSize
                        )
                    };
                }

                cells[(col, row)].Points.Add(point);
            }

            // Aggregate values
            var result = new List<GridCell>();
            foreach (var cell in cells.Values)
            {
                cell.Count = cell.Points.Count;
                cell.Value = AggregateValues(cell.Points, method);
                result.Add(cell);
            }

            return result;
        }

        /// <summary>
        /// Aggregates values using the specified method.
        /// </summary>
        private static double AggregateValues(List<HEAT_MAP_DATA_POINT> points, AggregationMethod method)
        {
            if (points.Count == 0)
                return 0;

            return method switch
            {
                AggregationMethod.Average => points.Average(p => p.Value),
                AggregationMethod.Sum => points.Sum(p => p.Value),
                AggregationMethod.Max => points.Max(p => p.Value),
                AggregationMethod.Min => points.Min(p => p.Value),
                AggregationMethod.Median => CalculateMedian(points.Select(p => p.Value).ToList()),
                AggregationMethod.Count => points.Count,
                _ => points.Average(p => p.Value)
            };
        }

        /// <summary>
        /// Calculates the median of a list of values.
        /// </summary>
        private static double CalculateMedian(List<double> values)
        {
            if (values.Count == 0)
                return 0;

            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            else
                return sorted[mid];
        }

        /// <summary>
        /// Converts pixel coordinates to hex coordinates.
        /// </summary>
        private static (int q, int r) PixelToHex(double x, double y, double radius)
        {
            double size = radius;
            double q = (2.0 / 3.0 * x) / size;
            double r = (-1.0 / 3.0 * x + Math.Sqrt(3) / 3.0 * y) / size;
            return HexRound(q, r);
        }

        /// <summary>
        /// Converts hex coordinates to pixel coordinates.
        /// </summary>
        private static (double x, double y) HexToPixel(int q, int r, double radius)
        {
            double size = radius;
            double x = size * (3.0 / 2.0 * q);
            double y = size * (Math.Sqrt(3) / 2.0 * q + Math.Sqrt(3) * r);
            return (x, y);
        }

        /// <summary>
        /// Rounds fractional hex coordinates to nearest hex.
        /// </summary>
        private static (int q, int r) HexRound(double q, double r)
        {
            double s = -q - r;
            int qInt = (int)Math.Round(q);
            int rInt = (int)Math.Round(r);
            int sInt = (int)Math.Round(s);

            double qDiff = Math.Abs(qInt - q);
            double rDiff = Math.Abs(rInt - r);
            double sDiff = Math.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = -rInt - sInt;
            else if (rDiff > sDiff)
                rInt = -qInt - sInt;

            return (qInt, rInt);
        }

        /// <summary>
        /// Renders a hexbin cell on the canvas.
        /// </summary>
        public static void RenderHexbinCell(SKCanvas canvas, HexbinCell cell, SKColor color, float scaleX, float scaleY, float offsetX, float offsetY)
        {
            float centerX = (float)(cell.CenterX * scaleX + offsetX);
            float centerY = (float)(cell.CenterY * scaleY + offsetY);
            float radius = (float)(cell.Radius * Math.Min(scaleX, scaleY));

            using (var path = new SKPath())
            {
                // Create hexagon
                for (int i = 0; i < 6; i++)
                {
                    double angle = Math.PI / 3.0 * i;
                    float x = centerX + radius * (float)Math.Cos(angle);
                    float y = centerY + radius * (float)Math.Sin(angle);
                    if (i == 0)
                        path.MoveTo(x, y);
                    else
                        path.LineTo(x, y);
                }
                path.Close();

                using (var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Renders a grid cell on the canvas.
        /// </summary>
        public static void RenderGridCell(SKCanvas canvas, GridCell cell, SKColor color, float scaleX, float scaleY, float offsetX, float offsetY)
        {
            float x = (float)(cell.Bounds.minX * scaleX + offsetX);
            float y = (float)(cell.Bounds.minY * scaleY + offsetY);
            float width = (float)((cell.Bounds.maxX - cell.Bounds.minX) * scaleX);
            float height = (float)((cell.Bounds.maxY - cell.Bounds.minY) * scaleY);

            using (var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawRect(x, y, width, height, paint);
            }
        }
    }
}

