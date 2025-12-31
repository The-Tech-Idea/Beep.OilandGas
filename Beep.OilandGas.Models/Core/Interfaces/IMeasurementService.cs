using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Measurement;
using Beep.OilandGas.Models.DTOs.Measurement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for measurement operations.
    /// </summary>
    public interface IMeasurementService
    {
        /// <summary>
        /// Records a manual measurement.
        /// </summary>
        Task<MEASUREMENT_RECORD> RecordManualMeasurementAsync(RecordManualMeasurementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records an automatic measurement.
        /// </summary>
        Task<MEASUREMENT_RECORD> RecordAutomaticMeasurementAsync(RecordAutomaticMeasurementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a measurement by ID.
        /// </summary>
        Task<MEASUREMENT_RECORD?> GetMeasurementAsync(string measurementId, string? connectionName = null);
        
        /// <summary>
        /// Gets measurements by well.
        /// </summary>
        Task<List<MEASUREMENT_RECORD>> GetMeasurementsByWellAsync(string wellId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Gets measurements by lease.
        /// </summary>
        Task<List<MEASUREMENT_RECORD>> GetMeasurementsByLeaseAsync(string leaseId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Validates a measurement.
        /// </summary>
        Task<MeasurementValidationResult> ValidateMeasurementAsync(string measurementId, string? connectionName = null);
        
        /// <summary>
        /// Gets measurement history.
        /// </summary>
        Task<List<MeasurementHistory>> GetMeasurementHistoryAsync(string wellId, string? connectionName = null);
        
        /// <summary>
        /// Gets measurement summary.
        /// </summary>
        Task<MeasurementSummary> GetMeasurementSummaryAsync(string wellId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    }
}

