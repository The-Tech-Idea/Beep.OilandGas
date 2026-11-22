using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The procurement columns of a supplier invoice (ACC-05): the author, the order and AFE it belongs to, the money split,
    /// and the three-way match with its exception approval.
    /// </summary>
    /// <remarks>
    /// The generated table held the number, the vendor, the dates, the total, the paid amount, the balance and a status -
    /// but no author, so AC-6 had nothing to compare; no order link, so there was nothing to match against; no subtotal or
    /// tax, which the capture screen asks for; and no match state at all, so a price above tolerance had nowhere to be
    /// recorded and would have passed silently, which is exactly what AC-2 forbids. These columns are additive.
    /// </remarks>
    public partial class AP_INVOICE
    {
        /// <summary>Who captured the invoice - the person who may not approve its exception.</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        /// <summary>The order the invoice quotes, when it quotes one.</summary>
        [MaxLength(128)]
        public string? PURCHASE_ORDER_ID
        {
            get => OrderValue;
            set => SetProperty(ref OrderValue, value);
        }

        private string? OrderValue;

        /// <summary>The authorisation this invoice commits against (AC-4).</summary>
        [MaxLength(128)]
        public string? AFE_ID
        {
            get => AfeValue;
            set => SetProperty(ref AfeValue, value);
        }

        private string? AfeValue;

        /// <summary>The lines' net total, before tax.</summary>
        public decimal? SUBTOTAL_AMOUNT
        {
            get => SubtotalValue;
            set => SetProperty(ref SubtotalValue, value);
        }

        private decimal? SubtotalValue;

        public decimal? TAX_AMOUNT
        {
            get => TaxValue;
            set => SetProperty(ref TaxValue, value);
        }

        private decimal? TaxValue;

        /// <summary>MATCHED, EXCEPTION, EXCEPTION_APPROVED or NON_PO, from ProcurementCodes.</summary>
        [MaxLength(32)]
        public string? MATCH_STATE
        {
            get => MatchStateValue;
            set => SetProperty(ref MatchStateValue, value);
        }

        private string? MatchStateValue;

        /// <summary>The exceptions the match found, one per line - the evidence a reviewer signs off.</summary>
        public string? MATCH_EXCEPTIONS_TEXT
        {
            get => MatchExceptionsValue;
            set => SetProperty(ref MatchExceptionsValue, value);
        }

        private string? MatchExceptionsValue;

        /// <summary>The matching policy whose tolerances were applied, so the decision names its rule.</summary>
        [MaxLength(128)]
        public string? MATCH_POLICY_ID
        {
            get => MatchPolicyValue;
            set => SetProperty(ref MatchPolicyValue, value);
        }

        private string? MatchPolicyValue;

        [MaxLength(128)]
        public string? EXCEPTION_APPROVED_BY
        {
            get => ExceptionByValue;
            set => SetProperty(ref ExceptionByValue, value);
        }

        private string? ExceptionByValue;

        public DateTime? EXCEPTION_APPROVED_AT
        {
            get => ExceptionAtValue;
            set => SetProperty(ref ExceptionAtValue, value);
        }

        private DateTime? ExceptionAtValue;
    }
}
