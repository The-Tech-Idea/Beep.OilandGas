using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Reporting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for reporting operations.
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Generates an operational report.
        /// </summary>
        Task<ReportResult> GenerateOperationalReportAsync(GenerateOperationalReportRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Generates a financial report.
        /// </summary>
        Task<ReportResult> GenerateFinancialReportAsync(GenerateFinancialReportRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Generates a royalty statement.
        /// </summary>
        Task<ReportResult> GenerateRoyaltyStatementAsync(GenerateRoyaltyStatementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Generates a JIB (Joint Interest Billing) statement.
        /// </summary>
        Task<ReportResult> GenerateJIBStatementAsync(GenerateJIBStatementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Schedules a report.
        /// </summary>
        Task<ReportSchedule> ScheduleReportAsync(ScheduleReportRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets scheduled reports.
        /// </summary>
        Task<List<ReportSchedule>> GetScheduledReportsAsync(string? connectionName = null);
        
        /// <summary>
        /// Distributes a report.
        /// </summary>
        Task<ReportDistributionResult> DistributeReportAsync(string reportId, ReportDistributionRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets report history.
        /// </summary>
        Task<List<ReportHistory>> GetReportHistoryAsync(string? reportType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    }
}




