using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Service for managing GL account operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class GLAccountService : IGLAccountService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<GLAccountService>? _logger;
        private readonly string _connectionName;
        private const string GL_ACCOUNT_TABLE = "GL_ACCOUNT";
        private const string GL_ENTRY_TABLE = "GL_ENTRY";

        public GLAccountService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<GLAccountService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new GL account.
        /// </summary>
        public async Task<GL_ACCOUNT> CreateAccountAsync(
            CreateGLAccountRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.AccountNumber))
                throw new ArgumentException("Account number is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            // Check if account number already exists
            var existingFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACCOUNT_NUMBER", Operator = "=", FilterValue = request.AccountNumber }
            };
            var existing = await repo.GetAsync(existingFilters);
            if (existing.Any())
            {
                throw new InvalidOperationException($"Account number {request.AccountNumber} already exists.");
            }

            var account = new GL_ACCOUNT
            {
                GL_ACCOUNT_ID = Guid.NewGuid().ToString(),
                ACCOUNT_NUMBER = request.AccountNumber,
                ACCOUNT_NAME = request.AccountName,
                ACCOUNT_TYPE = request.AccountType,
                PARENT_ACCOUNT_ID = request.ParentAccountId,
                NORMAL_BALANCE = request.NormalBalance,
                OPENING_BALANCE = request.OpeningBalance,
                CURRENT_BALANCE = request.OpeningBalance ?? 0m,
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            // Set common columns
            if (account is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(account);

            _logger?.LogDebug("Created GL account {AccountNumber}", request.AccountNumber);
            return account;
        }

        /// <summary>
        /// Gets a GL account by ID.
        /// </summary>
        public async Task<GL_ACCOUNT?> GetAccountAsync(string accountId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = accountId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<GL_ACCOUNT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets a GL account by account number.
        /// </summary>
        public async Task<GL_ACCOUNT?> GetAccountByNumberAsync(string accountNumber, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACCOUNT_NUMBER", Operator = "=", FilterValue = accountNumber }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<GL_ACCOUNT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets accounts by type.
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetAccountsByTypeAsync(string accountType, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountType))
                return new List<GL_ACCOUNT>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACCOUNT_TYPE", Operator = "=", FilterValue = accountType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<GL_ACCOUNT>().ToList();
        }

        /// <summary>
        /// Gets account hierarchy (parent-child relationships).
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetAccountHierarchyAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<GL_ACCOUNT>().OrderBy(a => a.ACCOUNT_NUMBER).ToList();
        }

        /// <summary>
        /// Updates a GL account.
        /// </summary>
        public async Task<GL_ACCOUNT> UpdateAccountAsync(
            UpdateGLAccountRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.GlAccountId))
                throw new ArgumentException("GL Account ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var existing = await GetAccountAsync(request.GlAccountId, connName);
            if (existing == null)
                throw new InvalidOperationException($"GL Account {request.GlAccountId} not found.");

            // Update properties
            existing.ACCOUNT_NUMBER = request.AccountNumber;
            existing.ACCOUNT_NAME = request.AccountName;
            existing.ACCOUNT_TYPE = request.AccountType;
            existing.PARENT_ACCOUNT_ID = request.ParentAccountId;
            existing.NORMAL_BALANCE = request.NormalBalance;
            existing.OPENING_BALANCE = request.OpeningBalance;
            existing.DESCRIPTION = request.Description;

            // Set common columns
            if (existing is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            await repo.UpdateAsync(existing);

            _logger?.LogDebug("Updated GL account {AccountId}", request.GlAccountId);
            return existing;
        }

        /// <summary>
        /// Deletes a GL account (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        public async Task<bool> DeleteAccountAsync(string accountId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                return false;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            var account = await GetAccountAsync(accountId, connName);
            if (account == null)
                return false;

            account.ACTIVE_IND = "N";

            if (account is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            await repo.UpdateAsync(account);

            _logger?.LogDebug("Deleted GL account {AccountId}", accountId);
            return true;
        }

        /// <summary>
        /// Gets account balance summary.
        /// </summary>
        public async Task<AccountBalanceSummary> GetAccountBalanceAsync(
            string accountId,
            DateTime? asOfDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                throw new ArgumentException("Account ID is required.", nameof(accountId));

            var connName = connectionName ?? _connectionName;
            var account = await GetAccountAsync(accountId, connName);
            if (account == null)
                throw new InvalidOperationException($"GL Account {accountId} not found.");

            // Get GL entries for this account
            var entryRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ENTRY), connName, GL_ENTRY_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = accountId }
            };

            if (asOfDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });
            }

            var entries = await entryRepo.GetAsync(filters);
            var glEntries = entries.Cast<GL_ENTRY>().ToList();

            var debitTotal = glEntries.Where(e => e.DEBIT_AMOUNT.HasValue).Sum(e => e.DEBIT_AMOUNT ?? 0m);
            var creditTotal = glEntries.Where(e => e.CREDIT_AMOUNT.HasValue).Sum(e => e.CREDIT_AMOUNT ?? 0m);

            var currentBalance = account.OPENING_BALANCE ?? 0m;
            if (account.NORMAL_BALANCE == "Debit")
            {
                currentBalance += debitTotal - creditTotal;
            }
            else
            {
                currentBalance += creditTotal - debitTotal;
            }

            return new AccountBalanceSummary
            {
                GlAccountId = account.GL_ACCOUNT_ID,
                AccountNumber = account.ACCOUNT_NUMBER,
                AccountName = account.ACCOUNT_NAME,
                OpeningBalance = account.OPENING_BALANCE,
                DebitTotal = debitTotal,
                CreditTotal = creditTotal,
                CurrentBalance = currentBalance,
                Difference = debitTotal - creditTotal,
                AsOfDate = asOfDate ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Reconciles an account.
        /// </summary>
        public async Task<AccountReconciliationResult> ReconcileAccountAsync(
            string accountId,
            DateTime reconciliationDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                throw new ArgumentException("Account ID is required.", nameof(accountId));

            var connName = connectionName ?? _connectionName;
            var balance = await GetAccountBalanceAsync(accountId, reconciliationDate, connName);

            // For now, reconciliation is just comparing book balance to itself
            // In a full implementation, this would compare to bank statements or sub-ledgers
            var isReconciled = Math.Abs(balance.Difference ?? 0m) < 0.01m;

            return new AccountReconciliationResult
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                GlAccountId = accountId,
                ReconciliationDate = reconciliationDate,
                BookBalance = balance.CurrentBalance,
                ReconciledBalance = balance.CurrentBalance,
                Difference = 0m,
                IsReconciled = isReconciled,
                Status = isReconciled ? "Reconciled" : "Pending",
                UserId = userId
            };
        }

        /// <summary>
        /// Gets accounts requiring reconciliation.
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetAccountsRequiringReconciliationAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GL_ACCOUNT), connName, GL_ACCOUNT_TABLE, null);

            // Get accounts that haven't been reconciled recently
            // This is a simplified implementation - in production, you'd track last reconciliation date
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "ACCOUNT_TYPE", Operator = "IN", FilterValue = "Asset, Liability, Equity" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<GL_ACCOUNT>().ToList();
        }
    }
}
