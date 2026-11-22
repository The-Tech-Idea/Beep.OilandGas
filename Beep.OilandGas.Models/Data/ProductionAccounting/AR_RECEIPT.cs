using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Cash received from a customer (ACC-04): the receipt header the allocations hang from.
    /// </summary>
    /// <remarks>
    /// <c>AR_PAYMENT</c> is one amount against one invoice, so a single bank receipt that settles three invoices had nowhere
    /// to live as one fact - and "unapplied cash" had no owner at all. This header is the receipt; each allocation is an
    /// <c>AR_PAYMENT</c> row carrying its <c>RECEIPT_ID</c>, so the cash, its allocations and what is left unapplied are one
    /// record rather than a convention.
    /// </remarks>
    public class AR_RECEIPT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string AR_RECEIPT_ID
        {
            get => ReceiptIdValue;
            set => SetProperty(ref ReceiptIdValue, value);
        }

        private string ReceiptIdValue = string.Empty;

        /// <summary>The number the receipt is given when it is posted, server-owned.</summary>
        [MaxLength(64)]
        public string? RECEIPT_NUMBER
        {
            get => NumberValue;
            set => SetProperty(ref NumberValue, value);
        }

        private string? NumberValue;

        [MaxLength(128)]
        public string CUSTOMER_BA_ID
        {
            get => CustomerValue;
            set => SetProperty(ref CustomerValue, value);
        }

        private string CustomerValue = string.Empty;

        public DateTime RECEIPT_DATE
        {
            get => DateValue;
            set => SetProperty(ref DateValue, value);
        }

        private DateTime DateValue;

        /// <summary>The cash received.</summary>
        public decimal AMOUNT
        {
            get => AmountValue;
            set => SetProperty(ref AmountValue, value);
        }

        private decimal AmountValue;

        /// <summary>What has not been applied to an invoice. AC-1: shown explicitly, never discarded.</summary>
        public decimal UNAPPLIED_AMOUNT
        {
            get => UnappliedValue;
            set => SetProperty(ref UnappliedValue, value);
        }

        private decimal UnappliedValue;

        [MaxLength(16)]
        public string CURRENCY_CODE
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string CurrencyValue = string.Empty;

        /// <summary>The bank reference the customer's payment quotes.</summary>
        [MaxLength(128)]
        public string? REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

        /// <summary>The bank account the cash landed in.</summary>
        [MaxLength(64)]
        public string? BANK_ACCOUNT
        {
            get => BankValue;
            set => SetProperty(ref BankValue, value);
        }

        private string? BankValue;

        /// <summary>DRAFT, POSTED, APPROVED or RETURNED - from ReceivableCodes.</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "DRAFT";

        /// <summary>Who recorded it - the person who may not approve it (AC-6).</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        /// <summary>
        /// The ledger entry posting produced. AC-5: there is at most one. AC-6 of ACC-03's pattern: a retry after a lost
        /// acknowledgement finds this and returns the same journal rather than writing another.
        /// </summary>
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
