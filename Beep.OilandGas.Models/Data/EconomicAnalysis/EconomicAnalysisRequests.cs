using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public class CalculateNPVRequest : ModelEntityBase
    {
        public List<double> CashFlows { get; set; } = new();
        public double DISCOUNT_RATE { get; set; }
    }

    public class CalculateIRRRequest : ModelEntityBase
    {
        public List<double> CashFlows { get; set; } = new();
        public double InitialGuess { get; set; } = 0.1;
    }

    public class AnalyzeRequest : ModelEntityBase
    {
        public List<double> CashFlows { get; set; } = new();
        public double DISCOUNT_RATE { get; set; }
        public double FinanceRate { get; set; }
        public double ReinvestRate { get; set; }
    }

    public class GenerateNPVProfileRequest : ModelEntityBase
    {
        public List<double> CashFlows { get; set; } = new();
        public double MinRate { get; set; }
        public double MaxRate { get; set; }
        public int Points { get; set; } = 20;
    }

    public class SaveAnalysisResultRequest : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public EconomicResult? Result { get; set; }
    }
}
