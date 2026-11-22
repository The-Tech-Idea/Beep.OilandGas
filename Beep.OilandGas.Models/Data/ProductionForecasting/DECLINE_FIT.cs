using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// One decline curve analysis run (RES-06): the engine it fitted, the parameters, the fit quality, the points used
    /// and excluded, the independent cross-check, and who ran it.
    /// </summary>
    /// <remarks>
    /// The parameters are stored on the run rather than recomputed on read, because the fit is what the engineer
    /// accepted: a later change to the history must not silently change a saved analysis. A run that was refused keeps
    /// its refusal code and message and no parameters, so a refused fit is never read as a result.
    /// </remarks>
    public class DECLINE_FIT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string FIT_ID
        {
            get => FitIdValue;
            set => SetProperty(ref FitIdValue, value);
        }

        private string FitIdValue = string.Empty;

        [MaxLength(64)]
        public string? WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string? WellIdValue;

        [MaxLength(64)]
        public string? POOL_ID
        {
            get => PoolIdValue;
            set => SetProperty(ref PoolIdValue, value);
        }

        private string? PoolIdValue;

        /// <summary>
        /// The stream the fit describes: OIL, GAS or WATER. RES-06 fitted one fluid's history, so the row has to say which
        /// one, or a forecast built from it could not tell which stream its profile belongs to.
        /// </summary>
        [MaxLength(16)]
        public string? FLUID
        {
            get => FluidValue;
            set => SetProperty(ref FluidValue, value);
        }

        private string? FluidValue;

        /// <summary>EXPONENTIAL, HARMONIC, HYPERBOLIC or MODIFIED_HYPERBOLIC.</summary>
        [MaxLength(32)]
        public string MODEL
        {
            get => ModelValue;
            set => SetProperty(ref ModelValue, value);
        }

        private string ModelValue = string.Empty;

        /// <summary>OK or REFUSED. A refused fit carries no parameters (the state table's refusals).</summary>
        [MaxLength(16)]
        public string OUTCOME
        {
            get => OutcomeValue;
            set => SetProperty(ref OutcomeValue, value);
        }

        private string OutcomeValue = "OK";

        [MaxLength(64)]
        public string? REFUSAL_CODE
        {
            get => RefusalCodeValue;
            set => SetProperty(ref RefusalCodeValue, value);
        }

        private string? RefusalCodeValue;

        [MaxLength(512)]
        public string? REFUSAL_MESSAGE
        {
            get => RefusalMessageValue;
            set => SetProperty(ref RefusalMessageValue, value);
        }

        private string? RefusalMessageValue;

        public decimal? QI
        {
            get => QiValue;
            set => SetProperty(ref QiValue, value);
        }

        private decimal? QiValue;

        /// <summary>Initial nominal decline rate per day.</summary>
        public decimal? DI_PER_DAY
        {
            get => DiValue;
            set => SetProperty(ref DiValue, value);
        }

        private decimal? DiValue;

        public decimal? B
        {
            get => BValue;
            set => SetProperty(ref BValue, value);
        }

        private decimal? BValue;

        /// <summary>'Y' when b sits inside the configured bounds; 'N' is a refusal, not a result (SG-06-6).</summary>
        [MaxLength(1)]
        public string BOUNDED_IND
        {
            get => BoundedIndValue;
            set => SetProperty(ref BoundedIndValue, value);
        }

        private string BoundedIndValue = "Y";

        /// <summary>Estimated ultimate recovery at the economic limit, in the history's own unit.</summary>
        public decimal? EUR
        {
            get => EurValue;
            set => SetProperty(ref EurValue, value);
        }

        private decimal? EurValue;

        public decimal? ECONOMIC_LIMIT
        {
            get => EconomicLimitValue;
            set => SetProperty(ref EconomicLimitValue, value);
        }

        private decimal? EconomicLimitValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        /// <summary>Coefficient of determination of the fit, in ln(rate) space.</summary>
        public decimal? R_SQUARED
        {
            get => RSquaredValue;
            set => SetProperty(ref RSquaredValue, value);
        }

        private decimal? RSquaredValue;

        public int POINTS_USED
        {
            get => PointsUsedValue;
            set => SetProperty(ref PointsUsedValue, value);
        }

        private int PointsUsedValue;

        public int POINTS_EXCLUDED
        {
            get => PointsExcludedValue;
            set => SetProperty(ref PointsExcludedValue, value);
        }

        private int PointsExcludedValue;

        /// <summary>Every excluded point with its reason, one per line (AC-3).</summary>
        public string? EXCLUSIONS_TEXT
        {
            get => ExclusionsTextValue;
            set => SetProperty(ref ExclusionsTextValue, value);
        }

        private string? ExclusionsTextValue;

        public DateTime? HISTORY_FROM
        {
            get => HistoryFromValue;
            set => SetProperty(ref HistoryFromValue, value);
        }

        private DateTime? HistoryFromValue;

        public DateTime? HISTORY_TO
        {
            get => HistoryToValue;
            set => SetProperty(ref HistoryToValue, value);
        }

        private DateTime? HistoryToValue;

        // ---- the independent cross-check (AC-4) ----

        [MaxLength(128)]
        public string? CROSS_CHECK_SOURCE
        {
            get => CrossCheckSourceValue;
            set => SetProperty(ref CrossCheckSourceValue, value);
        }

        private string? CrossCheckSourceValue;

        public decimal? CROSS_CHECK_QI
        {
            get => CrossCheckQiValue;
            set => SetProperty(ref CrossCheckQiValue, value);
        }

        private decimal? CrossCheckQiValue;

        public decimal? CROSS_CHECK_B
        {
            get => CrossCheckBValue;
            set => SetProperty(ref CrossCheckBValue, value);
        }

        private decimal? CrossCheckBValue;

        /// <summary>'Y' agree, 'N' disagree, null when the cross-check could not be run.</summary>
        [MaxLength(1)]
        public string? CROSS_CHECK_AGREES
        {
            get => CrossCheckAgreesValue;
            set => SetProperty(ref CrossCheckAgreesValue, value);
        }

        private string? CrossCheckAgreesValue;

        [MaxLength(512)]
        public string? CROSS_CHECK_TEXT
        {
            get => CrossCheckTextValue;
            set => SetProperty(ref CrossCheckTextValue, value);
        }

        private string? CrossCheckTextValue;

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
