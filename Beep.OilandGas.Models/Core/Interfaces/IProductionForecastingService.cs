using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calc = Beep.OilandGas.Models.DTOs.Calculations;

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
        Task<Calc.ProductionForecastResultDto> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod);

        /// <summary>
        /// Performs decline curve analysis.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <returns>Decline curve analysis result</returns>
        Task<Calc.DeclineCurveAnalysisDto> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Saves production forecast to database.
        /// </summary>
        /// <param name="forecast">Production forecast</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveForecastAsync(Calc.ProductionForecastResultDto forecast, string userId);
    }
}
