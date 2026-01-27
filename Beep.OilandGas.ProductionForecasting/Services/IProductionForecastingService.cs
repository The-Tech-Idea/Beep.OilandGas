using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using ProductionForecastResult = Beep.OilandGas.Models.Data.Calculations.ProductionForecastResult;
using DeclineCurveAnalysis = Beep.OilandGas.Models.Data.Calculations.DeclineCurveAnalysis;
using ProductionForecastPoint = Beep.OilandGas.Models.Data.Calculations.ProductionForecastPoint;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    /// <summary>
    /// Comprehensive production forecasting service interface
    /// Provides DCA, probabilistic forecasting, economic analysis, and risk assessment capabilities
    /// </summary>
    public interface IProductionForecastingService
    {
        #region Core Forecasting Methods

        /// <summary>
        /// Generates a production forecast using specified method
        /// </summary>
        Task<ProductionForecastResult> GenerateForecastAsync(GenerateForecastRequest request);

        /// <summary>
        /// Generates DCA-based forecast using historical production data
        /// </summary>
        Task<ProductionForecastResult> GenerateDCAForecastAsync(string wellUWI, ForecastType declineType, DateTime startDate, DateTime endDate, int forecastPeriod);

        /// <summary>
        /// Generates probabilistic forecast with uncertainty analysis
        /// </summary>
        Task<ProbabilisticForecast> GenerateProbabilisticForecastAsync(string wellUWI, ForecastType declineType, int forecastPeriod, int iterations = 1000);

        /// <summary>
        /// Performs decline curve analysis on historical production data
        /// </summary>
        Task<DeclineCurveAnalysis> PerformDeclineCurveAnalysisAsync(DeclineCurveAnalysisRequest request);

        #endregion

        #region Advanced Analysis Methods

        /// <summary>
        /// Performs economic analysis on forecast results
        /// </summary>
        Task<EconomicAnalysis> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request);

        /// <summary>
        /// Performs risk analysis on forecast scenarios
        /// </summary>
        Task<ForecastRiskAnalysisResult> PerformRiskAnalysisAsync(string forecastId);

        /// <summary>
        /// Validates forecast quality and reliability
        /// </summary>
        Task<ForecastValidationResult> ValidateForecastAsync(string forecastId);

        /// <summary>
        /// Optimizes forecast parameters using machine learning
        /// </summary>
        Task<ProductionForecastResult> OptimizeForecastAsync(string wellUWI, ForecastType forecastMethod);

        #endregion

        #region Data Management Methods

        /// <summary>
        /// Saves forecast to PPDM database
        /// </summary>
        Task SaveForecastAsync(ProductionForecastResult forecast, string userId);

        /// <summary>
        /// Retrieves forecast by ID
        /// </summary>
        Task<ProductionForecastResult?> GetForecastAsync(string forecastId);

        /// <summary>
        /// Retrieves forecasts for well or field
        /// </summary>
        Task<List<ProductionForecastResult>> GetForecastsAsync(string? wellUWI = null, string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Updates existing forecast
        /// </summary>
        Task UpdateForecastAsync(ProductionForecastResult forecast, string userId);

        /// <summary>
        /// Deletes forecast from database
        /// </summary>
        Task DeleteForecastAsync(string forecastId, string userId);

        #endregion

        #region Specialized Forecasting Methods

        /// <summary>
        /// Generates reservoir simulation-based forecast
        /// </summary>
        Task<ProductionForecastResult> GenerateReservoirSimulationForecastAsync(string wellUWI, ReservoirProperties reservoirProperties, int forecastPeriod);

        /// <summary>
        /// Generates type curve-based forecast
        /// </summary>
        Task<ProductionForecastResult> GenerateTypeCurveForecastAsync(string wellUWI, string typeCurveId, int forecastPeriod);

        /// <summary>
        /// Generates forecast using machine learning models
        /// </summary>
        Task<ProductionForecastResult> GenerateMLForecastAsync(string wellUWI, string modelType, int forecastPeriod);

        /// <summary>
        /// Combines multiple forecast methods for ensemble forecasting
        /// </summary>
        Task<ProductionForecastResult> GenerateEnsembleForecastAsync(string wellUWI, List<ForecastType> forecastMethods, int forecastPeriod);

        #endregion

        #region Reporting and Export

        /// <summary>
        /// Exports forecast to specified format
        /// </summary>
        Task<byte[]> ExportForecastAsync(string forecastId, string format = "CSV");

        /// <summary>
        /// Generates forecast report with charts and analysis
        /// </summary>
        Task<ForecastReport> GenerateForecastReportAsync(string forecastId);

        /// <summary>
        /// Compares multiple forecasts
        /// </summary>
        Task<ForecastComparison> CompareForecastsAsync(List<string> forecastIds);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets available forecast methods
        /// </summary>
        Task<List<ForecastMethod>> GetAvailableForecastMethodsAsync();

        /// <summary>
        /// Gets forecast statistics and quality metrics
        /// </summary>
        Task<ForecastStatistics> GetForecastStatisticsAsync(string forecastId);

        /// <summary>
        /// Performs sensitivity analysis on forecast parameters
        /// </summary>
        Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(string forecastId, List<string> parameters);

        #endregion
    }

    #region Supporting DTOs

    /// <summary>
    /// Reservoir properties for simulation-based forecasting
    /// </summary>
    public class ReservoirProperties
    {
        public decimal InitialPressure { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal Porosity { get; set; }
        public decimal Permeability { get; set; }
        public decimal InitialWaterSaturation { get; set; }
        public decimal Thickness { get; set; }
        public decimal DrainageArea { get; set; }
        public decimal OilViscosity { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal SolutionGOR { get; set; }
    }

    /// <summary>
    /// Forecast validation result
    /// </summary>
    public class ForecastValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal QualityScore { get; set; }
        public string ValidationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Forecast method information
    /// </summary>
    public class ForecastMethod
    {
        public string MethodId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool RequiresHistoricalData { get; set; }
        public List<string> RequiredParameters { get; set; } = new();
    }

    /// <summary>
    /// Forecast statistics
    /// </summary>
    public class ForecastStatistics
    {
        public int TotalForecasts { get; set; }
        public decimal AverageRSquared { get; set; }
        public decimal AverageRMSE { get; set; }
        public string MostUsedMethod { get; set; } = string.Empty;
        public Dictionary<string, int> MethodUsage { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis result
    /// </summary>
    public class SensitivityAnalysis
    {
        public string Parameter { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public List<SensitivityPoint> SensitivityPoints { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis point
    /// </summary>
    public class SensitivityPoint
    {
        public decimal ParameterValue { get; set; }
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal EUR { get; set; }
    }

    /// <summary>
    /// Forecast report
    /// </summary>
    public class ForecastReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string ForecastId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<ReportSection> Sections { get; set; } = new();
        public byte[]? ChartData { get; set; }
    }

    /// <summary>
    /// Report section
    /// </summary>
    public class ReportSection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Charts { get; set; } = new();
    }

    /// <summary>
    /// Forecast comparison result
    /// </summary>
    public class ForecastComparison
    {
        public string ComparisonId { get; set; } = string.Empty;
        public List<string> ForecastIds { get; set; } = new();
        public List<ComparisonMetric> Metrics { get; set; } = new();
        public string BestPerformingMethod { get; set; } = string.Empty;
        public decimal AverageDifference { get; set; }
    }

    /// <summary>
    /// Comparison metric
    /// </summary>
    public class ComparisonMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public Dictionary<string, decimal> Values { get; set; } = new();
        public string BestValue { get; set; } = string.Empty;
    }

    #endregion
}