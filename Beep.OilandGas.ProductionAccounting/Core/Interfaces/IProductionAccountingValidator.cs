using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;
using ValidationResult = Beep.OilandGas.Models.Data.ValidationResult;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for production accounting data validation.
    /// Provides comprehensive validation for accounting transactions and business rules.
    /// </summary>
    public interface IProductionAccountingValidator
    {
        /// <summary>
        /// Validates production data for accuracy and completeness.
        /// </summary>
        Task<ValidationResult> ValidateProductionDataAsync(RUN_TICKET productionData, string connectionName = "PPDM39");

        /// <summary>
        /// Validates allocation request for consistency and business rules.
        /// </summary>
        Task<ValidationResult> ValidateAllocationAsync(ALLOCATION_DETAIL allocation, string connectionName = "PPDM39");

        /// <summary>
        /// Validates royalty calculation input for accuracy.
        /// </summary>
        Task<ValidationResult> ValidateRoyaltyCalculationAsync(ROYALTY_CALCULATION royalty, string connectionName = "PPDM39");

        /// <summary>
        /// Validates journal entry for GL posting requirements.
        /// </summary>
        Task<ValidationResult> ValidateJournalEntryAsync(JOURNAL_ENTRY journalEntry, string connectionName = "PPDM39");

        /// <summary>
        /// Validates measurement data for quality and consistency.
        /// </summary>
        Task<ValidationResult> ValidateMeasurementAsync(MEASUREMENT_RECORD measurement, string connectionName = "PPDM39");

        /// <summary>
        /// Validates invoice for completeness and consistency.
        /// </summary>
        Task<ValidationResult> ValidateInvoiceAsync(INVOICE invoice, string connectionName = "PPDM39");

        /// <summary>
        /// Performs cross-entity validation (e.g., allocation sums to 100%).
        /// </summary>
        Task<ValidationResult> ValidateCrossEntityConstraintsAsync(string entityId, string connectionName = "PPDM39");

        /// <summary>
        /// Validates period closing prerequisites.
        /// </summary>
        Task<ValidationResult> ValidatePeriodClosingReadinessAsync(string periodId, string connectionName = "PPDM39");
    }

    /// <summary>
    /// Result of validation operation.
    /// </summary>
    
}

