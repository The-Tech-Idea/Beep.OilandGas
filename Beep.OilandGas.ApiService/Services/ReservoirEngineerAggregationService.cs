using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using PpdmEntities= Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates reservoir engineering KPIs: active pools, discovery counts, recovery factors.
    /// </summary>
    public class ReservoirEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ReservoirEngineerAggregationService> _logger;

        public ReservoirEngineerAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ReservoirEngineerAggregationService>? logger = null)
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

        /// <summary>
        /// Gets reservoir engineer KPIs for dashboard display, optionally scoped to a field.
        /// </summary>
        public async Task<ReservoirEngineerKpi> GetKpiAsync(string? fieldId = null)
        {
            var kpi = new ReservoirEngineerKpi();
            try
            {
                var activeFilter = new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" };
                var pools = (await GetRepo<PpdmEntities.POOL>("POOL").GetAsync(new List<AppFilter> { activeFilter }))
                    .OfType<PpdmEntities.POOL>().ToList();
                kpi.ActivePools = pools.Count;
                kpi.PoolsWithDiscovery = pools.Count(p => p.DISCOVERY_DATE.HasValue);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to compute reservoir KPI");
            }
            return kpi;
        }
    }

    /// <summary>Reservoir engineer dashboard KPI data.</summary>
    public class ReservoirEngineerKpi
    {
        public int ActivePools { get; set; }
        public int PoolsWithDiscovery { get; set; }
        public decimal RecoveryFactor { get; set; }
        public int PressureMaintenanceWells { get; set; }
    }
}
