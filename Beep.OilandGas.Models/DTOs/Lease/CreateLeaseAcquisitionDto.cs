using System;

namespace Beep.OilandGas.Models.DTOs.Lease
{
    /// <summary>
    /// DTO for creating a lease acquisition.
    /// Can represent either a fee mineral lease or government lease.
    /// </summary>
    public class CreateLeaseAcquisitionDto
    {
        public string LeaseType { get; set; } = string.Empty; // "FeeMineral" or "Government"
        public string? PropertyId { get; set; }
        public string LeaseNumber { get; set; } = string.Empty;
        public string LeaseName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? PrimaryTermMonths { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal RoyaltyRate { get; set; }
        
        // Fee Mineral Lease specific
        public string? MineralOwnerBaId { get; set; }
        public string? SurfaceOwnerBaId { get; set; }
        
        // Government Lease specific
        public string? GovernmentAgency { get; set; }
        public bool? IsFederal { get; set; }
        public bool? IsIndian { get; set; }
    }
}

