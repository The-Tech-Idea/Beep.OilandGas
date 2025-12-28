using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: AllocationResultDto and AllocationDetailDto are defined in AllocationModelsDto.cs
    // This file contains request classes for allocation operations.

    /// <summary>
    /// Request to perform allocation
    /// </summary>
    public class AllocationRequest
    {
        [Required]
        public DateTime AllocationDate { get; set; }
        [Required]
        public AllocationMethod Method { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalVolume { get; set; }
        [Required]
        public List<AllocationEntityRequest> Entities { get; set; } = new();
    }

    /// <summary>
    /// Request for allocation entity
    /// </summary>
    public class AllocationEntityRequest
    {
        [Required]
        public string EntityId { get; set; } = string.Empty;
        public string? EntityName { get; set; }
        public decimal? WorkingInterest { get; set; }
        public decimal? NetRevenueInterest { get; set; }
        public decimal? ProductionHistory { get; set; }
    }
}

