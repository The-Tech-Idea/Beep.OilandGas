using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: LeaseProvisionsDto and LeaseLocationDto are defined in LeaseModelsDto.cs
    // LeaseType enum is also defined in LeaseModelsDto.cs
    // This file contains DTO and request classes for lease operations.

    /// <summary>
    /// DTO for lease agreement data
    /// </summary>
    public class LeaseDto
    {
        public string LeaseId { get; set; } = string.Empty;
        public string LeaseName { get; set; } = string.Empty;
        public LeaseType LeaseType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int PrimaryTermMonths { get; set; }
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal RoyaltyRate { get; set; }
        public LeaseProvisionsDto? Provisions { get; set; }
        public LeaseLocationDto? Location { get; set; }
    }

    /// <summary>
    /// Request to create or update a lease
    /// </summary>
    public class CreateLeaseRequest
    {
        [Required]
        public string LeaseId { get; set; } = string.Empty;
        [Required]
        public string LeaseName { get; set; } = string.Empty;
        [Required]
        public LeaseType LeaseType { get; set; }
        [Required]
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        [Range(1, int.MaxValue)]
        public int PrimaryTermMonths { get; set; }
        [Range(0, 1)]
        public decimal WorkingInterest { get; set; }
        [Range(0, 1)]
        public decimal NetRevenueInterest { get; set; }
        [Range(0, 1)]
        public decimal RoyaltyRate { get; set; }
        public LeaseProvisionsDto? Provisions { get; set; }
        public LeaseLocationDto? Location { get; set; }
    }
}

