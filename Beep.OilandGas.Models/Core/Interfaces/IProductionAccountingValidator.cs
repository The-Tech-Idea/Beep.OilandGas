using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;

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
        Task<ValidationResult> ValidateProductionDataAsync(RUN_TICKET productionData, string? connectionName = null);

        /// <summary>
        /// Validates allocation request for consistency and business rules.
        /// </summary>
        Task<ValidationResult> ValidateAllocationAsync(ALLOCATION_DETAIL allocation, string? connectionName = null);

        /// <summary>
        /// Validates royalty calculation input for accuracy.
        /// </summary>
        Task<ValidationResult> ValidateRoyaltyCalculationAsync(ROYALTY_CALCULATION royalty, string? connectionName = null);

        /// <summary>
        /// Validates journal entry for GL posting requirements.
        /// </summary>
        Task<ValidationResult> ValidateJournalEntryAsync(JOURNAL_ENTRY journalEntry, string? connectionName = null);

        /// <summary>
        /// Validates measurement data for quality and consistency.
        /// </summary>
        Task<ValidationResult> ValidateMeasurementAsync(MEASUREMENT_RECORD measurement, string? connectionName = null);

        /// <summary>
        /// Validates invoice for completeness and consistency.
        /// </summary>
        Task<ValidationResult> ValidateInvoiceAsync(INVOICE invoice, string? connectionName = null);

        /// <summary>
        /// Performs cross-entity validation (e.g., allocation sums to 100%).
        /// </summary>
        Task<ValidationResult> ValidateCrossEntityConstraintsAsync(string entityId, string? connectionName = null);

        /// <summary>
        /// Validates period closing prerequisites.
        /// </summary>
        Task<ValidationResult> ValidatePeriodClosingReadinessAsync(string periodId, string? connectionName = null);
    }

    /// <summary>
    /// Result of validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> ValidationData { get; set; } = new();
    }
}
