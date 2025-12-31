using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.HeatMap;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for heat map result.
    /// </summary>
    public class HeatMapResultDto
    {
        public string HeatMapId { get; set; } = string.Empty;
        public string HeatMapName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<HeatMapDataPoint> DataPoints { get; set; } = new();
        public HeatMapConfigurationDto Configuration { get; set; } = new();
        public byte[]? RenderedImage { get; set; }
    }

    /// <summary>
    /// DTO for heat map configuration (simplified for storage).
    /// </summary>
    public class HeatMapConfigurationDto
    {
        public string ConfigurationId { get; set; } = string.Empty;
        public string ConfigurationName { get; set; } = string.Empty;
        public string ColorSchemeType { get; set; } = "Viridis";
        public int ColorSteps { get; set; } = 256;
        public bool ShowLegend { get; set; } = true;
        public bool UseInterpolation { get; set; } = false;
        public string InterpolationMethod { get; set; } = "InverseDistanceWeighting";
        public double InterpolationCellSize { get; set; } = 10.0;
        public DateTime CreatedDate { get; set; }
    }
}

