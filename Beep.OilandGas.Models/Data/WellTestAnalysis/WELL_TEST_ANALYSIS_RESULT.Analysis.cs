using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// The analysis columns of a well test result (WTA-01): the radial-flow window it was fitted over, the derivative's
    /// verdict, the independent Horner cross-check, the units and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held the permeability, the skin, the model and the fit quality, but nothing about which part of
    /// the test was used - which is what AC-6 requires and what makes a result reproducible. These columns are additive, so
    /// an existing row stays valid.
    /// </remarks>
    public partial class WELL_TEST_ANALYSIS_RESULT
    {
        /// <summary>The start of the radial-flow window, in hours of test time (AC-6).</summary>
        public decimal? WINDOW_START_HOURS
        {
            get => WindowStartValue;
            set => SetProperty(ref WindowStartValue, value);
        }

        private decimal? WindowStartValue;

        /// <summary>The end of the radial-flow window, in hours of test time.</summary>
        public decimal? WINDOW_END_HOURS
        {
            get => WindowEndValue;
            set => SetProperty(ref WindowEndValue, value);
        }

        private decimal? WindowEndValue;

        /// <summary>How many readings fell inside the window and were analysed.</summary>
        public int? POINTS_IN_WINDOW
        {
            get => PointsValue;
            set => SetProperty(ref PointsValue, value);
        }

        private int? PointsValue;

        /// <summary>The window as the screen prints it, so the record says what was fitted.</summary>
        [MaxLength(128)]
        public string? WINDOW_TEXT
        {
            get => WindowTextValue;
            set => SetProperty(ref WindowTextValue, value);
        }

        private string? WindowTextValue;

        /// <summary>'Y' when the analysis converged; 'N' when it was refused or unusable.</summary>
        [MaxLength(1)]
        public string CONVERGED_IND
        {
            get => ConvergedValue;
            set => SetProperty(ref ConvergedValue, value);
        }

        private string ConvergedValue = "N";

        // ---- the derivative diagnostic (SG-08-3) ----

        /// <summary>'Y' when the derivative produced usable points; a non-converged derivative is never plotted as data.</summary>
        [MaxLength(1)]
        public string DERIVATIVE_USABLE_IND
        {
            get => DerivativeUsableValue;
            set => SetProperty(ref DerivativeUsableValue, value);
        }

        private string DerivativeUsableValue = "N";

        /// <summary>Why the derivative is unusable, when it is.</summary>
        [MaxLength(512)]
        public string? DERIVATIVE_REASON
        {
            get => DerivativeReasonValue;
            set => SetProperty(ref DerivativeReasonValue, value);
        }

        private string? DerivativeReasonValue;

        public int? DERIVATIVE_POINTS
        {
            get => DerivativePointsValue;
            set => SetProperty(ref DerivativePointsValue, value);
        }

        private int? DerivativePointsValue;

        // ---- the independent cross-check (AC-4, SG-08-2) ----

        [MaxLength(128)]
        public string? CROSS_CHECK_SOURCE
        {
            get => CrossCheckSourceValue;
            set => SetProperty(ref CrossCheckSourceValue, value);
        }

        private string? CrossCheckSourceValue;

        /// <summary>The cross-check's semilog slope, psi per log cycle.</summary>
        public decimal? CROSS_CHECK_SLOPE
        {
            get => CrossCheckSlopeValue;
            set => SetProperty(ref CrossCheckSlopeValue, value);
        }

        private decimal? CrossCheckSlopeValue;

        /// <summary>The slope this analysis implies, from its own permeability, so the two can be compared directly.</summary>
        public decimal? ANALYSIS_SLOPE
        {
            get => AnalysisSlopeValue;
            set => SetProperty(ref AnalysisSlopeValue, value);
        }

        private decimal? AnalysisSlopeValue;

        public decimal? CROSS_CHECK_PERMEABILITY
        {
            get => CrossCheckPermeabilityValue;
            set => SetProperty(ref CrossCheckPermeabilityValue, value);
        }

        private decimal? CrossCheckPermeabilityValue;

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

        /// <summary>The units of the readings the analysis consumed, so a mixed-unit run is visible after the fact.</summary>
        [MaxLength(128)]
        public string? DATA_UNITS_TEXT
        {
            get => DataUnitsValue;
            set => SetProperty(ref DataUnitsValue, value);
        }

        private string? DataUnitsValue;

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
