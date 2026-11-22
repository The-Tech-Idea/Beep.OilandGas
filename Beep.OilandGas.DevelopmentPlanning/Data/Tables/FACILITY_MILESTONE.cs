using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One construction milestone of a planned facility (DEV-08): design, procurement, fabrication, installation,
    /// commissioning or handover, with its planned date, the date it actually happened and its state.
    /// </summary>
    /// <remarks>
    /// The milestone belongs to a <c>FACILITY_INVESTMENT</c> row of a development plan, so the facility identity and the
    /// budget stay where they already are. Overdue is never stored: it is computed from the planned date and the state.
    /// </remarks>
    public class FACILITY_MILESTONE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string MILESTONE_ID
        {
            get => MilestoneIdValue;
            set => SetProperty(ref MilestoneIdValue, value);
        }

        private string MilestoneIdValue = string.Empty;

        /// <summary>The <c>FACILITY_INVESTMENT.FACILITY_INV_ID</c> this milestone belongs to.</summary>
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

        /// <summary>DESIGN, PROCUREMENT, FABRICATION, INSTALLATION, COMMISSIONING or HANDOVER.</summary>
        [MaxLength(32)]
        public string MILESTONE_NAME
        {
            get => MilestoneNameValue;
            set => SetProperty(ref MilestoneNameValue, value);
        }

        private string MilestoneNameValue = string.Empty;

        /// <summary>1 for design through 6 for handover; the tracker shows them in this order.</summary>
        public int MILESTONE_SEQ
        {
            get => MilestoneSeqValue;
            set => SetProperty(ref MilestoneSeqValue, value);
        }

        private int MilestoneSeqValue;

        public DateTime? PLANNED_DATE
        {
            get => PlannedDateValue;
            set => SetProperty(ref PlannedDateValue, value);
        }

        private DateTime? PlannedDateValue;

        /// <summary>Set when the milestone is completed; a milestone without one is not complete.</summary>
        public DateTime? ACTUAL_DATE
        {
            get => ActualDateValue;
            set => SetProperty(ref ActualDateValue, value);
        }

        private DateTime? ActualDateValue;

        /// <summary>NOT_STARTED, IN_PROGRESS or COMPLETE. A milestone cannot complete without evidence (AC-1).</summary>
        [MaxLength(16)]
        public string STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string StateValue = "NOT_STARTED";

        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue;

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
