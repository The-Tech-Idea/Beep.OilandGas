using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates portfolio-level KPIs for executive dashboards.
    /// Queries PDEN_VOL_SUMMARY, FIELD, WELL, RESERVE, HSE_INCIDENT, REVENUE_TRANSACTION, OBLIGATION.
    /// Phase 3 of Role-Based Enhancement Plan.
    /// </summary>
    public class ExecutiveAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ExecutiveAggregationService> _logger;

        public ExecutiveAggregationService(
            IDMEEditor editor, ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39", ILogger<ExecutiveAggregationService>? logger = null)
        {
            _editor = editor; _commonColumnHandler = commonColumnHandler;
            _defaults = defaults; _metadata = metadata;
            _connectionName = connectionName; _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, tableName);

        public async Task<ExecutiveKpi> GetExecutiveKpiAsync()
        {
            var kpi = new ExecutiveKpi();

            try
            {
                // Production — sum PDEN_VOL_SUMMARY for current month
                var pdenRepo = GetRepo<Beep.OilandGas.Models.Data.Common.PDEN_VOL_SUMMARY>("PDEN_VOL_SUMMARY");
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var pdenFilters = new List<AppFilter>
                {
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                    new() { FieldName = "PRODUCTION_DATE", Operator = ">=", FilterValue = monthStart.ToString("yyyy-MM-dd") }
                };
                var pdenEntities = (await pdenRepo.GetAsync(pdenFilters))
                    .OfType<Beep.OilandGas.Models.Data.Common.PDEN_VOL_SUMMARY>().ToList();
                kpi.TotalProductionBoe = pdenEntities.Sum(e => (e.OIL_VOLUME ?? 0) + ((e.GAS_VOLUME ?? 0) / 6m));

                // Previous month for trend
                var prevMonth = monthStart.AddMonths(-1);
                pdenFilters[1] = new AppFilter { FieldName = "PRODUCTION_DATE", Operator = ">=", FilterValue = prevMonth.ToString("yyyy-MM-dd") };
                pdenFilters.Add(new AppFilter { FieldName = "PRODUCTION_DATE", Operator = "<", FilterValue = monthStart.ToString("yyyy-MM-dd") });
                var prevEntities = (await pdenRepo.GetAsync(pdenFilters))
                    .OfType<Beep.OilandGas.Models.Data.Common.PDEN_VOL_SUMMARY>().ToList();
                decimal prevBoe = prevEntities.Sum(e => (e.OIL_VOLUME ?? 0) + ((e.GAS_VOLUME ?? 0) / 6m));
                kpi.ProductionTrend = prevBoe > 0 ? ((kpi.TotalProductionBoe - prevBoe) / prevBoe) * 100 : 0;

                // Well counts
                var wellRepo = GetRepo<Beep.OilandGas.PPDM39.Models.WELL>("WELL");
                var wellFilters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
                var wells = (await wellRepo.GetAsync(wellFilters)).OfType<Beep.OilandGas.PPDM39.Models.WELL>().ToList();
                kpi.TotalWells = wells.Count;
                kpi.ActiveWells = wells.Count(w => string.Equals(w.WELL_STATUS, "PRODUCING", StringComparison.OrdinalIgnoreCase));

                // HSE — recent incidents
                var hseRepo = GetRepo<Beep.OilandGas.Models.Data.HSE.HSE_INCIDENT>("HSE_INCIDENT");
                var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
                var hseFilters = new List<AppFilter>
                {
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                    new() { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = yearStart.ToString("yyyy-MM-dd") }
                };
                var incidents = (await hseRepo.GetAsync(hseFilters))
                    .OfType<Beep.OilandGas.Models.Data.HSE.HSE_INCIDENT>().ToList();
                kpi.Tier1Events = incidents.Count(i =>
                    string.Equals(i.INCIDENT_TIER, "TIER_1", StringComparison.OrdinalIgnoreCase));
                kpi.TotalIncidentsYtd = incidents.Count;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to build executive KPI");
            }

            return kpi;
        }

        public async Task<List<AssetPerformance>> GetAssetPerformanceAsync()
        {
            var result = new List<AssetPerformance>();
            try
            {
                var fieldRepo = GetRepo<Beep.OilandGas.PPDM39.Models.FIELD>("FIELD");
                var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
                var fields = (await fieldRepo.GetAsync(filters)).OfType<Beep.OilandGas.PPDM39.Models.FIELD>().ToList();

                foreach (var field in fields)
                {
                    result.Add(new AssetPerformance
                    {
                        FieldId = field.FIELD_ID ?? "N/A",
                        FieldName = field.FIELD_NAME ?? field.FIELD_ID ?? "Unknown",
                        LifecyclePhase = field.CURRENT_PHASE ?? "UNKNOWN",
                        ActiveWells = 0 // Populated by FieldOrchestrator if available
                    });
                }
            }
            catch (Exception ex) { _logger?.LogWarning(ex, "Failed to load asset performance"); }
            return result.OrderByDescending(a => a.ActiveWells).ToList();
        }
    }

    public class ExecutiveKpi
    {
        public decimal TotalProductionBoe { get; set; }
        public decimal ProductionTrend { get; set; }
        public int TotalWells { get; set; }
        public int ActiveWells { get; set; }
        public int Tier1Events { get; set; }
        public int TotalIncidentsYtd { get; set; }
    }

    public class AssetPerformance
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string LifecyclePhase { get; set; } = string.Empty;
        public int ActiveWells { get; set; }
    }
}
