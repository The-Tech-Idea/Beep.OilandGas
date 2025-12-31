
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Manages General Ledger accounts (Chart of Accounts).
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class GLAccountManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<GLAccountManager>? _logger;
        private readonly string _connectionName;
        private const string GL_ACCOUNT_TABLE = "GL_ACCOUNT";

        public GLAccountManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<GLAccountManager>? logger = null,
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
        public GL_ACCOUNT CreateAccount(CreateGLAccountRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.AccountNumber))
                throw new ArgumentException("Account number is required.", nameof(request));

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

            // Save to database using IDataSource
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(GL_ACCOUNT_TABLE, account);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create GL account {AccountNumber}: {Error}", request.AccountNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save GL account: {errorMessage}");
            }

            _logger?.LogDebug("Created GL account {AccountNumber} in database", request.AccountNumber);
            return account;
        }

        /// <summary>
        /// Gets a GL account by ID.
        /// </summary>
        public GL_ACCOUNT? GetAccount(string accountId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = accountId }
            };

            var results = dataSource.GetEntityAsync(GL_ACCOUNT_TABLE, filters).GetAwaiter().GetResult();
            var accountData = results?.FirstOrDefault();
            
            if (accountData == null)
                return null;

            return accountData as GL_ACCOUNT;
        }

        /// <summary>
        /// Gets all GL accounts.
        /// </summary>
        public IEnumerable<GL_ACCOUNT> GetAllAccounts(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = dataSource.GetEntityAsync(GL_ACCOUNT_TABLE, null).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<GL_ACCOUNT>();

            return results.Cast<GL_ACCOUNT>().Where(a => a != null)!;
        }

        /// <summary>
        /// Gets accounts by type.
        /// </summary>
        public IEnumerable<GL_ACCOUNT> GetAccountsByType(string accountType, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(accountType))
                return Enumerable.Empty<GL_ACCOUNT>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACCOUNT_TYPE", Operator = "=", FilterValue = accountType }
            };

            var results = dataSource.GetEntityAsync(GL_ACCOUNT_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<GL_ACCOUNT>();

            return results.Cast<GL_ACCOUNT>().Where(a => a != null)!;
        }

        /// <summary>
        /// Converts GL_ACCOUNT entity to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertGLAccountToDictionary(GL_ACCOUNT account)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(account.GL_ACCOUNT_ID)) dict["GL_ACCOUNT_ID"] = account.GL_ACCOUNT_ID;
            if (!string.IsNullOrEmpty(account.ACCOUNT_NUMBER)) dict["ACCOUNT_NUMBER"] = account.ACCOUNT_NUMBER;
            if (!string.IsNullOrEmpty(account.ACCOUNT_NAME)) dict["ACCOUNT_NAME"] = account.ACCOUNT_NAME;
            if (!string.IsNullOrEmpty(account.ACCOUNT_TYPE)) dict["ACCOUNT_TYPE"] = account.ACCOUNT_TYPE;
            if (!string.IsNullOrEmpty(account.PARENT_ACCOUNT_ID)) dict["PARENT_ACCOUNT_ID"] = account.PARENT_ACCOUNT_ID;
            if (!string.IsNullOrEmpty(account.NORMAL_BALANCE)) dict["NORMAL_BALANCE"] = account.NORMAL_BALANCE;
            if (account.OPENING_BALANCE.HasValue) dict["OPENING_BALANCE"] = account.OPENING_BALANCE.Value;
            if (account.CURRENT_BALANCE.HasValue) dict["CURRENT_BALANCE"] = account.CURRENT_BALANCE.Value;
            if (!string.IsNullOrEmpty(account.DESCRIPTION)) dict["DESCRIPTION"] = account.DESCRIPTION;
            if (!string.IsNullOrEmpty(account.ACTIVE_IND)) dict["ACTIVE_IND"] = account.ACTIVE_IND;
            if (!string.IsNullOrEmpty(account.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = account.ROW_CREATED_BY;
            if (account.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = account.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(account.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = account.ROW_CHANGED_BY;
            if (account.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = account.ROW_CHANGED_DATE.Value;
            return dict;
        }

        /// <summary>
        /// Converts dictionary to GL_ACCOUNT entity.
        /// </summary>
        private GL_ACCOUNT ConvertDictionaryToGLAccount(Dictionary<string, object> dict)
        {
            var account = new GL_ACCOUNT();
            if (dict.TryGetValue("GL_ACCOUNT_ID", out var accountId)) account.GL_ACCOUNT_ID = accountId?.ToString();
            if (dict.TryGetValue("ACCOUNT_NUMBER", out var accountNumber)) account.ACCOUNT_NUMBER = accountNumber?.ToString();
            if (dict.TryGetValue("ACCOUNT_NAME", out var accountName)) account.ACCOUNT_NAME = accountName?.ToString();
            if (dict.TryGetValue("ACCOUNT_TYPE", out var accountType)) account.ACCOUNT_TYPE = accountType?.ToString();
            if (dict.TryGetValue("PARENT_ACCOUNT_ID", out var parentId)) account.PARENT_ACCOUNT_ID = parentId?.ToString();
            if (dict.TryGetValue("NORMAL_BALANCE", out var normalBalance)) account.NORMAL_BALANCE = normalBalance?.ToString();
            if (dict.TryGetValue("OPENING_BALANCE", out var openingBalance)) account.OPENING_BALANCE = openingBalance != null ? Convert.ToDecimal(openingBalance) : (decimal?)null;
            if (dict.TryGetValue("CURRENT_BALANCE", out var currentBalance)) account.CURRENT_BALANCE = currentBalance != null ? Convert.ToDecimal(currentBalance) : (decimal?)null;
            if (dict.TryGetValue("DESCRIPTION", out var description)) account.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) account.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) account.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) account.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) account.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) account.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return account;
        }
    }
}
