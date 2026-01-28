using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EconomicAnalysisOptions : ModelEntityBase
    {
        public double? FinanceRate { get; set; }
        public double? ReinvestRate { get; set; }
        public bool? GenerateNpvProfile { get; set; }
        public double? NpvProfileMinRate { get; set; }
        public double? NpvProfileMaxRate { get; set; }
        public int? NpvProfilePoints { get; set; }
    }
}
