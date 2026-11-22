using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One variance reason assigned to a reconciliation line (PRD-05 AC-2): which stage of the waterfall the line is
    /// explaining, the amount, the material flag at the time it was assigned, the reason code and a note.
    /// </summary>
    /// <remarks>
    /// A line is identified by the field, the month, the dimension (a well, or the field itself) and the stage that
    /// moved, so the same well can carry one reason per stage and the report can tell a reasoned variance from an
    /// unexplained one.
    /// </remarks>
    public class RECONCILIATION_VARIANCE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string VARIANCE_ID
        {
            get => VarianceIdValue;
            set => SetProperty(ref VarianceIdValue, value);
        }

        private string VarianceIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>The first day of the month the variance belongs to.</summary>
        public DateTime PERIOD_MONTH
        {
            get => PeriodMonthValue;
            set => SetProperty(ref PeriodMonthValue, value);
        }

        private DateTime PeriodMonthValue;

        /// <summary>The well id, or the field id for the field-level line.</summary>
        [MaxLength(128)]
        public string DIMENSION
        {
            get => DimensionValue;
            set => SetProperty(ref DimensionValue, value);
        }

        private string DimensionValue = string.Empty;

        /// <summary>The stage the variance was observed at: MEASURED, ALLOCATED, SOLD or BOOKED.</summary>
        [MaxLength(32)]
        public string STAGE
        {
            get => StageValue;
            set => SetProperty(ref StageValue, value);
        }

        private string StageValue = string.Empty;

        public decimal? AMOUNT
        {
            get => AmountValue;
            set => SetProperty(ref AmountValue, value);
        }

        private decimal? AmountValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        /// <summary>'Y' when the variance was above the materiality threshold when the reason was assigned.</summary>
        [MaxLength(1)]
        public string MATERIAL_IND
        {
            get => MaterialIndValue;
            set => SetProperty(ref MaterialIndValue, value);
        }

        private string MaterialIndValue = "N";

        [MaxLength(32)]
        public string REASON_CODE
        {
            get => ReasonCodeValue;
            set => SetProperty(ref ReasonCodeValue, value);
        }

        private string ReasonCodeValue = string.Empty;

        [MaxLength(512)]
        public string? NOTE
        {
            get => NoteValue;
            set => SetProperty(ref NoteValue, value);
        }

        private string? NoteValue;

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
