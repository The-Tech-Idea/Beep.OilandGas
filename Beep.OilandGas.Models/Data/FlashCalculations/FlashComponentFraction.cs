using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public class FlashComponentFraction : ModelEntityBase
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal Fraction { get; set; }

        // Alias for thermodynamic calculator compatibility
        public decimal MoleFraction { get => Fraction; set => Fraction = value; }
    }
}
