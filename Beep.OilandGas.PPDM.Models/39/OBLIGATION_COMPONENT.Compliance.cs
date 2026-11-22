using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.PPDM39.Models
{
    /// <summary>
    /// The compliance column of an obligation component (HSE-05): whether the component is required for the obligation to
    /// count as discharged.
    /// </summary>
    public partial class OBLIGATION_COMPONENT
    {
        /// <summary>'Y' when the component must be discharged, 'N' when it is optional; null reads as required.</summary>
        [MaxLength(1)]
        public string? REQUIRED_IND
        {
            get => RequiredValue;
            set => SetProperty(ref RequiredValue, value);
        }

        private string? RequiredValue;
    }
}
