using System;
using System.Collections.Generic;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Extended well schematic data with metadata and additional information.
    /// </summary>
    public class WellSchematicData
    {
        /// <summary>
        /// Gets or sets the well data.
        /// </summary>
        public WellData WellData { get; set; }

        /// <summary>
        /// Gets or sets the data source identifier.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Gets or sets when the data was loaded.
        /// </summary>
        public DateTime LoadedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the data version.
        /// </summary>
        public string DataVersion { get; set; }

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets validation status.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Gets or sets validation errors if any.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration for loading well schematic data.
    /// </summary>
    public class WellSchematicLoadConfiguration
    {
        /// <summary>
        /// Gets or sets whether to load casing data.
        /// </summary>
        public bool LoadCasing { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load tubing data.
        /// </summary>
        public bool LoadTubing { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load equipment data.
        /// </summary>
        public bool LoadEquipment { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load perforation data.
        /// </summary>
        public bool LoadPerforations { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load deviation survey data.
        /// </summary>
        public bool LoadDeviationSurvey { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate data after loading.
        /// </summary>
        public bool ValidateAfterLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum depth to load (0 = no limit).
        /// </summary>
        public float MaxDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum depth to load (0 = no limit).
        /// </summary>
        public float MinDepth { get; set; } = 0;
    }
}

