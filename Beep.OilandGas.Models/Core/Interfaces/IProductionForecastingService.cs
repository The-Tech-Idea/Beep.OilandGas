using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calc = Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for production forecasting operations.
    /// Provides production forecasting, decline curve analysis, and reserve estimation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the <strong>HTTP/API-facing</strong> contract: register and inject this interface from the API layer
    /// (<c>Beep.OilandGas.ApiService</c>) and use it in controllers. Extended capabilities (DCA variants, economics,
    /// exports, etc.) live on the feature-local interface
    /// <c>Beep.OilandGas.ProductionForecasting.Services.IProductionForecastingService</c> (same type name, different namespace).
    /// </para>
    /// <para>
    /// <see cref="GenerateForecastAsync(string?, string?, string, int)"/> accepts a string method name; the implementation
    /// maps it to <c>ForecastType</c> (see <c>ProductionForecastingService.ModelsCoreImpl</c> in the ProductionForecasting project).
    /// </para>
    /// </remarks>
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
        Task<Calc.ProductionForecastResult> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod);

        /// <summary>
        /// Generates production forecast using full request payload with optional decline parameters and history-fit controls.
        /// </summary>
        /// <param name="request">Forecast request payload.</param>
        /// <returns>Production forecast result.</returns>
        Task<Calc.ProductionForecastResult> GenerateForecastAsync(GenerateForecastRequest request);

        /// <summary>
        /// Performs decline curve analysis.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="startDate">Start date for analysis</param>
        /// <param name="endDate">End date for analysis</param>
        /// <returns>Decline curve analysis result</returns>
        Task<Calc.DeclineCurveAnalysis> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Saves production forecast to database.
        /// </summary>
        /// <param name="forecast">Production forecast</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveForecastAsync(Calc.ProductionForecastResult forecast, string userId);
    }
}

