using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents a depth interval that can be rendered alongside log tracks.
    /// </summary>
    public class LogIntervalData
    {
        /// <summary>
        /// Gets or sets the interval identifier.
        /// </summary>
        public string IntervalId { get; set; }

        /// <summary>
        /// Gets or sets the interval label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the top depth of the interval.
        /// </summary>
        public double TopDepth { get; set; }

        /// <summary>
        /// Gets or sets the bottom depth of the interval.
        /// </summary>
        public double BottomDepth { get; set; }

        /// <summary>
        /// Gets or sets the lithology associated with the interval.
        /// </summary>
        public string Lithology { get; set; }

        /// <summary>
        /// Gets or sets the facies associated with the interval.
        /// </summary>
        public string Facies { get; set; }

        /// <summary>
        /// Gets or sets the optional hex color code used to fill the interval.
        /// </summary>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets the optional pattern type for lithology rendering.
        /// </summary>
        public string PatternType { get; set; }

        /// <summary>
        /// Gets or sets whether the interval is considered pay.
        /// </summary>
        public bool IsPayZone { get; set; } = true;

        /// <summary>
        /// Gets the interval thickness.
        /// </summary>
        public double Thickness => Math.Abs(BottomDepth - TopDepth);

        /// <summary>
        /// Gets or sets metadata associated with the interval.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}