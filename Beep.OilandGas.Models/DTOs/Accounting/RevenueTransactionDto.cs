namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class RevenueTransactionDto
    {
        public string RevenueTransactionId { get; set; }
        public string PropertyId { get; set; }
        public string WellId { get; set; }
        public string FieldId { get; set; }
        public string ContractId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string RevenueType { get; set; }
        public decimal? GrossRevenue { get; set; }
        public decimal? NetRevenue { get; set; }
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public decimal? RoyaltyAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
    }

    public class CreateRevenueTransactionRequest
    {
        public string PropertyId { get; set; }
        public string WellId { get; set; }
        public string FieldId { get; set; }
        public string ContractId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string RevenueType { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
    }

    public class RevenueTransactionResponse : RevenueTransactionDto
    {
    }
}

