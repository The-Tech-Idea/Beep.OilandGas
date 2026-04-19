using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculationResultsResponse : ModelEntityBase
    {
        public string? CalculationType { get; set; }
        public List<DCAResult> DcaResults { get; set; } = new();
        public List<EconomicAnalysisResult> EconomicResults { get; set; } = new();
        public List<NodalAnalysisResult> NodalResults { get; set; } = new();
    }
}
