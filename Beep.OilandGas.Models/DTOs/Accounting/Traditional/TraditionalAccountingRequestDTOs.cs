using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Traditional
{
    /// <summary>
    /// Request DTO for creating an inventory transaction
    /// </summary>
    public class CreateInventoryTransactionRequest
    {
        public string InventoryItemId { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // Receipt, Issue, Adjustment
        public DateTime? TransactionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? Description { get; set; }
    }
}



