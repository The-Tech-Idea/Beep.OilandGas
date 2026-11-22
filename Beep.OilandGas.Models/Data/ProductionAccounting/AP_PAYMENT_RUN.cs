using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// A payment run (ACC-05): one decision that settles several due invoices, and the single journal it posts.
    /// </summary>
    /// <remarks>
    /// <c>AP_PAYMENT</c> is one amount against one invoice, so a run that settles six invoices had no single record and
    /// AC-5's "payments create exactly one journal entry" had nowhere to put that one entry. This is the header; each
    /// <c>AP_PAYMENT</c> row carries its <c>RUN_ID</c>. The header also holds the approval, because AC-6 says the person who
    /// proposed the run cannot approve it.
    /// </remarks>
    public class AP_PAYMENT_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string AP_PAYMENT_RUN_ID
        {
            get => RunIdValue;
            set => SetProperty(ref RunIdValue, value);
        }

        private string RunIdValue = string.Empty;

        /// <summary>The number the run is given when it is posted, server-owned.</summary>
        [MaxLength(64)]
        public string? RUN_NUMBER
        {
            get => NumberValue;
            set => SetProperty(ref NumberValue, value);
        }

        private string? NumberValue;

        public DateTime RUN_DATE
        {
            get => DateValue;
            set => SetProperty(ref DateValue, value);
        }

        private DateTime DateValue;

        /// <summary>What the run pays in total, which is the sum of its invoices.</summary>
        public decimal TOTAL_AMOUNT
        {
            get => TotalValue;
            set => SetProperty(ref TotalValue, value);
        }

        private decimal TotalValue;

        [MaxLength(16)]
        public string CURRENCY_CODE
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string CurrencyValue = string.Empty;

        [MaxLength(16)]
        public string? PAYMENT_METHOD
        {
            get => MethodValue;
            set => SetProperty(ref MethodValue, value);
        }

        private string? MethodValue;

        /// <summary>DRAFT, APPROVED, POSTED or RETURNED, from ProcurementCodes.</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "DRAFT";

        /// <summary>Who proposed the run - the person who may not approve it (AC-6).</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

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

        /// <summary>The one journal the run posts - at most one, however many invoices it settles (AC-5).</summary>
        [MaxLength(128)]
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

        [MaxLength(1000)]
        public string? RETURN_REASON
        {
            get => ReturnValue;
            set => SetProperty(ref ReturnValue, value);
        }

        private string? ReturnValue;

        [MaxLength(128)]
        public string? OWNER_TENANT_ID
        {
            get => TenantValue;
            set => SetProperty(ref TenantValue, value);
        }

        private string? TenantValue;

        [MaxLength(128)]
        public string? OWNER_COMPANY_BA_ID
        {
            get => CompanyValue;
            set => SetProperty(ref CompanyValue, value);
        }

        private string? CompanyValue;

        [MaxLength(128)]
        public string? BOOK_ID
        {
            get => BookValue;
            set => SetProperty(ref BookValue, value);
        }

        private string? BookValue;
    }
}
