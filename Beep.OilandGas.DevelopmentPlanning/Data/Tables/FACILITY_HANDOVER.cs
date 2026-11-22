using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// The handover of a constructed facility to operations (DEV-08 AC-4): the recipient, the date, the conditions the
    /// acceptance carried and the signature record.
    /// </summary>
    /// <remarks>
    /// One handover per facility investment; the service refuses a second. Handing over is a state of this row, so a
    /// retried handover cannot write a duplicate acceptance.
    /// </remarks>
    public class FACILITY_HANDOVER : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string HANDOVER_ID
        {
            get => HandoverIdValue;
            set => SetProperty(ref HandoverIdValue, value);
        }

        private string HandoverIdValue = string.Empty;

        [MaxLength(64)]
        public string FACILITY_INV_ID
        {
            get => FacilityInvIdValue;
            set => SetProperty(ref FacilityInvIdValue, value);
        }

        private string FacilityInvIdValue = string.Empty;

        [MaxLength(64)]
        public string? FACILITY_ID
        {
            get => FacilityIdValue;
            set => SetProperty(ref FacilityIdValue, value);
        }

        private string? FacilityIdValue;

        [MaxLength(64)]
        public string? FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string? FdpIdValue;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>Who the asset is handed to (an operations team, a receiving company, a role holder).</summary>
        [MaxLength(128)]
        public string HANDED_OVER_TO
        {
            get => HandedOverToValue;
            set => SetProperty(ref HandedOverToValue, value);
        }

        private string HandedOverToValue = string.Empty;

        public DateTime? HANDED_OVER_AT
        {
            get => HandedOverAtValue;
            set => SetProperty(ref HandedOverAtValue, value);
        }

        private DateTime? HandedOverAtValue;

        /// <summary>One condition per line; empty when the asset was accepted unconditionally.</summary>
        public string? CONDITIONS_TEXT
        {
            get => ConditionsTextValue;
            set => SetProperty(ref ConditionsTextValue, value);
        }

        private string? ConditionsTextValue;

        /// <summary>The acceptance signature record the handover was signed under.</summary>
        [MaxLength(256)]
        public string? SIGNATURE
        {
            get => SignatureValue;
            set => SetProperty(ref SignatureValue, value);
        }

        private string? SignatureValue;

        [MaxLength(128)]
        public string? SIGNED_BY
        {
            get => SignedByValue;
            set => SetProperty(ref SignedByValue, value);
        }

        private string? SignedByValue;

        public DateTime? SIGNED_AT
        {
            get => SignedAtValue;
            set => SetProperty(ref SignedAtValue, value);
        }

        private DateTime? SignedAtValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedAtValue;
            set => SetProperty(ref RecordedAtValue, value);
        }

        private DateTime? RecordedAtValue;
    }
}
