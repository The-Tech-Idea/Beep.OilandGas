using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 7 cash flow statement generation (direct from GL entries).
    /// </summary>
    public class CashFlowService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<CashFlowService> _logger;
        private const string ConnectionName = "PPDM39";

        public CashFlowService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<CashFlowService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<CashFlowStatement> GenerateCashFlowAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null,
            string? bookId = null)
        {
            if (periodEnd < periodStart)
                throw new ArgumentException("periodEnd must be >= periodStart", nameof(periodEnd));

            var cn = connectionName ?? ConnectionName;
            var cashAccount = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash);

            var entries = await GetGlEntriesAsync(periodStart, periodEnd, cn, bookId);
            var grouped = entries.GroupBy(e => e.JOURNAL_ENTRY_ID).ToList();

            var result = new CashFlowStatement
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };

            var accountTypeCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in grouped)
            {
                var cashDelta = group
                    .Where(e => string.Equals(e.GL_ACCOUNT_ID, cashAccount, StringComparison.OrdinalIgnoreCase))
                    .Sum(e => (e.DEBIT_AMOUNT ?? 0m) - (e.CREDIT_AMOUNT ?? 0m));

                if (cashDelta == 0m)
                    continue;

                var nonCashLines = group
                    .Where(e => !string.Equals(e.GL_ACCOUNT_ID, cashAccount, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (nonCashLines.Count == 0)
                {
                    AddFlow(result, CashFlowCategory.Operating, cashDelta, group.Key);
                    continue;
                }

                var totalWeight = nonCashLines.Sum(l => Math.Abs((l.DEBIT_AMOUNT ?? 0m) - (l.CREDIT_AMOUNT ?? 0m)));
                if (totalWeight == 0m)
                {
                    AddFlow(result, CashFlowCategory.Operating, cashDelta, group.Key);
                    continue;
                }

                foreach (var line in nonCashLines)
                {
                    var lineAmount = Math.Abs((line.DEBIT_AMOUNT ?? 0m) - (line.CREDIT_AMOUNT ?? 0m));
                    if (lineAmount == 0m)
                        continue;

                    var accountType = await GetAccountTypeAsync(line.GL_ACCOUNT_ID, accountTypeCache);
                    var category = MapAccountTypeToCategory(accountType);
                    var allocated = cashDelta * (lineAmount / totalWeight);
                    AddFlow(result, category, allocated, group.Key);
                }
            }

            result.NetChange = result.Operating + result.Investing + result.Financing;
            _logger?.LogInformation(
                "Cash flow generated for {Start} - {End}: Operating {Operating}, Investing {Investing}, Financing {Financing}",
                periodStart, periodEnd, result.Operating, result.Investing, result.Financing);

            return result;
        }

        private async Task<List<GL_ENTRY>> GetGlEntriesAsync(DateTime start, DateTime end, string cn, string? bookId)
        {
            var metadata = await _metadata.GetTableMetadataAsync("GL_ENTRY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(GL_ENTRY);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "GL_ENTRY");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (!string.IsNullOrWhiteSpace(bookId))
            {
                filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });
            }

            var results = await repo.GetAsync(filters);
            return results?.Cast<GL_ENTRY>().ToList() ?? new List<GL_ENTRY>();
        }

        private async Task<string> GetAccountTypeAsync(string? accountId, Dictionary<string, string> cache)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return string.Empty;

            if (cache.TryGetValue(accountId, out var cached))
                return cached;

            var account = await _glAccountService.GetAccountByNumberAsync(accountId);
            var accountType = account?.ACCOUNT_TYPE ?? string.Empty;
            cache[accountId] = accountType;
            return accountType;
        }

        private static CashFlowCategory MapAccountTypeToCategory(string accountType)
        {
            if (string.IsNullOrWhiteSpace(accountType))
                return CashFlowCategory.Operating;

            return accountType.ToUpperInvariant() switch
            {
                "REVENUE" => CashFlowCategory.Operating,
                "EXPENSE" => CashFlowCategory.Operating,
                "ASSET" => CashFlowCategory.Investing,
                "LIABILITY" => CashFlowCategory.Financing,
                "EQUITY" => CashFlowCategory.Financing,
                _ => CashFlowCategory.Operating
            };
        }

        private void AddFlow(CashFlowStatement statement, CashFlowCategory category, decimal amount, string? entryId)
        {
            switch (category)
            {
                case CashFlowCategory.Investing:
                    statement.Investing += amount;
                    break;
                case CashFlowCategory.Financing:
                    statement.Financing += amount;
                    break;
                default:
                    statement.Operating += amount;
                    break;
            }

            statement.Lines.Add(new CashFlowLine
            {
                JournalEntryId = entryId ?? string.Empty,
                Category = category,
                Amount = amount
            });
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}
