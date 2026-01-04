using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Request for performing production allocation
    /// </summary>
    public class AllocationRequest
    {
        /// <summary>
        /// Total volume to allocate
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "TotalVolume must be greater than or equal to 0")]
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Allocation method to use
        /// </summary>
        [Required]
        public AllocationMethod Method { get; set; }

        /// <summary>
        /// List of entities to allocate to
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one entity is required")]
        public List<AllocationEntityRequest> Entities { get; set; } = new();
    }

    /// <summary>
    /// Entity information for allocation
    /// </summary>
    public class AllocationEntityRequest
    {
        /// <summary>
        /// Entity identifier (well, lease, tract, etc.)
        /// </summary>
        [Required(ErrorMessage = "EntityId is required")]
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Entity name (optional)
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// Working interest (0-1 or 0-100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "WorkingInterest must be between 0 and 100")]
        public decimal? WorkingInterest { get; set; }

        /// <summary>
        /// Net revenue interest (0-1 or 0-100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "NetRevenueInterest must be between 0 and 100")]
        public decimal? NetRevenueInterest { get; set; }

        /// <summary>
        /// Production history for estimated allocation methods
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "ProductionHistory must be greater than or equal to 0")]
        public decimal? ProductionHistory { get; set; }
    }

    /// <summary>
    /// Request for volume reconciliation
    /// </summary>
    public class VolumeReconciliationRequest
    {
        /// <summary>
        /// Field identifier (optional)
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// Start date for reconciliation period
        /// </summary>
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for reconciliation period
        /// </summary>
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }
    }
}
