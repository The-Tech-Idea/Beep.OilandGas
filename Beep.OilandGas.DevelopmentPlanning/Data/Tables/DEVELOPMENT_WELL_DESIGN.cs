using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One version of a planned development well's design (DEV-03). The design is versioned: a cost revision appends a
    /// new row and marks the previous one not current, so the earlier objective, depths and cost are retained (AC-5).
    /// </summary>
    public class DEVELOPMENT_WELL_DESIGN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string DesignIdValue = string.Empty;

        [MaxLength(64)]
        public string SCHEDULE_ID
        {
            get => ScheduleIdValue;
            set => SetProperty(ref ScheduleIdValue, value);
        }

        private string ScheduleIdValue = string.Empty;

        [MaxLength(64)]
        public string FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string FdpIdValue = string.Empty;

        [MaxLength(64)]
        public string FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string FieldIdValue = string.Empty;

        [MaxLength(64)]
        public string PLANNED_WELL_ID
        {
            get => PlannedWellIdValue;
            set => SetProperty(ref PlannedWellIdValue, value);
        }

        private string PlannedWellIdValue = string.Empty;

        [MaxLength(128)]
        public string? WELL_NAME
        {
            get => WellNameValue;
            set => SetProperty(ref WellNameValue, value);
        }

        private string? WellNameValue;

        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue = 1;

        /// <summary>'Y' for the current version; a revision deactivates the previous row rather than deleting it.</summary>
        [MaxLength(1)]
        public string CURRENT_IND
        {
            get => CurrentIndValue;
            set => SetProperty(ref CurrentIndValue, value);
        }

        private string CurrentIndValue = "Y";

        /// <summary>CONCEPT, DEFINED, APPROVED or DRILLED.</summary>
        [MaxLength(32)]
        public string? DESIGN_STATE
        {
            get => DesignStateValue;
            set => SetProperty(ref DesignStateValue, value);
        }

        private string? DesignStateValue;

        public string? OBJECTIVE
        {
            get => ObjectiveValue;
            set => SetProperty(ref ObjectiveValue, value);
        }

        private string? ObjectiveValue;

        [MaxLength(128)]
        public string? TARGET_FORMATION
        {
            get => TargetFormationValue;
            set => SetProperty(ref TargetFormationValue, value);
        }

        private string? TargetFormationValue;

        public decimal TARGET_DEPTH
        {
            get => TargetDepthValue;
            set => SetProperty(ref TargetDepthValue, value);
        }

        private decimal TargetDepthValue;

        /// <summary>The unit of <see cref="TARGET_DEPTH"/> and of every casing set depth; required (AC-2).</summary>
        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;

        /// <summary>Trajectory intent only: vertical, deviated or horizontal, with a note. Geometry is ENG-06.</summary>
        public string? TRAJECTORY_INTENT
        {
            get => TrajectoryIntentValue;
            set => SetProperty(ref TrajectoryIntentValue, value);
        }

        private string? TrajectoryIntentValue;

        public string? COMPLETION_CONCEPT
        {
            get => CompletionConceptValue;
            set => SetProperty(ref CompletionConceptValue, value);
        }

        private string? CompletionConceptValue;

        public string? RISK_TEXT
        {
            get => RiskTextValue;
            set => SetProperty(ref RiskTextValue, value);
        }

        private string? RiskTextValue;

        public decimal ESTIMATED_COST
        {
            get => EstimatedCostValue;
            set => SetProperty(ref EstimatedCostValue, value);
        }

        private decimal EstimatedCostValue;

        [MaxLength(16)]
        public string? COST_CURRENCY
        {
            get => CostCurrencyValue;
            set => SetProperty(ref CostCurrencyValue, value);
        }

        private string? CostCurrencyValue;

        public DateTime? SPUD_WINDOW_START
        {
            get => SpudWindowStartValue;
            set => SetProperty(ref SpudWindowStartValue, value);
        }

        private DateTime? SpudWindowStartValue;

        public DateTime? SPUD_WINDOW_END
        {
            get => SpudWindowEndValue;
            set => SetProperty(ref SpudWindowEndValue, value);
        }

        private DateTime? SpudWindowEndValue;

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

        /// <summary>Why this version exists; a cost revision records the change here.</summary>
        public string? REVISION_REASON
        {
            get => RevisionReasonValue;
            set => SetProperty(ref RevisionReasonValue, value);
        }

        private string? RevisionReasonValue;

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
