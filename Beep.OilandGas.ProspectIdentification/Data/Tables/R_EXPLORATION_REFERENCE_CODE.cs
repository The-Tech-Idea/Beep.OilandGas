using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    /// <summary>
    /// Canonical exploration reference-code catalog used to persist process/module tokens and outcomes
    /// that are not represented by a dedicated PPDM standard <c>R_*</c> table.
    /// </summary>
    public partial class R_EXPLORATION_REFERENCE_CODE : ModelEntityBase
    {
        private string REFERENCE_SETValue;
        public string REFERENCE_SET
        {
            get => REFERENCE_SETValue;
            set => SetProperty(ref REFERENCE_SETValue, value);
        }

        private string REFERENCE_CODEValue;
        public string REFERENCE_CODE
        {
            get => REFERENCE_CODEValue;
            set => SetProperty(ref REFERENCE_CODEValue, value);
        }

        private string LONG_NAMEValue;
        public string LONG_NAME
        {
            get => LONG_NAMEValue;
            set => SetProperty(ref LONG_NAMEValue, value);
        }

        private string SHORT_NAMEValue;
        public string SHORT_NAME
        {
            get => SHORT_NAMEValue;
            set => SetProperty(ref SHORT_NAMEValue, value);
        }

    }
}
