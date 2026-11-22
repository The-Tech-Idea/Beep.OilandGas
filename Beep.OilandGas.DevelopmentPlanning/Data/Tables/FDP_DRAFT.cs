using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One editable draft of a field development plan (DEV-02). The draft carries the wizard's step values and the
    /// current step, and is separate from the plan row so a submitted plan can be frozen while a revision begins.
    /// </summary>
    public class FDP_DRAFT : ModelEntityBase
    {
        [Key, MaxLength(64)]
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

        [MaxLength(64)]
        public string FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string FieldIdValue = string.Empty;

        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue = 1;

        [MaxLength(32)]
        public string? STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string? StateValue;

        public int CURRENT_STEP
        {
            get => CurrentStepValue;
            set => SetProperty(ref CurrentStepValue, value);
        }

        private int CurrentStepValue = 1;

        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        public string? SCOPE_TEXT
        {
            get => ScopeTextValue;
            set => SetProperty(ref ScopeTextValue, value);
        }

        private string? ScopeTextValue;

        public string? RESERVOIR_TEXT
        {
            get => ReservoirTextValue;
            set => SetProperty(ref ReservoirTextValue, value);
        }

        private string? ReservoirTextValue;

        public int WELL_COUNT
        {
            get => WellCountValue;
            set => SetProperty(ref WellCountValue, value);
        }

        private int WellCountValue;

        public string? FACILITY_TEXT
        {
            get => FacilityTextValue;
            set => SetProperty(ref FacilityTextValue, value);
        }

        private string? FacilityTextValue;

        public decimal COST_AMOUNT
        {
            get => CostAmountValue;
            set => SetProperty(ref CostAmountValue, value);
        }

        private decimal CostAmountValue;

        [MaxLength(16)]
        public string? COST_CURRENCY
        {
            get => CostCurrencyValue;
            set => SetProperty(ref CostCurrencyValue, value);
        }

        private string? CostCurrencyValue;

        public string? RISK_TEXT
        {
            get => RiskTextValue;
            set => SetProperty(ref RiskTextValue, value);
        }

        private string? RiskTextValue;

        [MaxLength(64)]
        public string? BASELINE_ID
        {
            get => BaselineIdValue;
            set => SetProperty(ref BaselineIdValue, value);
        }

        private string? BaselineIdValue;

        public int? RETURNED_STEP
        {
            get => ReturnedStepValue;
            set => SetProperty(ref ReturnedStepValue, value);
        }

        private int? ReturnedStepValue;

        public string? RETURN_REASON
        {
            get => ReturnReasonValue;
            set => SetProperty(ref ReturnReasonValue, value);
        }

        private string? ReturnReasonValue;
    }
}
