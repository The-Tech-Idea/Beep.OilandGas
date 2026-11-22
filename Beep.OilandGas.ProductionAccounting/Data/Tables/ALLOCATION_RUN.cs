using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One allocation run of the workbench (PRD-04): the period, the field and facility it allocates, the method, the
    /// measured and allocated totals, the residual, the tolerance and the hash of its inputs.
    /// </summary>
    /// <remarks>
    /// The accounting <c>ALLOCATION_RESULT</c> and <c>ALLOCATION_DETAIL</c> remain the accounting record. They are not the
    /// store of this screen for two reasons: they carry no unit, residual, override reason, run state or input hash, and
    /// the entity declares both <c>ALLOCATION_METHOD</c> and the <c>METHOD</c> enum, which the framework's field builder
    /// collapses onto one column name - an entity read of that table throws
    /// "An item with the same key has already been added. Key: METHOD". The workbench therefore owns its own pair of
    /// rows, and the accounting flow converts them when the run is posted.
    /// </remarks>
    public class ALLOCATION_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ALLOCATION_RUN_ID
        {
            get => AllocationRunIdValue;
            set => SetProperty(ref AllocationRunIdValue, value);
        }

        private string AllocationRunIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>When the run is pinned to a facility's own measurement rather than the field's wells.</summary>
        [MaxLength(64)]
        public string? FACILITY_ID
        {
            get => FacilityIdValue;
            set => SetProperty(ref FacilityIdValue, value);
        }

        private string? FacilityIdValue;

        public DateTime PERIOD_DATE
        {
            get => PeriodDateValue;
            set => SetProperty(ref PeriodDateValue, value);
        }

        private DateTime PeriodDateValue;

        [MaxLength(32)]
        public string METHOD
        {
            get => MethodValue;
            set => SetProperty(ref MethodValue, value);
        }

        private string MethodValue = string.Empty;

        /// <summary>COMMITTED; a preview writes no row at all.</summary>
        [MaxLength(16)]
        public string STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string StateValue = "COMMITTED";

        public decimal? TOTAL_MEASURED
        {
            get => TotalMeasuredValue;
            set => SetProperty(ref TotalMeasuredValue, value);
        }

        private decimal? TotalMeasuredValue;

        public decimal? TOTAL_ALLOCATED
        {
            get => TotalAllocatedValue;
            set => SetProperty(ref TotalAllocatedValue, value);
        }

        private decimal? TotalAllocatedValue;

        public decimal? RESIDUAL
        {
            get => ResidualValue;
            set => SetProperty(ref ResidualValue, value);
        }

        private decimal? ResidualValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        public decimal TOLERANCE_PERCENT
        {
            get => TolerancePercentValue;
            set => SetProperty(ref TolerancePercentValue, value);
        }

        private decimal TolerancePercentValue;

        /// <summary>'Y' when a residual beyond tolerance was acknowledged before the commit.</summary>
        [MaxLength(1)]
        public string ACKNOWLEDGED_IND
        {
            get => AcknowledgedIndValue;
            set => SetProperty(ref AcknowledgedIndValue, value);
        }

        private string AcknowledgedIndValue = "N";

        /// <summary>The hash of the inputs (AC-6); the same hash means the same run (AC-4).</summary>
        [MaxLength(64)]
        public string INPUT_HASH
        {
            get => InputHashValue;
            set => SetProperty(ref InputHashValue, value);
        }

        private string InputHashValue = string.Empty;

        /// <summary>Where the measured total came from, as the page says it.</summary>
        [MaxLength(256)]
        public string? MEASURED_SOURCE
        {
            get => MeasuredSourceValue;
            set => SetProperty(ref MeasuredSourceValue, value);
        }

        private string? MeasuredSourceValue;

        [MaxLength(128)]
        public string? COMMITTED_BY
        {
            get => CommittedByValue;
            set => SetProperty(ref CommittedByValue, value);
        }

        private string? CommittedByValue;

        public DateTime? COMMITTED_AT
        {
            get => CommittedAtValue;
            set => SetProperty(ref CommittedAtValue, value);
        }

        private DateTime? CommittedAtValue;
    }
}
