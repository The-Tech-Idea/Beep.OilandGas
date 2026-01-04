using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Accounting.Revenue
{
    /// <summary>
    /// Request DTO for creating a revenue transaction
    /// </summary>
    public class CreateRevenueTransactionRequest
    {
        public decimal RevenueAmount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? RunTicketNumber { get; set; }
        public string? PropertyId { get; set; }
    }

    /// <summary>
    /// Request DTO for revenue allocation
    /// </summary>
    public class RevenueAllocationRequest
    {
        public string PropertyId { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public DateTime AllocationDate { get; set; }
        public string? RevenueTransactionId { get; set; }
        public string? AllocationMethod { get; set; }
        public List<WorkingInterestRequest>? WorkingInterests { get; set; }
    }

    /// <summary>
    /// Request DTO for working interest
    /// </summary>
    public class WorkingInterestRequest
    {
        public string OwnerId { get; set; } = string.Empty;
        public decimal InterestPercentage { get; set; }
    }
}
