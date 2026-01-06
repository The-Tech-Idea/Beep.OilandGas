using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    // NOTE: NominationDto, ActualDeliveryDto, and ImbalanceAdjustmentDto are defined in ImbalanceModelsDto.cs
    // This file contains request classes for imbalance operations.

    /// <summary>
    /// DTO for production avail
    /// </summary>
    public class ProductionAvailDto
    {
        public string AvailId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime AvailDate { get; set; }
        public decimal EstimatedVolume { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for imbalance
    /// </summary>
    public class ImbalanceDto
    {
        public string ImbalanceId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime ImbalanceDate { get; set; }
        public decimal ProductionAvail { get; set; }
        public decimal NominatedVolume { get; set; }
        public decimal ActualDelivered { get; set; }
        public decimal ImbalanceVolume { get; set; }
        public string ImbalanceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to reconcile imbalance
    /// </summary>
    public class ReconcileImbalanceRequest
    {
        [Required]
        public string ImbalanceId { get; set; } = string.Empty;
        [Required]
        public List<ImbalanceAdjustmentDto> Adjustments { get; set; } = new();
        [Required]
        public string ReconciledBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}




