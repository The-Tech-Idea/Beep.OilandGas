using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for production operations.
    /// Provides production monitoring, optimization, and operations management.
    /// </summary>
    public interface IProductionOperationsService
    {
        /// <summary>
        /// Gets production data for a well or field.
        /// </summary>
        /// <param name="wellUWI">Well UWI (optional)</param>
        /// <param name="fieldId">Field identifier (optional)</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of production data points</returns>
        Task<List<ProductionData>> GetProductionDataAsync(string? wellUWI, string? fieldId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Records production data.
        /// </summary>
        /// <param name="productionData">Production data to record</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task RecordProductionDataAsync(ProductionData productionData, string userId);

        /// <summary>
        /// Optimizes production operations.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="optimizationGoals">Optimization goals</param>
        /// <returns>Optimization recommendations</returns>
        Task<List<ProductionOptimizationRecommendation>> OptimizeProductionAsync(string wellUWI, Dictionary<string, object> optimizationGoals);
    }

    /// <summary>
    /// DTO for production data.
    /// </summary>
    public class ProductionData
    {
        public string ProductionId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public DateTime ProductionDate { get; set; }
        public decimal OilVolume { get; set; }
        public decimal GasVolume { get; set; }
        public decimal WaterVolume { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for production optimization recommendation.
    /// </summary>
    public class ProductionOptimizationRecommendation
    {
        public string RecommendationId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ExpectedImprovement { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}




