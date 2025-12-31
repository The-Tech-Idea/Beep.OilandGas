using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Export;
using Beep.OilandGas.Models.DTOs.Export;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for export operations.
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exports data to CSV format.
        /// </summary>
        Task<ExportResult> ExportToCsvAsync(ExportToCsvRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Exports data to Excel format.
        /// </summary>
        Task<ExportResult> ExportToExcelAsync(ExportToExcelRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Exports data to JSON format.
        /// </summary>
        Task<ExportResult> ExportToJsonAsync(ExportToJsonRequest request, string userId, bool trackHistory = false, string? connectionName = null);
        
        /// <summary>
        /// Gets export history.
        /// </summary>
        Task<List<EXPORT_HISTORY>> GetExportHistoryAsync(string? exportType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Schedules an export.
        /// </summary>
        Task<ScheduleExportRequest> ScheduleExportAsync(ScheduleExportRequest request, string userId, string? connectionName = null);
    }
}

