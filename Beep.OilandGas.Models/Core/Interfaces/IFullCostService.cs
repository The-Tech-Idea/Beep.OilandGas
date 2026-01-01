using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for Full Cost accounting operations.
    /// </summary>
    public interface IFullCostService
    {
        /// <summary>
        /// Records exploration costs to a cost center. All costs are capitalized.
        /// </summary>
        Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(
            string propertyId,
            string costCenterId,
            ExplorationCostsDto costs,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Records development costs to a cost center. All costs are capitalized.
        /// </summary>
        Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(
            string propertyId,
            string costCenterId,
            DevelopmentCostsDto costs,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Records acquisition costs to a cost center. All costs are capitalized.
        /// </summary>
        Task<UNPROVED_PROPERTY> RecordAcquisitionCostsAsync(
            string costCenterId,
            UnprovedPropertyDto property,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Creates a cost center.
        /// </summary>
        Task<COST_CENTER> CreateCostCenterAsync(CreateCostCenterRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a cost center by ID.
        /// </summary>
        Task<COST_CENTER?> GetCostCenterAsync(string costCenterId, string? connectionName = null);
        
        /// <summary>
        /// Gets all cost centers.
        /// </summary>
        Task<List<COST_CENTER>> GetCostCentersAsync(string? connectionName = null);
        
        /// <summary>
        /// Performs ceiling test to determine if impairment is needed.
        /// </summary>
        Task<CEILING_TEST_CALCULATION> PerformCeilingTestAsync(
            string costCenterId,
            DateTime testDate,
            ProvedReservesDto reserves,
            decimal discountRate,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets ceiling test history for a cost center.
        /// </summary>
        Task<List<CEILING_TEST_CALCULATION>> GetCeilingTestHistoryAsync(string costCenterId, string? connectionName = null);
        
        /// <summary>
        /// Calculates total capitalized costs for a cost center.
        /// </summary>
        Task<decimal> CalculateTotalCapitalizedCostsAsync(string costCenterId, string? connectionName = null);
        
        /// <summary>
        /// Gets accumulated amortization for a cost center.
        /// </summary>
        Task<decimal> GetAccumulatedAmortizationAsync(string costCenterId, string? connectionName = null);
        
        /// <summary>
        /// Records amortization for a cost center.
        /// </summary>
        Task<COST_CENTER> RecordAmortizationAsync(string costCenterId, decimal amortizationAmount, string userId, string? connectionName = null);
        
        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        Task<decimal> CalculateAmortizationAsync(
            string costCenterId,
            ProvedReservesDto reserves,
            Beep.OilandGas.Models.DTOs.ProductionAccounting.ProductionDataDto production,
            string? connectionName = null);
        
        /// <summary>
        /// Gets cost center rollup summary.
        /// </summary>
        Task<CostCenterRollup> GetCostCenterRollupAsync(string costCenterId, DateTime? asOfDate, string? connectionName = null);
        
        /// <summary>
        /// Calculates impairment based on ceiling test.
        /// </summary>
        Task<ImpairmentResult> CalculateImpairmentAsync(string costCenterId, DateTime testDate, string userId, string? connectionName = null);
    }
}

