namespace Beep.OilandGas.Models.Data.Accounting
{
    public class AccountingActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
    }

    public class ProductionAccountingSummaryDto
    {
        public decimal GrossRevenue { get; set; }
        public decimal TotalOpex { get; set; }
        public decimal NetRevenue { get; set; }
        public int OpenInvoices { get; set; }
        public decimal OutstandingUsd { get; set; }
        public string PeriodStatus { get; set; } = "OPEN";
    }

    public class RevenueLine
    {
        public string Description { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal AmountUsd { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}