using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculationResultsResponse : ModelEntityBase
    {
        public string? CalculationType { get; set; }
        public List<DCAResult> DcaResults { get; set; } = new();
        public List<EconomicAnalysisResult> EconomicResults { get; set; } = new();
        public List<NodalAnalysisResult> NodalResults { get; set; } = new();
        public List<WELL_TEST_ANALYSIS_RESULT> WellTestResults { get; set; } = new();
    }
}
