using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One approved baseline of a field development plan (DEV-02). A baseline is append-only: a revision appends a
    /// new row and never changes the one before it (AC-2, AC-6).
    /// </summary>
    public class FDP_BASELINE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string BASELINE_ID
        {
            get => BaselineIdValue;
            set => SetProperty(ref BaselineIdValue, value);
        }

        private string BaselineIdValue = string.Empty;

        [MaxLength(64)]
        public string FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string FdpIdValue = string.Empty;

        [MaxLength(64)]
        public string DRAFT_ID
        {
            get => DraftIdValue;
            set => SetProperty(ref DraftIdValue, value);
        }

        private string DraftIdValue = string.Empty;

        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue = 1;

        public DateTime? APPROVED_AT
        {
            get => ApprovedAtValue;
            set => SetProperty(ref ApprovedAtValue, value);
        }

        private DateTime? ApprovedAtValue;

        [MaxLength(128)]
        public string? APPROVED_BY
        {
            get => ApprovedByValue;
            set => SetProperty(ref ApprovedByValue, value);
        }

        private string? ApprovedByValue;

        public string? CONDITIONS_TEXT
        {
            get => ConditionsTextValue;
            set => SetProperty(ref ConditionsTextValue, value);
        }

        private string? ConditionsTextValue;

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
    }
}
