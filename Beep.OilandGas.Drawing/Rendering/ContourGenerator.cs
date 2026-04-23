using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Generates contour line segments from reservoir map surfaces using inverse-distance interpolation and marching squares.
    /// </summary>
    public static class ContourGenerator
    {
        /// <summary>
        /// Generates contour segments for a reservoir surface.
        /// </summary>
        public static IReadOnlyList<ContourLineSegment> Generate(ReservoirSurfaceData surface, ReservoirContourConfiguration configuration)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            configuration ??= new ReservoirContourConfiguration();
            var sourcePoints = (surface.Points ?? new List<Point3D>())
                .Where(point => point != null && double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z))
                .ToList();

            if (sourcePoints.Count < 3)
                return Array.Empty<ContourLineSegment>();

            var bounds = surface.BoundingBox ?? CreateBoundingBox(sourcePoints);
            if (bounds == null || bounds.MaxX <= bounds.MinX || bounds.MaxY <= bounds.MinY)
                return Array.Empty<ContourLineSegment>();

            int columns = Math.Max(2, configuration.SampleColumns);
            int rows = Math.Max(2, configuration.SampleRows);

            var xCoordinates = CreateAxis(bounds.MinX, bounds.MaxX, columns);
            var yCoordinates = CreateAxis(bounds.MinY, bounds.MaxY, rows);
            var scalarGrid = CreateScalarGrid(sourcePoints, xCoordinates, yCoordinates, configuration);

            double minimumValue = configuration.MinimumContourValue ?? sourcePoints.Min(point => point.Z);
            double maximumValue = configuration.MaximumContourValue ?? sourcePoints.Max(point => point.Z);
            if (maximumValue <= minimumValue)
                return Array.Empty<ContourLineSegment>();

            double interval = configuration.ContourInterval > 0
                ? configuration.ContourInterval
                : ComputeNiceInterval(minimumValue, maximumValue);
            if (interval <= 0)
                return Array.Empty<ContourLineSegment>();

            var levels = CreateContourLevels(minimumValue, maximumValue, interval);
            var segments = new List<ContourLineSegment>();

            for (int levelIndex = 0; levelIndex < levels.Count; levelIndex++)
            {
                double level = levels[levelIndex];
                bool isMajor = configuration.MajorContourEvery <= 1 || levelIndex % configuration.MajorContourEvery == 0;

                for (int row = 0; row < rows - 1; row++)
                {
                    for (int column = 0; column < columns - 1; column++)
                    {
                        var cellSegments = GenerateCellSegments(
                            xCoordinates[column],
                            xCoordinates[column + 1],
                            yCoordinates[row],
                            yCoordinates[row + 1],
                            scalarGrid[row, column],
                            scalarGrid[row, column + 1],
                            scalarGrid[row + 1, column + 1],
                            scalarGrid[row + 1, column],
                            level,
                            isMajor);

                        segments.AddRange(cellSegments);
                    }
                }
            }

            return segments;
        }

        private static double[,] CreateScalarGrid(
            IReadOnlyList<Point3D> sourcePoints,
            IReadOnlyList<double> xCoordinates,
            IReadOnlyList<double> yCoordinates,
            ReservoirContourConfiguration configuration)
        {
            var grid = new double[yCoordinates.Count, xCoordinates.Count];

            for (int row = 0; row < yCoordinates.Count; row++)
            {
                for (int column = 0; column < xCoordinates.Count; column++)
                {
                    grid[row, column] = InterpolateValue(sourcePoints, xCoordinates[column], yCoordinates[row], configuration);
                }
            }

            return grid;
        }

        private static double InterpolateValue(IReadOnlyList<Point3D> points, double x, double y, ReservoirContourConfiguration configuration)
        {
            return ReservoirSurfaceInterpolation.InterpolateZ(points, x, y, configuration.MaxInfluencePoints, configuration.InterpolationPower);
        }

        private static IReadOnlyList<double> CreateAxis(double minimum, double maximum, int count)
        {
            var axis = new double[count];
            double span = maximum - minimum;

            for (int index = 0; index < count; index++)
            {
                axis[index] = minimum + (span * index / Math.Max(1, count - 1));
            }

            return axis;
        }

        private static BoundingBox CreateBoundingBox(IReadOnlyList<Point3D> points)
        {
            if (points == null || points.Count == 0)
                return null;

            return new BoundingBox
            {
                MinX = points.Min(point => point.X),
                MaxX = points.Max(point => point.X),
                MinY = points.Min(point => point.Y),
                MaxY = points.Max(point => point.Y),
                MinZ = points.Min(point => point.Z),
                MaxZ = points.Max(point => point.Z)
            };
        }

        private static double ComputeNiceInterval(double minimumValue, double maximumValue)
        {
            double roughInterval = (maximumValue - minimumValue) / 12.0;
            if (roughInterval <= 0)
                return 0;

            double exponent = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double normalized = roughInterval / exponent;

            if (normalized <= 1)
                return exponent;
            if (normalized <= 2)
                return 2 * exponent;
            if (normalized <= 5)
                return 5 * exponent;

            return 10 * exponent;
        }

        private static List<double> CreateContourLevels(double minimumValue, double maximumValue, double interval)
        {
            var levels = new List<double>();
            double value = Math.Ceiling(minimumValue / interval) * interval;

            while (value <= maximumValue + (interval * 0.001))
            {
                levels.Add(value);
                value += interval;
            }

            return levels;
        }

        private static IReadOnlyList<ContourLineSegment> GenerateCellSegments(
            double x0,
            double x1,
            double y0,
            double y1,
            double v0,
            double v1,
            double v2,
            double v3,
            double level,
            bool isMajor)
        {
            int caseIndex = 0;
            if (v0 >= level) caseIndex |= 1;
            if (v1 >= level) caseIndex |= 2;
            if (v2 >= level) caseIndex |= 4;
            if (v3 >= level) caseIndex |= 8;

            if (caseIndex == 0 || caseIndex == 15)
                return Array.Empty<ContourLineSegment>();

            var p0 = new SKPoint((float)x0, (float)y0);
            var p1 = new SKPoint((float)x1, (float)y0);
            var p2 = new SKPoint((float)x1, (float)y1);
            var p3 = new SKPoint((float)x0, (float)y1);

            var edgePoints = new Dictionary<int, SKPoint>
            {
                [0] = Interpolate(p0, p1, v0, v1, level),
                [1] = Interpolate(p1, p2, v1, v2, level),
                [2] = Interpolate(p2, p3, v2, v3, level),
                [3] = Interpolate(p3, p0, v3, v0, level)
            };

            return GetCaseSegments(caseIndex)
                .Select(pair => new ContourLineSegment(level, edgePoints[pair.EdgeA], edgePoints[pair.EdgeB], isMajor))
                .ToList();
        }

        private static IReadOnlyList<(int EdgeA, int EdgeB)> GetCaseSegments(int caseIndex)
        {
            return caseIndex switch
            {
                1 => new[] { (3, 0) },
                2 => new[] { (0, 1) },
                3 => new[] { (3, 1) },
                4 => new[] { (1, 2) },
                5 => new[] { (3, 2), (0, 1) },
                6 => new[] { (0, 2) },
                7 => new[] { (3, 2) },
                8 => new[] { (2, 3) },
                9 => new[] { (0, 2) },
                10 => new[] { (0, 3), (1, 2) },
                11 => new[] { (1, 2) },
                12 => new[] { (3, 1) },
                13 => new[] { (0, 1) },
                14 => new[] { (3, 0) },
                _ => Array.Empty<(int, int)>()
            };
        }

        private static SKPoint Interpolate(SKPoint start, SKPoint end, double valueA, double valueB, double level)
        {
            if (Math.Abs(valueB - valueA) < 1e-12)
            {
                return new SKPoint((start.X + end.X) * 0.5f, (start.Y + end.Y) * 0.5f);
            }

            float t = (float)((level - valueA) / (valueB - valueA));
            t = Math.Max(0f, Math.Min(1f, t));
            return new SKPoint(
                start.X + ((end.X - start.X) * t),
                start.Y + ((end.Y - start.Y) * t));
        }
    }

    /// <summary>
    /// Represents a generated contour segment.
    /// </summary>
    public sealed record ContourLineSegment(double Level, SKPoint Start, SKPoint End, bool IsMajor)
    {
        /// <summary>
        /// Gets the segment length.
        /// </summary>
        public float Length => SKPoint.Distance(Start, End);

        /// <summary>
        /// Gets the midpoint used for simple label placement.
        /// </summary>
        public SKPoint Midpoint => new SKPoint((Start.X + End.X) * 0.5f, (Start.Y + End.Y) * 0.5f);
    }
}