using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.Calculations;
using ProductionForecastResultDto = Beep.OilandGas.Models.DTOs.Calculations.ProductionForecastResultDto;
using DeclineCurveAnalysisDto = Beep.OilandGas.Models.DTOs.Calculations.DeclineCurveAnalysisDto;
using ProductionForecastPointDto = Beep.OilandGas.Models.DTOs.Calculations.ProductionForecastPointDto;

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
        Task<ProductionForecastResultDto> GenerateForecastAsync(GenerateForecastRequest request);

        /// <summary>
        /// Generates DCA-based forecast using historical production data
        /// </summary>
        Task<ProductionForecastResultDto> GenerateDCAForecastAsync(string wellUWI, string declineType, DateTime startDate, DateTime endDate, int forecastPeriod);

        /// <summary>
        /// Generates probabilistic forecast with uncertainty analysis
        /// </summary>
        Task<ProbabilisticForecastDto> GenerateProbabilisticForecastAsync(string wellUWI, string declineType, int forecastPeriod, int iterations = 1000);

        /// <summary>
        /// Performs decline curve analysis on historical production data
        /// </summary>
        Task<DeclineCurveAnalysisDto> PerformDeclineCurveAnalysisAsync(DeclineCurveAnalysisRequest request);

        #endregion

        #region Advanced Analysis Methods

        /// <summary>
        /// Performs economic analysis on forecast results
        /// </summary>
        Task<EconomicAnalysisDto> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request);

        /// <summary>
        /// Performs risk analysis on forecast scenarios
        /// </summary>
        Task<RiskAnalysisResultDto> PerformRiskAnalysisAsync(string forecastId);

        /// <summary>
        /// Validates forecast quality and reliability
        /// </summary>
        Task<ForecastValidationResultDto> ValidateForecastAsync(string forecastId);

        /// <summary>
        /// Optimizes forecast parameters using machine learning
        /// </summary>
        Task<ProductionForecastResultDto> OptimizeForecastAsync(string wellUWI, string forecastMethod);

        #endregion

        #region Data Management Methods

        /// <summary>
        /// Saves forecast to PPDM database
        /// </summary>
        Task SaveForecastAsync(ProductionForecastResultDto forecast, string userId);

        /// <summary>
        /// Retrieves forecast by ID
        /// </summary>
        Task<ProductionForecastResultDto?> GetForecastAsync(string forecastId);

        /// <summary>
        /// Retrieves forecasts for well or field
        /// </summary>
        Task<List<ProductionForecastResultDto>> GetForecastsAsync(string? wellUWI = null, string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Updates existing forecast
        /// </summary>
        Task UpdateForecastAsync(ProductionForecastResultDto forecast, string userId);

        /// <summary>
        /// Deletes forecast from database
        /// </summary>
        Task DeleteForecastAsync(string forecastId, string userId);

        #endregion

        #region Specialized Forecasting Methods

        /// <summary>
        /// Generates reservoir simulation-based forecast
        /// </summary>
        Task<ProductionForecastResultDto> GenerateReservoirSimulationForecastAsync(string wellUWI, ReservoirPropertiesDto reservoirProperties, int forecastPeriod);

        /// <summary>
        /// Generates type curve-based forecast
        /// </summary>
        Task<ProductionForecastResultDto> GenerateTypeCurveForecastAsync(string wellUWI, string typeCurveId, int forecastPeriod);

        /// <summary>
        /// Generates forecast using machine learning models
        /// </summary>
        Task<ProductionForecastResultDto> GenerateMLForecastAsync(string wellUWI, string modelType, int forecastPeriod);

        /// <summary>
        /// Combines multiple forecast methods for ensemble forecasting
        /// </summary>
        Task<ProductionForecastResultDto> GenerateEnsembleForecastAsync(string wellUWI, List<string> forecastMethods, int forecastPeriod);

        #endregion

        #region Reporting and Export

        /// <summary>
        /// Exports forecast to specified format
        /// </summary>
        Task<byte[]> ExportForecastAsync(string forecastId, string format = "CSV");

        /// <summary>
        /// Generates forecast report with charts and analysis
        /// </summary>
        Task<ForecastReportDto> GenerateForecastReportAsync(string forecastId);

        /// <summary>
        /// Compares multiple forecasts
        /// </summary>
        Task<ForecastComparisonDto> CompareForecastsAsync(List<string> forecastIds);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets available forecast methods
        /// </summary>
        Task<List<ForecastMethodDto>> GetAvailableForecastMethodsAsync();

        /// <summary>
        /// Gets forecast statistics and quality metrics
        /// </summary>
        Task<ForecastStatisticsDto> GetForecastStatisticsAsync(string forecastId);

        /// <summary>
        /// Performs sensitivity analysis on forecast parameters
        /// </summary>
        Task<SensitivityAnalysisDto> PerformSensitivityAnalysisAsync(string forecastId, List<string> parameters);

        #endregion
    }

    #region Supporting DTOs

    /// <summary>
    /// Reservoir properties for simulation-based forecasting
    /// </summary>
    public class ReservoirPropertiesDto
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
    public class ForecastValidationResultDto
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
    public class ForecastMethodDto
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
    public class ForecastStatisticsDto
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
    public class SensitivityAnalysisDto
    {
        public string Parameter { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public List<SensitivityPointDto> SensitivityPoints { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis point
    /// </summary>
    public class SensitivityPointDto
    {
        public decimal ParameterValue { get; set; }
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal EUR { get; set; }
    }

    /// <summary>
    /// Forecast report
    /// </summary>
    public class ForecastReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string ForecastId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<ReportSectionDto> Sections { get; set; } = new();
        public byte[]? ChartData { get; set; }
    }

    /// <summary>
    /// Report section
    /// </summary>
    public class ReportSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Charts { get; set; } = new();
    }

    /// <summary>
    /// Forecast comparison result
    /// </summary>
    public class ForecastComparisonDto
    {
        public string ComparisonId { get; set; } = string.Empty;
        public List<string> ForecastIds { get; set; } = new();
        public List<ComparisonMetricDto> Metrics { get; set; } = new();
        public string BestPerformingMethod { get; set; } = string.Empty;
        public decimal AverageDifference { get; set; }
    }

    /// <summary>
    /// Comparison metric
    /// </summary>
    public class ComparisonMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public Dictionary<string, decimal> Values { get; set; } = new();
        public string BestValue { get; set; } = string.Empty;
    }

    #endregion
}