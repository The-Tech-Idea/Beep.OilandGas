using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;
using SurveyPathCalculator = Beep.OilandGas.Drawing.Rendering.WellborePathCalculator;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Calculates wellbore paths for vertical and deviated wellbores.
    /// </summary>
    public class WellborePathCalculator
    {
        private readonly DepthTransform depthSystem;
        private readonly WellSchematicConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WellborePathCalculator"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth transform.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public WellborePathCalculator(DepthTransform depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Calculates the wellbore path for a borehole.
        /// </summary>
        /// <param name="borehole">The borehole data.</param>
        /// <param name="deviationSurvey">Optional deviation survey for the borehole.</param>
        /// <param name="index">The borehole index.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <returns>The wellbore path result.</returns>
        public WellborePathResult CalculateWellborePath(WellData_Borehole borehole, DeviationSurvey deviationSurvey, int index, float centerX, float canvasHeight)
        {
            if (borehole.IsVertical)
            {
                return CalculateVerticalWellborePath(borehole, centerX, canvasHeight);
            }

            if (deviationSurvey?.SurveyPoints != null && deviationSurvey.SurveyPoints.Count > 1)
            {
                return CalculateSurveyWellborePath(borehole, deviationSurvey, centerX, canvasHeight);
            }

            if (!configuration.UseBezierCurves)
            {
                return CalculateVerticalWellborePath(borehole, centerX, canvasHeight);
            }

            else
            {
                return CalculateCurvedWellborePath(borehole, index, centerX, canvasHeight);
            }
        }

        /// <summary>
        /// Calculates the path for a vertical wellbore.
        /// </summary>
        private WellborePathResult CalculateVerticalWellborePath(WellData_Borehole borehole, float centerX, float canvasHeight)
        {
            float topY = depthSystem.ToScreenY(borehole.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(borehole.BottomDepth, canvasHeight);
            float outerRadius = (borehole.Diameter * configuration.DiameterScale) / 2.0f;
            float innerRadius = Math.Max(0.5f, ((borehole.Diameter - 2) * configuration.DiameterScale) / 2.0f);

            borehole.OuterDiameterOffset = outerRadius;

            var centerLine = new List<SKPoint>
            {
                new SKPoint(centerX, topY),
                new SKPoint(centerX, bottomY)
            };

            var outerPath = new List<SKPoint>
            {
                new SKPoint(centerX - outerRadius, topY),
                new SKPoint(centerX - outerRadius, bottomY),
                new SKPoint(centerX - innerRadius, bottomY),
                new SKPoint(centerX + innerRadius, bottomY),
                new SKPoint(centerX + outerRadius, bottomY),
                new SKPoint(centerX + outerRadius, topY)
            };

            var innerPath = new List<SKPoint>
            {
                new SKPoint(centerX - innerRadius, topY),
                new SKPoint(centerX - innerRadius, bottomY),
                new SKPoint(centerX + innerRadius, bottomY),
                new SKPoint(centerX + innerRadius, topY)
            };

            return new WellborePathResult(outerPath, innerPath, centerLine);
        }

        /// <summary>
        /// Calculates the path for a deviated wellbore from survey points.
        /// </summary>
        private WellborePathResult CalculateSurveyWellborePath(WellData_Borehole borehole, DeviationSurvey deviationSurvey, float centerX, float canvasHeight)
        {
            var centerLine = SurveyPathCalculator
                .CalculatePath(deviationSurvey, depthSystem.WithCanvasHeight(canvasHeight), configuration.HorizontalStretchFactor)
                .Select(point => new SKPoint(point.X + centerX, point.Y))
                .ToList();

            if (centerLine.Count < 2)
                return CalculateCurvedWellborePath(borehole, 0, centerX, canvasHeight);

            float outerRadius = (borehole.Diameter * configuration.DiameterScale) / 2.0f;
            float innerRadius = Math.Max(0.5f, ((borehole.Diameter - 2) * configuration.DiameterScale) / 2.0f);
            return CreateOffsetPathResult(centerLine, borehole, outerRadius, innerRadius);
        }

        /// <summary>
        /// Calculates the path for a curved/deviated wellbore using a fallback Bezier approximation.
        /// </summary>
        private WellborePathResult CalculateCurvedWellborePath(WellData_Borehole borehole, int index, float centerX, float canvasHeight)
        {
            bool curveRight = index % 2 == 0;
            float depthOffsetFactor = 0.5f;
            float controlDepth = (borehole.TopDepth + borehole.BottomDepth) * depthOffsetFactor;
            float horizontalOffset = curveRight ? 50.0f : -50.0f;

            float topY = depthSystem.ToScreenY(borehole.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(borehole.BottomDepth, canvasHeight);
            float controlY = depthSystem.ToScreenY(controlDepth, canvasHeight);

            // Create control points for Bezier curve
            var controlPoints = new List<SKPoint>
            {
                new SKPoint(centerX, topY),
                new SKPoint(centerX + horizontalOffset, controlY),
                new SKPoint(centerX, bottomY)
            };

            // Calculate Bezier curve points
            var bezierPoints = CalculateBezierCurve(controlPoints, configuration.BezierCurvePoints);

            float outerRadius = (borehole.Diameter * configuration.DiameterScale) / 2.0f;
            float innerRadius = Math.Max(0.5f, ((borehole.Diameter - 2) * configuration.DiameterScale) / 2.0f);
            return CreateOffsetPathResult(bezierPoints, borehole, outerRadius, innerRadius);
        }

        private WellborePathResult CreateOffsetPathResult(List<SKPoint> centerLine, WellData_Borehole borehole, float outerRadius, float innerRadius)
        {
            borehole.OuterDiameterOffset = outerRadius;

            var outerPath = CreateEnvelope(centerLine, outerRadius);
            var innerPath = CreateEnvelope(centerLine, innerRadius);
            return new WellborePathResult(outerPath, innerPath, centerLine);
        }

        private static List<SKPoint> CreateEnvelope(IReadOnlyList<SKPoint> centerLine, float radius)
        {
            var leftPoints = new List<SKPoint>();
            var rightPoints = new List<SKPoint>();

            for (int index = 0; index < centerLine.Count; index++)
            {
                var current = centerLine[index];
                var tangent = GetTangent(centerLine, index);
                var length = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
                if (length <= 0)
                    continue;

                var perpX = -tangent.Y / length;
                var perpY = tangent.X / length;

                leftPoints.Add(new SKPoint(current.X + perpX * radius, current.Y + perpY * radius));
                rightPoints.Add(new SKPoint(current.X - perpX * radius, current.Y - perpY * radius));
            }

            if (leftPoints.Count == 0)
                return centerLine.ToList();

            var envelope = new List<SKPoint>(leftPoints.Count + rightPoints.Count);
            envelope.AddRange(leftPoints);
            rightPoints.Reverse();
            envelope.AddRange(rightPoints);
            return envelope;
        }

        private static SKPoint GetTangent(IReadOnlyList<SKPoint> points, int index)
        {
            var previous = index == 0 ? points[index] : points[index - 1];
            var next = index == points.Count - 1 ? points[index] : points[index + 1];
            return new SKPoint(next.X - previous.X, next.Y - previous.Y);
        }

        /// <summary>
        /// Calculates a Bezier curve from control points.
        /// </summary>
        private List<SKPoint> CalculateBezierCurve(List<SKPoint> controlPoints, int numPoints)
        {
            var bezierPoints = new List<SKPoint>();

            for (int i = 0; i < numPoints; i++)
            {
                double t = (double)i / (numPoints - 1);
                if (t > 1.0) t = 1.0;

                SKPoint point = new SKPoint();
                int n = controlPoints.Count - 1;

                for (int j = 0; j < controlPoints.Count; j++)
                {
                    double bernstein = Bernstein(n, j, t);
                    point.X += (float)(controlPoints[j].X * bernstein);
                    point.Y += (float)(controlPoints[j].Y * bernstein);
                }

                bezierPoints.Add(point);
            }

            return bezierPoints;
        }

        /// <summary>
        /// Calculates Bernstein polynomial.
        /// </summary>
        private double Bernstein(int n, int i, double t)
        {
            return Combination(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        /// <summary>
        /// Calculates combination (n choose i).
        /// </summary>
        private int Combination(int n, int k)
        {
            if (k > n) return 0;
            if (k == 0 || k == n) return 1;

            int result = 1;
            for (int i = 0; i < k; i++)
            {
                result = result * (n - i) / (i + 1);
            }
            return result;
        }

        /// <summary>
        /// Calculates the tubing path.
        /// </summary>
        /// <param name="borehole">The borehole data.</param>
        /// <param name="tubeIndex">The tube index.</param>
        /// <param name="tubeCenterOffset">The tubing center offset from the borehole centerline.</param>
        /// <param name="boreholeCenterLine">The borehole centerline.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <returns>The tubing path points.</returns>
        public List<SKPoint> CalculateTubingPath(
            WellData_Borehole borehole,
            int tubeIndex,
            float tubeCenterOffset,
            List<SKPoint> boreholeCenterLine,
            float canvasHeight)
        {
            var tube = borehole.Tubing[tubeIndex];
            float topY = depthSystem.ToScreenY(tube.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(tube.BottomDepth, canvasHeight);

            var centerSegment = PathHelper.GetPathSegment(boreholeCenterLine, topY, bottomY);
            return PathHelper.CreateParallelPath(centerSegment, tubeCenterOffset);
        }
    }

    /// <summary>
    /// Result of wellbore path calculation.
    /// </summary>
    public class WellborePathResult
    {
        /// <summary>
        /// Gets the outer wellbore path.
        /// </summary>
        public List<SKPoint> OuterPath { get; }

        /// <summary>
        /// Gets the inner wellbore path.
        /// </summary>
        public List<SKPoint> InnerPath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WellborePathResult"/> class.
        /// </summary>
        /// <param name="outerPath">The outer path.</param>
        /// <param name="innerPath">The inner path.</param>
        public WellborePathResult(List<SKPoint> outerPath, List<SKPoint> innerPath, List<SKPoint> centerLine)
        {
            OuterPath = outerPath ?? throw new ArgumentNullException(nameof(outerPath));
            InnerPath = innerPath ?? throw new ArgumentNullException(nameof(innerPath));
            CenterLine = centerLine ?? throw new ArgumentNullException(nameof(centerLine));
        }

        /// <summary>
        /// Gets the centerline path used for equipment and marker placement.
        /// </summary>
        public List<SKPoint> CenterLine { get; }
    }
}

