using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for compliance tracking and reporting.
    /// </summary>
    public interface IPermitComplianceReportService
    {
        Task<IReadOnlyList<ExpiringPermitRecord>> GetExpiringPermitsAsync(DateTime asOfDate, int daysUntilExpiry);
        Task<ComplianceReport> GenerateComplianceReportAsync(DateTime asOfDate, int daysUntilExpiry);
    }
}
