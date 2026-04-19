using System;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class BonusPaymentResult : ModelEntityBase
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal BonusAmount { get; set; }
        public decimal CostPerAcre { get; set; }
        public decimal TotalArea { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
    }
}
