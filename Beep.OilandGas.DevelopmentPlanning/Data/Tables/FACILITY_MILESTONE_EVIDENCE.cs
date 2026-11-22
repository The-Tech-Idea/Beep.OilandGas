using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One evidence item recorded against a construction milestone (DEV-08): the document, its reference, the date, the
    /// reviewer and the outcome.
    /// </summary>
    /// <remarks>
    /// A milestone cannot complete without evidence (AC-1), and a rejected outcome leaves the milestone's submission
    /// unresolved, which hides the facility's create and edit controls until a later outcome resolves it (AC-3).
    /// </remarks>
    public class FACILITY_MILESTONE_EVIDENCE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string EVIDENCE_ID
        {
            get => EvidenceIdValue;
            set => SetProperty(ref EvidenceIdValue, value);
        }

        private string EvidenceIdValue = string.Empty;

        /// <summary>The <c>FACILITY_MILESTONE.MILESTONE_ID</c> this item proves.</summary>
        [MaxLength(64)]
        public string MILESTONE_ID
        {
            get => MilestoneIdValue;
            set => SetProperty(ref MilestoneIdValue, value);
        }

        private string MilestoneIdValue = string.Empty;

        /// <summary>Repeated here so the facility's evidence can be read without walking the milestones.</summary>
        [MaxLength(64)]
        public string FACILITY_INV_ID
        {
            get => FacilityInvIdValue;
            set => SetProperty(ref FacilityInvIdValue, value);
        }

        private string FacilityInvIdValue = string.Empty;

        /// <summary>What the evidence is (for example signed drawing, FAT certificate, commissioning report).</summary>
        [MaxLength(256)]
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

        public DateTime? EVIDENCE_DATE
        {
            get => EvidenceDateValue;
            set => SetProperty(ref EvidenceDateValue, value);
        }

        private DateTime? EvidenceDateValue;

        [MaxLength(128)]
        public string? REVIEWER
        {
            get => ReviewerValue;
            set => SetProperty(ref ReviewerValue, value);
        }

        private string? ReviewerValue;

        /// <summary>ACCEPTED, ACCEPTED_WITH_NOTES or REJECTED.</summary>
        [MaxLength(32)]
        public string OUTCOME
        {
            get => OutcomeValue;
            set => SetProperty(ref OutcomeValue, value);
        }

        private string OutcomeValue = string.Empty;

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
