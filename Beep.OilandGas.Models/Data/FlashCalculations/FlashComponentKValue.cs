using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public class FlashComponentKValue : ModelEntityBase
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal KValue { get; set; }
    }
}
