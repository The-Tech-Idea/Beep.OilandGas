using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Request to create a journal entry
    /// </summary>
    public class CreateJournalEntryRequest
    {
        public string? EntryNumber { get; set; }

        [Required(ErrorMessage = "EntryDate is required")]
        public DateTime? EntryDate { get; set; }

        public string? EntryType { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "At least one line is required")]
        [MinLength(1, ErrorMessage = "At least one journal entry line is required")]
        public List<JournalEntryLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Request for a journal entry line
    /// </summary>
    public class JournalEntryLineRequest
    {
        [Required(ErrorMessage = "GlAccountId is required")]
        public string GlAccountId { get; set; } = string.Empty;

        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request to create an inventory transaction
    /// </summary>
    public class CreateInventoryTransactionRequest
    {
        [Required(ErrorMessage = "TransactionDate is required")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "TransactionType is required")]
        public string TransactionType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT, etc.

        [Required(ErrorMessage = "ItemId is required")]
        public string ItemId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        public decimal? UnitCost { get; set; }
        public string? LocationId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
    }
}
