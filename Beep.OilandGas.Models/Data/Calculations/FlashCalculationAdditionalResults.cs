using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationAdditionalResults : ModelEntityBase
    {
        public decimal? Pressure { get; set; }
        public decimal? Temperature { get; set; }
        public int? ComponentCount { get; set; }

        /// <summary>
        /// Canonical <c>FLASH_EOS_MODEL</c> code from <c>R_FLASH_CALCULATION_REFERENCE_CODE</c> (PR, SRK, SRK_MODIFIED, IDEAL_K).
        /// </summary>
        public string? EosModelReferenceCode { get; set; }
    }
}
