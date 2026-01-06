using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Imbalance;
using Beep.OilandGas.Models.DTOs.Imbalance;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for imbalance operations.
    /// </summary>
    public interface IImbalanceService
    {
        /// <summary>
        /// Creates a production avail.
        /// </summary>
        Task<PRODUCTION_AVAIL> CreateProductionAvailAsync(CreateProductionAvailRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a production avail by ID.
        /// </summary>
        Task<PRODUCTION_AVAIL?> GetProductionAvailAsync(string availId, string? connectionName = null);
        
        /// <summary>
        /// Creates a nomination.
        /// </summary>
        Task<NOMINATION> CreateNominationAsync(CreateNominationRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets nominations by period.
        /// </summary>
        Task<List<NOMINATION>> GetNominationsByPeriodAsync(DateTime periodStart, DateTime periodEnd, string? connectionName = null);
        
        /// <summary>
        /// Records an actual delivery.
        /// </summary>
        Task<ACTUAL_DELIVERY> RecordActualDeliveryAsync(CreateActualDeliveryRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets deliveries by nomination.
        /// </summary>
        Task<List<ACTUAL_DELIVERY>> GetDeliveriesByNominationAsync(string nominationId, string? connectionName = null);
        
        /// <summary>
        /// Calculates imbalance for a nomination.
        /// </summary>
        Task<IMBALANCE> CalculateImbalanceAsync(string nominationId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets imbalances by period.
        /// </summary>
        Task<List<IMBALANCE>> GetImbalancesByPeriodAsync(DateTime periodStart, DateTime periodEnd, string? connectionName = null);
        
        /// <summary>
        /// Reconciles an imbalance.
        /// </summary>
        Task<ImbalanceReconciliationResult> ReconcileImbalanceAsync(string imbalanceId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Settles an imbalance.
        /// </summary>
        Task<ImbalanceSettlementResult> SettleImbalanceAsync(string imbalanceId, DateTime settlementDate, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets imbalance summary.
        /// </summary>
        Task<List<ImbalanceSummary>> GetImbalanceSummaryAsync(DateTime? periodStart, DateTime? periodEnd, string? connectionName = null);
    }
}




