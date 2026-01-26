using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Additional economic analysis results beyond core KPIs.
    /// </summary>
    public class EconomicAnalysisAdditionalResults : ModelEntityBase
    {
        public List<NPVProfilePoint>? NpvProfile { get; set; }
        public double? Mirr { get; set; }
        public double? DiscountedPaybackPeriod { get; set; }
        public double? TotalCashFlow { get; set; }
        public double? PresentValue { get; set; }
    }
}
