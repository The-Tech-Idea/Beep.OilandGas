using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The posting columns of a joint-interest statement (ACC-07): its state, who raised and approved it, the ledger entry it
    /// posts, and the source volume it is traceable to.
    /// </summary>
    /// <remarks>
    /// <c>JOINT_INTEREST_STATEMENT</c> already held the period, the operator, the total charges, the total credits and the net -
    /// which is AC-4's shape exactly. It had no state, so an approval meant nothing; no author, so segregation of duties had
    /// nothing to compare; no journal link, so SG-10-7's "post to the configured accounts" could not be traced; and no source
    /// volume reference, so a bill could not be followed back to the production that produced it. These columns are additive.
    /// </remarks>
    public partial class JOINT_INTEREST_STATEMENT
    {
        /// <summary>DRAFT, APPROVED, POSTED or RETURNED - from JibStatus.</summary>
        [MaxLength(16)]
        public string? STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string? StatusValue = "DRAFT";

        /// <summary>The partner the statement bills.</summary>
        [MaxLength(128)]
        public string? PARTICIPANT_ID
        {
            get => ParticipantValue;
            set => SetProperty(ref ParticipantValue, value);
        }

        private string? ParticipantValue;

        /// <summary>Who raised the statement - the person who may not approve or post it.</summary>
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

        /// <summary>
        /// The ledger entry the posting produced (SG-10-7). At most one, and a retry adopts it rather than posting twice.
        /// </summary>
        [MaxLength(64)]
        public string? JOURNAL_ENTRY_ID
        {
            get => JournalValue;
            set => SetProperty(ref JournalValue, value);
        }

        private string? JournalValue;

        /// <summary>
        /// The production the bill is traceable to (AC-6): the allocation detail, run ticket or period volume the charges
        /// were computed from. A joint-interest bill with no source volume cannot be proved, so the field is required by the
        /// service rather than optional.
        /// </summary>
        [MaxLength(128)]
        public string? SOURCE_VOLUME_REFERENCE
        {
            get => SourceValue;
            set => SetProperty(ref SourceValue, value);
        }

        private string? SourceValue;

        [MaxLength(16)]
        public string? CURRENCY_CODE
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;
    }
}
