using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class RoyaltyCalculationRequest
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime ProductionMonth { get; set; }
        public decimal GrossProduction { get; set; }
        public string ProductType { get; set; } = string.Empty; // Oil, Gas
    }
}
