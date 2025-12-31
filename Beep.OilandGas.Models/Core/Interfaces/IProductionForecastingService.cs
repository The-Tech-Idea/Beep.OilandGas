using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for production forecasting operations.
    /// Provides production forecasting, decline curve analysis, and reserve estimation.
    /// </summary>
    public interface IProductionForecastingService
    {
        /// <summary>
        /// Generates production forecast for a well or field.
        /// </summary>
        /// <param name="wellUWI">Well UWI (optional, use null for field forecast)</param>
        /// <param name="fieldId">Field identifier (optional, use null for well forecast)</param>
        /// <param name="forecastMethod">Forecast method (e.g., "Exponential", "Hyperbolic", "Harmonic")</param>
        /// <param name="forecastPeriod">Forecast period in months</param>
        /// <returns>Production forecast result</returns>
        Task<ProductionForecastResultDto> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod);

        /// <summary>
        /// Performs decline curve analysis.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <returns>Decline curve analysis result</returns>
        Task<DeclineCurveAnalysisDto> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Saves production forecast to database.
        /// </summary>
        /// <param name="forecast">Production forecast</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveForecastAsync(ProductionForecastResultDto forecast, string userId);
    }

    /// <summary>
    /// DTO for production forecast result.
    /// </summary>
    public class ProductionForecastResultDto
    {
        public string ForecastId { get; set; } = string.Empty;
        public string? WellUWI { get; set; }
        public string? FieldId { get; set; }
        public DateTime ForecastDate { get; set; }
        public string ForecastMethod { get; set; } = string.Empty;
        public List<ProductionForecastPointDto> ForecastPoints { get; set; } = new();
        public decimal EstimatedReserves { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for production forecast point.
    /// </summary>
    public class ProductionForecastPointDto
    {
        public DateTime Date { get; set; }
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal WaterRate { get; set; }
    }

    /// <summary>
    /// DTO for decline curve analysis result.
    /// </summary>
    public class DeclineCurveAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string DeclineType { get; set; } = string.Empty;
        public decimal DeclineRate { get; set; }
        public decimal InitialRate { get; set; }
        public decimal EstimatedReserves { get; set; }
    }
}

