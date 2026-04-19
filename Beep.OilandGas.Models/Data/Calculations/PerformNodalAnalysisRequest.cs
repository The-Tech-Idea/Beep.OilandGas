using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PerformNodalAnalysisRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public NodalAnalysisParameters? AnalysisParameters { get; set; }
    }
}
