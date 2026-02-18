using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class RoyaltyCalculationResult
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal TotalRoyaltyAmount { get; set; }
        public decimal NetRoyaltyAmount { get; set; }
        public decimal DeductionsAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime CalculationDate { get; set; }
        public string Status { get; set; } = "Calculated";
    }
}
