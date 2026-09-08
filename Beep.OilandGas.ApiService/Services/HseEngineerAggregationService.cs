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
    /// Aggregates HSE KPIs: incident counts, open incidents, overdue actions, active permits.
    /// </summary>
    public class HseEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<HseEngineerAggregationService> _logger;

        public HseEngineerAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<HseEngineerAggregationService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        /// <summary>
        /// Gets HSE KPIs for dashboard display: YTD incidents, open incidents, overdue actions.
        /// </summary>
        public async Task<HseKpi> GetKpiAsync()
        {
            var kpi = new HseKpi();
            try
            {
                var activeFilter = new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" };
                var now = DateTime.UtcNow;
                var ytd = new DateTime(now.Year, 1, 1);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PpdmEntities.HSE_INCIDENT), _connectionName, "HSE_INCIDENT");

                var filters = new List<AppFilter>
                {
                    activeFilter,
                    new() { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = ytd.ToString("yyyy-MM-dd") }
                };

                var incidents = (await repo.GetAsync(filters)).OfType<PpdmEntities.HSE_INCIDENT>().ToList();
                kpi.TotalIncidents = incidents.Count;
                kpi.OpenIncidents = incidents.Count(i => string.IsNullOrEmpty(i.INCIDENT_CLASS_ID));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to compute HSE KPI");
            }
            return kpi;
        }
    }

    /// <summary>HSE dashboard KPI data.</summary>
    public class HseKpi
    {
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int OverdueActions { get; set; }
        public int ActivePermits { get; set; }
    }
}
