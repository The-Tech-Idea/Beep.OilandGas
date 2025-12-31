
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Service to handle automatic GL posting for all accounting operations.
    /// Enforces double-entry bookkeeping (debit = credit validation).
    /// </summary>
    public class GLIntegrationService
    {
        private readonly JournalEntryManager _journalEntryManager;
        private readonly GLAccountMappingService _accountMapping;
        private readonly ILogger<GLIntegrationService>? _logger;
        private static int _entryNumberCounter = 0;

        public GLIntegrationService(
            JournalEntryManager journalEntryManager,
            GLAccountMappingService accountMapping,
            ILogger<GLIntegrationService>? logger = null)
        {
            _journalEntryManager = journalEntryManager ?? throw new ArgumentNullException(nameof(journalEntryManager));
            _accountMapping = accountMapping ?? throw new ArgumentNullException(nameof(accountMapping));
            _logger = logger;
        }

        /// <summary>
        /// Posts production revenue to GL (Run Ticket -> Revenue).
        /// </summary>
        public async Task<string> PostProductionToGL(
            string runTicketNumber,
            decimal revenueAmount,
            string? arAccountId = null,
            string? cashAccountId = null,
            bool isCash = false,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                var revenueAccountId = _accountMapping.GetAccountId("ProductionRevenue");

                if (string.IsNullOrEmpty(revenueAccountId))

                    throw new GLPostingException("Production revenue GL account not found", runTicketNumber, "Production");


                var assetAccountId = isCash 

                    ? (cashAccountId ?? _accountMapping.GetAccountId("ProductionRevenue_Cash"))

                    : (arAccountId ?? _accountMapping.GetAccountId("ProductionRevenue_AR"));


                if (string.IsNullOrEmpty(assetAccountId))

                    throw new GLPostingException("Asset GL account (AR/Cash) not found", runTicketNumber, "Production");


                var lines = new List<JournalEntryLineData>

                {

                    new JournalEntryLineData

                    {

                        GlAccountId = assetAccountId,

                        DebitAmount = revenueAmount,

                        CreditAmount = null,

                        Description = $"Production Revenue - Run Ticket {runTicketNumber}"

                    },

                    new JournalEntryLineData

                    {

                        GlAccountId = revenueAccountId,

                        DebitAmount = null,

                        CreditAmount = revenueAmount,

                        Description = $"Production Revenue - Run Ticket {runTicketNumber}"

                    }

                };


                return await PostToGL("Production", runTicketNumber, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post production to GL: {ex.Message}", runTicketNumber, "Production", ex);
            }
        }

        /// <summary>
        /// Posts revenue transaction to GL.
        /// </summary>
        public async Task<string> PostRevenueToGL(
            string transactionId,
            decimal revenueAmount,
            string? arAccountId = null,
            string? cashAccountId = null,
            bool isCash = false,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                var revenueAccountId = _accountMapping.GetAccountId("Revenue");

                if (string.IsNullOrEmpty(revenueAccountId))

                    throw new GLPostingException("Revenue GL account not found", transactionId, "Revenue");


                var assetAccountId = isCash

                    ? (cashAccountId ?? _accountMapping.GetAccountId("Cash"))

                    : (arAccountId ?? _accountMapping.GetAccountId("AccountsReceivable"));


                if (string.IsNullOrEmpty(assetAccountId))

                    throw new GLPostingException("Asset GL account (AR/Cash) not found", transactionId, "Revenue");


                var lines = new List<JournalEntryLineData>

                {

                    new JournalEntryLineData

                    {

                        GlAccountId = assetAccountId,

                        DebitAmount = revenueAmount,

                        CreditAmount = null,

                        Description = $"Revenue Transaction {transactionId}"

                    },

                    new JournalEntryLineData

                    {

                        GlAccountId = revenueAccountId,

                        DebitAmount = null,

                        CreditAmount = revenueAmount,

                        Description = $"Revenue Transaction {transactionId}"

                    }

                };


                return await PostToGL("Revenue", transactionId, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post revenue to GL: {ex.Message}", transactionId, "Revenue", ex);
            }
        }

        /// <summary>
        /// Posts cost transaction to GL.
        /// </summary>
        public async Task<string> PostCostToGL(
            string costTransactionId,
            decimal costAmount,
            bool isCapitalized = false,
            string? apAccountId = null,
            string? cashAccountId = null,
            bool isCash = false,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                var expenseAccountId = isCapitalized

                    ? _accountMapping.GetAccountId("CapitalizedCost")

                    : _accountMapping.GetAccountId("OperatingExpense");


                if (string.IsNullOrEmpty(expenseAccountId))

                    throw new GLPostingException("Expense/Capitalized Cost GL account not found", costTransactionId, "Cost");


                var liabilityAccountId = isCash

                    ? (cashAccountId ?? _accountMapping.GetAccountId("Cost_Cash"))

                    : (apAccountId ?? _accountMapping.GetAccountId("Cost_AP"));


                if (string.IsNullOrEmpty(liabilityAccountId))

                    throw new GLPostingException("Liability GL account (AP/Cash) not found", costTransactionId, "Cost");


                var lines = new List<JournalEntryLineData>

                {

                    new JournalEntryLineData

                    {

                        GlAccountId = expenseAccountId,

                        DebitAmount = costAmount,

                        CreditAmount = null,

                        Description = $"Cost Transaction {costTransactionId}"

                    },

                    new JournalEntryLineData

                    {

                        GlAccountId = liabilityAccountId,

                        DebitAmount = null,

                        CreditAmount = costAmount,

                        Description = $"Cost Transaction {costTransactionId}"

                    }

                };


                return await PostToGL("Cost", costTransactionId, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post cost to GL: {ex.Message}", costTransactionId, "Cost", ex);
            }
        }

        /// <summary>
        /// Posts royalty payment to GL.
        /// </summary>
        public async Task<string> PostRoyaltyToGL(
            string royaltyPaymentId,
            decimal royaltyAmount,
            string? cashAccountId = null,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                var royaltyExpenseAccountId = _accountMapping.GetAccountId("RoyaltyExpense");

                if (string.IsNullOrEmpty(royaltyExpenseAccountId))

                    throw new GLPostingException("Royalty expense GL account not found", royaltyPaymentId, "Royalty");


                var cashAccount = cashAccountId ?? _accountMapping.GetAccountId("Royalty_Cash");

                if (string.IsNullOrEmpty(cashAccount))

                    throw new GLPostingException("Cash GL account not found", royaltyPaymentId, "Royalty");


                var lines = new List<JournalEntryLineData>

                {

                    new JournalEntryLineData

                    {

                        GlAccountId = royaltyExpenseAccountId,

                        DebitAmount = royaltyAmount,

                        CreditAmount = null,

                        Description = $"Royalty Payment {royaltyPaymentId}"

                    },

                    new JournalEntryLineData

                    {

                        GlAccountId = cashAccount,

                        DebitAmount = null,

                        CreditAmount = royaltyAmount,

                        Description = $"Royalty Payment {royaltyPaymentId}"

                    }

                };


                return await PostToGL("Royalty", royaltyPaymentId, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post royalty to GL: {ex.Message}", royaltyPaymentId, "Royalty", ex);
            }
        }

        /// <summary>
        /// Posts financial accounting entry to GL (Successful Efforts, Full Cost, etc.).
        /// </summary>
        public async Task<string> PostFinancialAccountingToGL(
            string transactionId,
            string entryType, // "ExplorationExpense", "UnprovedProperty", "DevelopmentCost", "AmortizationExpense", "ImpairmentExpense"
            decimal amount,
            string? offsetAccountId = null,
            bool isCash = false,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                var expenseAccountId = _accountMapping.GetAccountId(entryType);

                if (string.IsNullOrEmpty(expenseAccountId))

                    throw new GLPostingException($"GL account not found for entry type: {entryType}", transactionId, "FinancialAccounting");


                var offsetAccount = offsetAccountId ?? (isCash 

                    ? _accountMapping.GetAccountId("Cash")

                    : _accountMapping.GetAccountId("AccountsPayable"));


                if (string.IsNullOrEmpty(offsetAccount))

                    throw new GLPostingException("Offset GL account (AP/Cash) not found", transactionId, "FinancialAccounting");


                var lines = new List<JournalEntryLineData>

                {

                    new JournalEntryLineData

                    {

                        GlAccountId = expenseAccountId,

                        DebitAmount = amount,

                        CreditAmount = null,

                        Description = $"{entryType} - Transaction {transactionId}"

                    },

                    new JournalEntryLineData

                    {

                        GlAccountId = offsetAccount,

                        DebitAmount = null,

                        CreditAmount = amount,

                        Description = $"{entryType} - Transaction {transactionId}"

                    }

                };


                return await PostToGL("FinancialAccounting", transactionId, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post financial accounting to GL: {ex.Message}", transactionId, "FinancialAccounting", ex);
            }
        }

        /// <summary>
        /// Posts traditional accounting entry to GL (Invoice, PO, AP, AR, Inventory).
        /// </summary>
        public async Task<string> PostTraditionalAccountingToGL(
            string transactionId,
            string entryType, // "AR_Invoice", "AP_Invoice", "Inventory", etc.
            List<JournalEntryLineData> lines,
            DateTime? transactionDate = null,
            string userId = "system")
        {
            try
            {

                // Validate all accounts exist

                foreach (var line in lines)

                {

                    if (!_accountMapping.ValidateAccount(line.GlAccountId))

                    {

                        throw new GLPostingException($"Invalid GL account: {line.GlAccountId}", transactionId, "TraditionalAccounting", line.GlAccountId);

                    }

                }


                return await PostToGL("TraditionalAccounting", transactionId, lines, transactionDate ?? DateTime.UtcNow, userId);
            }
            catch (Exception ex) when (!(ex is GLPostingException))
            {

                throw new GLPostingException($"Failed to post traditional accounting to GL: {ex.Message}", transactionId, "TraditionalAccounting", ex);
            }
        }

        /// <summary>
        /// Core method to post journal entry to GL with double-entry validation.
        /// </summary>
        private async Task<string> PostToGL(
            string sourceModule,
            string referenceNumber,
            List<JournalEntryLineData> lines,
            DateTime entryDate,
            string userId)
        {
            // Validate double-entry: total debits must equal total credits
            decimal totalDebits = lines.Sum(l => l.DebitAmount ?? 0m);
            decimal totalCredits = lines.Sum(l => l.CreditAmount ?? 0m);

            if (Math.Abs(totalDebits - totalCredits) > 0.01m)
            {

                throw new GLPostingException(

                    $"Journal entry is not balanced. Debits: {totalDebits}, Credits: {totalCredits}",

                    referenceNumber,

                    sourceModule);
            }

            // Validate all accounts exist and are active
            foreach (var line in lines)
            {

                if (!_accountMapping.ValidateAccount(line.GlAccountId))

                {

                    throw new GLPostingException(

                        $"Invalid or inactive GL account: {line.GlAccountId}",

                        referenceNumber,

                        sourceModule,

                        line.GlAccountId);

                }
            }

            // Generate entry number
            var entryNumber = $"JE-{DateTime.UtcNow:yyyyMMdd}-{++_entryNumberCounter:D6}";

            // Create journal entry
            var journalEntry = _journalEntryManager.CreateJournalEntry(

                entryNumber,

                entryDate,

                sourceModule,

                $"Auto-posted from {sourceModule} - {referenceNumber}",

                lines,

                userId);

            // Post the journal entry
            _journalEntryManager.PostJournalEntry(journalEntry.JOURNAL_ENTRY_ID, userId);

            _logger?.LogInformation(

                "Posted journal entry {EntryId} to GL for {SourceModule} transaction {ReferenceNumber}. Debits: {Debits}, Credits: {Credits}",

                journalEntry.JOURNAL_ENTRY_ID,

                sourceModule,

                referenceNumber,

                totalDebits,

                totalCredits);

            return journalEntry.JOURNAL_ENTRY_ID;
        }
    }
}
