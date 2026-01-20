using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// IAS 33 earnings per share calculations and disclosure storage.
    /// </summary>
    public class EarningsPerShareService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<EarningsPerShareService> _logger;
        private const string ConnectionName = "PPDM39";

        public EarningsPerShareService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<EarningsPerShareService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(decimal BasicEps, decimal? DilutedEps)> RecordEarningsPerShareAsync(
            DateTime periodEnd,
            decimal netIncome,
            decimal weightedAverageShares,
            string userId,
            decimal? dilutedShares = null,
            string? connectionName = null)
        {
            if (weightedAverageShares <= 0m)
                throw new InvalidOperationException("Weighted average shares must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var basicEps = netIncome / weightedAverageShares;
            decimal? dilutedEps = null;
            if (dilutedShares.HasValue && dilutedShares.Value > 0m)
                dilutedEps = netIncome / dilutedShares.Value;

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);

            var remark = BuildRemark(basicEps, dilutedEps, weightedAverageShares, dilutedShares);
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "EARNINGS_PER_SHARE",
                COST_TYPE = "EARNINGS_PER_SHARE",
                COST_CATEGORY = "IAS33",
                AMOUNT = netIncome,
                COST_DATE = periodEnd,
                DESCRIPTION = "Earnings per share disclosure",
                REMARK = remark,
                SOURCE = "IAS33",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(cost, userId);

            _logger?.LogInformation("Recorded IAS 33 EPS basic {Basic} diluted {Diluted}",
                basicEps, dilutedEps);

            return (basicEps, dilutedEps);
        }

        private static string BuildRemark(decimal basicEps, decimal? dilutedEps, decimal weightedAvgShares, decimal? dilutedShares)
        {
            var culture = CultureInfo.InvariantCulture;
            var diluted = dilutedEps.HasValue ? dilutedEps.Value.ToString("0.########", culture) : "N/A";
            var dilutedShareText = dilutedShares.HasValue ? dilutedShares.Value.ToString("0.########", culture) : "N/A";
            return $"BASIC_EPS={basicEps.ToString("0.########", culture)};DILUTED_EPS={diluted};WTD_AVG_SHARES={weightedAvgShares.ToString("0.########", culture)};DILUTED_SHARES={dilutedShareText}";
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
