using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// The reference set of non-productive-time reason codes (DEV-06 AC-5). The application does not ship a taxonomy of
    /// its own: the list is data the operating company (or its IADC code book) provides, and an NPT line whose code is
    /// not in this table is refused rather than accepted as free text.
    /// </summary>
    public class R_DRILLING_NPT_CODE : ModelEntityBase
    {
        [Key, MaxLength(32)]
        public string NPT_CODE
        {
            get => NptCodeValue;
            set => SetProperty(ref NptCodeValue, value);
        }

        private string NptCodeValue = string.Empty;

        [MaxLength(128)]
        public string? NPT_NAME
        {
            get => NptNameValue;
            set => SetProperty(ref NptNameValue, value);
        }

        private string? NptNameValue;

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
