using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents a deviation survey with multiple survey points.
    /// </summary>
    public class DeviationSurvey
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the borehole identifier.
        /// </summary>
        public string BoreholeIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the survey points.
        /// </summary>
        public List<DeviationSurveyPoint> SurveyPoints { get; set; } = new List<DeviationSurveyPoint>();

        /// <summary>
        /// Gets or sets the survey date.
        /// </summary>
        public DateTime? SurveyDate { get; set; }

        /// <summary>
        /// Gets or sets the survey company.
        /// </summary>
        public string SurveyCompany { get; set; }

        /// <summary>
        /// Gets or sets the survey method.
        /// </summary>
        public string SurveyMethod { get; set; }

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the total measured depth.
        /// </summary>
        public double? TotalMeasuredDepth => SurveyPoints?.Count > 0 ? SurveyPoints.Max(p => p.MD) : null;

        /// <summary>
        /// Gets the maximum deviation angle.
        /// </summary>
        public double? MaxDeviationAngle => SurveyPoints?.Count > 0 ? SurveyPoints.Max(p => p.DEVIATION_ANGLE) : null;

        /// <summary>
        /// Gets the maximum horizontal displacement.
        /// </summary>
        public double? MaxHorizontalDisplacement
        {
            get
            {
                if (SurveyPoints == null || SurveyPoints.Count == 0)
                    return null;

                double maxDisplacement = 0;
                foreach (var point in SurveyPoints)
                {
                    double displacement = point.MD * Math.Sin(DegreesToRadians(point.DEVIATION_ANGLE));
                    if (displacement > maxDisplacement)
                        maxDisplacement = displacement;
                }
                return maxDisplacement;
            }
        }

        /// <summary>
        /// Gets a survey point at a specific measured depth.
        /// </summary>
        public DeviationSurveyPoint GetPointAtDepth(double measuredDepth)
        {
            if (SurveyPoints == null || SurveyPoints.Count == 0)
                return null;

            // Find closest point
            return SurveyPoints.OrderBy(p => Math.Abs(p.MD - measuredDepth)).FirstOrDefault();
        }

        /// <summary>
        /// Gets interpolated survey point at a specific measured depth.
        /// </summary>
        public DeviationSurveyPoint GetInterpolatedPointAtDepth(double measuredDepth)
        {
            if (SurveyPoints == null || SurveyPoints.Count == 0)
                return null;

            // Find surrounding points
            var points = SurveyPoints.OrderBy(p => p.MD).ToList();
            var before = points.LastOrDefault(p => p.MD <= measuredDepth);
            var after = points.FirstOrDefault(p => p.MD >= measuredDepth);

            if (before == null)
                return after;
            if (after == null)
                return before;
            if (before == after)
                return before;

            // Interpolate
            double t = (measuredDepth - before.MD) / (after.MD - before.MD);
            return new DeviationSurveyPoint
            {
                MD = measuredDepth,
                DEVIATION_ANGLE = before.DEVIATION_ANGLE + t * (after.DEVIATION_ANGLE - before.DEVIATION_ANGLE),
                AZIMUTH = InterpolateAngle(before.AZIMUTH, after.AZIMUTH, t)
            };
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Interpolates an angle (handles 0-360 wrap).
        /// </summary>
        private double InterpolateAngle(double angle1, double angle2, double t)
        {
            double diff = angle2 - angle1;
            if (Math.Abs(diff) > 180)
            {
                if (diff > 0)
                    diff -= 360;
                else
                    diff += 360;
            }
            return angle1 + t * diff;
        }
    }
}

