namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class CostTransactionDto
    {
        public string CostTransactionId { get; set; }
        public string PropertyId { get; set; }
        public string WellId { get; set; }
        public string FieldId { get; set; }
        public string CostType { get; set; }
        public string CostCategory { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string IsCapitalized { get; set; }
        public string IsExpensed { get; set; }
        public string AfeId { get; set; }
        public string CostCenterId { get; set; }
        public string Description { get; set; }
    }

    public class CreateCostTransactionRequest
    {
        public string PropertyId { get; set; }
        public string WellId { get; set; }
        public string FieldId { get; set; }
        public string CostType { get; set; }
        public string CostCategory { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string IsCapitalized { get; set; }
        public string IsExpensed { get; set; }
        public string AfeId { get; set; }
        public string CostCenterId { get; set; }
        public string Description { get; set; }
    }

    public class CostTransactionResponse : CostTransactionDto
    {
    }
}




