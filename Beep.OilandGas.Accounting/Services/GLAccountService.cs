using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Manages GL Account master data and account balance calculations
    /// </summary>
    public class GLAccountService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<GLAccountService> _logger;
        private const string ConnectionName = "PPDM39";

        public GLAccountService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<GLAccountService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get GL account by account number
        /// </summary>
        public async Task<GL_ACCOUNT?> GetAccountByNumberAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentNullException(nameof(accountNumber));

            _logger?.LogInformation("Getting GL account for number: {AccountNumber}", accountNumber);

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("GL_ACCOUNT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(GL_ACCOUNT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "GL_ACCOUNT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACCOUNT_NUMBER", Operator = "=", FilterValue = accountNumber },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var accounts = await repo.GetAsync(filters);
                return accounts?.FirstOrDefault() as GL_ACCOUNT;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account {AccountNumber}: {Message}", accountNumber, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all active GL accounts
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetAllAccountsAsync()
        {
            _logger?.LogInformation("Getting all GL accounts");

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("GL_ACCOUNT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(GL_ACCOUNT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "GL_ACCOUNT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var accounts = await repo.GetAsync(filters);
                return accounts?.Cast<GL_ACCOUNT>().ToList() ?? new List<GL_ACCOUNT>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all accounts: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get accounts filtered by type (ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE)
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetAccountsByTypeAsync(string accountType)
        {
            if (string.IsNullOrWhiteSpace(accountType))
                throw new ArgumentNullException(nameof(accountType));

            _logger?.LogInformation("Getting GL accounts for type: {AccountType}", accountType);

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("GL_ACCOUNT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(GL_ACCOUNT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "GL_ACCOUNT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACCOUNT_TYPE", Operator = "=", FilterValue = accountType },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var accounts = await repo.GetAsync(filters);
                return accounts?.Cast<GL_ACCOUNT>().ToList() ?? new List<GL_ACCOUNT>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting accounts by type {AccountType}: {Message}", accountType, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate account balance from GL entries
        /// Respects NORMAL_BALANCE direction for debit/credit calculation
        /// </summary>
        public async Task<decimal> GetAccountBalanceAsync(string accountNumber, DateTime? asOfDate = null)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentNullException(nameof(accountNumber));

            try
            {
                var account = await GetAccountByNumberAsync(accountNumber);
                if (account == null)
                    throw new InvalidOperationException($"Account {accountNumber} not found");

                _logger?.LogInformation("Calculating balance for account {AccountNumber}", accountNumber);

                // Get GL entries for this account
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY_LINE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY_LINE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY_LINE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "GL_ACCOUNT_NUMBER", Operator = "=", FilterValue = accountNumber }
                };

                if (asOfDate.HasValue)
                {
                    filters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });
                }

                var entries = await repo.GetAsync(filters);
                if (entries == null || entries.Count() == 0)
                    return 0m;

                decimal balance = 0m;
                foreach (var entry in entries?.Cast<JOURNAL_ENTRY_LINE>() ?? Enumerable.Empty<JOURNAL_ENTRY_LINE>())
                {
                    decimal debit = entry.DEBIT_AMOUNT ?? 0m;
                    decimal credit = entry.CREDIT_AMOUNT ?? 0m;

                    // For debit-normal accounts (ASSET, EXPENSE)
                    if (account.NORMAL_BALANCE == "DEBIT")
                    {
                        balance += debit - credit;
                    }
                    // For credit-normal accounts (LIABILITY, EQUITY, REVENUE)
                    else if (account.NORMAL_BALANCE == "CREDIT")
                    {
                        balance += credit - debit;
                    }
                }

                _logger?.LogInformation("Account {AccountNumber} balance: {Balance}", accountNumber, balance);
                return balance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating balance for {AccountNumber}: {Message}", accountNumber, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Create a new GL account
        /// </summary>
        public async Task<GL_ACCOUNT> CreateAccountAsync(
            string accountNumber,
            string accountName,
            string accountType,
            string normalBalance,
            string description,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentNullException(nameof(accountNumber));
            if (string.IsNullOrWhiteSpace(accountName))
                throw new ArgumentNullException(nameof(accountName));
            if (string.IsNullOrWhiteSpace(accountType))
                throw new ArgumentNullException(nameof(accountType));
            if (string.IsNullOrWhiteSpace(normalBalance))
                throw new ArgumentNullException(nameof(normalBalance));

            _logger?.LogInformation("Creating GL account {AccountNumber}", accountNumber);

            try
            {
                // Check if account already exists
                var existing = await GetAccountByNumberAsync(accountNumber);
                if (existing != null)
                    throw new InvalidOperationException($"Account {accountNumber} already exists");

                var account = new GL_ACCOUNT
                {
                    GL_ACCOUNT_ID = Guid.NewGuid().ToString(),
                    ACCOUNT_NUMBER = accountNumber,
                    ACCOUNT_NAME = accountName,
                    ACCOUNT_TYPE = accountType,
                    NORMAL_BALANCE = normalBalance,
                    DESCRIPTION = description,
                    OPENING_BALANCE = 0m,
                    CURRENT_BALANCE = 0m,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var metadata = await _metadata.GetTableMetadataAsync("GL_ACCOUNT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(GL_ACCOUNT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "GL_ACCOUNT");

                await repo.InsertAsync(account, userId);
                _logger?.LogInformation("GL account {AccountNumber} created", accountNumber);
                return account;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating account {AccountNumber}: {Message}", accountNumber, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validate GL account exists and is active
        /// </summary>
        public async Task<bool> ValidateAccountAsync(string accountNumber)
        {
            try
            {
                var account = await GetAccountByNumberAsync(accountNumber);
                return account != null && account.ACTIVE_IND == "Y";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate account type matches expected type
        /// </summary>
        public async Task<bool> ValidateAccountTypeAsync(string accountNumber, string expectedType)
        {
            try
            {
                var account = await GetAccountByNumberAsync(accountNumber);
                return account != null && account.ACCOUNT_TYPE == expectedType;
            }
            catch
            {
                return false;
            }
        }
    }
}
