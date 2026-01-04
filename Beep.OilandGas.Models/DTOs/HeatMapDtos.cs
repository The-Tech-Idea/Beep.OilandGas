using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    /// <summary>
    /// Request for generating a heat map
    /// </summary>
    public class GenerateHeatMapRequest
    {
        [Required(ErrorMessage = "DataPoints are required")]
        [MinLength(1, ErrorMessage = "At least one data point is required")]
        public List<HeatMapDataPoint> DataPoints { get; set; } = new();

        [Required(ErrorMessage = "Configuration is required")]
        public HeatMapConfiguration Configuration { get; set; } = null!;
    }

    /// <summary>
    /// Request for generating a production heat map
    /// </summary>
    public class GenerateProductionHeatMapRequest
    {
        [Required(ErrorMessage = "FieldId is required")]
        public string FieldId { get; set; } = string.Empty;

        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }

        public string? ProductionType { get; set; } // OIL, GAS, WATER
    }
}

