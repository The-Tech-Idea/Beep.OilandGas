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
    /// IFRS 18 presentation enhancements and management-defined performance measures.
    /// </summary>
    public class PresentationEnhancementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly FinancialStatementService _financialStatementService;
        private readonly ILogger<PresentationEnhancementService> _logger;
        private const string ConnectionName = "PPDM39";

        public PresentationEnhancementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            FinancialStatementService financialStatementService,
            ILogger<PresentationEnhancementService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PresentationPackage> BuildIFRS18PackageAsync(
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

            var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(periodStart, periodEnd, bookId: bookId);
            var balanceSheet = await _financialStatementService.GenerateBalanceSheetAsync(periodEnd, bookId: bookId);
            var cashFlow = await _financialStatementService.GenerateCashFlowStatementAsync(periodStart, periodEnd, bookId: bookId);

            var package = new PresentationPackage
            {
                EntityName = entityName,
                ReportingCurrency = reportingCurrency,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                GeneratedDate = DateTime.UtcNow,
                IncomeStatement = incomeStatement,
                BalanceSheet = balanceSheet,
                CashFlowStatement = cashFlow,
                RequiredDisclosures = new List<string>
                {
                    "Operating, investing, and financing categories",
                    "Management-defined performance measures reconciliation",
                    "Unusual income and expense presentation policy"
                }
            };

            await RecordIFRS18DisclosureAsync(periodEnd, "PresentationEnhancement", userId, connectionName ?? ConnectionName);

            _logger?.LogInformation("IFRS 18 presentation package generated for {Entity} {Start}-{End}",
                entityName, periodStart, periodEnd);

            return package;
        }

        private async Task RecordIFRS18DisclosureAsync(DateTime periodEnd, string disclosureType, string userId, string cn)
        {
            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "IFRS18_DISCLOSURE",
                COST_TYPE = "IFRS18_DISCLOSURE",
                COST_CATEGORY = disclosureType,
                AMOUNT = 0m,
                COST_DATE = periodEnd,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = "IFRS 18 presentation disclosure recorded",
                SOURCE = "IFRS18",
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
