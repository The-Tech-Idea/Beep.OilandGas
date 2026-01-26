using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentWellTypeDistributionEntry : ModelEntityBase
    {
        public string WellType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
