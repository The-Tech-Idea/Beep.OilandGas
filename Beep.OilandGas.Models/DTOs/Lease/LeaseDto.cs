using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Lease
{
    public class CreateFeeMineralLeaseRequest
    {
        public string PropertyId { get; set; }
        public string LeaseNumber { get; set; }
        public string LeaseName { get; set; }
        public string MineralOwnerBaId { get; set; }
        public string SurfaceOwnerBaId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int PrimaryTermMonths { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal RoyaltyRate { get; set; }
    }

    public class CreateGovernmentLeaseRequest
    {
        public string PropertyId { get; set; }
        public string GovernmentAgency { get; set; }
        public string LeaseNumber { get; set; }
        public bool IsFederal { get; set; }
        public bool IsIndian { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal RoyaltyRate { get; set; }
    }

    public class LeaseRenewalRequest
    {
        public DateTime NewExpirationDate { get; set; }
        public string RenewalReason { get; set; }
        public decimal? NewRoyaltyRate { get; set; }
    }

    public class LeaseRenewalResult
    {
        public string LeaseId { get; set; }
        public bool IsRenewed { get; set; }
        public DateTime NewExpirationDate { get; set; }
        public DateTime RenewalDate { get; set; } = DateTime.UtcNow;
        public string RenewedBy { get; set; }
    }

    public class LeaseExpirationAlert
    {
        public string LeaseId { get; set; }
        public string LeaseNumber { get; set; }
        public string LeaseName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public string AlertLevel { get; set; }
    }

    public class LeaseSummary
    {
        public string LeaseId { get; set; }
        public string LeaseNumber { get; set; }
        public string LeaseName { get; set; }
        public string LeaseType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal RoyaltyRate { get; set; }
        public bool IsHeldByProduction { get; set; }
    }
}

