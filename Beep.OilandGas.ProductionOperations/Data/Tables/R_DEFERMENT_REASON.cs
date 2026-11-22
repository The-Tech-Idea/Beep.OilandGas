using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// The reference set of deferment reason codes (PRD-03 AC-1). The application ships no taxonomy of its own: the codes
    /// are the operating company's (or the regulator's) data, and a deferment whose code is not in this table is refused
    /// rather than accepted as free text.
    /// </summary>
    public class R_DEFERMENT_REASON : ModelEntityBase
    {
        [Key, MaxLength(32)]
        public string REASON_CODE
        {
            get => ReasonCodeValue;
            set => SetProperty(ref ReasonCodeValue, value);
        }

        private string ReasonCodeValue = string.Empty;

        [MaxLength(128)]
        public string? REASON_NAME
        {
            get => ReasonNameValue;
            set => SetProperty(ref ReasonNameValue, value);
        }

        private string? ReasonNameValue;

        /// <summary>For example EQUIPMENT, PROCESS, RESERVOIR, MARKET, WEATHER, REGULATORY.</summary>
        [MaxLength(64)]
        public string? CATEGORY
        {
            get => CategoryValue;
            set => SetProperty(ref CategoryValue, value);
        }

        private string? CategoryValue;

        [MaxLength(1)]
        public string ACTIVE_IND
        {
            get => ActiveIndValue;
            set => SetProperty(ref ActiveIndValue, value);
        }

        private string ActiveIndValue = "Y";
    }
}
