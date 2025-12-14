using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Calculates wellbore paths for vertical and deviated wellbores.
    /// </summary>
    public class WellborePathCalculator
    {
        private readonly CoordinateSystem depthSystem;
        private readonly WellSchematicConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WellborePathCalculator"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public WellborePathCalculator(CoordinateSystem depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Calculates the wellbore path for a borehole.
        /// </summary>
        /// <param name="borehole">The borehole data.</param>
        /// <param name="index">The borehole index.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <returns>The wellbore path result.</returns>
        public WellborePathResult CalculateWellborePath(WellData_Borehole borehole, int index, float centerX, float canvasHeight)
        {
            if (borehole.IsVertical)
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
            float innerRadius = ((borehole.Diameter - 2) * configuration.DiameterScale) / 2.0f;

            borehole.OuterDiameterOffset = outerRadius;

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

            return new WellborePathResult(outerPath, innerPath);
        }

        /// <summary>
        /// Calculates the path for a curved/deviated wellbore using Bezier curves.
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

            // Calculate outer and inner paths
            float outerRadius = (borehole.Diameter * configuration.DiameterScale) / 2.0f;
            float innerRadius = ((borehole.Diameter - 2) * configuration.DiameterScale) / 2.0f;

            borehole.OuterDiameterOffset = outerRadius;

            var outerPath = new List<SKPoint>();
            var innerPath = new List<SKPoint>();

            foreach (var point in bezierPoints)
            {
                // Calculate perpendicular direction for offset
                int nextIndex = bezierPoints.IndexOf(point) + 1;
                if (nextIndex < bezierPoints.Count)
                {
                    var nextPoint = bezierPoints[nextIndex];
                    float dx = nextPoint.X - point.X;
                    float dy = nextPoint.Y - point.Y;
                    float length = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    if (length > 0)
                    {
                        // Perpendicular vector
                        float perpX = -dy / length;
                        float perpY = dx / length;

                        outerPath.Add(new SKPoint(point.X - perpX * outerRadius, point.Y - perpY * outerRadius));
                        innerPath.Add(new SKPoint(point.X - perpX * innerRadius, point.Y - perpY * innerRadius));
                    }
                }
            }

            return new WellborePathResult(outerPath, innerPath);
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
        /// <param name="boreholeIndex">The borehole index.</param>
        /// <param name="tubeIndex">The tube index.</param>
        /// <param name="tubeSpace">The tube spacing.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <returns>The tubing path points.</returns>
        public List<SKPoint> CalculateTubingPath(WellData_Borehole borehole, int boreholeIndex, int tubeIndex, float tubeSpace, float centerX, float canvasHeight)
        {
            var tube = borehole.Tubing[tubeIndex];
            float topY = depthSystem.ToScreenY(tube.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(tube.BottomDepth, canvasHeight);
            float scaledTubeDiameter = tube.Diameter * configuration.DiameterScale;

            float leftPadding = configuration.PaddingToSide;
            float tubeXLeft = centerX - borehole.OuterDiameterOffset + leftPadding + tubeSpace * tubeIndex;
            float tubeXRight = tubeXLeft + scaledTubeDiameter;

            var path = new List<SKPoint>
            {
                new SKPoint(tubeXLeft, topY),
                new SKPoint(tubeXRight, topY),
                new SKPoint(tubeXRight, bottomY),
                new SKPoint(tubeXLeft, bottomY)
            };

            return path;
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
        public WellborePathResult(List<SKPoint> outerPath, List<SKPoint> innerPath)
        {
            OuterPath = outerPath ?? throw new ArgumentNullException(nameof(outerPath));
            InnerPath = innerPath ?? throw new ArgumentNullException(nameof(innerPath));
        }
    }
}

