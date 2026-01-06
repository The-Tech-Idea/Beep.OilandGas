using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Ownership
{
    /// <summary>
    /// Request DTO for registering ownership interest
    /// </summary>
    public class RegisterOwnershipInterestRequest
    {
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        public OwnerRequest Owner { get; set; } = new();
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    /// <summary>
    /// Request DTO for owner information
    /// </summary>
    public class OwnerRequest
    {
        public string OwnerName { get; set; } = string.Empty;
        public string? TaxId { get; set; }
    }
}



