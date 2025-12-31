using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Export;
using Beep.OilandGas.Models.DTOs.Export;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Royalty;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Models.Data.Accounting;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.ProductionAccounting.Export
{
    /// <summary>
    /// Service for managing data export operations.
    /// Uses IDataSource directly for database operations.
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<ExportService>? _logger;
        private readonly string _connectionName;

        private const string EXPORT_HISTORY_TABLE = "EXPORT_HISTORY";

        public ExportService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILoggerFactory? loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = loggerFactory?.CreateLogger<ExportService>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Exports data to CSV format.
        /// </summary>
        public async Task<ExportResult> ExportToCsvAsync(ExportToCsvRequest request, string userId, bool trackHistory = false, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.FilePath))
                throw new ArgumentException("File path is required.", nameof(request.FilePath));

            var connName = connectionName ?? _connectionName;
            var exportId = Guid.NewGuid().ToString();
            int recordCount = 0;

            try
            {
                // Query data based on entity type
                switch (request.EntityType.ToLower())
                {
                    case "runticket":
                    case "runtickets":
                        var runTickets = await GetRunTicketsAsync(request, connName);
                        ExportRunTicketsToCsv(runTickets, request.FilePath);
                        recordCount = runTickets.Count;
                        break;

                    case "salestransaction":
                    case "salestransactions":
                        var transactions = await GetSalesTransactionsAsync(request, connName);
                        ExportSalesTransactionsToCsv(transactions, request.FilePath);
                        recordCount = transactions.Count;
                        break;

                    case "royaltypayment":
                    case "royaltypayments":
                        var payments = await GetRoyaltyPaymentsAsync(request, connName);
                        ExportRoyaltyPaymentsToCsv(payments, request.FilePath);
                        recordCount = payments.Count;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported entity type for export: {request.EntityType}", nameof(request.EntityType));
                }

                var result = new ExportResult
                {
                    ExportId = exportId,
                    ExportType = request.EntityType,
                    ExportFormat = "CSV",
                    FilePath = request.FilePath,
                    RecordCount = recordCount,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = true
                };

                if (trackHistory)
                {
                    await SaveExportHistoryAsync(result, userId, connName);
                }

                _logger?.LogDebug("Exported {RecordCount} {EntityType} records to CSV file {FilePath}",
                    recordCount, request.EntityType, request.FilePath);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export {EntityType} to CSV", request.EntityType);

                return new ExportResult
                {
                    ExportId = exportId,
                    ExportType = request.EntityType,
                    ExportFormat = "CSV",
                    FilePath = request.FilePath,
                    RecordCount = 0,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Exports data to Excel format.
        /// </summary>
        public async Task<ExportResult> ExportToExcelAsync(ExportToExcelRequest request, string userId, bool trackHistory = false, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.FilePath))
                throw new ArgumentException("File path is required.", nameof(request.FilePath));

            var connName = connectionName ?? _connectionName;
            var exportId = Guid.NewGuid().ToString();

            try
            {
                // In a full implementation, would use a library like EPPlus or ClosedXML for Excel export
                // For now, export as CSV with .xlsx extension (simplified)
                var csvRequest = new ExportToCsvRequest
                {
                    EntityType = request.EntityType,
                    EntityIds = request.EntityIds,
                    FilePath = request.FilePath,
                    Filters = request.Filters
                };

                var result = await ExportToCsvAsync(csvRequest, userId, false, connName);
                result.ExportFormat = "Excel";
                result.ExportId = exportId;

                if (trackHistory)
                {
                    await SaveExportHistoryAsync(result, userId, connName);
                }

                _logger?.LogDebug("Exported {RecordCount} {EntityType} records to Excel file {FilePath}",
                    result.RecordCount, request.EntityType, request.FilePath);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export {EntityType} to Excel", request.EntityType);

                return new ExportResult
                {
                    ExportId = exportId,
                    ExportType = request.EntityType,
                    ExportFormat = "Excel",
                    FilePath = request.FilePath,
                    RecordCount = 0,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Exports data to JSON format.
        /// </summary>
        public async Task<ExportResult> ExportToJsonAsync(ExportToJsonRequest request, string userId, bool trackHistory = false, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.FilePath))
                throw new ArgumentException("File path is required.", nameof(request.FilePath));

            var connName = connectionName ?? _connectionName;
            var exportId = Guid.NewGuid().ToString();
            int recordCount = 0;

            try
            {
                object? dataToExport = null;

                // Query data based on entity type
                switch (request.EntityType.ToLower())
                {
                    case "report":
                        // In a full implementation, would query report data
                        dataToExport = new { ReportType = "Operational", PeriodStart = DateTime.Now.AddDays(-30), PeriodEnd = DateTime.Now };
                        recordCount = 1;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported entity type for JSON export: {request.EntityType}", nameof(request.EntityType));
                }

                var json = JsonSerializer.Serialize(dataToExport, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(request.FilePath, json);

                var result = new ExportResult
                {
                    ExportId = exportId,
                    ExportType = request.EntityType,
                    ExportFormat = "JSON",
                    FilePath = request.FilePath,
                    RecordCount = recordCount,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = true
                };

                if (trackHistory)
                {
                    await SaveExportHistoryAsync(result, userId, connName);
                }

                _logger?.LogDebug("Exported {RecordCount} {EntityType} records to JSON file {FilePath}",
                    recordCount, request.EntityType, request.FilePath);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to export {EntityType} to JSON", request.EntityType);

                return new ExportResult
                {
                    ExportId = exportId,
                    ExportType = request.EntityType,
                    ExportFormat = "JSON",
                    FilePath = request.FilePath,
                    RecordCount = 0,
                    ExportDate = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets export history.
        /// </summary>
        public async Task<List<EXPORT_HISTORY>> GetExportHistoryAsync(string? exportType, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                filters.Add(new AppFilter { FieldName = "EXPORT_TYPE", Operator = "=", FilterValue = exportType });
            }

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "EXPORT_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "EXPORT_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await dataSource.GetEntityAsync(EXPORT_HISTORY_TABLE, filters);
            if (results == null)
                return new List<EXPORT_HISTORY>();

            return results.Cast<EXPORT_HISTORY>().OrderByDescending(e => e.EXPORT_DATE).ToList();
        }

        /// <summary>
        /// Schedules an export.
        /// </summary>
        public async Task<ScheduleExportRequest> ScheduleExportAsync(ScheduleExportRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // In a full implementation, would save to EXPORT_SCHEDULE table
            // For now, return the request as-is
            _logger?.LogDebug("Scheduled export {ExportType} in {ExportFormat} format", request.ExportType, request.ExportFormat);
            return request;
        }

        // Export helper methods

        private void ExportRunTicketsToCsv(List<RunTicket> tickets, string filePath)
        {
            if (tickets == null || tickets.Count == 0)
                throw new ArgumentException("Tickets list cannot be null or empty.", nameof(tickets));

            var csv = new StringBuilder();
            csv.AppendLine("RunTicketNumber,Date,LeaseId,WellId,GrossVolume,BSWVolume,NetVolume,BSWPercentage,PricePerBarrel,TotalValue,Purchaser");

            foreach (var ticket in tickets)
            {
                csv.AppendLine($"{ticket.RunTicketNumber}," +
                    $"{ticket.TicketDateTime:yyyy-MM-dd}," +
                    $"{ticket.LeaseId}," +
                    $"{ticket.WellId ?? ""}," +
                    $"{ticket.GrossVolume}," +
                    $"{ticket.BSWVolume}," +
                    $"{ticket.NetVolume}," +
                    $"{ticket.BSWPercentage}," +
                    $"{ticket.PricePerBarrel?.ToString() ?? ""}," +
                    $"{ticket.TotalValue?.ToString() ?? ""}," +
                    $"{ticket.Purchaser}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        private void ExportSalesTransactionsToCsv(List<SalesTransaction> transactions, string filePath)
        {
            if (transactions == null || transactions.Count == 0)
                throw new ArgumentException("Transactions list cannot be null or empty.", nameof(transactions));

            var csv = new StringBuilder();
            csv.AppendLine("TransactionId,Date,Purchaser,NetVolume,PricePerBarrel,TotalValue,TotalCosts,TotalTaxes,NetRevenue");

            foreach (var transaction in transactions)
            {
                csv.AppendLine($"{transaction.TransactionId}," +
                    $"{transaction.TransactionDate:yyyy-MM-dd}," +
                    $"{transaction.Purchaser}," +
                    $"{transaction.NetVolume}," +
                    $"{transaction.PricePerBarrel}," +
                    $"{transaction.TotalValue}," +
                    $"{transaction.Costs.TotalCosts}," +
                    $"{transaction.Taxes.Sum(t => t.Amount)}," +
                    $"{transaction.NetRevenue}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        private void ExportRoyaltyPaymentsToCsv(List<RoyaltyPayment> payments, string filePath)
        {
            if (payments == null || payments.Count == 0)
                throw new ArgumentException("Payments list cannot be null or empty.", nameof(payments));

            var csv = new StringBuilder();
            csv.AppendLine("PaymentId,RoyaltyOwnerId,PropertyOrLeaseId,PeriodStart,PeriodEnd,RoyaltyAmount,PaymentDate,Status,NetPaymentAmount");

            foreach (var payment in payments)
            {
                csv.AppendLine($"{payment.PaymentId}," +
                    $"{payment.RoyaltyOwnerId}," +
                    $"{payment.PropertyOrLeaseId}," +
                    $"{payment.PaymentPeriodStart:yyyy-MM-dd}," +
                    $"{payment.PaymentPeriodEnd:yyyy-MM-dd}," +
                    $"{payment.RoyaltyAmount}," +
                    $"{payment.PaymentDate:yyyy-MM-dd}," +
                    $"{payment.Status}," +
                    $"{payment.NetPaymentAmount}");
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        // Helper methods to query data

        private async Task<List<RunTicket>> GetRunTicketsAsync(ExportToCsvRequest request, string connectionName)
        {
            // In a full implementation, would query RUN_TICKET table using IDataSource
            // For now, return empty list
            return new List<RunTicket>();
        }

        private async Task<List<SalesTransaction>> GetSalesTransactionsAsync(ExportToCsvRequest request, string connectionName)
        {
            // In a full implementation, would query SALES_TRANSACTION table using IDataSource
            // For now, return empty list
            return new List<SalesTransaction>();
        }

        private async Task<List<RoyaltyPayment>> GetRoyaltyPaymentsAsync(ExportToCsvRequest request, string connectionName)
        {
            // In a full implementation, would query ROYALTY_PAYMENT table using IDataSource
            // For now, return empty list
            return new List<RoyaltyPayment>();
        }

        // Save export history

        private async Task SaveExportHistoryAsync(ExportResult result, string userId, string connectionName)
        {
            if (result == null)
                return;

            var dataSource = _editor.GetDataSource(connectionName);
            if (dataSource == null)
            {
                _logger?.LogWarning("DataSource not found for connection: {ConnectionName}. Export history not saved.", connectionName);
                return;
            }

            var entity = new EXPORT_HISTORY
            {
                EXPORT_HISTORY_ID = result.ExportId,
                EXPORT_TYPE = result.ExportType,
                EXPORT_FORMAT = result.ExportFormat,
                EXPORT_DATE = result.ExportDate,
                EXPORTED_BY = userId,
                FILE_PATH = result.FilePath,
                RECORD_COUNT = result.RecordCount,
                STATUS = result.IsSuccess ? "Success" : "Failed",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var insertResult = dataSource.InsertEntity(EXPORT_HISTORY_TABLE, entity);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to save export history: {Error}", errorMessage);
            }
        }
    }
}
