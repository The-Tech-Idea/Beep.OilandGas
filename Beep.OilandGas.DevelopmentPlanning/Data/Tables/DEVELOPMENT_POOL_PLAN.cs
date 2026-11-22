using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// The development plan's view of one PPDM pool (DEV-04): how much is in place, how much is expected to be
    /// recovered, how many wells are planned and which facility the pool produces to.
    /// </summary>
    /// <remarks>
    /// The PPDM <c>POOL</c> table owns the pool's identity and status; this row owns only the plan's numbers and the
    /// facility routing, so a pool is not duplicated. It carries the plan and the plan version it was written against
    /// (AC-4).
    /// </remarks>
    public class DEVELOPMENT_POOL_PLAN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string POOL_PLAN_ID
        {
            get => PoolPlanIdValue;
            set => SetProperty(ref PoolPlanIdValue, value);
        }

        private string PoolPlanIdValue = string.Empty;

        [MaxLength(64)]
        public string FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string FdpIdValue = string.Empty;

        /// <summary>The plan version this pool plan belongs to (AC-4).</summary>
        public int PLAN_VERSION
        {
            get => PlanVersionValue;
            set => SetProperty(ref PlanVersionValue, value);
        }

        private int PlanVersionValue = 1;

        [MaxLength(64)]
        public string FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string FieldIdValue = string.Empty;

        /// <summary>The PPDM <c>POOL.POOL_ID</c> this plan row describes; empty for a pool with no PPDM record yet.</summary>
        [MaxLength(64)]
        public string? POOL_ID
        {
            get => PoolIdValue;
            set => SetProperty(ref PoolIdValue, value);
        }

        private string? PoolIdValue;

        [MaxLength(128)]
        public string POOL_NAME
        {
            get => PoolNameValue;
            set => SetProperty(ref PoolNameValue, value);
        }

        private string PoolNameValue = string.Empty;

        /// <summary>The reservoir or stratigraphic unit the pool drains, as the plan records it.</summary>
        [MaxLength(128)]
        public string? RESERVOIR
        {
            get => ReservoirValue;
            set => SetProperty(ref ReservoirValue, value);
        }

        private string? ReservoirValue;

        /// <summary>
        /// The pool's drive mechanism as the plan records it (for example solution gas, gas cap or water drive). It is
        /// free text because the mechanism is a reservoir-engineering judgement, not a code this module owns.
        /// </summary>
        [MaxLength(64)]
        public string? DRIVE_MECHANISM
        {
            get => DriveMechanismValue;
            set => SetProperty(ref DriveMechanismValue, value);
        }

        private string? DriveMechanismValue;

        public decimal OOIP
        {
            get => OoipValue;
            set => SetProperty(ref OoipValue, value);
        }

        private decimal OoipValue;

        /// <summary>The unit of <see cref="OOIP"/>; a volume without it is refused and two units are never added.</summary>
        [MaxLength(16)]
        public string? OOIP_OUOM
        {
            get => OoipOuomValue;
            set => SetProperty(ref OoipOuomValue, value);
        }

        private string? OoipOuomValue;

        /// <summary>A fraction between 0 and 1; the recoverable volume is OOIP x factor.</summary>
        public decimal RECOVERY_FACTOR
        {
            get => RecoveryFactorValue;
            set => SetProperty(ref RecoveryFactorValue, value);
        }

        private decimal RecoveryFactorValue;

        public int PLANNED_WELLS
        {
            get => PlannedWellsValue;
            set => SetProperty(ref PlannedWellsValue, value);
        }

        private int PlannedWellsValue;

        /// <summary>The <c>FACILITY_INVESTMENT.FACILITY_ID</c> the pool's volumes are routed to.</summary>
        [MaxLength(64)]
        public string? FACILITY_ID
        {
            get => FacilityIdValue;
            set => SetProperty(ref FacilityIdValue, value);
        }

        private string? FacilityIdValue;

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
