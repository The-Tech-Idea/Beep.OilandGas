using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One item of completion evidence attached to a submission (DEV-07). The item records the well it belongs to, so an
    /// item that names a different well is refused rather than filed against this one (AC-2).
    /// </summary>
    public class DEVELOPMENT_COMPLETION_EVIDENCE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string EVIDENCE_ID
        {
            get => EvidenceIdValue;
            set => SetProperty(ref EvidenceIdValue, value);
        }

        private string EvidenceIdValue = string.Empty;

        [MaxLength(64)]
        public string SUBMISSION_ID
        {
            get => SubmissionIdValue;
            set => SetProperty(ref SubmissionIdValue, value);
        }

        private string SubmissionIdValue = string.Empty;

        /// <summary>The well this evidence belongs to; it must be the submission's well (AC-2).</summary>
        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        /// <summary>What the evidence is (for example CBL LOG, PRESSURE CHART, ACCEPTANCE CERTIFICATE).</summary>
        [MaxLength(128)]
        public string? DOCUMENT
        {
            get => DocumentValue;
            set => SetProperty(ref DocumentValue, value);
        }

        private string? DocumentValue;

        /// <summary>The document, file or system reference that holds it.</summary>
        [MaxLength(256)]
        public string? REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

        [MaxLength(128)]
        public string? ADDED_BY
        {
            get => AddedByValue;
            set => SetProperty(ref AddedByValue, value);
        }

        private string? AddedByValue;

        public DateTime? ADDED_AT
        {
            get => AddedAtValue;
            set => SetProperty(ref AddedAtValue, value);
        }

        private DateTime? AddedAtValue;
    }
}
