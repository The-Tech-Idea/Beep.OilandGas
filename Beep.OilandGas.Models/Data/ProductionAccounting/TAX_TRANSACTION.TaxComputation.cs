using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The computation columns of a tax transaction (ACC-08): the rate that was applied and where it came from, the basis it
    /// applied to, the period, the ledger entry, and who computed and posted it.
    /// </summary>
    /// <remarks>
    /// <c>TAX_TRANSACTION</c> already held the kind, the date, the amount and the jurisdiction. It had no rate and no basis, so
    /// AC-1's "a computation names its jurisdiction, rate and effective date" could not be satisfied and nobody could check
    /// the arithmetic from the row; and no journal link, so AC-4's "exactly one entry per computation" could not be traced.
    /// These columns are additive.
    /// </remarks>
    public partial class TAX_TRANSACTION
    {
        /// <summary>The rate as a fraction (12.5 % is 0.125), normalised the owner's way.</summary>
        public decimal? TAX_RATE
        {
            get => RateValue;
            set => SetProperty(ref RateValue, value);
        }

        private decimal? RateValue;

        /// <summary>What the rate was applied to - gross revenue for a production tax, taxable income for a provision.</summary>
        public decimal? TAX_BASIS
        {
            get => BasisValue;
            set => SetProperty(ref BasisValue, value);
        }

        private decimal? BasisValue;

        /// <summary>What the basis is, in words, so the number is never bare.</summary>
        [MaxLength(64)]
        public string? BASIS_TEXT
        {
            get => BasisTextView;
            set => SetProperty(ref BasisTextView, value);
        }

        private string? BasisTextView;

        /// <summary>The date the rate took effect - AC-1's effective date.</summary>
        public DateTime? RATE_EFFECTIVE_DATE
        {
            get => EffectiveValue;
            set => SetProperty(ref EffectiveValue, value);
        }

        private DateTime? EffectiveValue;

        /// <summary>Where the rate came from, so a reviewer can find the record it was read from.</summary>
        [MaxLength(256)]
        public string? RATE_SOURCE
        {
            get => SourceValue;
            set => SetProperty(ref SourceValue, value);
        }

        private string? SourceValue;

        public DateTime? PERIOD_FROM
        {
            get => FromValue;
            set => SetProperty(ref FromValue, value);
        }

        private DateTime? FromValue;

        public DateTime? PERIOD_TO
        {
            get => ToValue;
            set => SetProperty(ref ToValue, value);
        }

        private DateTime? ToValue;

        /// <summary>The fiscal year the computation belongs to, as <c>yyyy</c>.</summary>
        [MaxLength(8)]
        public string? FISCAL_YEAR
        {
            get => YearValue;
            set => SetProperty(ref YearValue, value);
        }

        private string? YearValue;

        [MaxLength(16)]
        public string? CURRENCY_CODE

        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        /// <summary>COMPUTED or POSTED - from TaxComputationCodes.</summary>
        [MaxLength(16)]
        public string? STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string? StatusValue = "COMPUTED";

        /// <summary>Who computed it - the person who may not post it (AC-6).</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        /// <summary>
        /// The ledger entry the posting produced (AC-4). At most one per computation, and a retry adopts it rather than
        /// writing a second.
        /// </summary>
        [MaxLength(64)]
        public string? JOURNAL_ENTRY_ID
        {
            get => JournalValue;
            set => SetProperty(ref JournalValue, value);
        }

        private string? JournalValue;

        [MaxLength(128)]
        public string? POSTED_BY
        {
            get => PostedByValue;
            set => SetProperty(ref PostedByValue, value);
        }

        private string? PostedByValue;

        public DateTime? POSTED_AT
        {
            get => PostedAtValue;
            set => SetProperty(ref PostedAtValue, value);
        }

        private DateTime? PostedAtValue;
    }
}
