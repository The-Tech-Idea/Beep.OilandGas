using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// The study columns of a nodal analysis result (LFT-01): both correlations, the convergence and the binding
    /// constraint, the inputs the run used, the independent cross-check and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held only the operating rate, the pressure and a status, which cannot reproduce a study or say
    /// which correlations produced it (AC-5). These columns are additive, so an existing row stays valid.
    /// </remarks>
    public partial class NODAL_ANALYSIS_RESULT
    {
        /// <summary>VOGEL, STANDING or FETKOVICH - the inflow model (AC-5).</summary>
        [MaxLength(32)]
        public string? IPR_MODEL
        {
            get => IprModelValue;
            set => SetProperty(ref IprModelValue, value);
        }

        private string? IprModelValue;

        /// <summary>The outflow correlation the engine actually applied (AC-5).</summary>
        [MaxLength(32)]
        public string? VLP_CORRELATION
        {
            get => VlpCorrelationValue;
            set => SetProperty(ref VlpCorrelationValue, value);
        }

        private string? VlpCorrelationValue;

        /// <summary>'Y' when the two curves cross, 'N' when they do not - the state that must never read as a zero rate.</summary>
        [MaxLength(1)]
        public string CONVERGED
        {
            get => ConvergedValue;
            set => SetProperty(ref ConvergedValue, value);
        }

        private string ConvergedValue = "N";

        /// <summary>The bisection steps the intersection took, for a study that had to be looked at.</summary>
        public int? ITERATIONS
        {
            get => IterationsValue;
            set => SetProperty(ref IterationsValue, value);
        }

        private int? IterationsValue;

        /// <summary>Which limit set the answer: the inflow, the outflow, the wellhead pressure or nothing.</summary>
        [MaxLength(64)]
        public string? BINDING_CONSTRAINT
        {
            get => BindingConstraintValue;
            set => SetProperty(ref BindingConstraintValue, value);
        }

        private string? BindingConstraintValue;

        /// <summary>Why there is no operating point, in words, when there is none.</summary>
        [MaxLength(1024)]
        public string? DIAGNOSTICS
        {
            get => DiagnosticsValue;
            set => SetProperty(ref DiagnosticsValue, value);
        }

        private string? DiagnosticsValue;

        /// <summary>The inputs as they were entered, so the study can be reproduced.</summary>
        public string? INPUTS_JSON
        {
            get => InputsJsonValue;
            set => SetProperty(ref InputsJsonValue, value);
        }

        private string? InputsJsonValue;

        [MaxLength(16)]
        public string? RATE_OUOM
        {
            get => RateOuomValue;
            set => SetProperty(ref RateOuomValue, value);
        }

        private string? RateOuomValue;

        // ---- the independent cross-check (AC-3) ----

        [MaxLength(256)]
        public string? CROSS_CHECK_SOURCE
        {
            get => CrossCheckSourceValue;
            set => SetProperty(ref CrossCheckSourceValue, value);
        }

        private string? CrossCheckSourceValue;

        public decimal? CROSS_CHECK_FLOW_RATE
        {
            get => CrossCheckFlowRateValue;
            set => SetProperty(ref CrossCheckFlowRateValue, value);
        }

        private decimal? CrossCheckFlowRateValue;

        public decimal? CROSS_CHECK_PRESSURE
        {
            get => CrossCheckPressureValue;
            set => SetProperty(ref CrossCheckPressureValue, value);
        }

        private decimal? CrossCheckPressureValue;

        /// <summary>'Y' agree, 'N' disagree, null when the cross-check could not be run.</summary>
        [MaxLength(1)]
        public string? CROSS_CHECK_AGREES
        {
            get => CrossCheckAgreesValue;
            set => SetProperty(ref CrossCheckAgreesValue, value);
        }

        private string? CrossCheckAgreesValue;

        [MaxLength(1024)]
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
