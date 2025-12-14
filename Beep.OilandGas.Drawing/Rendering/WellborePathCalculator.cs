using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Calculates wellbore paths from deviation surveys for directional/deviated wells.
    /// </summary>
    public static class WellborePathCalculator
    {
        /// <summary>
        /// Calculates a wellbore path from a deviation survey.
        /// </summary>
        /// <param name="survey">The deviation survey.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="horizontalStretchFactor">Stretch factor for horizontal sections.</param>
        /// <returns>List of points representing the wellbore path in screen coordinates.</returns>
        public static List<SKPoint> CalculatePath(
            DeviationSurvey survey,
            DepthCoordinateSystem depthSystem,
            float horizontalStretchFactor = 1f)
        {
            if (survey == null || survey.SurveyPoints == null || survey.SurveyPoints.Count == 0)
                return new List<SKPoint>();

            var path = new List<SKPoint>();
            double currentX = 0;
            double currentY = 0;
            double currentZ = 0;

            var sortedPoints = survey.SurveyPoints.OrderBy(p => p.MD).ToList();

            foreach (var point in sortedPoints)
            {
                // Calculate 3D position from survey point
                double md = point.MD;
                double deviationRad = DegreesToRadians(point.DEVIATION_ANGLE);
                double azimuthRad = DegreesToRadians(point.AZIMUTH);

                // Calculate incremental displacement
                double deltaMD = md - (path.Count > 0 ? sortedPoints[path.Count - 1].MD : 0);
                if (path.Count == 0)
                    deltaMD = md;

                double deltaX = deltaMD * Math.Sin(deviationRad) * Math.Cos(azimuthRad);
                double deltaY = deltaMD * Math.Sin(deviationRad) * Math.Sin(azimuthRad);
                double deltaZ = deltaMD * Math.Cos(deviationRad);

                currentX += deltaX;
                currentY += deltaY;
                currentZ += deltaZ;

                // Convert to screen coordinates
                float screenX = (float)(currentX * horizontalStretchFactor);
                float screenY = depthSystem.ToScreenY((float)currentZ, null);

                path.Add(new SKPoint(screenX, screenY));
            }

            return path;
        }

        /// <summary>
        /// Calculates a wellbore path for a vertical well (no deviation survey).
        /// </summary>
        /// <param name="topDepth">Top depth.</param>
        /// <param name="bottomDepth">Bottom depth.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="xPosition">X position for the wellbore.</param>
        /// <returns>List of points representing the vertical wellbore path.</returns>
        public static List<SKPoint> CalculateVerticalPath(
            double topDepth,
            double bottomDepth,
            DepthCoordinateSystem depthSystem,
            float xPosition = 0)
        {
            var path = new List<SKPoint>();
            float topY = depthSystem.ToScreenY((float)topDepth, null);
            float bottomY = depthSystem.ToScreenY((float)bottomDepth, null);

            path.Add(new SKPoint(xPosition, topY));
            path.Add(new SKPoint(xPosition, bottomY));

            return path;
        }

        /// <summary>
        /// Calculates a wellbore path with both vertical and horizontal sections.
        /// </summary>
        /// <param name="verticalTopDepth">Top of vertical section.</param>
        /// <param name="verticalBottomDepth">Bottom of vertical section (kickoff point).</param>
        /// <param name="horizontalLength">Length of horizontal section.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="xPosition">X position for the vertical section.</param>
        /// <param name="horizontalStretchFactor">Stretch factor for horizontal section.</param>
        /// <param name="horizontalAlignment">How to align horizontal section.</param>
        /// <returns>List of points representing the wellbore path.</returns>
        public static List<SKPoint> CalculateVerticalHorizontalPath(
            double verticalTopDepth,
            double verticalBottomDepth,
            double horizontalLength,
            DepthCoordinateSystem depthSystem,
            float xPosition = 0,
            float horizontalStretchFactor = 10f,
            HorizontalSectionAlignment horizontalAlignment = HorizontalSectionAlignment.LaterallyCompressed)
        {
            var path = new List<SKPoint>();

            // Vertical section
            float topY = depthSystem.ToScreenY((float)verticalTopDepth, null);
            float bottomY = depthSystem.ToScreenY((float)verticalBottomDepth, null);

            path.Add(new SKPoint(xPosition, topY));
            path.Add(new SKPoint(xPosition, bottomY));

            // Horizontal section
            float horizontalX = xPosition;
            float horizontalY = bottomY;

            // Calculate horizontal stretch based on alignment
            float stretchedLength = horizontalAlignment switch
            {
                HorizontalSectionAlignment.LaterallyCompressed => (float)horizontalLength / horizontalStretchFactor,
                HorizontalSectionAlignment.TrueScale => (float)horizontalLength,
                HorizontalSectionAlignment.Stretched => (float)horizontalLength * horizontalStretchFactor,
                _ => (float)horizontalLength / horizontalStretchFactor
            };

            path.Add(new SKPoint(horizontalX + stretchedLength, horizontalY));

            return path;
        }

        /// <summary>
        /// Gets depth at a point along the wellbore path.
        /// </summary>
        /// <param name="path">The wellbore path.</param>
        /// <param name="pointIndex">The point index.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="survey">The deviation survey (for directional wells).</param>
        /// <returns>The depth at the point, or null if not found.</returns>
        public static double? GetDepthAtPoint(
            List<SKPoint> path,
            int pointIndex,
            DepthCoordinateSystem depthSystem,
            DeviationSurvey survey = null)
        {
            if (path == null || pointIndex < 0 || pointIndex >= path.Count)
                return null;

            if (survey != null && survey.SurveyPoints != null && survey.SurveyPoints.Count > pointIndex)
            {
                var sortedPoints = survey.SurveyPoints.OrderBy(p => p.MD).ToList();
                if (pointIndex < sortedPoints.Count)
                {
                    return sortedPoints[pointIndex].MD; // Measured depth
                }
            }

            // For vertical wells, convert screen Y back to depth
            float screenY = path[pointIndex].Y;
            return depthSystem.ToDepth(screenY, null);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Calculates the total measured depth of a path.
        /// </summary>
        public static double CalculateTotalMeasuredDepth(List<SKPoint> path, DeviationSurvey survey = null)
        {
            if (survey != null && survey.TotalMeasuredDepth.HasValue)
                return survey.TotalMeasuredDepth.Value;

            // For vertical paths, calculate from Y coordinates
            if (path != null && path.Count >= 2)
            {
                double totalDepth = 0;
                for (int i = 1; i < path.Count; i++)
                {
                    double deltaY = Math.Abs(path[i].Y - path[i - 1].Y);
                    double deltaX = Math.Abs(path[i].X - path[i - 1].X);
                    totalDepth += Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                }
                return totalDepth;
            }

            return 0;
        }
    }
}

