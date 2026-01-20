using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Royalty;
using Beep.OilandGas.Models.Data.Reporting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Reporting service for operational, financial, royalty, and JIB reports.
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IJointInterestBillingService _jibService;
        private readonly ILogger<ReportingService> _logger;
        private const string ConnectionName = "PPDM39";

        public ReportingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IJointInterestBillingService jibService,
            ILogger<ReportingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _jibService = jibService ?? throw new ArgumentNullException(nameof(jibService));
            _logger = logger;
        }

        public async Task<ReportResult> GenerateOperationalReportAsync(GenerateOperationalReportRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var runTickets = await GetRunTicketsAsync(request.PeriodStart, request.PeriodEnd, request.WellIds, request.LeaseIds, connectionName);
            var totalVolume = runTickets.Sum(rt => rt.NET_VOLUME ?? 0m);
            var report = new OPERATIONAL_REPORT
            {
                OPERATIONAL_REPORT_ID = Guid.NewGuid().ToString(),
                REPORT_TYPE = "OPERATIONAL",
                REPORT_PERIOD_START = request.PeriodStart,
                REPORT_PERIOD_END = request.PeriodEnd,
                GENERATION_DATE = DateTime.UtcNow,
                GENERATED_BY = userId,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                REMARK = $"RunTickets={runTickets.Count}, TotalNetVolume={totalVolume}",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<OPERATIONAL_REPORT>("OPERATIONAL_REPORT", connectionName);
            await repo.InsertAsync(report, userId);

            return new ReportResult
            {
                ReportId = report.OPERATIONAL_REPORT_ID,
                ReportType = report.REPORT_TYPE,
                GeneratedBy = userId,
                ReportData = report
            };
        }

        public async Task<ReportResult> GenerateFinancialReportAsync(GenerateFinancialReportRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var entries = await GetGlEntriesAsync(request.PeriodStart, request.PeriodEnd, connectionName);
            decimal totalRevenue = 0m;
            decimal totalExpenses = 0m;

            foreach (var entry in entries)
            {
                var accountId = entry.GL_ACCOUNT_ID ?? string.Empty;
                var netAmount = (entry.CREDIT_AMOUNT ?? 0m) - (entry.DEBIT_AMOUNT ?? 0m);

                if (accountId.StartsWith("4", StringComparison.OrdinalIgnoreCase))
                    totalRevenue += netAmount;
                if (accountId.StartsWith("5", StringComparison.OrdinalIgnoreCase) || accountId.StartsWith("6", StringComparison.OrdinalIgnoreCase))
                    totalExpenses += (entry.DEBIT_AMOUNT ?? 0m) - (entry.CREDIT_AMOUNT ?? 0m);
            }

            var netIncome = totalRevenue - totalExpenses;
            decimal? royaltyAmount = null;
            decimal? deductionAmount = null;
            decimal? netPayment = null;

            if (string.Equals(request.ReportType, "OWNER", StringComparison.OrdinalIgnoreCase) && request.PropertyIds?.Any() == true)
            {
                var propertyId = request.PropertyIds.First();
                var calculations = await GetRoyaltyCalculationsAsync(
                    ownerId: string.Empty,
                    propertyId: propertyId,
                    start: request.PeriodStart,
                    end: request.PeriodEnd,
                    connectionName: connectionName);

                royaltyAmount = calculations.Sum(c => c.ROYALTY_AMOUNT ?? 0m);
                deductionAmount = calculations.Sum(c =>
                    (c.TRANSPORTATION_COST ?? 0m) + (c.AD_VALOREM_TAX ?? 0m) + (c.SEVERANCE_TAX ?? 0m));
                netPayment = royaltyAmount - deductionAmount;
            }

            var report = new FINANCIAL_REPORT
            {
                FINANCIAL_REPORT_ID = Guid.NewGuid().ToString(),
                REPORT_TYPE = request.ReportType,
                PERIOD_START = request.PeriodStart,
                PERIOD_END = request.PeriodEnd,
                PROPERTY_ID = request.PropertyIds?.FirstOrDefault(),
                TOTAL_REVENUE = totalRevenue,
                TOTAL_EXPENSES = totalExpenses,
                NET_INCOME = netIncome,
                TAXABLE_INCOME = request.ReportType?.Equals("TAX", StringComparison.OrdinalIgnoreCase) == true ? netIncome : null,
                TAX_LIABILITY = request.ReportType?.Equals("TAX", StringComparison.OrdinalIgnoreCase) == true ? netIncome * 0.21m : null,
                ROYALTY_AMOUNT = royaltyAmount,
                DEDUCTIONS = deductionAmount,
                NET_PAYMENT = netPayment,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<FINANCIAL_REPORT>("FINANCIAL_REPORT", connectionName);
            await repo.InsertAsync(report, userId);

            return new ReportResult
            {
                ReportId = report.FINANCIAL_REPORT_ID,
                ReportType = report.REPORT_TYPE,
                GeneratedBy = userId,
                ReportData = report
            };
        }

        public async Task<ReportResult> GenerateRoyaltyStatementAsync(GenerateRoyaltyStatementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.RoyaltyOwnerBaId))
                throw new ProductionAccountingException("Royalty owner BA ID is required");

            var calculations = await GetRoyaltyCalculationsAsync(
                request.RoyaltyOwnerBaId,
                request.PropertyId,
                request.PeriodStart,
                request.PeriodEnd,
                connectionName);

            var totalRoyalty = calculations.Sum(c => c.ROYALTY_AMOUNT ?? 0m);
            var totalDeductions = calculations.Sum(c =>
                (c.TRANSPORTATION_COST ?? 0m) + (c.AD_VALOREM_TAX ?? 0m) + (c.SEVERANCE_TAX ?? 0m));
            var netPayment = totalRoyalty - totalDeductions;

            var statement = new ROYALTY_STATEMENT
            {
                ROYALTY_STATEMENT_ID = Guid.NewGuid().ToString(),
                ROYALTY_OWNER_BA_ID = request.RoyaltyOwnerBaId,
                PROPERTY_ID = request.PropertyId,
                STATEMENT_PERIOD_START = request.PeriodStart,
                STATEMENT_PERIOD_END = request.PeriodEnd,
                TOTAL_ROYALTY_AMOUNT = totalRoyalty,
                TOTAL_DEDUCTIONS = totalDeductions,
                NET_PAYMENT_AMOUNT = netPayment,
                STATEMENT_DATE = DateTime.UtcNow,
                STATUS = "GENERATED",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ROYALTY_STATEMENT>("ROYALTY_STATEMENT", connectionName);
            await repo.InsertAsync(statement, userId);

            return new ReportResult
            {
                ReportId = statement.ROYALTY_STATEMENT_ID,
                ReportType = "ROYALTY_STATEMENT",
                GeneratedBy = userId,
                ReportData = statement
            };
        }

        public async Task<ReportResult> GenerateJIBStatementAsync(GenerateJIBStatementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.LeaseId))
                throw new ProductionAccountingException("Lease ID is required");

            await _jibService.GenerateStatementAsync(request.LeaseId, request.PeriodEnd, userId, connectionName ?? ConnectionName);
            var statement = await GetJointInterestStatementAsync(request.LeaseId, request.PeriodStart, request.PeriodEnd, connectionName);

            return new ReportResult
            {
                ReportId = statement?.JOINT_INTEREST_STATEMENT_ID ?? Guid.NewGuid().ToString(),
                ReportType = "JIB_STATEMENT",
                GeneratedBy = userId,
                ReportData = statement
            };
        }

        public Task<ReportSchedule> ScheduleReportAsync(ScheduleReportRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var nextRun = request.StartDate;
            if (string.Equals(request.ScheduleFrequency, "WEEKLY", StringComparison.OrdinalIgnoreCase))
                nextRun = request.StartDate.AddDays(7);
            else if (string.Equals(request.ScheduleFrequency, "MONTHLY", StringComparison.OrdinalIgnoreCase))
                nextRun = request.StartDate.AddMonths(1);
            else if (string.Equals(request.ScheduleFrequency, "DAILY", StringComparison.OrdinalIgnoreCase))
                nextRun = request.StartDate.AddDays(1);

            var schedule = new ReportSchedule
            {
                ScheduleId = Guid.NewGuid().ToString(),
                ReportType = request.ReportType,
                ScheduleFrequency = request.ScheduleFrequency,
                NextRunDate = nextRun,
                LastRunDate = null,
                Status = "SCHEDULED"
            };

            return Task.FromResult(schedule);
        }

        public Task<List<ReportSchedule>> GetScheduledReportsAsync(string? connectionName = null)
        {
            return Task.FromResult(new List<ReportSchedule>());
        }

        public Task<ReportDistributionResult> DistributeReportAsync(string reportId, ReportDistributionRequest request, string userId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(reportId))
                throw new ArgumentNullException(nameof(reportId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new ReportDistributionResult
            {
                ReportId = reportId,
                IsDistributed = true,
                RecipientCount = request.RecipientEmails?.Count ?? 0
            };

            return Task.FromResult(result);
        }

        public async Task<List<ReportHistory>> GetReportHistoryAsync(string? reportType, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            var histories = new List<ReportHistory>();

            if (string.IsNullOrWhiteSpace(reportType) || string.Equals(reportType, "OPERATIONAL", StringComparison.OrdinalIgnoreCase))
            {
                var operational = await GetOperationalReportsAsync(startDate, endDate, connectionName);
                histories.AddRange(operational.Select(r => new ReportHistory
                {
                    ReportId = r.OPERATIONAL_REPORT_ID,
                    ReportType = r.REPORT_TYPE,
                    GeneratedDate = r.GENERATION_DATE ?? DateTime.UtcNow,
                    GeneratedBy = r.GENERATED_BY,
                    PeriodStart = r.REPORT_PERIOD_START ?? DateTime.MinValue,
                    PeriodEnd = r.REPORT_PERIOD_END ?? DateTime.MinValue,
                    Status = "GENERATED"
                }));
            }

            if (string.IsNullOrWhiteSpace(reportType) || string.Equals(reportType, "FINANCIAL", StringComparison.OrdinalIgnoreCase))
            {
                var financial = await GetFinancialReportsAsync(startDate, endDate, connectionName);
                histories.AddRange(financial.Select(r => new ReportHistory
                {
                    ReportId = r.FINANCIAL_REPORT_ID,
                    ReportType = r.REPORT_TYPE,
                    GeneratedDate = r.ROW_CREATED_DATE ?? DateTime.UtcNow,
                    GeneratedBy = r.ROW_CREATED_BY,
                    PeriodStart = r.PERIOD_START ?? DateTime.MinValue,
                    PeriodEnd = r.PERIOD_END ?? DateTime.MinValue,
                    Status = "GENERATED"
                }));
            }

            return histories;
        }

        private async Task<List<RUN_TICKET>> GetRunTicketsAsync(DateTime start, DateTime end, List<string>? wellIds, List<string>? leaseIds, string? connectionName)
        {
            var repo = await GetRepoAsync<RUN_TICKET>("RUN_TICKET", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (wellIds != null && wellIds.Count > 0)
                filters.Add(new AppFilter { FieldName = "WELL_ID", Operator = "IN", FilterValue = string.Join(",", wellIds) });
            if (leaseIds != null && leaseIds.Count > 0)
                filters.Add(new AppFilter { FieldName = "LEASE_ID", Operator = "IN", FilterValue = string.Join(",", leaseIds) });

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().ToList() ?? new List<RUN_TICKET>();
        }

        private async Task<List<OPERATIONAL_REPORT>> GetOperationalReportsAsync(DateTime? startDate, DateTime? endDate, string? connectionName)
        {
            var repo = await GetRepoAsync<OPERATIONAL_REPORT>("OPERATIONAL_REPORT", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "GENERATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "GENERATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            return results?.Cast<OPERATIONAL_REPORT>().ToList() ?? new List<OPERATIONAL_REPORT>();
        }

        private async Task<List<FINANCIAL_REPORT>> GetFinancialReportsAsync(DateTime? startDate, DateTime? endDate, string? connectionName)
        {
            var repo = await GetRepoAsync<FINANCIAL_REPORT>("FINANCIAL_REPORT", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            return results?.Cast<FINANCIAL_REPORT>().ToList() ?? new List<FINANCIAL_REPORT>();
        }

        private async Task<List<GL_ENTRY>> GetGlEntriesAsync(DateTime start, DateTime end, string? connectionName)
        {
            var repo = await GetRepoAsync<GL_ENTRY>("GL_ENTRY", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<GL_ENTRY>().ToList() ?? new List<GL_ENTRY>();
        }

        private async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(
            string ownerId,
            string propertyId,
            DateTime start,
            DateTime end,
            string? connectionName)
        {
            var repo = await GetRepoAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(ownerId))
                filters.Add(new AppFilter { FieldName = "ROYALTY_OWNER_ID", Operator = "=", FilterValue = ownerId });

            var results = await repo.GetAsync(filters);
            return results?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();
        }

        private async Task<JOINT_INTEREST_STATEMENT?> GetJointInterestStatementAsync(
            string leaseId,
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName)
        {
            var repo = await GetRepoAsync<JOINT_INTEREST_STATEMENT>("JOINT_INTEREST_STATEMENT", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "JIB_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "REPORT_PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "REPORT_PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<JOINT_INTEREST_STATEMENT>().OrderByDescending(s => s.GENERATION_DATE).FirstOrDefault();
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName ?? ConnectionName, tableName);
        }
    }
}
