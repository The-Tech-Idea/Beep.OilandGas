using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One recorded casing-design check (ENG-02): the inputs it was run with and what came out of it.
    /// </summary>
    /// <remarks>
    /// The owner records that a design's <em>inputs</em> were checked (<see cref="DEVELOPMENT_CASING_CHECK"/>, DEV-03), but not
    /// that the pipe was rated against loads - and with that went the load case, the design factors and the governing result. A
    /// run keeps them, so a verdict can be reproduced without re-entering the inputs, and the detail rows hang off its id.
    /// </remarks>
    public class CASING_DESIGN_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string RUN_ID
        {
            get => RunIdValue;
            set => SetProperty(ref RunIdValue, value);
        }

        private string RunIdValue = string.Empty;

        [MaxLength(64)]
        public string DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string DesignIdValue = string.Empty;

        /// <summary>The mud density outside the casing, in ppg: the collapse load is this gradient at the shoe.</summary>
        public decimal FLUID_GRADIENT_PPG
        {
            get => FluidGradientValue;
            set => SetProperty(ref FluidGradientValue, value);
        }

        private decimal FluidGradientValue;

        /// <summary>The surface test pressure applied over the string, in psi. Zero means no burst case was run.</summary>
        public decimal TEST_PRESSURE_PSI
        {
            get => TestPressureValue;
            set => SetProperty(ref TestPressureValue, value);
        }

        private decimal TestPressureValue;

        /// <summary>Pulling force added to the buoyed string weight, in lbf.</summary>
        public decimal OVERPULL_LBF
        {
            get => OverpullValue;
            set => SetProperty(ref OverpullValue, value);
        }

        private decimal OverpullValue;

        /// <summary>The burst, collapse and tension design factors as they were applied, as JSON.</summary>
        [MaxLength(1000)]
        public string? DESIGN_FACTORS_JSON
        {
            get => DesignFactorsValue;
            set => SetProperty(ref DesignFactorsValue, value);
        }

        private string? DesignFactorsValue;

        /// <summary>The section and case with the highest utilisation - AC-3's governing case.</summary>
        [MaxLength(256)]
        public string? GOVERNING_CASE
        {
            get => GoverningCaseValue;
            set => SetProperty(ref GoverningCaseValue, value);
        }

        private string? GoverningCaseValue;

        /// <summary>The governing utilisation: above 1 the design does not pass.</summary>
        public decimal GOVERNING_UTILISATION
        {
            get => GoverningUtilisationValue;
            set => SetProperty(ref GoverningUtilisationValue, value);
        }

        private decimal GoverningUtilisationValue;

        /// <summary>PASS when every verdict meets its design factor, FAIL otherwise.</summary>
        [MaxLength(16)]
        public string? OUTCOME
        {
            get => OutcomeValue;
            set => SetProperty(ref OutcomeValue, value);
        }

        private string? OutcomeValue;

        /// <summary>How many sections were rated.</summary>
        public int SECTIONS_CHECKED
        {
            get => SectionsCheckedValue;
            set => SetProperty(ref SectionsCheckedValue, value);
        }

        private int SectionsCheckedValue;

        [MaxLength(128)]
        public string? CHECKED_BY
        {
            get => CheckedByValue;
            set => SetProperty(ref CheckedByValue, value);
        }

        private string? CheckedByValue;

        public DateTime CHECKED_AT_UTC
        {
            get => CheckedAtValue;
            set => SetProperty(ref CheckedAtValue, value);
        }

        private DateTime CheckedAtValue;
    }
}
