using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One review decision on a field development plan draft (DEV-02). Append-only, so the return reason and the
    /// step it names survive a later approval (AC-3).
    /// </summary>
    public class FDP_REVIEW : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string REVIEW_ID
        {
            get => ReviewIdValue;
            set => SetProperty(ref ReviewIdValue, value);
        }

        private string ReviewIdValue = string.Empty;

        [MaxLength(64)]
        public string DRAFT_ID
        {
            get => DraftIdValue;
            set => SetProperty(ref DraftIdValue, value);
        }

        private string DraftIdValue = string.Empty;

        [MaxLength(64)]
        public string FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string FdpIdValue = string.Empty;

        [MaxLength(32)]
        public string? DECISION
        {
            get => DecisionValue;
            set => SetProperty(ref DecisionValue, value);
        }

        private string? DecisionValue;

        public string? RATIONALE
        {
            get => RationaleValue;
            set => SetProperty(ref RationaleValue, value);
        }

        private string? RationaleValue;

        public int? RETURNED_STEP
        {
            get => ReturnedStepValue;
            set => SetProperty(ref ReturnedStepValue, value);
        }

        private int? ReturnedStepValue;

        public string? CONDITIONS_TEXT
        {
            get => ConditionsTextValue;
            set => SetProperty(ref ConditionsTextValue, value);
        }

        private string? ConditionsTextValue;

        [MaxLength(128)]
        public string? REVIEWER
        {
            get => ReviewerValue;
            set => SetProperty(ref ReviewerValue, value);
        }

        private string? ReviewerValue;

        public DateTime? REVIEWED_AT
        {
            get => ReviewedAtValue;
            set => SetProperty(ref ReviewedAtValue, value);
        }

        private DateTime? ReviewedAtValue;
    }
}
