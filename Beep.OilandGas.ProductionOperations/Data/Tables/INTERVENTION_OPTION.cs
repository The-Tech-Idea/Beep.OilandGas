using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// One candidate intervention of a decision (PRD-06): the method, the gain it is expected to recover with its unit,
    /// the cost and currency, the risk, and the basis the estimate rests on.
    /// </summary>
    /// <remarks>
    /// The options are rows rather than a JSON blob on the decision, so the chosen one is identifiable by key, the basis
    /// of every candidate is a column that validation can refuse (AC-2), and the gain's unit is a column too (AC-3). The
    /// options that were not chosen stay attached to the decision: the page shows what was considered, not only the
    /// answer.
    /// </remarks>
    public class INTERVENTION_OPTION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string OPTION_ID
        {
            get => OptionIdValue;
            set => SetProperty(ref OptionIdValue, value);
        }

        private string OptionIdValue = string.Empty;

        /// <summary>The <c>INTERVENTION_DECISION.DECISION_ID</c> this candidate belongs to.</summary>
        [MaxLength(64)]
        public string DECISION_ID
        {
            get => DecisionIdValue;
            set => SetProperty(ref DecisionIdValue, value);
        }

        private string DecisionIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        /// <summary>The method code from <c>R_INTERVENTION_METHOD</c>; a code outside the set is refused.</summary>
        [MaxLength(32)]
        public string METHOD
        {
            get => MethodValue;
            set => SetProperty(ref MethodValue, value);
        }

        private string MethodValue = string.Empty;

        public decimal? ESTIMATED_GAIN
        {
            get => EstimatedGainValue;
            set => SetProperty(ref EstimatedGainValue, value);
        }

        private decimal? EstimatedGainValue;

        /// <summary>The unit of <see cref="ESTIMATED_GAIN"/>; required when a gain is stated (AC-3).</summary>
        [MaxLength(16)]
        public string? GAIN_OUOM
        {
            get => GainOuomValue;
            set => SetProperty(ref GainOuomValue, value);
        }

        private string? GainOuomValue;

        public decimal? ESTIMATED_COST
        {
            get => EstimatedCostValue;
            set => SetProperty(ref EstimatedCostValue, value);
        }

        private decimal? EstimatedCostValue;

        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        /// <summary>LOW, MEDIUM or HIGH. Stated, not scored: the plan does not define a weighting.</summary>
        [MaxLength(16)]
        public string? RISK
        {
            get => RiskValue;
            set => SetProperty(ref RiskValue, value);
        }

        private string? RiskValue;

        /// <summary>TEST, ANALOGUE or MODEL - the basis the estimate rests on (AC-2).</summary>
        [MaxLength(16)]
        public string BASIS
        {
            get => BasisValue;
            set => SetProperty(ref BasisValue, value);
        }

        private string BasisValue = string.Empty;

        /// <summary>The evidence behind the basis: which test, which analogue well, which model.</summary>
        [MaxLength(512)]
        public string? BASIS_TEXT
        {
            get => BasisTextValue;
            set => SetProperty(ref BasisTextValue, value);
        }

        private string? BasisTextValue;

        /// <summary>'Y' on the option the decision chose; exactly one option of a decision carries it.</summary>
        [MaxLength(1)]
        public string CHOSEN_IND
        {
            get => ChosenIndValue;
            set => SetProperty(ref ChosenIndValue, value);
        }

        private string ChosenIndValue = "N";
    }
}
