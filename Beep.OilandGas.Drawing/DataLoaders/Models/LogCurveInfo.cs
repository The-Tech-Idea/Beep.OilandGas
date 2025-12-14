using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Information about a log curve.
    /// </summary>
    public class LogCurveInfo
    {
        /// <summary>
        /// Gets or sets the curve name/mnemonic.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the original mnemonic (vendor-specific).
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the curve unit.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the curve description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the API code for the curve.
        /// </summary>
        public string ApiCode { get; set; }

        /// <summary>
        /// Gets or sets the curve type (wireline, mud, production, etc.).
        /// </summary>
        public string CurveType { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in the curve.
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in the curve.
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the null value (value representing missing data).
        /// </summary>
        public double? NullValue { get; set; } = -999.25;

        /// <summary>
        /// Gets or sets the number of data points.
        /// </summary>
        public int DataPointCount { get; set; }

        /// <summary>
        /// Gets or sets the data type (float, integer, etc.).
        /// </summary>
        public string DataType { get; set; } = "float";

        /// <summary>
        /// Gets or sets the curve format.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets additional properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Collection of log curve information.
    /// </summary>
    public class LogCurveInfoCollection : List<LogCurveInfo>
    {
        /// <summary>
        /// Gets curve info by name.
        /// </summary>
        public LogCurveInfo GetByName(string name)
        {
            return this.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all curve names.
        /// </summary>
        public List<string> GetCurveNames()
        {
            return this.ConvertAll(c => c.Name);
        }
    }
}

