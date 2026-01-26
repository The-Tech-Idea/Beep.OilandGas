using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentCostEscalationFactorEntry : ModelEntityBase
    {
        public string CostCategory { get; set; } = string.Empty;
        public double Factor { get; set; }
    }
}
