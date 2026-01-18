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
    /// Configuration for generating a heat map
    /// </summary>
    public class HeatMapConfiguration
    {
        public string ConfigurationName { get; set; } = string.Empty;
        public string ColorSchemeType { get; set; } = "Viridis";
        public int ColorSteps { get; set; } = 256;
        public bool ShowLegend { get; set; } = true;
        public bool UseInterpolation { get; set; } = false;
        public string InterpolationMethod { get; set; } = "InverseDistanceWeighting";
        public double InterpolationCellSize { get; set; } = 10.0;
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

     /// <summary>
     /// DTO for thermal analysis results
     /// </summary>
     public class ThermalAnalysisResultDto
     {
         public string AnalysisId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal AverageTemperature { get; set; }
         public decimal MaximumTemperature { get; set; }
         public decimal MinimumTemperature { get; set; }
         public decimal TemperatureGradient { get; set; }
         public decimal StandardDeviation { get; set; }
         public string ThermalPattern { get; set; } = string.Empty; // Hot Spot, Cold Spot, Uniform
         public int DataPointCount { get; set; }
         public decimal TemperatureRange { get; set; }
     }

     /// <summary>
     /// DTO for thermal anomaly detection
     /// </summary>
     public class ThermalAnomalyDto
     {
         public string AnomalyId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime DetectionDate { get; set; }
         public decimal AnomalyTemperature { get; set; }
         public decimal ExpectedTemperature { get; set; }
         public decimal TemperatureDeviation { get; set; }
         public decimal DeviationPercent { get; set; }
         public string AnomalyType { get; set; } = string.Empty; // Hot Anomaly, Cold Anomaly, Gradient Anomaly
         public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
         public decimal X { get; set; }
         public decimal Y { get; set; }
         public string Description { get; set; } = string.Empty;
         public List<string> RecommendedActions { get; set; } = new();
     }

     /// <summary>
     /// DTO for thermal trend analysis
     /// </summary>
     public class ThermalTrendAnalysisDto
     {
         public string TrendId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public int MonthsAnalyzed { get; set; }
         public decimal TemperatureTrend { get; set; } // °C/month
         public decimal TrendSlope { get; set; }
         public string TrendDirection { get; set; } = string.Empty; // Increasing, Decreasing, Stable
         public decimal PercentChange { get; set; }
         public List<decimal> HistoricalTemperatures { get; set; } = new();
         public decimal PredictedTemperature { get; set; }
         public int PredictionMonths { get; set; }
         public decimal RSquared { get; set; } // Trend confidence
     }

     /// <summary>
     /// DTO for temperature gradient analysis
     /// </summary>
     public class TemperatureGradientAnalysisDto
     {
         public string GradientId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime AnalysisDate { get; set; }
         public decimal AverageGradient { get; set; } // °C per unit distance
         public decimal MaxGradient { get; set; }
         public decimal MinGradient { get; set; }
         public decimal HorizontalGradient { get; set; }
         public decimal VerticalGradient { get; set; }
         public string GradientPattern { get; set; } = string.Empty; // Linear, Exponential, Nonlinear
         public List<GradientPoint> GradientPoints { get; set; } = new();
     }

     /// <summary>
     /// Point in gradient analysis
     /// </summary>
     public class GradientPoint
     {
         public decimal Distance { get; set; }
         public decimal Temperature { get; set; }
         public decimal LocalGradient { get; set; }
     }

     /// <summary>
     /// DTO for temperature zone identification
     /// </summary>
     public class TemperatureZoneDto
     {
         public string ZoneId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime IdentificationDate { get; set; }
         public decimal MinTemperature { get; set; }
         public decimal MaxTemperature { get; set; }
         public decimal AverageTemperature { get; set; }
         public string ZoneClassification { get; set; } = string.Empty; // Hot Zone, Normal Zone, Cold Zone
         public decimal Area { get; set; }
         public int PointCount { get; set; }
         public List<decimal> BoundaryCoordinates { get; set; } = new();
     }

     /// <summary>
     /// DTO for thermal imaging quality assessment
     /// </summary>
     public class ThermalImageQualityDto
     {
         public string QualityId { get; set; } = string.Empty;
         public string ImageId { get; set; } = string.Empty;
         public DateTime AssessmentDate { get; set; }
         public decimal Clarity { get; set; } // 0-100
         public decimal NoiseLevel { get; set; } // 0-100
         public decimal Contrast { get; set; } // 0-100
         public decimal OverallQualityScore { get; set; } // 0-100
         public string QualityRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
         public List<string> QualityIssues { get; set; } = new();
         public List<string> RecommendedImprovements { get; set; } = new();
     }

     /// <summary>
     /// DTO for thermal comparison analysis (before/after)
     /// </summary>
     public class ThermalComparisonResultDto
     {
         public string ComparisonId { get; set; } = string.Empty;
         public string LocationId { get; set; } = string.Empty;
         public DateTime ComparisonDate { get; set; }
         public DateTime BaselineDate { get; set; }
         public DateTime CurrentDate { get; set; }
         public decimal BaselineAverageTemperature { get; set; }
         public decimal CurrentAverageTemperature { get; set; }
         public decimal TemperatureChange { get; set; }
         public decimal PercentChange { get; set; }
         public decimal BaselineStdDev { get; set; }
         public decimal CurrentStdDev { get; set; }
         public string SignificantChange { get; set; } = string.Empty; // Yes, No
         public List<string> ChangePatterns { get; set; } = new();
     }
}




