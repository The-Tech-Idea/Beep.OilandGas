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
                var repo = await GetRepoAsync<GL_ACCOUNT>("GL_ACCOUNT");

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
                var repo = await GetRepoAsync<GL_ACCOUNT>("GL_ACCOUNT");

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
                var repo = await GetRepoAsync<GL_ACCOUNT>("GL_ACCOUNT");

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
        public async Task<decimal> GetAccountBalanceAsync(string accountNumber, DateTime? asOfDate = null, string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentNullException(nameof(accountNumber));

            try
            {
                var account = await GetAccountByNumberAsync(accountNumber);
                if (account == null)
                    throw new InvalidOperationException($"Account {accountNumber} not found");

                _logger?.LogInformation("Calculating balance for account {AccountNumber}", accountNumber);

                // Get posted GL entries for this account
                var repo = await GetRepoAsync<GL_ENTRY>("GL_ENTRY");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "GL_ACCOUNT_ID", Operator = "=", FilterValue = accountNumber },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                if (!string.IsNullOrWhiteSpace(bookId))
                {
                    filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });
                }

                if (asOfDate.HasValue)
                {
                    filters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });
                }

                var entries = await repo.GetAsync(filters);
                if (entries == null || entries.Count() == 0)
                    return 0m;

                decimal balance = 0m;
                foreach (var entry in entries?.Cast<GL_ENTRY>() ?? Enumerable.Empty<GL_ENTRY>())
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
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var repo = await GetRepoAsync<GL_ACCOUNT>("GL_ACCOUNT");

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
        /// <summary>
        /// Generates default GL accounts if they do not exist.
        /// </summary>
        public async Task GenerateDefaultAccountsAsync(string userId)
        {
            _logger?.LogInformation("Starting default GL account generation.");
            var defaults = GetDefaultAccountDefinitions();

            foreach (var def in defaults)
            {
                if (!await ValidateAccountAsync(def.AccountNumber))
                {
                    try
                    {
                        await CreateAccountAsync(
                            def.AccountNumber,
                            def.AccountName,
                            def.AccountType,
                            def.NormalBalance,
                            def.Description,
                            userId);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to generate default account {AccountNumber}", def.AccountNumber);
                    }
                }
            }
            _logger?.LogInformation("Finished default GL account generation.");
        }

        private List<DefaultAccountDefinition> GetDefaultAccountDefinitions()
        {
            return new List<DefaultAccountDefinition>
            {
                // Assets
                new DefaultAccountDefinition("1000", "Cash", "ASSET", "DEBIT", "Cash and Cash Equivalents"),
                new DefaultAccountDefinition("1110", "Accounts Receivable", "ASSET", "DEBIT", "Trade Receivables"),
                new DefaultAccountDefinition("1150", "Contract Asset", "ASSET", "DEBIT", "IFRS 15 Contract Assets"),
                new DefaultAccountDefinition("1151", "Contract Asset (GAAP)", "ASSET", "DEBIT", "ASC 606 Contract Assets"),
                new DefaultAccountDefinition("1160", "Deferred Tax Asset", "ASSET", "DEBIT", "IAS 12 Deferred Tax Assets"),
                new DefaultAccountDefinition("1170", "Grant Receivable", "ASSET", "DEBIT", "Government Grant Receivables"),
                new DefaultAccountDefinition("1180", "Financial Instrument Asset", "ASSET", "DEBIT", "Financial Assets at Fair Value"),
                new DefaultAccountDefinition("1185", "Retirement Plan Asset", "ASSET", "DEBIT", "Plan Assets"),
                new DefaultAccountDefinition("1188", "Reinsurance Asset", "ASSET", "DEBIT", "Reinsurance Contracts Held"),
                new DefaultAccountDefinition("1190", "Loss Allowance", "ASSET", "CREDIT", "ECL / Bad Debt Allowance (Contra Asset)"),
                new DefaultAccountDefinition("1191", "CECL Allowance", "ASSET", "CREDIT", "ASC 326 CECL Allowance"),
                new DefaultAccountDefinition("1195", "Intercompany Receivable", "ASSET", "DEBIT", "Intercompany Receivables"),
                new DefaultAccountDefinition("1200", "Fixed Assets", "ASSET", "DEBIT", "Property, Plant and Equipment"),
                new DefaultAccountDefinition("1210", "Accumulated Depreciation", "ASSET", "CREDIT", "Accumulated Depreciation (Contra Asset)"),
                new DefaultAccountDefinition("1220", "Asset Retirement Cost", "ASSET", "DEBIT", "ARC Asset"),
                new DefaultAccountDefinition("1230", "Right of Use Asset", "ASSET", "DEBIT", "IFRS 16 ROU Asset"),
                new DefaultAccountDefinition("1231", "Right of Use Asset (GAAP)", "ASSET", "DEBIT", "ASC 842 ROU Asset"),
                new DefaultAccountDefinition("1235", "Impairment Allowance", "ASSET", "CREDIT", "Impairment Allowance (Contra Asset)"),
                new DefaultAccountDefinition("1250", "Intangible Assets", "ASSET", "DEBIT", "Intangible Assets"),
                new DefaultAccountDefinition("1255", "Accumulated Amortization", "ASSET", "CREDIT", "Accumulated Amortization (Contra Asset)"),
                new DefaultAccountDefinition("1260", "Investment Property", "ASSET", "DEBIT", "IAS 40 Investment Property"),
                new DefaultAccountDefinition("1270", "Investment in Associate", "ASSET", "DEBIT", "Equity Method Investment"),
                new DefaultAccountDefinition("1275", "Investment in Joint Venture", "ASSET", "DEBIT", "Equity Method Investment"),
                new DefaultAccountDefinition("1280", "Biological Assets", "ASSET", "DEBIT", "IAS 41 Biological Assets"),
                new DefaultAccountDefinition("1290", "Goodwill", "ASSET", "DEBIT", "Business Combination Goodwill"),
                new DefaultAccountDefinition("1300", "Inventory", "ASSET", "DEBIT", "Inventory and Materials"),
                new DefaultAccountDefinition("1310", "Exploration Asset", "ASSET", "DEBIT", "IFRS 6 Exploration & Evaluation"),
                new DefaultAccountDefinition("1320", "Regulatory Deferral Asset", "ASSET", "DEBIT", "IFRS 14 Regulatory Asset"),
                new DefaultAccountDefinition("1400", "Assets Held for Sale", "ASSET", "DEBIT", "IFRS 5 Assets Held for Sale"),

                // Liabilities
                new DefaultAccountDefinition("2000", "Accounts Payable", "LIABILITY", "CREDIT", "Trade Payables"),
                new DefaultAccountDefinition("2050", "Contract Liability", "LIABILITY", "CREDIT", "IFRS 15 Contract Liabilities"),
                new DefaultAccountDefinition("2051", "Contract Liability (GAAP)", "LIABILITY", "CREDIT", "ASC 606 Contract Liabilities"),
                new DefaultAccountDefinition("2100", "Asset Retirement Obligation", "LIABILITY", "CREDIT", "ARO Liability"),
                new DefaultAccountDefinition("2101", "Income Tax Payable", "LIABILITY", "CREDIT", "Current Tax Liability"),
                new DefaultAccountDefinition("2102", "Deferred Tax Liability", "LIABILITY", "CREDIT", "IAS 12 Deferred Tax Liability"),
                new DefaultAccountDefinition("2105", "Lease Liability", "LIABILITY", "CREDIT", "IFRS 16 Lease Liability"),
                new DefaultAccountDefinition("2106", "Interest Payable", "LIABILITY", "CREDIT", "Accrued Interest"),
                new DefaultAccountDefinition("2107", "Lease Liability (GAAP)", "LIABILITY", "CREDIT", "ASC 842 Lease Liability"),
                new DefaultAccountDefinition("2110", "Employee Benefit Liability", "LIABILITY", "CREDIT", "Defined Benefit Obligation"),
                new DefaultAccountDefinition("2115", "Deferred Grant Liability", "LIABILITY", "CREDIT", "Deferred Government Grant Income"),
                new DefaultAccountDefinition("2120", "Financial Instrument Liability", "LIABILITY", "CREDIT", "Financial Liabilities at Fair Value"),
                new DefaultAccountDefinition("2130", "Insurance Contract Liability", "LIABILITY", "CREDIT", "IFRS 17 Insurance Liability"),
                new DefaultAccountDefinition("2135", "Contractual Service Margin", "LIABILITY", "CREDIT", "IFRS 17 CSM"),
                new DefaultAccountDefinition("2150", "Liabilities Held for Sale", "LIABILITY", "CREDIT", "IFRS 5 Allowed Liabilities"),
                new DefaultAccountDefinition("2160", "Regulatory Deferral Liability", "LIABILITY", "CREDIT", "IFRS 14 Regulatory Liability"),
                new DefaultAccountDefinition("2195", "Intercompany Payable", "LIABILITY", "CREDIT", "Intercompany Payables"),
                new DefaultAccountDefinition("2200", "Accrued Royalties", "LIABILITY", "CREDIT", "Royalties Payable"),

                // Equity
                new DefaultAccountDefinition("3000", "Retained Earnings", "EQUITY", "CREDIT", "Retained Earnings"),
                new DefaultAccountDefinition("3050", "Restatement Reserve", "EQUITY", "CREDIT", "IAS 29 Hyperinflation Reserve"),
                new DefaultAccountDefinition("3060", "Share-based Payment Reserve", "EQUITY", "CREDIT", "IFRS 2 Equity Reserve"),

                // Revenue
                new DefaultAccountDefinition("4001", "Revenue", "REVENUE", "CREDIT", "Sales Revenue"),
                new DefaultAccountDefinition("4100", "Insurance Revenue", "REVENUE", "CREDIT", "IFRS 17 Insurance Revenue"),
                new DefaultAccountDefinition("4200", "Foreign Exchange Gain", "REVENUE", "CREDIT", "Unrealized/Realized FX Gain"),
                new DefaultAccountDefinition("4205", "Fair Value Gain (Property)", "REVENUE", "CREDIT", "Investment Property Fair Value Gain"),
                new DefaultAccountDefinition("4210", "Financial Instrument Gain", "REVENUE", "CREDIT", "Financial Asset Fair Value Gain"),
                new DefaultAccountDefinition("4212", "Fair Value Gain", "REVENUE", "CREDIT", "General Fair Value Gain"),
                new DefaultAccountDefinition("4215", "Equity Method Earnings", "REVENUE", "CREDIT", "Share of Profit from Associates"),
                new DefaultAccountDefinition("4218", "Bargain Purchase Gain", "REVENUE", "CREDIT", "Negative Goodwill"),
                new DefaultAccountDefinition("4220", "Inflation Gain", "REVENUE", "CREDIT", "Monetary Gain (IAS 29)"),
                new DefaultAccountDefinition("4230", "Biological Asset Gain", "REVENUE", "CREDIT", "Gain on Biological Assets"),
                new DefaultAccountDefinition("4300", "Grant Income", "REVENUE", "CREDIT", "Amortization of Grant Liability"),
                new DefaultAccountDefinition("4310", "Regulatory Income", "REVENUE", "CREDIT", "Regulatory Account Income"),

                // Expenses
                new DefaultAccountDefinition("5000", "Cost of Goods Sold", "EXPENSE", "DEBIT", "COGS"),
                new DefaultAccountDefinition("6001", "Operating Expense", "EXPENSE", "DEBIT", "General Operating Expenses"),
                new DefaultAccountDefinition("6100", "Royalty Expense", "EXPENSE", "DEBIT", "Royalties"),
                new DefaultAccountDefinition("6101", "Depreciation Expense", "EXPENSE", "DEBIT", "Depreciation"),
                new DefaultAccountDefinition("6102", "Accretion Expense", "EXPENSE", "DEBIT", "ARO Accretion"),
                new DefaultAccountDefinition("6103", "Lease Interest Expense", "EXPENSE", "DEBIT", "IFRS 16 Lease Interest"),
                new DefaultAccountDefinition("6104", "Lease Amortization Expense", "EXPENSE", "DEBIT", "First-Time Adoption Adjustment"), // Correction name?
                new DefaultAccountDefinition("6105", "Amortization Expense", "EXPENSE", "DEBIT", "Intangible Amortization"),
                new DefaultAccountDefinition("6106", "Employee Benefit Expense", "EXPENSE", "DEBIT", "Salaries and Benefits"),
                new DefaultAccountDefinition("6107", "Borrowing Cost Expense", "EXPENSE", "DEBIT", "Interest Expense"),
                new DefaultAccountDefinition("6108", "Retirement Plan Expense", "EXPENSE", "DEBIT", "Pension Expense"),
                new DefaultAccountDefinition("6110", "ECL Expense", "EXPENSE", "DEBIT", "Expected Credit Loss"),
                new DefaultAccountDefinition("6111", "CECL Expense", "EXPENSE", "DEBIT", "ASC 326 Credit Loss"),
                new DefaultAccountDefinition("6120", "Insurance Service Expense", "EXPENSE", "DEBIT", "Claims and Expenses"),
                new DefaultAccountDefinition("6125", "Insurance Finance Expense", "EXPENSE", "DEBIT", "Unwind of Discount"),
                new DefaultAccountDefinition("6130", "Share-based Comp Expense", "EXPENSE", "DEBIT", "Stock Option Expense"),
                new DefaultAccountDefinition("6135", "Exploration Expense", "EXPENSE", "DEBIT", "Dry Hole / Exploration Costs"),
                new DefaultAccountDefinition("6140", "Regulatory Expense", "EXPENSE", "DEBIT", "Regulatory Account Expense"),
                new DefaultAccountDefinition("6150", "Income Tax Expense", "EXPENSE", "DEBIT", "Current and Deferred Tax"),
                new DefaultAccountDefinition("6200", "Foreign Exchange Loss", "EXPENSE", "DEBIT", "FX Loss"),
                new DefaultAccountDefinition("6205", "Fair Value Loss (Property)", "EXPENSE", "DEBIT", "Investment Property Fair Value Loss"),
                new DefaultAccountDefinition("6210", "Financial Instrument Loss", "EXPENSE", "DEBIT", "Financial Asset Fair Value Loss"),
                new DefaultAccountDefinition("6212", "Fair Value Loss", "EXPENSE", "DEBIT", "General Fair Value Loss"),
                new DefaultAccountDefinition("6215", "Equity Method Loss", "EXPENSE", "DEBIT", "Share of Loss from Associates"),
                new DefaultAccountDefinition("6220", "Inflation Loss", "EXPENSE", "DEBIT", "Monetary Loss (IAS 29)"),
                new DefaultAccountDefinition("6230", "Biological Asset Loss", "EXPENSE", "DEBIT", "Loss on Biological Assets"),
                new DefaultAccountDefinition("6300", "Impairment Loss", "EXPENSE", "DEBIT", "Asset Impairment"),
                new DefaultAccountDefinition("6310", "Held for Sale Impairment", "EXPENSE", "DEBIT", "Remeasurement Loss")
            };
        }
        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), ConnectionName, tableName);
        }
    }
}
