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
    public class ReservoirAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ReservoirAggregationService> _logger;

        public ReservoirAggregationService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39", ILogger<ReservoirAggregationService>? logger = null)
        { _editor = editor; _commonColumnHandler = commonColumnHandler; _defaults = defaults; _metadata = metadata; _connectionName = connectionName; _logger = logger; }

        private PPDMGenericRepository GetRepo<T>(string tableName) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, tableName);

        public async Task<List<PoolSummary>> GetPoolSummaryAsync(string? fieldId = null)
        {
            var result = new List<PoolSummary>();
            try
            {
                var repo = GetRepo<Beep.OilandGas.PPDM39.Models.POOL>("POOL");
                var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
                if (!string.IsNullOrWhiteSpace(fieldId))
                    filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
                var pools = (await repo.GetAsync(filters)).OfType<Beep.OilandGas.PPDM39.Models.POOL>().ToList();
                result = pools.Select(p => new PoolSummary
                {
                    PoolId = p.POOL_ID ?? "N/A",
                    PoolName = p.POOL_NAME ?? p.POOL_ID ?? "Unknown",
                    DiscoveryDate = p.DISCOVERY_DATE,
                    Status = p.POOL_STATUS ?? "UNKNOWN"
                }).OrderBy(p => p.PoolName).ToList();
            }
            catch (Exception ex) { _logger?.LogWarning(ex, "Failed to load pool summary"); }
            return result;
        }
    }

    public class PoolSummary
    {
        public string PoolId { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public DateTime? DiscoveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
