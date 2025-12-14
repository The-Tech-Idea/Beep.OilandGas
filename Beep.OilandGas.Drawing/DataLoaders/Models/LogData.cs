using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents log data with curves and metadata.
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the log name.
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// Gets or sets the log type (wireline, mud, production, etc.).
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Gets or sets the start depth.
        /// </summary>
        public double StartDepth { get; set; }

        /// <summary>
        /// Gets or sets the end depth.
        /// </summary>
        public double EndDepth { get; set; }

        /// <summary>
        /// Gets or sets the depth step.
        /// </summary>
        public double DepthStep { get; set; }

        /// <summary>
        /// Gets or sets the depth unit (feet, meters).
        /// </summary>
        public string DepthUnit { get; set; } = "feet";

        /// <summary>
        /// Gets or sets the log curves (curve name to values).
        /// </summary>
        public Dictionary<string, List<double>> Curves { get; set; } = new Dictionary<string, List<double>>();

        /// <summary>
        /// Gets or sets the depth values.
        /// </summary>
        public List<double> Depths { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the curve metadata (units, descriptions, etc.).
        /// </summary>
        public Dictionary<string, LogCurveMetadata> CurveMetadata { get; set; } = new Dictionary<string, LogCurveMetadata>();

        /// <summary>
        /// Gets or sets the log metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the service company.
        /// </summary>
        public string ServiceCompany { get; set; }

        /// <summary>
        /// Gets the number of data points.
        /// </summary>
        public int DataPointCount => Depths?.Count ?? 0;

        /// <summary>
        /// Gets a curve value at a specific depth index.
        /// </summary>
        /// <param name="curveName">The curve name.</param>
        /// <param name="index">The depth index.</param>
        /// <returns>The curve value, or null if not found.</returns>
        public double? GetCurveValue(string curveName, int index)
        {
            if (Curves.ContainsKey(curveName) && index >= 0 && index < Curves[curveName].Count)
            {
                return Curves[curveName][index];
            }
            return null;
        }

        /// <summary>
        /// Gets a curve value at a specific depth.
        /// </summary>
        /// <param name="curveName">The curve name.</param>
        /// <param name="depth">The depth.</param>
        /// <returns>The curve value, or null if not found.</returns>
        public double? GetCurveValueAtDepth(string curveName, double depth)
        {
            if (!Curves.ContainsKey(curveName) || Depths == null || Depths.Count == 0)
                return null;

            // Find closest depth index
            int index = Depths.BinarySearch(depth);
            if (index < 0)
            {
                index = ~index;
                if (index >= Depths.Count)
                    index = Depths.Count - 1;
                else if (index > 0 && Math.Abs(Depths[index - 1] - depth) < Math.Abs(Depths[index] - depth))
                    index--;
            }

            return GetCurveValue(curveName, index);
        }
    }

    /// <summary>
    /// Metadata for a log curve.
    /// </summary>
    public class LogCurveMetadata
    {
        /// <summary>
        /// Gets or sets the original curve mnemonic (vendor-specific).
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the display name (PWLS-mapped name if PWLS mapping is enabled).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the curve unit.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the curve description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the null value (value representing missing data).
        /// </summary>
        public double? NullValue { get; set; } = -999.25;
    }
}

