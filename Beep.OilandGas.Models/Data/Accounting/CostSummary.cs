using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CostSummary
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // Well, Lease, Field
        public string Period { get; set; } = string.Empty; // YYYY-MM
        public decimal TotalOperatingCost { get; set; }
        public decimal TotalCapitalCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
