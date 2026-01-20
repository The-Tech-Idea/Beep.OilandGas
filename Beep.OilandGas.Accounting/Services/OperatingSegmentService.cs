using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IFRS 8 operating segment disclosures.
    /// </summary>
    public class OperatingSegmentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<OperatingSegmentService> _logger;
        private const string ConnectionName = "PPDM39";

        public OperatingSegmentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<OperatingSegmentService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ACCOUNTING_COST> RecordSegmentMetricAsync(
            string segmentName,
            string metricName,
            DateTime periodEnd,
            decimal amount,
            string userId,
            string? segmentId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(segmentName))
                throw new ArgumentNullException(nameof(segmentName));
            if (string.IsNullOrWhiteSpace(metricName))
                throw new ArgumentNullException(nameof(metricName));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var remark = $"SEGMENT={segmentName};METRIC={metricName}";

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = segmentId ?? "SEGMENT_REPORT",
                COST_TYPE = "SEGMENT_REPORT",
                COST_CATEGORY = "IFRS8",
                AMOUNT = amount,
                COST_DATE = periodEnd,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = "Operating segment metric",
                REMARK = remark,
                SOURCE = "IFRS8",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            _logger?.LogInformation("Recorded IFRS 8 segment metric {Segment} {Metric} {Amount}",
                segmentName, metricName, amount);

            return cost;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
