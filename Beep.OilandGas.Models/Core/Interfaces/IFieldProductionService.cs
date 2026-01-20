using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for production phase service, field-scoped
    /// Extends IPPDMProductionService with field-aware methods
    /// </summary>
    public interface IFieldProductionService : IPPDMProductionService
    {
        /// <summary>
        /// Get production data for a specific field
        /// </summary>
        Task<List<ProductionResponse>> GetProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create production record for a well in the specified field
        /// </summary>
        Task<ProductionResponse> CreateProductionForFieldAsync(string fieldId, ProductionRequest productionData, string userId);

        /// <summary>
        /// Get production by pool for a specific field
        /// </summary>
        Task<List<ProductionResponse>> GetProductionByPoolForFieldAsync(string fieldId, string? poolId = null);

        /// <summary>
        /// Get reserves data for a specific field
        /// </summary>
        Task<List<ReservesResponse>> GetReservesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get well test data for a well (must belong to the specified field)
        /// </summary>
        Task<List<WellTestResponse>> GetWellTestsForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get production forecasts for a specific field
        /// </summary>
        Task<List<ProductionForecastResponse>> GetProductionForecastsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create production forecast for a specific field
        /// </summary>
        Task<ProductionForecastResponse> CreateProductionForecastForFieldAsync(string fieldId, ProductionForecastRequest forecastData, string userId);

        /// <summary>
        /// Get facility production data for facilities in a specific field
        /// </summary>
        Task<List<ProductionResponse>> GetFacilityProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null);
    }
}




