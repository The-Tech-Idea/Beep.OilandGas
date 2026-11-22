using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// The recorded result of a casing-design check on one design version (DEV-03 AC-3). It is linked to the design it
    /// checked and keeps the findings, so a later design revision does not claim the old result.
    /// </summary>
    public class DEVELOPMENT_CASING_CHECK : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string CHECK_ID
        {
            get => CheckIdValue;
            set => SetProperty(ref CheckIdValue, value);
        }

        private string CheckIdValue = string.Empty;

        [MaxLength(64)]
        public string DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string DesignIdValue = string.Empty;

        /// <summary>PASS or FAIL.</summary>
        [MaxLength(16)]
        public string? OUTCOME
        {
            get => OutcomeValue;
            set => SetProperty(ref OutcomeValue, value);
        }

        private string? OutcomeValue;

        /// <summary>One finding per line; a passing check records what was verified.</summary>
        public string? FINDINGS_TEXT
        {
            get => FindingsTextValue;
            set => SetProperty(ref FindingsTextValue, value);
        }

        private string? FindingsTextValue;

        [MaxLength(128)]
        public string? CHECKED_BY
        {
            get => CheckedByValue;
            set => SetProperty(ref CheckedByValue, value);
        }

        private string? CheckedByValue;

        public DateTime? CHECKED_AT
        {
            get => CheckedAtValue;
            set => SetProperty(ref CheckedAtValue, value);
        }

        private DateTime? CheckedAtValue;
    }
}
