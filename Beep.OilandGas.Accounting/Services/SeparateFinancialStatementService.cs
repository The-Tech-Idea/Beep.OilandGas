using System;
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
    /// IAS 27 separate financial statements (entity-level packaging).
    /// </summary>
    public class SeparateFinancialStatementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PresentationService _presentationService;
        private readonly ILogger<SeparateFinancialStatementService> _logger;
        private const string ConnectionName = "PPDM39";

        public SeparateFinancialStatementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PresentationService presentationService,
            ILogger<SeparateFinancialStatementService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PresentationPackage> BuildSeparateStatementsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string reportingCurrency,
            string entityName,
            string userId,
            string? connectionName = null,
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(reportingCurrency))
                throw new ArgumentNullException(nameof(reportingCurrency));
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentNullException(nameof(entityName));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var package = await _presentationService.BuildPresentationPackageAsync(
                periodStart,
                periodEnd,
                reportingCurrency,
                entityName,
                bookId);

            await RecordSeparateStatementAsync(periodEnd, entityName, userId, connectionName ?? ConnectionName);

            _logger?.LogInformation("IAS 27 separate statements generated for {Entity} {Start}-{End}",
                entityName, periodStart, periodEnd);

            return package;
        }

        private async Task RecordSeparateStatementAsync(DateTime periodEnd, string entityName, string userId, string cn)
        {
            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "SEPARATE_STATEMENTS",
                COST_TYPE = "SEPARATE_STATEMENTS",
                COST_CATEGORY = "IAS27",
                AMOUNT = 0m,
                COST_DATE = periodEnd,
                DESCRIPTION = $"Separate statements generated: {entityName}",
                SOURCE = "IAS27",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(cost, userId);
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }
    }
}
