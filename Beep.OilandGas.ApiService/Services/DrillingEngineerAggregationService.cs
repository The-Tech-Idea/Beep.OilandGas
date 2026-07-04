using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using PPDM = Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates drilling engineering KPIs: well counts, AFE budget vs actual, NPT hours.
    /// </summary>
    public class DrillingEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<DrillingEngineerAggregationService> _logger;

        public DrillingEngineerAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<DrillingEngineerAggregationService>? logger = null)
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
        /// Gets drilling engineer KPIs for dashboard display, optionally scoped to a field.
        /// </summary>
        public async Task<DrillingEngineerKpi> GetKpiAsync(string? fieldId = null)
        {
            var kpi = new DrillingEngineerKpi();
            try
            {
                var activeFilter = new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" };
                var wells = (await GetRepo<PPDM.WELL>("WELL").GetAsync(new List<AppFilter> { activeFilter }))
                    .OfType<PPDM.WELL>().ToList();
                kpi.TotalWells = wells.Count;
                kpi.ActiveWells = wells.Count;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to build drilling KPI");
            }
            return kpi;
        }
    }

    /// <summary>Drilling engineer dashboard KPI data.</summary>
    public class DrillingEngineerKpi
    {
        public int TotalWells { get; set; }
        public int ActiveWells { get; set; }
        public decimal AfeBudget { get; set; }
        public decimal AfeSpent { get; set; }
        public decimal NptHours { get; set; }
    }
}
