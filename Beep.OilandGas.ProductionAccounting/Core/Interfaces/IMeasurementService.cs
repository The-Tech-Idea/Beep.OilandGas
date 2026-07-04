using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Measurement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for production measurement recording and validation.
    /// </summary>
    public interface IMeasurementService
    {
        Task<MEASUREMENT_RECORD> RecordAsync(RUN_TICKET ticket, string userId, string connectionName = "PPDM39");
        Task<MEASUREMENT_RECORD?> GetAsync(string measurementId, string connectionName = "PPDM39");
        Task<List<MEASUREMENT_RECORD>> GetByWellAsync(string wellId, DateTime start, DateTime end, string connectionName = "PPDM39");
        Task<List<MEASUREMENT_RECORD>> GetByLeaseAsync(string leaseId, DateTime start, DateTime end, string connectionName = "PPDM39");
        Task<bool> ValidateAsync(MEASUREMENT_RECORD measurement, string connectionName = "PPDM39");
    }
}

