using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Request to create an AFE (Authorization for Expenditure)
    /// </summary>
    public class CreateAFERequest
    {
        [Required(ErrorMessage = "AfeNumber is required")]
        public string AfeNumber { get; set; } = string.Empty;

        public string? AfeName { get; set; }

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "BudgetAmount must be greater than 0")]
        public decimal BudgetAmount { get; set; }

        public DateTime? EffectiveDate { get; set; }
    }

    /// <summary>
    /// Request for cost allocation
    /// </summary>
    public class CostAllocationRequest
    {
        [Required(ErrorMessage = "CostTransactionId is required")]
        public string CostTransactionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AllocationMethod is required")]
        public string AllocationMethod { get; set; } = string.Empty;

        public List<CostAllocationTarget> Targets { get; set; } = new();
    }

    /// <summary>
    /// Cost allocation target
    /// </summary>
    public class CostAllocationTarget
    {
        [Required(ErrorMessage = "TargetId is required")]
        public string TargetId { get; set; } = string.Empty;

        public string TargetType { get; set; } = string.Empty; // Well, Lease, Property, etc.
        public decimal? AllocationPercentage { get; set; }
        public decimal? AllocationAmount { get; set; }
    }

    /// <summary>
    /// Request to create a cost transaction
    /// </summary>
    public class CreateCostTransactionRequest
    {
        [Required(ErrorMessage = "TransactionDate is required")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "CostCategory is required")]
        public string CostCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? PropertyId { get; set; }
        public string? WellId { get; set; }
        public string? LeaseId { get; set; }
        public string? Description { get; set; }
        public string? VendorId { get; set; }
        public string? InvoiceNumber { get; set; }
    }
}
