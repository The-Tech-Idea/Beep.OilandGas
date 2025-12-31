using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for Successful Efforts accounting operations.
    /// </summary>
    public interface ISuccessfulEffortsService
    {
        /// <summary>
        /// Records acquisition of an unproved property.
        /// </summary>
        Task<UNPROVED_PROPERTY> RecordAcquisitionAsync(UnprovedPropertyDto property, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records exploration costs. G&G costs are expensed, drilling costs are capitalized.
        /// </summary>
        Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(ExplorationCostsDto costs, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records development costs. All development costs are capitalized.
        /// </summary>
        Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(DevelopmentCostsDto costs, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records production costs (lifting costs). These are expensed as incurred.
        /// </summary>
        Task<PRODUCTION_COSTS> RecordProductionCostsAsync(ProductionCostsDto costs, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records a dry hole expense for an exploratory well.
        /// </summary>
        Task<EXPLORATION_COSTS> RecordDryHoleAsync(ExplorationCostsDto costs, string userId, string? connectionName = null);
        
        /// <summary>
        /// Reclassifies an unproved property as proved when reserves are discovered.
        /// </summary>
        Task<PROVED_PROPERTY> ReclassifyToProvedPropertyAsync(string propertyId, ProvedReservesDto reserves, string userId, string? connectionName = null);
        
        /// <summary>
        /// Tests impairment of an unproved property.
        /// </summary>
        Task<ImpairmentResult> TestImpairmentAsync(string propertyId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records impairment of an unproved property.
        /// </summary>
        Task<UNPROVED_PROPERTY> RecordImpairmentAsync(string propertyId, decimal impairmentAmount, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets all unproved properties.
        /// </summary>
        Task<List<UNPROVED_PROPERTY>> GetUnprovedPropertiesAsync(string? connectionName = null);
        
        /// <summary>
        /// Gets all proved properties.
        /// </summary>
        Task<List<PROVED_PROPERTY>> GetProvedPropertiesAsync(string? connectionName = null);
        
        /// <summary>
        /// Gets total exploration costs for a property.
        /// </summary>
        Task<decimal> GetTotalExplorationCostsAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Gets total development costs for a property.
        /// </summary>
        Task<decimal> GetTotalDevelopmentCostsAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Gets total G&G costs expensed for a property.
        /// </summary>
        Task<decimal> GetTotalGGCostsExpensedAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Gets total dry hole costs expensed for a property.
        /// </summary>
        Task<decimal> GetTotalDryHoleCostsExpensedAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Calculates amortization for a proved property using units-of-production method.
        /// </summary>
        Task<decimal> CalculateAmortizationAsync(ProvedPropertyDto property, ProvedReservesDto reserves, Beep.OilandGas.Models.DTOs.ProductionAccounting.ProductionDataDto production);
    }
}

