using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The reference set of reconciliation variance reasons (PRD-05 AC-2). The application ships no taxonomy of its own:
    /// the codes are the operating company's closing categories, and a reason that is not in this table is refused rather
    /// than accepted as free text.
    /// </summary>
    public class R_RECONCILIATION_REASON : ModelEntityBase
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

        /// <summary>For example MEASUREMENT, ALLOCATION, SALES, TIMING, DATA.</summary>
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
