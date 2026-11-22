using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    /// <summary>
    /// The evaluation columns of an economic result (ECO-01): the scenario, the forecast version it used, the fiscal terms,
    /// the inputs, the drivers, the approval and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held the NPV, the IRR, the payback and the discount rate, which cannot satisfy AC-6 (an approved
    /// evaluation identifies its forecast version and is immutable) or SG-09-3 (a comparison states which forecast each
    /// scenario used). These columns are additive, so an existing row stays valid.
    /// </remarks>
    public partial class ECONOMIC_ANALYSIS_RESULT
    {
        /// <summary>The scenario this evaluation is - read from <c>R_ECONOMIC_SCENARIO</c> (SG-09-3).</summary>
        [MaxLength(32)]
        public string? SCENARIO
        {
            get => ScenarioValue;
            set => SetProperty(ref ScenarioValue, value);
        }

        private string? ScenarioValue;

        /// <summary>
        /// The forecast version the volumes came from (AC-6, SG-09-3). One evaluation, one forecast version - a comparison
        /// across scenarios states each one's.
        /// </summary>
        [MaxLength(64)]
        public string? FORECAST_VERSION
        {
            get => ForecastVersionValue;
            set => SetProperty(ref ForecastVersionValue, value);
        }

        private string? ForecastVersionValue;

        /// <summary>The well, pool or field the evaluation is for.</summary>
        [MaxLength(128)]
        public string? ENTITY_ID
        {
            get => EntityValue;
            set => SetProperty(ref EntityValue, value);
        }

        private string? EntityValue;

        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        /// <summary>DRAFT or APPROVED. Approved rows are never updated again (AC-6).</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "DRAFT";

        /// <summary>How many times the scenario has been re-run; a re-run is a new row, not an edit.</summary>
        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue = 1;

        // ---- the fiscal regime the run was made under (AC-4, SG-09-2) ----

        [MaxLength(128)]
        public string? FISCAL_REGIME
        {
            get => FiscalValue;
            set => SetProperty(ref FiscalValue, value);
        }

        private string? FiscalValue;

        public decimal? ROYALTY_RATE
        {
            get => RoyaltyValue;
            set => SetProperty(ref RoyaltyValue, value);
        }

        private decimal? RoyaltyValue;

        public decimal? TAX_RATE
        {
            get => TaxRateValue;
            set => SetProperty(ref TaxRateValue, value);
        }

        private decimal? TaxRateValue;

        // ---- the inputs, so a result can be re-run from its own row ----

        public int? PERIOD_COUNT
        {
            get => PeriodCountValue;
            set => SetProperty(ref PeriodCountValue, value);
        }

        private int? PeriodCountValue;

        public int? START_YEAR
        {
            get => StartYearValue;
            set => SetProperty(ref StartYearValue, value);
        }

        private int? StartYearValue;

        public decimal? ANNUAL_VOLUME
        {
            get => VolumeValue;
            set => SetProperty(ref VolumeValue, value);
        }

        private decimal? VolumeValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        public decimal? UNIT_PRICE
        {
            get => PriceValue;
            set => SetProperty(ref PriceValue, value);
        }

        private decimal? PriceValue;

        [MaxLength(32)]
        public string? PRICE_OUOM
        {
            get => PriceOuomValue;
            set => SetProperty(ref PriceOuomValue, value);
        }

        private string? PriceOuomValue;

        public decimal? OPEX_PER_UNIT
        {
            get => OpexValue;
            set => SetProperty(ref OpexValue, value);
        }

        private decimal? OpexValue;

        public decimal? ANNUAL_CAPEX
        {
            get => CapexValue;
            set => SetProperty(ref CapexValue, value);
        }

        private decimal? CapexValue;

        public decimal? ESCALATION_RATE
        {
            get => EscalationValue;
            set => SetProperty(ref EscalationValue, value);
        }

        private decimal? EscalationValue;

        public decimal? ABANDONMENT_COST
        {
            get => AbandonmentValue;
            set => SetProperty(ref AbandonmentValue, value);
        }

        private decimal? AbandonmentValue;

        // ---- the rest of the answer ----

        /// <summary>'Y' when an IRR exists; 'N' when the cash flow never changes sign (AC-2).</summary>
        [MaxLength(1)]
        public string IRR_DEFINED_IND
        {
            get => IrrDefinedValue;
            set => SetProperty(ref IrrDefinedValue, value);
        }

        private string IrrDefinedValue = "N";

        public decimal? PEAK_FUNDING
        {
            get => PeakFundingValue;
            set => SetProperty(ref PeakFundingValue, value);
        }

        private decimal? PeakFundingValue;

        public decimal? UNDISCOUNTED_TOTAL
        {
            get => UndiscountedValue;
            set => SetProperty(ref UndiscountedValue, value);
        }

        private decimal? UndiscountedValue;

        /// <summary>The sensitivity drivers with their low and high NPVs, one per line.</summary>
        public string? DRIVERS_TEXT
        {
            get => DriversValue;
            set => SetProperty(ref DriversValue, value);
        }

        private string? DriversValue;

        /// <summary>The regime's own description, including that it is a screening model.</summary>
        [MaxLength(512)]
        public string? FISCAL_REGIME_TEXT
        {
            get => FiscalTextValue;
            set => SetProperty(ref FiscalTextValue, value);
        }

        private string? FiscalTextValue;

        // ---- who and when ----

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedValue;
            set => SetProperty(ref RecordedValue, value);
        }

        private DateTime? RecordedValue;

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
    }
}
