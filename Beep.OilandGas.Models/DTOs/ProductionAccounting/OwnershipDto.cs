using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: DivisionOrderDto, OwnerInformationDto, and OwnershipInterestDto are defined in OwnershipModelsDto.cs
    // This file contains additional DTOs and request classes for ownership operations.

    /// <summary>
    /// DTO for ownership tree node
    /// </summary>
    public class OwnershipTreeNodeDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public List<OwnershipTreeNodeDto> Children { get; set; } = new();
    }

    /// <summary>
    /// Request to register ownership interest
    /// </summary>
    public class RegisterOwnershipInterestRequest
    {
        [Required]
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        [Required]
        public OwnerInformationDto Owner { get; set; } = new();
        [Required]
        [Range(0, 1)]
        public decimal WorkingInterest { get; set; }
        [Required]
        [Range(0, 1)]
        public decimal NetRevenueInterest { get; set; }
        [Range(0, 1)]
        public decimal? RoyaltyInterest { get; set; }
        [Range(0, 1)]
        public decimal? OverridingRoyaltyInterest { get; set; }
        [Required]
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}

