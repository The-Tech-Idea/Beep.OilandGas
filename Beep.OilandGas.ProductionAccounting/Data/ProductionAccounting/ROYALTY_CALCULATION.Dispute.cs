using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The dispute columns of a royalty calculation (ACC-07): whether it is disputed, the reference that blocks payment, and
    /// how it was resolved.
    /// </summary>
    /// <remarks>
    /// The owner's <c>ROYALTY_DISPUTE</c> record is keyed to <c>ROYALTY_STATEMENT</c>, a table this app never populates, so a
    /// dispute raised here would have pointed at nothing. The dispute is therefore recorded against the calculation that is
    /// actually paid, and AC-3's "payment blocked with the dispute reference" is the reference below.
    /// </remarks>
    public partial class ROYALTY_CALCULATION
    {
        /// <summary>'Y' while the calculation is in dispute; the payment command refuses it.</summary>
        [MaxLength(1)]
        public string DISPUTED_IND
        {
            get => DisputedValue;
            set => SetProperty(ref DisputedValue, value);
        }

        private string DisputedValue = "N";

        /// <summary>The reference shown when a payment is refused, e.g. DIS-2026-0001.</summary>
        [MaxLength(64)]
        public string? DISPUTE_REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

        [MaxLength(1000)]
        public string? DISPUTE_REASON
        {
            get => ReasonValue;
            set => SetProperty(ref ReasonValue, value);
        }

        private string? ReasonValue;

        [MaxLength(128)]
        public string? DISPUTED_BY
        {
            get => DisputedByValue;
            set => SetProperty(ref DisputedByValue, value);
        }

        private string? DisputedByValue;

        public DateTime? DISPUTED_AT
        {
            get => DisputedAtValue;
            set => SetProperty(ref DisputedAtValue, value);
        }

        private DateTime? DisputedAtValue;

        [MaxLength(128)]
        public string? DISPUTE_RESOLVED_BY
        {
            get => ResolvedByValue;
            set => SetProperty(ref ResolvedByValue, value);
        }

        private string? ResolvedByValue;

        public DateTime? DISPUTE_RESOLVED_AT
        {
            get => ResolvedAtValue;
            set => SetProperty(ref ResolvedAtValue, value);
        }

        private DateTime? ResolvedAtValue;

        /// <summary>Who calculated the royalty - the person who may not pay it (the pay command's segregation of duties).</summary>
        /// <summary>
        /// When the royalty was paid, who paid it, how much, and the journal that cleared the accrual.
        /// </summary>
        /// <remarks>
        /// The payment is recorded here rather than in <c>ROYALTY_PAYMENT</c> because that entity declares both <c>STATUS</c>
        /// and <c>Status</c> - two properties differing only in case - so the unit of work's case-insensitive property map
        /// throws before it writes and no payment row can be persisted through it. The defect is recorded in the evidence;
        /// the payment itself is the journal plus this state.
        /// </remarks>
        public DateTime? PAID_AT
        {
            get => PaidAtValue;
            set => SetProperty(ref PaidAtValue, value);
        }

        private DateTime? PaidAtValue;

        [MaxLength(128)]
        public string? PAID_BY
        {
            get => PaidByValue;
            set => SetProperty(ref PaidByValue, value);
        }

        private string? PaidByValue;

        public decimal? PAID_AMOUNT
        {
            get => PaidAmountValue;
            set => SetProperty(ref PaidAmountValue, value);
        }

        private decimal? PaidAmountValue;

        /// <summary>The journal that cleared the accrual into cash, kept apart from the accrual entry.</summary>
        [MaxLength(64)]
        public string? PAYMENT_JOURNAL_ENTRY_ID
        {
            get => PaymentJournalValue;
            set => SetProperty(ref PaymentJournalValue, value);
        }

        private string? PaymentJournalValue;

        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        [MaxLength(16)]
        public string? CURRENCY_CODE
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;
    }
}
