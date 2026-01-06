using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Cost
{
    /// <summary>
    /// Request DTO for creating a cost transaction
    /// </summary>
    public class CreateCostTransactionRequest
    {
        public decimal CostAmount { get; set; }
        public bool IsCapitalized { get; set; }
        public bool IsCash { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? PropertyId { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request DTO for cost allocation
    /// </summary>
    public class CostAllocationRequest
    {
        public string? FieldId { get; set; }
        public DateTime AllocationDate { get; set; }
        public string AllocationMethod { get; set; } = string.Empty;
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? TotalCapitalCosts { get; set; }
    }
}



