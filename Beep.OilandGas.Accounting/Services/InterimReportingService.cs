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
    /// IAS 34 interim financial reporting support.
    /// </summary>
    public class InterimReportingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly FinancialStatementService _financialStatementService;
        private readonly ILogger<InterimReportingService> _logger;
        private const string ConnectionName = "PPDM39";

        public InterimReportingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            FinancialStatementService financialStatementService,
            ILogger<InterimReportingService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PresentationPackage> BuildInterimPackageAsync(
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
                    "Interim reporting basis and accounting policies",
                    "Seasonality and unusual items",
                    "Significant changes in estimates",
                    "Dividends declared",
                    "Events after the reporting period"
                }
            };

            await RecordInterimReportAsync(periodEnd, entityName, userId, connectionName ?? ConnectionName);

            _logger?.LogInformation("IAS 34 interim package generated for {Entity} {Start} - {End}",
                entityName, periodStart, periodEnd);

            return package;
        }

        private async Task RecordInterimReportAsync(DateTime periodEnd, string entityName, string userId, string cn)
        {
            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = "INTERIM_REPORT",
                COST_TYPE = "INTERIM_REPORT",
                COST_CATEGORY = "IAS34",
                AMOUNT = 0m,
                COST_DATE = periodEnd,
                DESCRIPTION = $"Interim report generated: {entityName}",
                SOURCE = "IAS34",
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
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
