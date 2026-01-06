using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Ownership
{
    public class CreateDivisionOrderRequest
    {
        public string PropertyId { get; set; }
        public string OwnerBaId { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal? RoyaltyInterest { get; set; }
        public decimal? OverridingRoyaltyInterest { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Notes { get; set; }
    }

    public class CreateTransferOrderRequest
    {
        public string PropertyId { get; set; }
        public string FromOwnerBaId { get; set; }
        public string ToOwnerBaId { get; set; }
        public decimal InterestTransferred { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class CreateOwnershipInterestRequest
    {
        public string PropertyId { get; set; }
        public string OwnerBaId { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal? RoyaltyInterest { get; set; }
        public decimal? OverridingRoyaltyInterest { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string DivisionOrderId { get; set; }
    }

    public class OwnershipChangeRequest
    {
        public string PropertyId { get; set; }
        public string ChangeType { get; set; } // DivisionOrder, TransferOrder
        public string ChangeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; }
    }

    public class OwnershipChangeResult
    {
        public string ChangeId { get; set; }
        public string PropertyId { get; set; }
        public string ChangeType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
    }

    public class OwnershipApprovalResult
    {
        public string ChangeId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class OwnershipChangeHistory
    {
        public string ChangeId { get; set; }
        public string PropertyId { get; set; }
        public string ChangeType { get; set; }
        public DateTime ChangeDate { get; set; }
        public string OwnerBaId { get; set; }
        public decimal? InterestBefore { get; set; }
        public decimal? InterestAfter { get; set; }
        public string Status { get; set; }
        public string ApprovedBy { get; set; }
    }
}




