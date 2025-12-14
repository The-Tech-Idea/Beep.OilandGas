using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Configuration for loading log data.
    /// </summary>
    public class LogLoadConfiguration
    {
        /// <summary>
        /// Gets or sets the curves to load (null = load all).
        /// </summary>
        public List<string> CurvesToLoad { get; set; }

        /// <summary>
        /// Gets or sets the minimum depth to load (0 = no limit).
        /// </summary>
        public double MinDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum depth to load (0 = no limit).
        /// </summary>
        public double MaxDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the depth step (0 = use file step).
        /// </summary>
        public double DepthStep { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether to interpolate missing values.
        /// </summary>
        public bool InterpolateMissingValues { get; set; } = false;

        /// <summary>
        /// Gets or sets the null value to use for missing data.
        /// </summary>
        public double NullValue { get; set; } = -999.25;

        /// <summary>
        /// Gets or sets whether to validate data after loading.
        /// </summary>
        public bool ValidateAfterLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to normalize depth values.
        /// </summary>
        public bool NormalizeDepths { get; set; } = false;

        /// <summary>
        /// Gets or sets the target depth unit for normalization.
        /// </summary>
        public string TargetDepthUnit { get; set; } = "feet";

        /// <summary>
        /// Gets or sets whether to map curve mnemonics to PWLS (Practical Well Log Standard) property names.
        /// When enabled, vendor-specific mnemonics (e.g., "GR", "RT", "NPHI") are mapped to standard PWLS names.
        /// </summary>
        public bool UsePwlsMapping { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to keep original mnemonics in addition to PWLS names (creates aliases).
        /// </summary>
        public bool KeepOriginalMnemonics { get; set; } = true;
    }
}

