using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for amortization operations.
    /// </summary>
    public interface IAmortizationService
    {
        /// <summary>
        /// Calculates and records amortization.
        /// </summary>
        Task<AMORTIZATION_RECORD> CalculateAndRecordAmortizationAsync(
            CalculateAmortizationRequest request,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets an amortization record by ID.
        /// </summary>
        Task<AMORTIZATION_RECORD?> GetAmortizationRecordAsync(string recordId, string? connectionName = null);
        
        /// <summary>
        /// Gets amortization history for a property or cost center.
        /// </summary>
        Task<List<AMORTIZATION_RECORD>> GetAmortizationHistoryAsync(
            string? propertyId = null,
            string? costCenterId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? connectionName = null);
        
        /// <summary>
        /// Generates an amortization schedule.
        /// </summary>
        Task<AmortizationSchedule> GenerateAmortizationScheduleAsync(
            GenerateScheduleRequest request,
            string? connectionName = null);
        
        /// <summary>
        /// Gets amortization summary for a property or cost center.
        /// </summary>
        Task<AmortizationSummary> GetAmortizationSummaryAsync(
            string? propertyId = null,
            string? costCenterId = null,
            DateTime? asOfDate = null,
            string? connectionName = null);
    }
}

