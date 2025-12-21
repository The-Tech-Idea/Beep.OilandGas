using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Revenue
{
    /// <summary>
    /// Represents a sales journal entry.
    /// </summary>
    public class SalesJournalEntry
    {
        /// <summary>
        /// Gets or sets the entry identifier.
        /// </summary>
        public string EntryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction reference.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry date.
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Gets or sets the account code.
        /// </summary>
        public string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the debit amount.
        /// </summary>
        public decimal DebitAmount { get; set; }

        /// <summary>
        /// Gets or sets the credit amount.
        /// </summary>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a sales journal.
    /// </summary>
    public class SalesJournal
    {
        /// <summary>
        /// Gets or sets the journal identifier.
        /// </summary>
        public string JournalId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the journal period start date.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the journal period end date.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        public List<SalesJournalEntry> Entries { get; set; } = new();

        /// <summary>
        /// Gets the total debits.
        /// </summary>
        public decimal TotalDebits => Entries.Sum(e => e.DebitAmount);

        /// <summary>
        /// Gets the total credits.
        /// </summary>
        public decimal TotalCredits => Entries.Sum(e => e.CreditAmount);

        /// <summary>
        /// Gets whether the journal is balanced.
        /// </summary>
        public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
    }

    /// <summary>
    /// Provides sales journal entry creation.
    /// </summary>
    public static class SalesJournalEntryGenerator
    {
        /// <summary>
        /// Creates journal entries for a sales transaction.
        /// </summary>
        public static List<SalesJournalEntry> CreateEntries(SalesTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var entries = new List<SalesJournalEntry>();

            // Debit: Accounts Receivable
            entries.Add(new SalesJournalEntry
            {
                EntryId = Guid.NewGuid().ToString(),
                TransactionId = transaction.TransactionId,
                EntryDate = transaction.TransactionDate,
                AccountCode = "1200",
                AccountName = "Accounts Receivable - Oil Sales",
                DebitAmount = transaction.TotalValue,
                CreditAmount = 0,
                Description = $"Oil sales to {transaction.Purchaser} - {transaction.NetVolume} bbl"
            });

            // Credit: Oil Sales Revenue
            entries.Add(new SalesJournalEntry
            {
                EntryId = Guid.NewGuid().ToString(),
                TransactionId = transaction.TransactionId,
                EntryDate = transaction.TransactionDate,
                AccountCode = "4100",
                AccountName = "Oil Sales Revenue",
                DebitAmount = 0,
                CreditAmount = transaction.TotalValue,
                Description = $"Oil sales revenue - {transaction.NetVolume} bbl @ ${transaction.PricePerBarrel}/bbl"
            });

            // Debit: Production Costs
            if (transaction.Costs.TotalCosts > 0)
            {
                entries.Add(new SalesJournalEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    TransactionId = transaction.TransactionId,
                    EntryDate = transaction.TransactionDate,
                    AccountCode = "5100",
                    AccountName = "Production Costs",
                    DebitAmount = transaction.Costs.TotalCosts,
                    CreditAmount = 0,
                    Description = "Production and marketing costs"
                });

                entries.Add(new SalesJournalEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    TransactionId = transaction.TransactionId,
                    EntryDate = transaction.TransactionDate,
                    AccountCode = "2100",
                    AccountName = "Accrued Production Costs",
                    DebitAmount = 0,
                    CreditAmount = transaction.Costs.TotalCosts,
                    Description = "Accrued production costs"
                });
            }

            // Debit: Production Taxes
            foreach (var tax in transaction.Taxes)
            {
                entries.Add(new SalesJournalEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    TransactionId = transaction.TransactionId,
                    EntryDate = transaction.TransactionDate,
                    AccountCode = "5200",
                    AccountName = $"Production Taxes - {tax.TaxType}",
                    DebitAmount = tax.Amount,
                    CreditAmount = 0,
                    Description = $"{tax.TaxType} tax - {tax.TaxAuthority}"
                });

                entries.Add(new SalesJournalEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    TransactionId = transaction.TransactionId,
                    EntryDate = transaction.TransactionDate,
                    AccountCode = "2200",
                    AccountName = "Accrued Production Taxes",
                    DebitAmount = 0,
                    CreditAmount = tax.Amount,
                    Description = $"Accrued {tax.TaxType} tax"
                });
            }

            return entries;
        }
    }
}

