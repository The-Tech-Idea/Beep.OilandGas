using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using Serilog;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Analytics
{
    /// <summary>
    /// Computes field-level process performance KPIs from the PPDM39 database.
    /// Standards: SPE PRMS §7, API RP 97, IOGP KPI Report 2022e, NI 51-101.
    /// </summary>
    public class ProcessAnalyticsService : IProcessAnalyticsService
    {
        private readonly IDMEEditor                  _editor;
        private readonly ICommonColumnHandler        _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository   _defaults;
        private readonly IPPDMMetadataRepository     _metadata;
        private readonly string                      _connectionName;
        private readonly ILogger<ProcessAnalyticsService> _logger;

        public ProcessAnalyticsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<ProcessAnalyticsService> logger)
        {
            _editor              = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults            = defaults;
            _metadata            = metadata;
            _connectionName      = connectionName;
            _logger              = logger;
        }

        // ── Work Orders ───────────────────────────────────────────────────────

        public async Task<WorkOrderKPISet> GetWorkOrderKPIsAsync(string fieldId, DateRangeFilter range)
        {
            Log.Information("Computing WO KPIs for field {FieldId}, range {From}–{To}",
                fieldId, range.From, range.To);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROJECT");

                var filters = BuildBaseFilters(fieldId, range);
                var all = (await repo.GetAsync(filters)).ToList();

                int total     = all.Count;
                int completed = CountByProperty(all, "PROJECT_STATUS", "COMPLETED");
                int overdue   = CountOverdue(all, "PLAN_END_DATE");
                int withBA    = await CountWithContractorAsync(fieldId);

                double completionRate       = total > 0 ? (completed  / (double)total)  * 100 : 0;
                double contractorRate       = total > 0 ? (withBA     / (double)total)  * 100 : 0;
                double mttc                 = ComputeMeanDuration(all, "PROPOSED_DATE", "ACTUAL_DATE");
                double onTimeRate           = ComputeOnTimeRate(all, "PLAN_END_DATE", "ACTUAL_DATE");

                return new WorkOrderKPISet(completionRate, onTimeRate, mttc, overdue, contractorRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing WO KPIs for field {FieldId}", fieldId);
                return new WorkOrderKPISet(0, 0, 0, 0, 0);
            }
        }

        // ── Gate Reviews ──────────────────────────────────────────────────────

        public async Task<GateReviewKPISet> GetGateReviewKPIsAsync(string fieldId, DateRangeFilter range)
        {
            Log.Information("Computing Gate KPIs for field {FieldId}", fieldId);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STATUS");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROJECT_STATUS");

                var filters = BuildBaseFilters(fieldId, range);
                filters.Add(new AppFilter { FieldName = "STATUS_TYPE", Operator = "LIKE", FilterValue = "GATE%" });
                var all = (await repo.GetAsync(filters)).ToList();

                int total    = all.Count;
                int approved = CountByProperty(all, "STATUS", "APPROVED");
                int pending  = total - approved - CountByProperty(all, "STATUS", "REJECTED");

                double passRate   = total > 0 ? (approved / (double)total) * 100 : 0;
                double cycleTime  = ComputeMeanDuration(all, "PROPOSED_DATE", "ACTUAL_DATE");

                return new GateReviewKPISet(cycleTime, passRate, 0, pending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing Gate KPIs for field {FieldId}", fieldId);
                return new GateReviewKPISet(0, 0, 0, 0);
            }
        }

        // ── HSE ───────────────────────────────────────────────────────────────

        public async Task<HSEKPISet> GetHSEKPIsAsync(
            string fieldId, DateRangeFilter range, double exposureHours)
        {
            Log.Information("Computing HSE KPIs for field {FieldId}", fieldId);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("HSE_INCIDENT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "HSE_INCIDENT");

                var filters = BuildBaseFilters(fieldId, range);
                var all = (await repo.GetAsync(filters)).ToList();

                int tier1      = CountByProperty(all, "INCIDENT_TIER", "1");
                int tier2      = CountByProperty(all, "INCIDENT_TIER", "2");
                int nearMiss   = CountByProperty(all, "INCIDENT_TYPE", "NEAR_MISS");
                int total      = all.Count;
                int recordable = tier1 + tier2 + CountByProperty(all, "INCIDENT_TIER", "3");

                double hours   = exposureHours > 0 ? exposureHours : 1_000_000;
                double pse1    = (tier1    / hours) * 1_000_000;
                double pse2    = (tier2    / hours) * 1_000_000;
                double trir    = (recordable / hours) * 200_000;
                double nmRate  = total > 0 ? (nearMiss / (double)total) * 100 : 0;
                double meanCA  = ComputeMeanDuration(all, "INCIDENT_DATE", "CLOSE_DATE");

                return new HSEKPISet(pse1, pse2, trir, nmRate, meanCA);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing HSE KPIs for field {FieldId}", fieldId);
                return new HSEKPISet(0, 0, 0, 0, 0);
            }
        }

        // ── Compliance ────────────────────────────────────────────────────────

        public async Task<ComplianceKPISet> GetComplianceKPIsAsync(string fieldId, DateRangeFilter range)
        {
            Log.Information("Computing Compliance KPIs for field {FieldId}", fieldId);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("OBLIGATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "OBLIGATION");

                var filters = BuildBaseFilters(fieldId, range);
                var all = (await repo.GetAsync(filters)).ToList();

                int total     = all.Count;
                int fulfilled = CountByProperty(all, "OBLIG_STATUS", "FULFILLED");
                int overdue   = CountOverdue(all, "DUE_DATE");

                double onTimeRate = total > 0 ? (fulfilled / (double)total) * 100 : 0;

                return new ComplianceKPISet(onTimeRate, overdue, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing Compliance KPIs for field {FieldId}", fieldId);
                return new ComplianceKPISet(0, 0, 0);
            }
        }

        // ── Production ────────────────────────────────────────────────────────

        public async Task<ProductionKPISet> GetProductionKPIsAsync(string fieldId, DateRangeFilter range)
        {
            Log.Information("Computing Production KPIs for field {FieldId}", fieldId);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PDEN_VOL_SUMMARY");

                var filters = BuildBaseFilters(fieldId, range);
                var all = (await repo.GetAsync(filters)).ToList();

                double totalBOE    = SumProperty(all, "PROD_BOE");
                double monthCount  = Math.Max(1, (range.To - range.From).TotalDays / 30.0);
                double monthlyBOE  = totalBOE / monthCount;

                return new ProductionKPISet(monthlyBOE, 0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing Production KPIs for field {FieldId}", fieldId);
                return new ProductionKPISet(0, 0, 0);
            }
        }

        // ── Dashboard summary ─────────────────────────────────────────────────

        public async Task<AnalyticsDashboardSummary> GetDashboardSummaryAsync(
            string fieldId, DateRangeFilter range, double exposureHours)
        {
            var wo         = await GetWorkOrderKPIsAsync(fieldId, range);
            var gate       = await GetGateReviewKPIsAsync(fieldId, range);
            var hse        = await GetHSEKPIsAsync(fieldId, range, exposureHours);
            var compliance = await GetComplianceKPIsAsync(fieldId, range);
            var production = await GetProductionKPIsAsync(fieldId, range);

            return new AnalyticsDashboardSummary(
                wo, gate, hse, compliance, production, DateTime.UtcNow);
        }

        // ── Production trend ──────────────────────────────────────────────────

        public async Task<List<KPITrendPoint>> GetProductionTrendAsync(
            string fieldId, DateRangeFilter range, string seriesName = "BOE")
        {
            Log.Information("Computing production trend for field {FieldId}", fieldId);
            var result = new List<KPITrendPoint>();
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PDEN_VOL_SUMMARY");

                var filters = BuildBaseFilters(fieldId, range);
                var all = (await repo.GetAsync(filters)).ToList();
                if (all.Count == 0) return result;

                // Group by month using reflection
                var grouped = all
                    .Select(item => new
                    {
                        Date  = GetDateProperty(item, "PROD_PERIOD_DATE"),
                        Value = GetDoubleProperty(item, "PROD_BOE")
                    })
                    .Where(x => x.Date.HasValue)
                    .GroupBy(x => new DateTime(x.Date!.Value.Year, x.Date.Value.Month, 1))
                    .OrderBy(g => g.Key);

                foreach (var g in grouped)
                {
                    result.Add(new KPITrendPoint
                    {
                        PeriodDate  = g.Key,
                        PeriodLabel = g.Key.ToString("yyyy-MM"),
                        Value       = g.Sum(x => x.Value),
                        SeriesName  = seriesName
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing production trend for field {FieldId}", fieldId);
            }
            return result;
        }

        // ── Reserves maturation ───────────────────────────────────────────────

        public async Task<ReservesMaturationSummary> GetReservesMaturationAsync(string fieldId)
        {
            Log.Information("Computing reserves maturation for field {FieldId}", fieldId);
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PDEN_VOL_SUMMARY");

                var filters = new List<AppFilter>
                {
                    new() { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"     }
                };
                var all = (await repo.GetAsync(filters)).ToList();

                double prospective = SumPropertyByCategory(all, "PROD_BOE", "RESERVES_CLASS", "PROSPECTIVE");
                double contingent  = SumPropertyByCategory(all, "PROD_BOE", "RESERVES_CLASS", "CONTINGENT");
                double proved      = SumPropertyByCategory(all, "PROD_BOE", "RESERVES_CLASS", "PROVED");

                double total          = prospective + contingent + proved;
                double maturationRate = total > 0 ? ((contingent + proved) / total) * 100 : 0;

                return new ReservesMaturationSummary
                {
                    FieldId           = fieldId,
                    ProspectiveMMBOE  = prospective / 1_000_000,
                    ContingentMMBOE   = contingent  / 1_000_000,
                    ProvedMMBOE       = proved       / 1_000_000,
                    MaturationRatePct = maturationRate,
                    AsOfDate          = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing reserves maturation for field {FieldId}", fieldId);
                return new ReservesMaturationSummary { FieldId = fieldId, AsOfDate = DateTime.UtcNow };
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static List<AppFilter> BuildBaseFilters(string fieldId, DateRangeFilter range)
        {
            return new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",    Operator = "=",  FilterValue = fieldId },
                new() { FieldName = "ACTIVE_IND",  Operator = "=",  FilterValue = "Y"     },
                new() { FieldName = "CREATE_DATE", Operator = ">=", FilterValue = range.From.ToString("yyyy-MM-dd") },
                new() { FieldName = "CREATE_DATE", Operator = "<=", FilterValue = range.To.ToString("yyyy-MM-dd")   }
            };
        }

        private static int CountByProperty(List<object>? items, string propertyName, string value)
        {
            if (items == null) return 0;
            return items.Count(item =>
            {
                var val = item.GetType().GetProperty(propertyName)?.GetValue(item)?.ToString();
                return string.Equals(val, value, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int CountOverdue(List<object>? items, string datePropertyName)
        {
            if (items == null) return 0;
            var now = DateTime.UtcNow;
            return items.Count(item =>
            {
                var d = GetDateProperty(item, datePropertyName);
                return d.HasValue && d.Value < now;
            });
        }

        private static double SumProperty(List<object>? items, string propertyName)
        {
            if (items == null) return 0;
            return items.Sum(item => GetDoubleProperty(item, propertyName));
        }

        private static double SumPropertyByCategory(
            List<object>? items, string valueProperty, string categoryProperty, string categoryValue)
        {
            if (items == null) return 0;
            return items
                .Where(item =>
                {
                    var cat = item.GetType().GetProperty(categoryProperty)?.GetValue(item)?.ToString();
                    return string.Equals(cat, categoryValue, StringComparison.OrdinalIgnoreCase);
                })
                .Sum(item => GetDoubleProperty(item, valueProperty));
        }

        private static double GetDoubleProperty(object item, string propertyName)
        {
            var val = item.GetType().GetProperty(propertyName)?.GetValue(item);
            return val is double d ? d : val is decimal m ? (double)m : val is float f ? f : 0;
        }

        private static DateTime? GetDateProperty(object item, string propertyName)
        {
            var val = item.GetType().GetProperty(propertyName)?.GetValue(item);
            if (val is DateTime dt) return dt;
            if (val is string s && DateTime.TryParse(s, out var parsed)) return parsed;
            return null;
        }

        private static double ComputeMeanDuration(
            List<object>? items, string startProperty, string endProperty)
        {
            if (items == null || items.Count == 0) return 0;
            var durations = items
                .Select(item =>
                {
                    var start = GetDateProperty(item, startProperty);
                    var end   = GetDateProperty(item, endProperty);
                    return start.HasValue && end.HasValue ? (end.Value - start.Value).TotalHours : (double?)null;
                })
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();
            return durations.Count > 0 ? durations.Average() : 0;
        }

        private static double ComputeOnTimeRate(
            List<object>? items, string scheduledEndProperty, string actualEndProperty)
        {
            if (items == null || items.Count == 0) return 0;
            int withDue = 0, onTime = 0;
            foreach (var item in items)
            {
                var scheduled = GetDateProperty(item, scheduledEndProperty);
                var actual    = GetDateProperty(item, actualEndProperty);
                if (!scheduled.HasValue) continue;
                withDue++;
                if (actual.HasValue && actual.Value <= scheduled.Value) onTime++;
            }
            return withDue > 0 ? (onTime / (double)withDue) * 100 : 0;
        }

        private async Task<int> CountWithContractorAsync(string fieldId)
        {
            try
            {
                var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_BA");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
                var repo       = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROJECT_STEP_BA");
                var filters = new List<AppFilter>
                {
                    new() { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"     },
                    new() { FieldName = "ROLE",       Operator = "=", FilterValue = "CONTRACTOR" }
                };
                var result = await repo.GetAsync(filters);
                return result.Count();
            }
            catch { return 0; }
        }
    }
}
