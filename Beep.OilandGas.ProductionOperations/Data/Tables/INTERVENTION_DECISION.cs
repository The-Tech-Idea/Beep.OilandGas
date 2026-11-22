using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// One intervention decision for an underperforming well (PRD-06): the well, the chosen option, the rationale, the
    /// owner and the target date, plus the work-order reference once one is raised.
    /// </summary>
    /// <remarks>
    /// The decision is recorded once and then read; the chosen option is stored on its own row so the candidates and
    /// their bases are retained beside the choice. The work-order reference is a column rather than a second table, which
    /// is what makes "cannot be created twice for the same decision" a single-row check (AC-5).
    /// </remarks>
    public class INTERVENTION_DECISION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string DECISION_ID
        {
            get => DecisionIdValue;
            set => SetProperty(ref DecisionIdValue, value);
        }

        private string DecisionIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        /// <summary>The option the decision chose; the other options stay attached to the decision.</summary>
        [MaxLength(64)]
        public string? CHOSEN_OPTION_ID
        {
            get => ChosenOptionIdValue;
            set => SetProperty(ref ChosenOptionIdValue, value);
        }

        private string? ChosenOptionIdValue;

        public string? RATIONALE
        {
            get => RationaleValue;
            set => SetProperty(ref RationaleValue, value);
        }

        private string? RationaleValue;

        [MaxLength(128)]
        public string? OWNER
        {
            get => OwnerValue;
            set => SetProperty(ref OwnerValue, value);
        }

        private string? OwnerValue;

        public DateTime? TARGET_DATE
        {
            get => TargetDateValue;
            set => SetProperty(ref TargetDateValue, value);
        }

        private DateTime? TargetDateValue;

        /// <summary>DECIDED, or ACTED once a work-order reference is recorded.</summary>
        [MaxLength(16)]
        public string STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string StateValue = "DECIDED";

        /// <summary>The work order raised from this decision; a second one is refused (AC-5).</summary>
        [MaxLength(128)]
        public string? WORK_ORDER_REF
        {
            get => WorkOrderRefValue;
            set => SetProperty(ref WorkOrderRefValue, value);
        }

        private string? WorkOrderRefValue;

        public DateTime? WORK_ORDER_AT
        {
            get => WorkOrderAtValue;
            set => SetProperty(ref WorkOrderAtValue, value);
        }

        private DateTime? WorkOrderAtValue;

        [MaxLength(128)]
        public string? WORK_ORDER_BY
        {
            get => WorkOrderByValue;
            set => SetProperty(ref WorkOrderByValue, value);
        }

        private string? WorkOrderByValue;

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
