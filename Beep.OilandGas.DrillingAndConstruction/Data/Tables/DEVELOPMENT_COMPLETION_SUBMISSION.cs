using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// The completion submission of one well and the independent decision on it (DEV-07). One row carries the whole
    /// lifecycle - draft, submitted, decided - and, once accepted, the acceptance record itself: who accepted, when, the
    /// conditions and who the well is handed to. A lost acknowledgement cannot duplicate the acceptance because the
    /// acceptance is a state of this one row, not a second record.
    /// </summary>
    public class DEVELOPMENT_COMPLETION_SUBMISSION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string SUBMISSION_ID
        {
            get => SubmissionIdValue;
            set => SetProperty(ref SubmissionIdValue, value);
        }

        private string SubmissionIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        [MaxLength(64)]
        public string? PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string? ProgramIdValue;

        public string? SUMMARY
        {
            get => SummaryValue;
            set => SetProperty(ref SummaryValue, value);
        }

        private string? SummaryValue;

        /// <summary>DRAFT, SUBMITTED, ACCEPTED, ACCEPTED_WITH_CONDITIONS, RETURNED or REJECTED.</summary>
        [MaxLength(32)]
        public string? STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string? StateValue;

        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        public DateTime? SUBMITTED_AT
        {
            get => SubmittedAtValue;
            set => SetProperty(ref SubmittedAtValue, value);
        }

        private DateTime? SubmittedAtValue;

        /// <summary>The reviewer who decided; the author is refused here (AC-3).</summary>
        [MaxLength(128)]
        public string? REVIEWER
        {
            get => ReviewerValue;
            set => SetProperty(ref ReviewerValue, value);
        }

        private string? ReviewerValue;

        public string? RATIONALE
        {
            get => RationaleValue;
            set => SetProperty(ref RationaleValue, value);
        }

        private string? RationaleValue;

        /// <summary>One condition per line; an acceptance keeps them on the row the handover reads (AC-4).</summary>
        public string? CONDITIONS_TEXT
        {
            get => ConditionsTextValue;
            set => SetProperty(ref ConditionsTextValue, value);
        }

        private string? ConditionsTextValue;

        public DateTime? ACCEPTED_AT
        {
            get => AcceptedAtValue;
            set => SetProperty(ref AcceptedAtValue, value);
        }

        private DateTime? AcceptedAtValue;

        [MaxLength(128)]
        public string? ACCEPTED_BY
        {
            get => AcceptedByValue;
            set => SetProperty(ref AcceptedByValue, value);
        }

        private string? AcceptedByValue;

        /// <summary>Who the well is handed to for production (AC-4).</summary>
        [MaxLength(128)]
        public string? HANDOVER_TO
        {
            get => HandoverToValue;
            set => SetProperty(ref HandoverToValue, value);
        }

        private string? HandoverToValue;

        public DateTime? HANDOVER_AT
        {
            get => HandoverAtValue;
            set => SetProperty(ref HandoverAtValue, value);
        }

        private DateTime? HandoverAtValue;
    }
}
