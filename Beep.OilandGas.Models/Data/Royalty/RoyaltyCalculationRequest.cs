using System;

namespace Beep.OilandGas.Models.Data.Royalty
{
    public class RoyaltyCalculationRequest
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime ProductionMonth { get; set; }
        public decimal GrossProductionVolume { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal Deductions { get; set; }
        public string ProductType { get; set; } = "Oil"; // Oil, Gas, NGL
    }
}
