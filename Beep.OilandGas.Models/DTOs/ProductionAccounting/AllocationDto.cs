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
        public string AllocationRequest_ID { get; set; }
        public string RUN_TICKET_ID { get; set; }
        public DateTime ALLOCATION_DATE { get; set; }
        public string ALLOCATION_METHOD { get; set; }
        public decimal TOTAL_VOLUME { get; set; }
        public decimal ALLOCATED_VOLUME { get; set; }
        public decimal ALLOCATION_VARIANCE { get; set; }
        public string ACTIVE_IND { get; set; }
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

