using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One section's verdict for one load case (ENG-02): the load it carries, what the pipe resists, and the utilisation that
    /// decides whether it passes.
    /// </summary>
    /// <remarks>
    /// A verdict is written per section per case rather than as one summary line, so the grid on screen and the record behind it
    /// are the same rows - the feature's <c>OgCasingVerdict</c> - and a failure can be traced to the section and case that caused
    /// it without recomputing anything.
    /// </remarks>
    public class CASING_DESIGN_VERDICT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string VERDICT_ID
        {
            get => VerdictIdValue;
            set => SetProperty(ref VerdictIdValue, value);
        }

        private string VerdictIdValue = string.Empty;

        [MaxLength(64)]
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

        [MaxLength(64)]
        public string CASING_STRING_ID
        {
            get => CasingStringIdValue;
            set => SetProperty(ref CasingStringIdValue, value);
        }

        private string CasingStringIdValue = string.Empty;

        /// <summary>The section as it reads on screen: SURFACE, INTERMEDIATE, PRODUCTION, LINER.</summary>
        [MaxLength(64)]
        public string? SECTION_NAME
        {
            get => SectionNameValue;
            set => SetProperty(ref SectionNameValue, value);
        }

        private string? SectionNameValue;

        /// <summary>BURST, COLLAPSE or TENSION.</summary>
        [MaxLength(16)]
        public string LOAD_CASE
        {
            get => LoadCaseValue;
            set => SetProperty(ref LoadCaseValue, value);
        }

        private string LoadCaseValue = string.Empty;

        /// <summary>The load: psi for burst and collapse, lbf for tension.</summary>
        public decimal LOAD
        {
            get => LoadValue;
            set => SetProperty(ref LoadValue, value);
        }

        private decimal LoadValue;

        /// <summary>The pipe's resistance in the same unit: the API 5C3 rating.</summary>
        public decimal RESISTANCE
        {
            get => ResistanceValue;
            set => SetProperty(ref ResistanceValue, value);
        }

        private decimal ResistanceValue;

        /// <summary>The design utilisation: load times the required factor over the resistance. Above 1 does not pass.</summary>
        public decimal UTILISATION
        {
            get => UtilisationValue;
            set => SetProperty(ref UtilisationValue, value);
        }

        private decimal UtilisationValue;

        /// <summary>The design factor this case was checked against, as it was configured.</summary>
        public decimal REQUIRED_FACTOR
        {
            get => RequiredFactorValue;
            set => SetProperty(ref RequiredFactorValue, value);
        }

        private decimal RequiredFactorValue;

        /// <summary>Y when the utilisation is at or below 1.</summary>
        [MaxLength(1)]
        public string PASSES
        {
            get => PassesValue;
            set => SetProperty(ref PassesValue, value);
        }

        private string PassesValue = "N";
    }
}
