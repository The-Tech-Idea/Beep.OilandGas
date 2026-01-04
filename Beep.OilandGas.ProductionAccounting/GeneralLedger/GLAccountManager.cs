
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.GeneralLedger
{
    /// <summary>
    /// Manages General Ledger accounts (Chart of Accounts).
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
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
                DESCRIPTION = request.Description
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(account, userId);
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
    }
}
