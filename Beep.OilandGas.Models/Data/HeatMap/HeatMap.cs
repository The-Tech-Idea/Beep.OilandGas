using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for heat map result.
    /// </summary>
    public class HeatMapResult : ModelEntityBase
    {
        private string HeatMapIdValue = string.Empty;

        public string HeatMapId

        {

            get { return this.HeatMapIdValue; }

            set { SetProperty(ref HeatMapIdValue, value); }

        }
        private string HeatMapNameValue = string.Empty;

        public string HeatMapName

        {

            get { return this.HeatMapNameValue; }

            set { SetProperty(ref HeatMapNameValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private List<HeatMapDataPoint> DataPointsValue = new();

        public List<HeatMapDataPoint> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }
        private HeatMapConfigurationRecord ConfigurationValue = new();

        public HeatMapConfigurationRecord Configuration

        {

            get { return this.ConfigurationValue; }

            set { SetProperty(ref ConfigurationValue, value); }

        }
        private byte[]? RenderedImageValue;

        public byte[]? RenderedImage

        {

            get { return this.RenderedImageValue; }

            set { SetProperty(ref RenderedImageValue, value); }

        }
    }

    /// <summary>
    /// DTO for heat map configuration (simplified for storage).
    /// </summary>
    public class HeatMapConfigurationRecord : ModelEntityBase
    {
        private string ConfigurationIdValue = string.Empty;

        public string ConfigurationId

        {

            get { return this.ConfigurationIdValue; }

            set { SetProperty(ref ConfigurationIdValue, value); }

        }
        private string ConfigurationNameValue = string.Empty;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ColorSchemeTypeValue = "Viridis";

        public string ColorSchemeType

        {

            get { return this.ColorSchemeTypeValue; }

            set { SetProperty(ref ColorSchemeTypeValue, value); }

        }
        private int ColorStepsValue = 256;

        public int ColorSteps

        {

            get { return this.ColorStepsValue; }

            set { SetProperty(ref ColorStepsValue, value); }

        }
        private bool ShowLegendValue = true;

        public bool ShowLegend

        {

            get { return this.ShowLegendValue; }

            set { SetProperty(ref ShowLegendValue, value); }

        }
        private bool UseInterpolationValue = false;

        public bool UseInterpolation

        {

            get { return this.UseInterpolationValue; }

            set { SetProperty(ref UseInterpolationValue, value); }

        }
        private string InterpolationMethodValue = "InverseDistanceWeighting";

        public string InterpolationMethod

        {

            get { return this.InterpolationMethodValue; }

            set { SetProperty(ref InterpolationMethodValue, value); }

        }
        private double InterpolationCellSizeValue = 10.0;

        public double InterpolationCellSize

        {

            get { return this.InterpolationCellSizeValue; }

            set { SetProperty(ref InterpolationCellSizeValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
    }

    /// <summary>
    /// Configuration for generating a heat map
    /// </summary>
    public class HeatMapConfiguration : ModelEntityBase
    {
        private string ConfigurationNameValue = string.Empty;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ColorSchemeTypeValue = "Viridis";

        public string ColorSchemeType

        {

            get { return this.ColorSchemeTypeValue; }

            set { SetProperty(ref ColorSchemeTypeValue, value); }

        }
        private int ColorStepsValue = 256;

        public int ColorSteps

        {

            get { return this.ColorStepsValue; }

            set { SetProperty(ref ColorStepsValue, value); }

        }
        private bool ShowLegendValue = true;

        public bool ShowLegend

        {

            get { return this.ShowLegendValue; }

            set { SetProperty(ref ShowLegendValue, value); }

        }
        private bool UseInterpolationValue = false;

        public bool UseInterpolation

        {

            get { return this.UseInterpolationValue; }

            set { SetProperty(ref UseInterpolationValue, value); }

        }
        private string InterpolationMethodValue = "InverseDistanceWeighting";

        public string InterpolationMethod

        {

            get { return this.InterpolationMethodValue; }

            set { SetProperty(ref InterpolationMethodValue, value); }

        }
        private double InterpolationCellSizeValue = 10.0;

        public double InterpolationCellSize

        {

            get { return this.InterpolationCellSizeValue; }

            set { SetProperty(ref InterpolationCellSizeValue, value); }

        }
    }

    /// <summary>
    /// Request for generating a heat map
    /// </summary>
    public class GenerateHeatMapRequest : ModelEntityBase
    {
        private List<HeatMapDataPoint> DataPointsValue = new();

        [Required(ErrorMessage = "DataPoints are required")]
        [MinLength(1, ErrorMessage = "At least one data point is required")]
        public List<HeatMapDataPoint> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }

        private HeatMapConfiguration ConfigurationValue = null!;


        [Required(ErrorMessage = "Configuration is required")]
        public HeatMapConfiguration Configuration


        {


            get { return this.ConfigurationValue; }


            set { SetProperty(ref ConfigurationValue, value); }


        }
    }

    /// <summary>
    /// Request for generating a production heat map
    /// </summary>
    public class GenerateProductionHeatMapRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        [Required(ErrorMessage = "FieldId is required")]
        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        private DateTime StartDateValue;


        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate


        {


            get { return this.StartDateValue; }


            set { SetProperty(ref StartDateValue, value); }


        }

        private DateTime EndDateValue;


        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate


        {


            get { return this.EndDateValue; }


            set { SetProperty(ref EndDateValue, value); }


        }

         private string? ProductionTypeValue;


         public string? ProductionType


         {


             get { return this.ProductionTypeValue; }


             set { SetProperty(ref ProductionTypeValue, value); }


         } // OIL, GAS, WATER
     }

     /// <summary>
     /// DTO for thermal analysis results
     /// </summary>
     public class ThermalAnalysisResult : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal AverageTemperatureValue;

         public decimal AverageTemperature

         {

             get { return this.AverageTemperatureValue; }

             set { SetProperty(ref AverageTemperatureValue, value); }

         }
         private decimal MaximumTemperatureValue;

         public decimal MaximumTemperature

         {

             get { return this.MaximumTemperatureValue; }

             set { SetProperty(ref MaximumTemperatureValue, value); }

         }
         private decimal MinimumTemperatureValue;

         public decimal MinimumTemperature

         {

             get { return this.MinimumTemperatureValue; }

             set { SetProperty(ref MinimumTemperatureValue, value); }

         }
         private decimal TemperatureGradientValue;

         public decimal TemperatureGradient

         {

             get { return this.TemperatureGradientValue; }

             set { SetProperty(ref TemperatureGradientValue, value); }

         }
         private decimal StandardDeviationValue;

         public decimal StandardDeviation

         {

             get { return this.StandardDeviationValue; }

             set { SetProperty(ref StandardDeviationValue, value); }

         }
         private string ThermalPatternValue = string.Empty;

         public string ThermalPattern

         {

             get { return this.ThermalPatternValue; }

             set { SetProperty(ref ThermalPatternValue, value); }

         } // Hot Spot, Cold Spot, Uniform
         private int DataPointCountValue;

         public int DataPointCount

         {

             get { return this.DataPointCountValue; }

             set { SetProperty(ref DataPointCountValue, value); }

         }
         private decimal TemperatureRangeValue;

         public decimal TemperatureRange

         {

             get { return this.TemperatureRangeValue; }

             set { SetProperty(ref TemperatureRangeValue, value); }

         }
     }

     /// <summary>
     /// DTO for thermal anomaly detection
     /// </summary>
     public class ThermalAnomaly : ModelEntityBase
     {
         private string AnomalyIdValue = string.Empty;

         public string AnomalyId

         {

             get { return this.AnomalyIdValue; }

             set { SetProperty(ref AnomalyIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime DetectionDateValue;

         public DateTime DetectionDate

         {

             get { return this.DetectionDateValue; }

             set { SetProperty(ref DetectionDateValue, value); }

         }
         private decimal AnomalyTemperatureValue;

         public decimal AnomalyTemperature

         {

             get { return this.AnomalyTemperatureValue; }

             set { SetProperty(ref AnomalyTemperatureValue, value); }

         }
         private decimal ExpectedTemperatureValue;

         public decimal ExpectedTemperature

         {

             get { return this.ExpectedTemperatureValue; }

             set { SetProperty(ref ExpectedTemperatureValue, value); }

         }
         private decimal TemperatureDeviationValue;

         public decimal TemperatureDeviation

         {

             get { return this.TemperatureDeviationValue; }

             set { SetProperty(ref TemperatureDeviationValue, value); }

         }
         private decimal DeviationPercentValue;

         public decimal DeviationPercent

         {

             get { return this.DeviationPercentValue; }

             set { SetProperty(ref DeviationPercentValue, value); }

         }
         private string AnomalyTypeValue = string.Empty;

         public string AnomalyType

         {

             get { return this.AnomalyTypeValue; }

             set { SetProperty(ref AnomalyTypeValue, value); }

         } // Hot Anomaly, Cold Anomaly, Gradient Anomaly
         private string SeverityValue = string.Empty;

         public string Severity

         {

             get { return this.SeverityValue; }

             set { SetProperty(ref SeverityValue, value); }

         } // Low, Medium, High, Critical
         private decimal XValue;

         public decimal X

         {

             get { return this.XValue; }

             set { SetProperty(ref XValue, value); }

         }
         private decimal YValue;

         public decimal Y

         {

             get { return this.YValue; }

             set { SetProperty(ref YValue, value); }

         }
         private string DescriptionValue = string.Empty;

         public string Description

         {

             get { return this.DescriptionValue; }

             set { SetProperty(ref DescriptionValue, value); }

         }
         private List<string> RecommendedActionsValue = new();

         public List<string> RecommendedActions

         {

             get { return this.RecommendedActionsValue; }

             set { SetProperty(ref RecommendedActionsValue, value); }

         }
     }

     /// <summary>
     /// DTO for thermal trend analysis
     /// </summary>
     public class ThermalTrendAnalysis : ModelEntityBase
     {
         private string TrendIdValue = string.Empty;

         public string TrendId

         {

             get { return this.TrendIdValue; }

             set { SetProperty(ref TrendIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private int MonthsAnalyzedValue;

         public int MonthsAnalyzed

         {

             get { return this.MonthsAnalyzedValue; }

             set { SetProperty(ref MonthsAnalyzedValue, value); }

         }
         private decimal TemperatureTrendValue;

         public decimal TemperatureTrend

         {

             get { return this.TemperatureTrendValue; }

             set { SetProperty(ref TemperatureTrendValue, value); }

         } // °C/month
         private decimal TrendSlopeValue;

         public decimal TrendSlope

         {

             get { return this.TrendSlopeValue; }

             set { SetProperty(ref TrendSlopeValue, value); }

         }
         private string TrendDirectionValue = string.Empty;

         public string TrendDirection

         {

             get { return this.TrendDirectionValue; }

             set { SetProperty(ref TrendDirectionValue, value); }

         } // Increasing, Decreasing, Stable
         private decimal PercentChangeValue;

         public decimal PercentChange

         {

             get { return this.PercentChangeValue; }

             set { SetProperty(ref PercentChangeValue, value); }

         }
         private List<decimal> HistoricalTemperaturesValue = new();

         public List<decimal> HistoricalTemperatures

         {

             get { return this.HistoricalTemperaturesValue; }

             set { SetProperty(ref HistoricalTemperaturesValue, value); }

         }
         private decimal PredictedTemperatureValue;

         public decimal PredictedTemperature

         {

             get { return this.PredictedTemperatureValue; }

             set { SetProperty(ref PredictedTemperatureValue, value); }

         }
         private int PredictionMonthsValue;

         public int PredictionMonths

         {

             get { return this.PredictionMonthsValue; }

             set { SetProperty(ref PredictionMonthsValue, value); }

         }
         private decimal RSquaredValue;

         public decimal RSquared

         {

             get { return this.RSquaredValue; }

             set { SetProperty(ref RSquaredValue, value); }

         } // Trend confidence
     }

     /// <summary>
     /// DTO for temperature gradient analysis
     /// </summary>
     public class TemperatureGradientAnalysis : ModelEntityBase
     {
         private string GradientIdValue = string.Empty;

         public string GradientId

         {

             get { return this.GradientIdValue; }

             set { SetProperty(ref GradientIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal AverageGradientValue;

         public decimal AverageGradient

         {

             get { return this.AverageGradientValue; }

             set { SetProperty(ref AverageGradientValue, value); }

         } // °C per unit distance
         private decimal MaxGradientValue;

         public decimal MaxGradient

         {

             get { return this.MaxGradientValue; }

             set { SetProperty(ref MaxGradientValue, value); }

         }
         private decimal MinGradientValue;

         public decimal MinGradient

         {

             get { return this.MinGradientValue; }

             set { SetProperty(ref MinGradientValue, value); }

         }
         private decimal HorizontalGradientValue;

         public decimal HorizontalGradient

         {

             get { return this.HorizontalGradientValue; }

             set { SetProperty(ref HorizontalGradientValue, value); }

         }
         private decimal VerticalGradientValue;

         public decimal VerticalGradient

         {

             get { return this.VerticalGradientValue; }

             set { SetProperty(ref VerticalGradientValue, value); }

         }
         private string GradientPatternValue = string.Empty;

         public string GradientPattern

         {

             get { return this.GradientPatternValue; }

             set { SetProperty(ref GradientPatternValue, value); }

         } // Linear, Exponential, Nonlinear
         private List<GradientPoint> GradientPointsValue = new();

         public List<GradientPoint> GradientPoints

         {

             get { return this.GradientPointsValue; }

             set { SetProperty(ref GradientPointsValue, value); }

         }
     }

     /// <summary>
     /// Point in gradient analysis
     /// </summary>
     public class GradientPoint : ModelEntityBase
     {
         private decimal DistanceValue;

         public decimal Distance

         {

             get { return this.DistanceValue; }

             set { SetProperty(ref DistanceValue, value); }

         }
         private decimal TemperatureValue;

         public decimal Temperature

         {

             get { return this.TemperatureValue; }

             set { SetProperty(ref TemperatureValue, value); }

         }
         private decimal LocalGradientValue;

         public decimal LocalGradient

         {

             get { return this.LocalGradientValue; }

             set { SetProperty(ref LocalGradientValue, value); }

         }
     }

     /// <summary>
     /// DTO for temperature zone identification
     /// </summary>
     public class TemperatureZone : ModelEntityBase
     {
         private string ZoneIdValue = string.Empty;

         public string ZoneId

         {

             get { return this.ZoneIdValue; }

             set { SetProperty(ref ZoneIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime IdentificationDateValue;

         public DateTime IdentificationDate

         {

             get { return this.IdentificationDateValue; }

             set { SetProperty(ref IdentificationDateValue, value); }

         }
         private decimal MinTemperatureValue;

         public decimal MinTemperature

         {

             get { return this.MinTemperatureValue; }

             set { SetProperty(ref MinTemperatureValue, value); }

         }
         private decimal MaxTemperatureValue;

         public decimal MaxTemperature

         {

             get { return this.MaxTemperatureValue; }

             set { SetProperty(ref MaxTemperatureValue, value); }

         }
         private decimal AverageTemperatureValue;

         public decimal AverageTemperature

         {

             get { return this.AverageTemperatureValue; }

             set { SetProperty(ref AverageTemperatureValue, value); }

         }
         private string ZoneClassificationValue = string.Empty;

         public string ZoneClassification

         {

             get { return this.ZoneClassificationValue; }

             set { SetProperty(ref ZoneClassificationValue, value); }

         } // Hot Zone, Normal Zone, Cold Zone
         private decimal AreaValue;

         public decimal Area

         {

             get { return this.AreaValue; }

             set { SetProperty(ref AreaValue, value); }

         }
         private int PointCountValue;

         public int PointCount

         {

             get { return this.PointCountValue; }

             set { SetProperty(ref PointCountValue, value); }

         }
         private List<decimal> BoundaryCoordinatesValue = new();

         public List<decimal> BoundaryCoordinates

         {

             get { return this.BoundaryCoordinatesValue; }

             set { SetProperty(ref BoundaryCoordinatesValue, value); }

         }
     }

     /// <summary>
     /// DTO for thermal imaging quality assessment
     /// </summary>
     public class ThermalImageQuality : ModelEntityBase
     {
         private string QualityIdValue = string.Empty;

         public string QualityId

         {

             get { return this.QualityIdValue; }

             set { SetProperty(ref QualityIdValue, value); }

         }
         private string ImageIdValue = string.Empty;

         public string ImageId

         {

             get { return this.ImageIdValue; }

             set { SetProperty(ref ImageIdValue, value); }

         }
         private DateTime AssessmentDateValue;

         public DateTime AssessmentDate

         {

             get { return this.AssessmentDateValue; }

             set { SetProperty(ref AssessmentDateValue, value); }

         }
         private decimal ClarityValue;

         public decimal Clarity

         {

             get { return this.ClarityValue; }

             set { SetProperty(ref ClarityValue, value); }

         } // 0-100
         private decimal NoiseLevelValue;

         public decimal NoiseLevel

         {

             get { return this.NoiseLevelValue; }

             set { SetProperty(ref NoiseLevelValue, value); }

         } // 0-100
         private decimal ContrastValue;

         public decimal Contrast

         {

             get { return this.ContrastValue; }

             set { SetProperty(ref ContrastValue, value); }

         } // 0-100
         private decimal OverallQualityScoreValue;

         public decimal OverallQualityScore

         {

             get { return this.OverallQualityScoreValue; }

             set { SetProperty(ref OverallQualityScoreValue, value); }

         } // 0-100
         private string QualityRatingValue = string.Empty;

         public string QualityRating

         {

             get { return this.QualityRatingValue; }

             set { SetProperty(ref QualityRatingValue, value); }

         } // Excellent, Good, Fair, Poor
         private List<string> QualityIssuesValue = new();

         public List<string> QualityIssues

         {

             get { return this.QualityIssuesValue; }

             set { SetProperty(ref QualityIssuesValue, value); }

         }
         private List<string> RecommendedImprovementsValue = new();

         public List<string> RecommendedImprovements

         {

             get { return this.RecommendedImprovementsValue; }

             set { SetProperty(ref RecommendedImprovementsValue, value); }

         }
     }

     /// <summary>
     /// DTO for thermal comparison analysis (before/after)
     /// </summary>
     public class ThermalComparisonResult : ModelEntityBase
     {
         private string ComparisonIdValue = string.Empty;

         public string ComparisonId

         {

             get { return this.ComparisonIdValue; }

             set { SetProperty(ref ComparisonIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime ComparisonDateValue;

         public DateTime ComparisonDate

         {

             get { return this.ComparisonDateValue; }

             set { SetProperty(ref ComparisonDateValue, value); }

         }
         private DateTime BaselineDateValue;

         public DateTime BaselineDate

         {

             get { return this.BaselineDateValue; }

             set { SetProperty(ref BaselineDateValue, value); }

         }
         private DateTime CurrentDateValue;

         public DateTime CurrentDate

         {

             get { return this.CurrentDateValue; }

             set { SetProperty(ref CurrentDateValue, value); }

         }
         private decimal BaselineAverageTemperatureValue;

         public decimal BaselineAverageTemperature

         {

             get { return this.BaselineAverageTemperatureValue; }

             set { SetProperty(ref BaselineAverageTemperatureValue, value); }

         }
         private decimal CurrentAverageTemperatureValue;

         public decimal CurrentAverageTemperature

         {

             get { return this.CurrentAverageTemperatureValue; }

             set { SetProperty(ref CurrentAverageTemperatureValue, value); }

         }
         private decimal TemperatureChangeValue;

         public decimal TemperatureChange

         {

             get { return this.TemperatureChangeValue; }

             set { SetProperty(ref TemperatureChangeValue, value); }

         }
         private decimal PercentChangeValue;

         public decimal PercentChange

         {

             get { return this.PercentChangeValue; }

             set { SetProperty(ref PercentChangeValue, value); }

         }
         private decimal BaselineStdDevValue;

         public decimal BaselineStdDev

         {

             get { return this.BaselineStdDevValue; }

             set { SetProperty(ref BaselineStdDevValue, value); }

         }
         private decimal CurrentStdDevValue;

         public decimal CurrentStdDev

         {

             get { return this.CurrentStdDevValue; }

             set { SetProperty(ref CurrentStdDevValue, value); }

         }
         private string SignificantChangeValue = string.Empty;

         public string SignificantChange

         {

             get { return this.SignificantChangeValue; }

             set { SetProperty(ref SignificantChangeValue, value); }

         } // Yes, No
         private List<string> ChangePatternsValue = new();

         public List<string> ChangePatterns

         {

             get { return this.ChangePatternsValue; }

             set { SetProperty(ref ChangePatternsValue, value); }

         }
     }
}







