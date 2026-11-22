using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One row of a workbench allocation run (PRD-04): the well, what it measured, what it was allocated, the factor, the
    /// residual, the method and the manual override with its reason.
    /// </summary>
    public class ALLOCATION_RUN_ROW : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ALLOCATION_RUN_ROW_ID
        {
            get => AllocationRunRowIdValue;
            set => SetProperty(ref AllocationRunRowIdValue, value);
        }

        private string AllocationRunRowIdValue = string.Empty;

        [MaxLength(64)]
        public string ALLOCATION_RUN_ID
        {
            get => AllocationRunIdValue;
            set => SetProperty(ref AllocationRunIdValue, value);
        }

        private string AllocationRunIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        [MaxLength(128)]
        public string? WELL_NAME
        {
            get => WellNameValue;
            set => SetProperty(ref WellNameValue, value);
        }

        private string? WellNameValue;

        public decimal? MEASURED_VOLUME
        {
            get => MeasuredVolumeValue;
            set => SetProperty(ref MeasuredVolumeValue, value);
        }

        private decimal? MeasuredVolumeValue;

        /// <summary>Null on an unallocated row: the row is unallocated, not allocated zero (AC-1).</summary>
        public decimal? ALLOCATED_VOLUME
        {
            get => AllocatedVolumeValue;
            set => SetProperty(ref AllocatedVolumeValue, value);
        }

        private decimal? AllocatedVolumeValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        /// <summary>The share of the measured total this row received.</summary>
        public decimal? FACTOR
        {
            get => FactorValue;
            set => SetProperty(ref FactorValue, value);
        }

        private decimal? FactorValue;

        /// <summary>Allocated minus measured for this row (AC-1).</summary>
        public decimal? RESIDUAL
        {
            get => ResidualValue;
            set => SetProperty(ref ResidualValue, value);
        }

        private decimal? ResidualValue;

        /// <summary>MEASURED_SHARE, PRIOR_PERIOD, EQUAL or UNALLOCATED.</summary>
        [MaxLength(32)]
        public string METHOD
        {
            get => MethodValue;
            set => SetProperty(ref MethodValue, value);
        }

        private string MethodValue = string.Empty;

        /// <summary>ALLOCATED, OVERRIDDEN or UNALLOCATED.</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = string.Empty;

        [MaxLength(256)]
        public string? BASIS_TEXT
        {
            get => BasisTextValue;
            set => SetProperty(ref BasisTextValue, value);
        }

        private string? BasisTextValue;

        /// <summary>AC-3: an override is recorded with why it was made.</summary>
        [MaxLength(256)]
        public string? OVERRIDE_REASON
        {
            get => OverrideReasonValue;
            set => SetProperty(ref OverrideReasonValue, value);
        }

        private string? OverrideReasonValue;

        [MaxLength(128)]
        public string? OVERRIDE_BY
        {
            get => OverrideByValue;
            set => SetProperty(ref OverrideByValue, value);
        }

        private string? OverrideByValue;
    }
}
