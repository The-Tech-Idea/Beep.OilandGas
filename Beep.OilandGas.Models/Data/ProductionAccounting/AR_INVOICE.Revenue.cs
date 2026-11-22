using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The revenue columns of a sales invoice (ACC-03): the author, the money split, the tax rate applied, the contract,
    /// the journal that issuing produced, and the review.
    /// </summary>
    /// <remarks>
    /// The AR invoice already held the total, the paid amount and the balance due, which is most of what a receivable needs.
    /// It lacked the author (AC-4 needs to know who may not approve), the subtotal and tax split (AC-5), the contract
    /// (the currency-mismatch check), and the journal link that makes "issuing creates exactly one entry" and "a lost
    /// acknowledgement does not duplicate it" provable (AC-2, AC-6). These columns are additive.
    /// </remarks>
    public partial class AR_INVOICE
    {
        /// <summary>Who built the invoice - the person who may not approve it (AC-4).</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        /// <summary>The lines' net total, before tax.</summary>
        public decimal? SUBTOTAL_AMOUNT
        {
            get => SubtotalValue;
            set => SetProperty(ref SubtotalValue, value);
        }

        private decimal? SubtotalValue;

        /// <summary>The tax the owning service computed for the whole invoice (AC-5).</summary>
        public decimal? TAX_AMOUNT
        {
            get => TaxValue;
            set => SetProperty(ref TaxValue, value);
        }

        private decimal? TaxValue;

        /// <summary>The rate that produced the tax, stored so the invoice says how it was taxed.</summary>
        public decimal? TAX_RATE
        {
            get => TaxRateValue;
            set => SetProperty(ref TaxRateValue, value);
        }

        private decimal? TaxRateValue;

        /// <summary>The sales contract the invoice was raised under.</summary>
        [MaxLength(128)]
        public string? CONTRACT_ID
        {
            get => ContractValue;
            set => SetProperty(ref ContractValue, value);
        }

        private string? ContractValue;

        /// <summary>
        /// The ledger entry issuing produced. AC-2: there is at most one. AC-6: after a lost acknowledgement the retry
        /// finds this and returns the same journal instead of writing another.
        /// </summary>
        [MaxLength(128)]
        public string? JOURNAL_ENTRY_ID
        {
            get => JournalValue;
            set => SetProperty(ref JournalValue, value);
        }

        private string? JournalValue;

        [MaxLength(128)]
        public string? ISSUED_BY
        {
            get => IssuedByValue;
            set => SetProperty(ref IssuedByValue, value);
        }

        private string? IssuedByValue;

        public DateTime? ISSUED_AT
        {
            get => IssuedAtValue;
            set => SetProperty(ref IssuedAtValue, value);
        }

        private DateTime? IssuedAtValue;

        /// <summary>The credits issued against this invoice; the balance is the total less paid less this.</summary>
        public decimal? CREDITED_AMOUNT
        {
            get => CreditedValue;
            set => SetProperty(ref CreditedValue, value);
        }

        private decimal? CreditedValue;

        [MaxLength(128)]
        public string? APPROVED_BY
        {
            get => ApprovedByValue;
            set => SetProperty(ref ApprovedByValue, value);
        }

        private string? ApprovedByValue;

        public DateTime? APPROVED_AT
        {
            get => ApprovedAtValue;
            set => SetProperty(ref ApprovedAtValue, value);
        }

        private DateTime? ApprovedAtValue;

        /// <summary>Why a reviewer returned it, shown on the invoice.</summary>
        [MaxLength(1000)]
        public string? RETURN_REASON
        {
            get => ReturnValue;
            set => SetProperty(ref ReturnValue, value);
        }

        private string? ReturnValue;
    }
}
