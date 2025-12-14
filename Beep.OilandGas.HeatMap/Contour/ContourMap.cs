using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Contour
{
    /// <summary>
    /// Represents a contour line with its level and points.
    /// </summary>
    public class ContourLine
    {
        /// <summary>
        /// Gets or sets the contour level value.
        /// </summary>
        public double Level { get; set; }

        /// <summary>
        /// Gets the points forming this contour line.
        /// </summary>
        public List<SKPoint> Points { get; } = new List<SKPoint>();

        /// <summary>
        /// Gets or sets whether this is a closed contour.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the color for this contour line.
        /// </summary>
        public SKColor Color { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the line width.
        /// </summary>
        public float LineWidth { get; set; } = 1f;
    }

    /// <summary>
    /// Represents a filled contour region.
    /// </summary>
    public class ContourRegion
    {
        /// <summary>
        /// Gets or sets the minimum level of this region.
        /// </summary>
        public double MinLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum level of this region.
        /// </summary>
        public double MaxLevel { get; set; }

        /// <summary>
        /// Gets the polygons forming this region.
        /// </summary>
        public List<List<SKPoint>> Polygons { get; } = new List<List<SKPoint>>();

        /// <summary>
        /// Gets or sets the fill color for this region.
        /// </summary>
        public SKColor FillColor { get; set; }

        /// <summary>
        /// Gets or sets the border color for this region.
        /// </summary>
        public SKColor BorderColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        public float BorderWidth { get; set; } = 0.5f;
    }

    /// <summary>
    /// Provides contour map generation and rendering capabilities.
    /// </summary>
    public static class ContourMap
    {
        /// <summary>
        /// Generates contour lines from a grid using marching squares algorithm.
        /// </summary>
        /// <param name="grid">2D array of interpolated values.</param>
        /// <param name="contourLevels">Array of contour levels to generate.</param>
        /// <param name="minX">Minimum X coordinate of the grid.</param>
        /// <param name="maxX">Maximum X coordinate of the grid.</param>
        /// <param name="minY">Minimum Y coordinate of the grid.</param>
        /// <param name="maxY">Maximum Y coordinate of the grid.</param>
        /// <returns>List of contour lines grouped by level.</returns>
        public static Dictionary<double, List<ContourLine>> GenerateContourLines(
            double[,] grid,
            double[] contourLevels,
            double minX, double maxX, double minY, double maxY)
        {
            if (grid == null || contourLevels == null || contourLevels.Length == 0)
                return new Dictionary<double, List<ContourLine>>();

            var result = new Dictionary<double, List<ContourLine>>();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            double cellWidth = (maxX - minX) / (cols - 1);
            double cellHeight = (maxY - minY) / (rows - 1);

            foreach (var level in contourLevels)
            {
                var contours = new List<ContourLine>();
                var edgeMap = new Dictionary<(int, int, int, int), List<SKPoint>>();

                // Marching squares algorithm
                for (int row = 0; row < rows - 1; row++)
                {
                    for (int col = 0; col < cols - 1; col++)
                    {
                        double tl = grid[row, col];
                        double tr = grid[row, col + 1];
                        double bl = grid[row + 1, col];
                        double br = grid[row + 1, col + 1];

                        // Calculate cell corners in data coordinates
                        double x0 = minX + col * cellWidth;
                        double x1 = minX + (col + 1) * cellWidth;
                        double y0 = minY + row * cellHeight;
                        double y1 = minY + (row + 1) * cellHeight;

                        // Determine which edges are intersected
                        List<SKPoint> edgePoints = new List<SKPoint>();

                        // Top edge
                        if ((tl < level && tr >= level) || (tl >= level && tr < level))
                        {
                            double t = (level - tl) / (tr - tl + 1e-10);
                            double x = x0 + (x1 - x0) * t;
                            edgePoints.Add(new SKPoint((float)x, (float)y0));
                        }

                        // Right edge
                        if ((tr < level && br >= level) || (tr >= level && br < level))
                        {
                            double t = (level - tr) / (br - tr + 1e-10);
                            double y = y0 + (y1 - y0) * t;
                            edgePoints.Add(new SKPoint((float)x1, (float)y));
                        }

                        // Bottom edge
                        if ((bl < level && br >= level) || (bl >= level && br < level))
                        {
                            double t = (level - bl) / (br - bl + 1e-10);
                            double x = x0 + (x1 - x0) * t;
                            edgePoints.Add(new SKPoint((float)x, (float)y1));
                        }

                        // Left edge
                        if ((tl < level && bl >= level) || (tl >= level && bl < level))
                        {
                            double t = (level - tl) / (bl - tl + 1e-10);
                            double y = y0 + (y1 - y0) * t;
                            edgePoints.Add(new SKPoint((float)x0, (float)y));
                        }

                        // Store edge points for connection
                        if (edgePoints.Count == 2)
                        {
                            var key1 = (col, row, col + 1, row);
                            var key2 = (col + 1, row, col + 1, row + 1);
                            var key3 = (col + 1, row + 1, col, row + 1);
                            var key4 = (col, row + 1, col, row);

                            if (!edgeMap.ContainsKey(key1)) edgeMap[key1] = new List<SKPoint>();
                            if (!edgeMap.ContainsKey(key2)) edgeMap[key2] = new List<SKPoint>();
                            if (!edgeMap.ContainsKey(key3)) edgeMap[key3] = new List<SKPoint>();
                            if (!edgeMap.ContainsKey(key4)) edgeMap[key4] = new List<SKPoint>();

                            if (edgePoints.Count >= 1)
                            {
                                // Connect points based on edge
                                if (edgePoints[0].Y == y0) // Top edge
                                {
                                    edgeMap[key1].Add(edgePoints[0]);
                                }
                                if (edgePoints.Count > 1 && edgePoints[1].X == x1) // Right edge
                                {
                                    edgeMap[key2].Add(edgePoints[1]);
                                }
                                else if (edgePoints.Count > 1 && edgePoints[1].Y == y1) // Bottom edge
                                {
                                    edgeMap[key3].Add(edgePoints[1]);
                                }
                                else if (edgePoints.Count > 1 && edgePoints[1].X == x0) // Left edge
                                {
                                    edgeMap[key4].Add(edgePoints[1]);
                                }
                            }
                        }
                    }
                }

                // Connect edge points into contour lines
                contours = ConnectContourPoints(edgeMap, level);
                result[level] = contours;
            }

            return result;
        }

        /// <summary>
        /// Connects edge points into continuous contour lines.
        /// </summary>
        private static List<ContourLine> ConnectContourPoints(
            Dictionary<(int, int, int, int), List<SKPoint>> edgeMap,
            double level)
        {
            var contours = new List<ContourLine>();
            var usedEdges = new HashSet<(int, int, int, int)>();

            foreach (var kvp in edgeMap)
            {
                if (usedEdges.Contains(kvp.Key) || kvp.Value.Count == 0)
                    continue;

                var contour = new ContourLine { Level = level };
                var currentEdge = kvp.Key;
                var currentPoint = kvp.Value[0];

                contour.Points.Add(currentPoint);
                usedEdges.Add(currentEdge);

                // Try to find connected edges
                bool found = true;
                while (found)
                {
                    found = false;
                    foreach (var edge in edgeMap)
                    {
                        if (usedEdges.Contains(edge.Key) || edge.Value.Count == 0)
                            continue;

                        // Check if this edge connects to current point
                        var (x1, y1, x2, y2) = edge.Key;
                        SKPoint edgePoint = edge.Value[0];

                        double distance = Math.Sqrt(
                            Math.Pow(currentPoint.X - edgePoint.X, 2) +
                            Math.Pow(currentPoint.Y - edgePoint.Y, 2));

                        if (distance < 0.1) // Threshold for connection
                        {
                            contour.Points.Add(edgePoint);
                            usedEdges.Add(edge.Key);
                            currentPoint = edgePoint;
                            found = true;
                            break;
                        }
                    }
                }

                // Check if contour is closed
                if (contour.Points.Count > 2)
                {
                    double distToStart = Math.Sqrt(
                        Math.Pow(contour.Points[0].X - contour.Points[contour.Points.Count - 1].X, 2) +
                        Math.Pow(contour.Points[0].Y - contour.Points[contour.Points.Count - 1].Y, 2));
                    contour.IsClosed = distToStart < 1.0;
                }

                if (contour.Points.Count > 1)
                {
                    contours.Add(contour);
                }
            }

            return contours;
        }

        /// <summary>
        /// Generates filled contour regions from a grid.
        /// </summary>
        /// <param name="grid">2D array of interpolated values.</param>
        /// <param name="contourLevels">Array of contour levels (sorted).</param>
        /// <param name="minX">Minimum X coordinate.</param>
        /// <param name="maxX">Maximum X coordinate.</param>
        /// <param name="minY">Minimum Y coordinate.</param>
        /// <param name="maxY">Maximum Y coordinate.</param>
        /// <param name="colorScheme">Color scheme for regions.</param>
        /// <returns>List of contour regions.</returns>
        public static List<ContourRegion> GenerateFilledContours(
            double[,] grid,
            double[] contourLevels,
            double minX, double maxX, double minY, double maxY,
            SKColor[] colorScheme)
        {
            if (grid == null || contourLevels == null || contourLevels.Length == 0)
                return new List<ContourRegion>();

            var regions = new List<ContourRegion>();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            double cellWidth = (maxX - minX) / (cols - 1);
            double cellHeight = (maxY - minY) / (rows - 1);

            // Sort levels
            var sortedLevels = contourLevels.OrderBy(l => l).ToArray();

            // Create regions between levels
            for (int i = 0; i < sortedLevels.Length; i++)
            {
                double minLevel = i == 0 ? double.MinValue : sortedLevels[i - 1];
                double maxLevel = sortedLevels[i];

                var region = new ContourRegion
                {
                    MinLevel = minLevel,
                    MaxLevel = maxLevel
                };

                // Generate polygons for this region
                var polygons = new List<List<SKPoint>>();

                for (int row = 0; row < rows - 1; row++)
                {
                    for (int col = 0; col < cols - 1; col++)
                    {
                        double tl = grid[row, col];
                        double tr = grid[row, col + 1];
                        double bl = grid[row + 1, col];
                        double br = grid[row + 1, col + 1];

                        // Check if cell is in this region
                        bool inRegion = (tl >= minLevel && tl < maxLevel) ||
                                       (tr >= minLevel && tr < maxLevel) ||
                                       (bl >= minLevel && bl < maxLevel) ||
                                       (br >= minLevel && br < maxLevel);

                        if (inRegion)
                        {
                            double x0 = minX + col * cellWidth;
                            double x1 = minX + (col + 1) * cellWidth;
                            double y0 = minY + row * cellHeight;
                            double y1 = minY + (row + 1) * cellHeight;

                            // Create polygon for this cell
                            var polygon = new List<SKPoint>
                            {
                                new SKPoint((float)x0, (float)y0),
                                new SKPoint((float)x1, (float)y0),
                                new SKPoint((float)x1, (float)y1),
                                new SKPoint((float)x0, (float)y1)
                            };
                            polygons.Add(polygon);
                        }
                    }
                }

                region.Polygons.Clear();
                region.Polygons.AddRange(polygons);

                // Assign color based on level
                if (colorScheme != null && colorScheme.Length > 0)
                {
                    double normalizedLevel = (maxLevel - sortedLevels[0]) / (sortedLevels[sortedLevels.Length - 1] - sortedLevels[0]);
                    int colorIndex = (int)(normalizedLevel * (colorScheme.Length - 1));
                    colorIndex = Math.Max(0, Math.Min(colorScheme.Length - 1, colorIndex));
                    region.FillColor = colorScheme[colorIndex];
                }
                else
                {
                    region.FillColor = SKColors.Gray;
                }

                if (polygons.Count > 0)
                {
                    regions.Add(region);
                }
            }

            return regions;
        }

        /// <summary>
        /// Renders contour lines on the canvas.
        /// </summary>
        public static void RenderContourLines(
            SKCanvas canvas,
            Dictionary<double, List<ContourLine>> contourLines,
            float scaleX, float scaleY, float offsetX, float offsetY,
            SKColor defaultColor, float defaultLineWidth,
            bool showLabels = false, float labelFontSize = 10f)
        {
            if (contourLines == null)
                return;

            foreach (var levelGroup in contourLines.OrderBy(kvp => kvp.Key))
            {
                foreach (var contour in levelGroup.Value)
                {
                    if (contour.Points.Count < 2)
                        continue;

                    using (var path = new SKPath())
                    {
                        path.MoveTo(
                            contour.Points[0].X * scaleX + offsetX,
                            contour.Points[0].Y * scaleY + offsetY);

                        for (int i = 1; i < contour.Points.Count; i++)
                        {
                            path.LineTo(
                                contour.Points[i].X * scaleX + offsetX,
                                contour.Points[i].Y * scaleY + offsetY);
                        }

                        if (contour.IsClosed)
                        {
                            path.Close();
                        }

                        using (var paint = new SKPaint
                        {
                            Color = contour.Color.Alpha == 0 ? defaultColor : contour.Color,
                            StrokeWidth = contour.LineWidth > 0 ? contour.LineWidth : defaultLineWidth,
                            Style = SKPaintStyle.Stroke,
                            IsAntialias = true
                        })
                        {
                            canvas.DrawPath(path, paint);
                        }

                        // Draw label
                        if (showLabels && contour.Points.Count > 10)
                        {
                            int midIndex = contour.Points.Count / 2;
                            float labelX = contour.Points[midIndex].X * scaleX + offsetX;
                            float labelY = contour.Points[midIndex].Y * scaleY + offsetY;

                            using (var textPaint = new SKPaint
                            {
                                Color = SKColors.Black,
                                TextSize = labelFontSize,
                                IsAntialias = true,
                                TextAlign = SKTextAlign.Center
                            })
                            using (var bgPaint = new SKPaint
                            {
                                Color = new SKColor(255, 255, 255, 200),
                                Style = SKPaintStyle.Fill
                            })
                            {
                                string label = $"{contour.Level:F1}";
                                var bounds = new SKRect();
                                textPaint.MeasureText(label, ref bounds);
                                bounds.Offset(labelX, labelY);
                                bounds.Inflate(3, 2);

                                canvas.DrawRect(bounds, bgPaint);
                                canvas.DrawText(label, labelX, labelY + labelFontSize / 3, textPaint);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders filled contour regions on the canvas.
        /// </summary>
        public static void RenderFilledContours(
            SKCanvas canvas,
            List<ContourRegion> regions,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            if (regions == null)
                return;

            foreach (var region in regions)
            {
                foreach (var polygon in region.Polygons)
                {
                    using (var path = new SKPath())
                    {
                        if (polygon.Count > 0)
                        {
                            path.MoveTo(
                                polygon[0].X * scaleX + offsetX,
                                polygon[0].Y * scaleY + offsetY);

                            for (int i = 1; i < polygon.Count; i++)
                            {
                                path.LineTo(
                                    polygon[i].X * scaleX + offsetX,
                                    polygon[i].Y * scaleY + offsetY);
                            }
                            path.Close();
                        }

                        // Fill
                        using (var fillPaint = new SKPaint
                        {
                            Color = region.FillColor,
                            Style = SKPaintStyle.Fill,
                            IsAntialias = true
                        })
                        {
                            canvas.DrawPath(path, fillPaint);
                        }

                        // Border
                        if (region.BorderWidth > 0)
                        {
                            using (var borderPaint = new SKPaint
                            {
                                Color = region.BorderColor,
                                Style = SKPaintStyle.Stroke,
                                StrokeWidth = region.BorderWidth,
                                IsAntialias = true
                            })
                            {
                                canvas.DrawPath(path, borderPaint);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates automatic contour levels from min/max values.
        /// </summary>
        public static double[] GenerateContourLevels(double minValue, double maxValue, int numLevels)
        {
            if (numLevels <= 0)
                numLevels = 5;

            var levels = new double[numLevels];
            double range = maxValue - minValue;
            double step = range / (numLevels + 1);

            for (int i = 0; i < numLevels; i++)
            {
                levels[i] = minValue + step * (i + 1);
            }

            return levels;
        }
    }
}

