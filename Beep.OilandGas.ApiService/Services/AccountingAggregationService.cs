using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates PPDM accounting data for role-based dashboards.
    /// Queries RUN_TICKET, REVENUE_TRANSACTION, ROYALTY_CALCULATION, COST_TRANSACTION,
    /// AFE, JIB_CHARGE, and GL_ENTRY tables using PPDMGenericRepository.
    ///
    /// Phase 2 of Role-Based Enhancement Plan.
    /// </summary>
    public class AccountingAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<AccountingAggregationService> _logger;

        public AccountingAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<AccountingAggregationService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, tableName);

        // ── Revenue Aggregation ────────────────────────────────────────────

        public async Task<RevenueSummary> GetRevenueSummaryAsync(
            string? fieldId = null, DateTime? start = null, DateTime? end = null)
        {
            var result = new RevenueSummary();
            var repo = GetRepo<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");
            var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var entities = (await repo.GetAsync(filters)).OfType<REVENUE_TRANSACTION>().ToList();
            if (!entities.Any()) return result;

            result.TotalRevenue = entities.Sum(e => e.AMOUNT ?? 0);
            result.TransactionCount = entities.Count;
            result.ByProduct = entities.GroupBy(e => e.PRODUCT_TYPE ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.AMOUNT ?? 0));
            result.AveragePrice = entities.Any(e => e.VOLUME > 0)
                ? entities.Where(e => e.VOLUME > 0).Average(e => (e.AMOUNT ?? 0) / (e.VOLUME ?? 1))
                : 0;

            // Simple trend: compare first half to second half of period
            var ordered = entities.OrderBy(e => e.TRANSACTION_DATE).ToList();
            var mid = ordered.Count / 2;
            var firstHalf = ordered.Take(mid).Sum(e => e.AMOUNT ?? 0);
            var secondHalf = ordered.Skip(mid).Sum(e => e.AMOUNT ?? 0);
            result.Trend = firstHalf > 0 ? (secondHalf - firstHalf) / firstHalf * 100 : 0;

            return result;
        }

        // ── Cost Aggregation ───────────────────────────────────────────────

        public async Task<CostSummary> GetCostSummaryAsync(
            string? fieldId = null, DateTime? start = null, DateTime? end = null)
        {
            var result = new CostSummary();
            var repo = GetRepo<COST_TRANSACTION>("COST_TRANSACTION");
            var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var entities = (await repo.GetAsync(filters)).OfType<COST_TRANSACTION>().ToList();
            if (!entities.Any()) return result;

            result.TotalCost = entities.Sum(e => e.AMOUNT ?? 0);
            result.ByCostCenter = entities.GroupBy(e => e.COST_CENTER_ID ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.AMOUNT ?? 0));
            result.LOE = entities.Where(e => string.Equals(e.COST_CLASSIFICATION, "LOE", StringComparison.OrdinalIgnoreCase))
                .Sum(e => e.AMOUNT ?? 0);
            result.CAPEX = entities.Where(e => string.Equals(e.COST_CLASSIFICATION, "CAPEX", StringComparison.OrdinalIgnoreCase))
                .Sum(e => e.AMOUNT ?? 0);
            result.TransactionCount = entities.Count;

            return result;
        }

        // ── Royalty Aggregation ────────────────────────────────────────────

        public async Task<RoyaltySummary> GetRoyaltySummaryAsync(
            string? fieldId = null, DateTime? start = null, DateTime? end = null)
        {
            var result = new RoyaltySummary();
            var repo = GetRepo<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");
            var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var entities = (await repo.GetAsync(filters)).OfType<ROYALTY_CALCULATION>().ToList();
            if (!entities.Any()) return result;

            result.TotalRoyalties = entities.Sum(e => e.ROYALTY_AMOUNT ?? 0);
            result.CalculationCount = entities.Count;
            result.PendingPayments = entities.Count(e =>
                string.Equals(e.CALCULATION_STATUS, "PENDING", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.CALCULATION_STATUS, "READY_FOR_PAYMENT", StringComparison.OrdinalIgnoreCase));

            return result;
        }

        // ── AFE Aggregation ────────────────────────────────────────────────

        public async Task<List<AfeSummary>> GetAFESummaryAsync(string? fieldId = null)
        {
            var result = new List<AfeSummary>();
            var repo = GetRepo<AFE>("AFE");
            var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });

            var entities = (await repo.GetAsync(filters)).OfType<AFE>().ToList();

            foreach (var afe in entities)
            {
                result.Add(new AfeSummary
                {
                    AfeId = afe.AFE_ID ?? "N/A",
                    Description = afe.DESCRIPTION ?? "",
                    Budget = afe.BUDGET_AMOUNT ?? 0,
                    Spent = afe.SPENT_AMOUNT ?? 0,
                    Remaining = (afe.BUDGET_AMOUNT ?? 0) - (afe.SPENT_AMOUNT ?? 0),
                    Status = afe.AFE_STATUS ?? "UNKNOWN",
                    ApprovedDate = afe.APPROVED_DATE
                });
            }

            return result.OrderByDescending(a => a.Budget).ToList();
        }

        // ── Period Close Status ────────────────────────────────────────────

        public Task<PeriodCloseStatus> GetPeriodCloseStatusAsync(string? fieldId = null)
        {
            // PERIOD_CLOSE entity type is not yet defined in the Models project.
            // When available, query PPDM PERIOD_CLOSE table via PPDMGenericRepository.
            // For now, return a status indicating the feature is pending.
            return Task.FromResult(new PeriodCloseStatus
            {
                DaysSinceLastClose = 0,
                OpenPeriods = 0,
                ClosedPeriods = 0
            });
        }
    }

    // ── DTOs ───────────────────────────────────────────────────────────────

    public class RevenueSummary
    {
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
        public Dictionary<string, decimal> ByProduct { get; set; } = new();
        public decimal AveragePrice { get; set; }
        public decimal Trend { get; set; }
    }

    public class CostSummary
    {
        public decimal TotalCost { get; set; }
        public decimal LOE { get; set; }
        public decimal CAPEX { get; set; }
        public Dictionary<string, decimal> ByCostCenter { get; set; } = new();
        public int TransactionCount { get; set; }
    }

    public class RoyaltySummary
    {
        public decimal TotalRoyalties { get; set; }
        public int CalculationCount { get; set; }
        public int PendingPayments { get; set; }
    }

    public class AfeSummary
    {
        public string AfeId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }
    }

    public class PeriodCloseStatus
    {
        public DateTime? LastCloseDate { get; set; }
        public int DaysSinceLastClose { get; set; }
        public int OpenPeriods { get; set; }
        public int ClosedPeriods { get; set; }
    }
}
