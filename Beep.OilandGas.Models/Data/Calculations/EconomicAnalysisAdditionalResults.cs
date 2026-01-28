using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EconomicAnalysisAdditionalResults : ModelEntityBase
    {
        public List<NPV_PROFILE_POINT>? NpvProfile { get; set; }
        public double? Mirr { get; set; }
        public double? DiscountedPaybackPeriod { get; set; }
        public double? TotalCashFlow { get; set; }
        public double? PresentValue { get; set; }
    }
}
