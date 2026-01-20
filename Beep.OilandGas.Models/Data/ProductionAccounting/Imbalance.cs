using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: Nomination, ActualDelivery, and ImbalanceAdjustment are defined in ImbalanceModelsDto.cs
    // This file contains request classes for imbalance operations.

    /// <summary>
    /// DTO for production avail
    /// </summary>
    public class ProductionAvail : ModelEntityBase
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
    public class Imbalance : ModelEntityBase
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
    public class ReconcileImbalanceRequest : ModelEntityBase
    {
        [Required]
        public string ImbalanceId { get; set; } = string.Empty;
        [Required]
        public List<ImbalanceAdjustment> Adjustments { get; set; } = new();
        [Required]
        public string ReconciledBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}





