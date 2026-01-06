using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Royalty
{
    public class CreateRoyaltyInterestRequest
    {
        public string PropertyId { get; set; }
        public string RoyaltyOwnerBaId { get; set; }
        public string InterestType { get; set; }
        public decimal InterestPercentage { get; set; }
        public decimal? RoyaltyRate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string DivisionOrderId { get; set; }
    }

    public class CreateRoyaltyPaymentRequest
    {
        public string RevenueTransactionId { get; set; }
        public string RoyaltyInterestId { get; set; }
        public string RoyaltyOwnerBaId { get; set; }
        public decimal RoyaltyInterest { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentPeriodStart { get; set; }
        public DateTime PaymentPeriodEnd { get; set; }
    }

    public class CreateRoyaltyStatementRequest
    {
        public string RoyaltyOwnerBaId { get; set; }
        public string PropertyId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class RoyaltyOwnerSummary
    {
        public string OwnerBaId { get; set; }
        public string OwnerName { get; set; }
        public int PropertyCount { get; set; }
        public decimal TotalRoyaltyInterest { get; set; }
        public decimal TotalRoyaltyPaid { get; set; }
        public decimal TotalRoyaltyPending { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }

    public class RoyaltyPaymentApprovalResult
    {
        public string PaymentId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class RoyaltyAuditTrail
    {
        public string AuditId { get; set; }
        public string InterestId { get; set; }
        public string ChangeType { get; set; }
        public DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Description { get; set; }
    }
}




