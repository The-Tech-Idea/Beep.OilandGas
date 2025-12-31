
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Maps operations to GL accounts based on operation type and configuration.
    /// </summary>
    public class GLAccountMappingService
    {
        private readonly GLAccountManager _glAccountManager;
        private readonly ILogger<GLAccountMappingService>? _logger;
        private readonly Dictionary<string, string> _accountMappings;

        public GLAccountMappingService(GLAccountManager glAccountManager, ILogger<GLAccountMappingService>? logger = null)
        {
            _glAccountManager = glAccountManager ?? throw new ArgumentNullException(nameof(glAccountManager));
            _logger = logger;
            _accountMappings = InitializeAccountMappings();
        }

        /// <summary>
        /// Gets the GL account ID for a given operation type.
        /// </summary>
        public string? GetAccountId(string operationType, string? accountNumber = null)
        {
            // If account number provided, try to find by account number first
            if (!string.IsNullOrEmpty(accountNumber))
            {

                var account = _glAccountManager.GetAllAccounts()

                    .FirstOrDefault(a => a.ACCOUNT_NUMBER == accountNumber);

                if (account != null)

                    return account.GL_ACCOUNT_ID;
            }

            // Otherwise, use mapping
            if (_accountMappings.TryGetValue(operationType, out var mappedAccountNumber))
            {

                var account = _glAccountManager.GetAllAccounts()

                    .FirstOrDefault(a => a.ACCOUNT_NUMBER == mappedAccountNumber);

                if (account != null)

                    return account.GL_ACCOUNT_ID;
            }

            _logger?.LogWarning("No GL account mapping found for operation type: {OperationType}", operationType);
            return null;
        }

        /// <summary>
        /// Validates that a GL account exists and is active.
        /// </summary>
        public bool ValidateAccount(string glAccountId)
        {
            var account = _glAccountManager.GetAccount(glAccountId);
            if (account == null)
            {

                _logger?.LogWarning("GL account not found: {GlAccountId}", glAccountId);

                return false;
            }

            if (account.ACTIVE_IND != "Y")
            {

                _logger?.LogWarning("GL account is not active: {GlAccountId}", glAccountId);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes default account mappings for common operation types.
        /// In production, these would come from configuration or database.
        /// </summary>
        private Dictionary<string, string> InitializeAccountMappings()
        {
            return new Dictionary<string, string>
            {

                // Production Revenue

                { "ProductionRevenue", "4000" },

                { "ProductionRevenue_AR", "1200" }, // Accounts Receivable

                { "ProductionRevenue_Cash", "1000" }, // Cash


                // Cost Operations

                { "OperatingExpense", "5000" },

                { "CapitalizedCost", "1500" },

                { "Cost_AP", "2000" }, // Accounts Payable

                { "Cost_Cash", "1000" }, // Cash


                // Royalty

                { "RoyaltyExpense", "5100" },

                { "Royalty_Cash", "1000" }, // Cash


                // Financial Accounting

                { "ExplorationExpense", "5200" },

                { "UnprovedProperty", "1600" },

                { "ProvedProperty", "1700" },

                { "DevelopmentCost", "1800" },

                { "AmortizationExpense", "5300" },

                { "ImpairmentExpense", "5400" },


                // Traditional Accounting

                { "AR_Invoice", "1200" }, // Accounts Receivable

                { "AR_Revenue", "4000" }, // Revenue

                { "AP_Invoice", "2000" }, // Accounts Payable

                { "AP_Expense", "5000" }, // Expense

                { "Inventory", "1300" },

                { "Inventory_CostOfGoodsSold", "5100" },


                // Default accounts

                { "Cash", "1000" },

                { "AccountsReceivable", "1200" },

                { "AccountsPayable", "2000" },

                { "Revenue", "4000" },

                { "Expense", "5000" }
            };
        }
    }
}
