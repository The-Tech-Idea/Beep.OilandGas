using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// One recorded production deferment of a well (PRD-03): when the lost production started, when it ended (or that it
    /// is still ongoing), the reason code from the reference set, the deferred volume with its unit, a comment and who
    /// reported it.
    /// </summary>
    /// <remarks>
    /// The plan's open question - "deferment may be derived from status history rather than stored separately" - is
    /// answered here: PPDM's <c>WELL_STATUS</c> carries a status, a status type and a start/end time, but no deferred
    /// volume and no coded reason, so a deferment that explains lost volume needs its own record. The status board still
    /// reads <c>WELL_STATUS</c> for the status itself; this row is the volume-bearing explanation, keyed to the same well.
    /// </remarks>
    public class PRODUCTION_DEFERMENT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string DEFERMENT_ID
        {
            get => DefermentIdValue;
            set => SetProperty(ref DefermentIdValue, value);
        }

        private string DefermentIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        public DateTime START_AT
        {
            get => StartAtValue;
            set => SetProperty(ref StartAtValue, value);
        }

        private DateTime StartAtValue;

        /// <summary>Null while the deferment is ongoing (AC-5).</summary>
        public DateTime? END_AT
        {
            get => EndAtValue;
            set => SetProperty(ref EndAtValue, value);
        }

        private DateTime? EndAtValue;

        /// <summary>The reason code from <c>R_DEFERMENT_REASON</c>; a code outside the set is refused (AC-1).</summary>
        [MaxLength(32)]
        public string REASON_CODE
        {
            get => ReasonCodeValue;
            set => SetProperty(ref ReasonCodeValue, value);
        }

        private string ReasonCodeValue = string.Empty;

        public decimal? DEFERRED_VOLUME
        {
            get => DeferredVolumeValue;
            set => SetProperty(ref DeferredVolumeValue, value);
        }

        private decimal? DeferredVolumeValue;

        /// <summary>The unit of <see cref="DEFERRED_VOLUME"/>; required when a volume is recorded (AC-3).</summary>
        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        public string? COMMENT
        {
            get => CommentValue;
            set => SetProperty(ref CommentValue, value);
        }

        private string? CommentValue;

        [MaxLength(128)]
        public string? REPORTER
        {
            get => ReporterValue;
            set => SetProperty(ref ReporterValue, value);
        }

        private string? ReporterValue;

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
