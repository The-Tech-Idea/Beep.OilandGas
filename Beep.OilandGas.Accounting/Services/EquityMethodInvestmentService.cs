using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 28 investments in associates and joint ventures (equity method).
    /// </summary>
    public class EquityMethodInvestmentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<EquityMethodInvestmentService> _logger;
        private const string ConnectionName = "PPDM39";

        public EquityMethodInvestmentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<EquityMethodInvestmentService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<ACCOUNTING_COST> RecordInvestmentAsync(
            string investmentName,
            DateTime acquisitionDate,
            decimal amount,
            bool isJointVenture,
            string userId,
            string? investmentId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(investmentName))
                throw new ArgumentNullException(nameof(investmentName));
            if (amount <= 0m)
                throw new InvalidOperationException("Investment amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = investmentId ?? "EQUITY_INVESTMENT",
                COST_TYPE = "EQUITY_INVESTMENT",
                COST_CATEGORY = isJointVenture ? "JOINT_VENTURE" : "ASSOCIATE",
                AMOUNT = amount,
                COST_DATE = acquisitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorYes(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Equity investment: {investmentName}",
                SOURCE = "IAS28",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var cashAccount = string.IsNullOrWhiteSpace(cashAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : cashAccountId;

            var investmentAccount = isJointVenture
                ? GetAccountId(AccountMappingKeys.JointVentureInvestment, DefaultGlAccounts.JointVentureInvestment)
                : GetAccountId(AccountMappingKeys.AssociateInvestment, DefaultGlAccounts.AssociateInvestment);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                investmentAccount,
                cashAccount,
                amount,
                $"Equity investment {investmentName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IAS 28 investment {Investment} amount {Amount}",
                investmentName, amount);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordEquityMethodEarningsAsync(
            string investmentName,
            DateTime recognitionDate,
            decimal amount,
            bool isLoss,
            bool isJointVenture,
            string userId,
            string? investmentId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(investmentName))
                throw new ArgumentNullException(nameof(investmentName));
            if (amount <= 0m)
                throw new InvalidOperationException("Amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = investmentId ?? "EQUITY_INVESTMENT",
                COST_TYPE = "EQUITY_METHOD",
                COST_CATEGORY = isLoss ? "LOSS" : "EARNINGS",
                AMOUNT = amount,
                COST_DATE = recognitionDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = isLoss ? _defaults.GetActiveIndicatorYes() : _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Equity method {(isLoss ? "loss" : "earnings")}: {investmentName}",
                SOURCE = "IAS28",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var investmentAccount = isJointVenture
                ? GetAccountId(AccountMappingKeys.JointVentureInvestment, DefaultGlAccounts.JointVentureInvestment)
                : GetAccountId(AccountMappingKeys.AssociateInvestment, DefaultGlAccounts.AssociateInvestment);

            if (!isLoss)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    investmentAccount,
                    GetAccountId(AccountMappingKeys.EquityMethodEarnings, DefaultGlAccounts.EquityMethodEarnings),
                    amount,
                    $"Equity method earnings {investmentName}",
                    userId,
                    cn);
            }
            else
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.EquityMethodLoss, DefaultGlAccounts.EquityMethodLoss),
                    investmentAccount,
                    amount,
                    $"Equity method loss {investmentName}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 28 equity method {Investment} amount {Amount} loss={Loss}",
                investmentName, amount, isLoss);

            return cost;
        }

        public async Task<ACCOUNTING_COST> RecordDividendAsync(
            string investmentName,
            DateTime receiptDate,
            decimal amount,
            bool isJointVenture,
            string userId,
            string? investmentId = null,
            string? cashAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(investmentName))
                throw new ArgumentNullException(nameof(investmentName));
            if (amount <= 0m)
                throw new InvalidOperationException("Dividend amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = investmentId ?? "EQUITY_INVESTMENT",
                COST_TYPE = "EQUITY_METHOD",
                COST_CATEGORY = "DIVIDEND",
                AMOUNT = amount,
                COST_DATE = receiptDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Dividend received: {investmentName}",
                SOURCE = "IAS28",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var cashAccount = string.IsNullOrWhiteSpace(cashAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : cashAccountId;

            var investmentAccount = isJointVenture
                ? GetAccountId(AccountMappingKeys.JointVentureInvestment, DefaultGlAccounts.JointVentureInvestment)
                : GetAccountId(AccountMappingKeys.AssociateInvestment, DefaultGlAccounts.AssociateInvestment);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                cashAccount,
                investmentAccount,
                amount,
                $"Dividend received {investmentName}",
                userId,
                cn);

            _logger?.LogInformation("Recorded IAS 28 dividend {Investment} amount {Amount}",
                investmentName, amount);

            return cost;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



