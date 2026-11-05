using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using PpdmEntities= Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates reservoir engineering KPIs: active pools, pools with a discovery date, and the wells under pressure
    /// maintenance (with an injection operation on record).
    /// </summary>
    /// <remarks>
    /// The recovery factor and the pressure-maintenance count had been answered as 0, which nothing computed (2026-10-10).
    /// The count now comes from the enhanced-recovery service's injection operations; the recovery factor is null.
    /// </remarks>
    public class ReservoirEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IEnhancedRecoveryService _enhancedRecovery;
        private readonly WellServices _wells;
        private readonly string _connectionName;
        private readonly ILogger<ReservoirEngineerAggregationService>? _logger;

        public ReservoirEngineerAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IEnhancedRecoveryService enhancedRecovery,
            WellServices wells,
            string connectionName = "PPDM39",
            ILogger<ReservoirEngineerAggregationService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _enhancedRecovery = enhancedRecovery ?? throw new ArgumentNullException(nameof(enhancedRecovery));
            _wells = wells ?? throw new ArgumentNullException(nameof(wells));
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, tableName);

        /// <summary>
        /// Gets reservoir engineer KPIs for dashboard display, scoped to a field when one is named.
        /// </summary>
        public async Task<ReservoirEngineerKpi> GetKpiAsync(string? fieldId = null)
        {
            var kpi = new ReservoirEngineerKpi();
            var poolFilters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
            // The field was accepted and never applied: every field's pools were counted for any field asked about.
            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                poolFilters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            }

            var pools = (await GetRepo<PpdmEntities.POOL>("POOL").GetAsync(poolFilters))
                .OfType<PpdmEntities.POOL>().ToList();
            kpi.ActivePools = pools.Count;
            kpi.PoolsWithDiscovery = pools.Count(p => p.DISCOVERY_DATE.HasValue);

            // Not yet computed: no pool's in-place volume or recoverable reserves is read anywhere in the API (POOL carries
            // neither), so there is no recovery factor to average; it stays null rather than 0.
            kpi.RecoveryFactor = null;

            // Pressure maintenance is injection: the wells with an active injection operation the enhanced-recovery service
            // records, in the field when one is named.
            var injectionWells = (await _enhancedRecovery.GetInjectionOperationsAsync())
                .Select(operation => operation.WellUWI?.Trim())
                .Where(uwi => !string.IsNullOrEmpty(uwi))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(fieldId) && injectionWells.Count > 0)
            {
                var fieldWells = (await _wells.GetWellsByFieldAsync(fieldId))
                    .Select(well => well.UWI)
                    .Where(uwi => !string.IsNullOrWhiteSpace(uwi))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                injectionWells.IntersectWith(fieldWells);
            }

            kpi.PressureMaintenanceWells = injectionWells.Count;
            return kpi;
        }
    }

    /// <summary>Reservoir engineer dashboard KPI data. A null figure is one nothing computes.</summary>
    public class ReservoirEngineerKpi
    {
        public int ActivePools { get; set; }
        public int PoolsWithDiscovery { get; set; }

        /// <summary>Not computed (no pool's in-place volume is recorded): always null for now.</summary>
        public decimal? RecoveryFactor { get; set; }

        /// <summary>Wells with an active injection operation on record.</summary>
        public int PressureMaintenanceWells { get; set; }
    }
}
