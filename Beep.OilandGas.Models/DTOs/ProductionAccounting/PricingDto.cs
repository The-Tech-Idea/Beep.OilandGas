using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: RUN_TICKET_VALUATIONDto, QualityAdjustmentsDto, LocationAdjustmentsDto, TimeAdjustmentsDto, and PriceIndexDto 
    // are defined in PricingModelsDto.cs
    // This file contains request classes for pricing operations.

    /// <summary>
    /// Request to value a run ticket
    /// </summary>
    public class ValueRunTicketRequest
    {
        [Required]
        public string RunTicketNumber { get; set; } = string.Empty;
        [Required]
        public PricingMethod PricingMethod { get; set; }
        public decimal? FixedPrice { get; set; }
        public string? IndexName { get; set; }
        public decimal? Differential { get; set; }
        public DateTime? ValuationDate { get; set; }
    }

    /// <summary>
    /// Request to add or update price index
    /// </summary>
    public class PriceIndexRequest
    {
        [Required]
        public string IndexName { get; set; } = string.Empty;
        [Required]
        public DateTime IndexDate { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public string? Currency { get; set; } = "USD";
    }
}




