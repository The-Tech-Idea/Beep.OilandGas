using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Nomination status enumeration.
    /// </summary>
    public enum NominationStatus
    {
        /// <summary>
        /// Pending approval.
        /// </summary>
        Pending,

        /// <summary>
        /// Approved.
        /// </summary>
        Approved,

        /// <summary>
        /// Rejected.
        /// </summary>
        Rejected,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Imbalance status enumeration.
    /// </summary>
    public enum ImbalanceStatus
    {
        /// <summary>
        /// Balanced (within tolerance).
        /// </summary>
        Balanced,

        /// <summary>
        /// Over-delivered (actual > nominated).
        /// </summary>
        OverDelivered,

        /// <summary>
        /// Under-delivered (actual < nominated).
        /// </summary>
        UnderDelivered,

        /// <summary>
        /// Pending reconciliation.
        /// </summary>
        PendingReconciliation,

        /// <summary>
        /// Reconciled.
        /// </summary>
        Reconciled
    }

    /// <summary>
    /// Represents production avails estimate (DTO for calculations/reporting).
    /// </summary>
    public class ProductionAvails : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the avails identifier.
        /// </summary>
        public string AvailsId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the estimated production in barrels.
        /// </summary>
        public decimal EstimatedProduction { get; set; }

        /// <summary>
        /// Gets or sets the available for delivery in barrels.
        /// </summary>
        public decimal AvailableForDelivery { get; set; }

        /// <summary>
        /// Gets or sets the allocations.
        /// </summary>
        public List<AvailsAllocation> Allocations { get; set; } = new();
    }

    /// <summary>
    /// Represents an avails allocation.
    /// </summary>
    public class AvailsAllocation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the entity identifier (well, lease, etc.).
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the allocated volume in barrels.
        /// </summary>
        public decimal AllocatedVolume { get; set; }

        /// <summary>
        /// Gets or sets the allocation percentage (0-100).
        /// </summary>
        public decimal AllocationPercentage { get; set; }
    }

    /// <summary>
    /// Represents a nomination.
    /// </summary>
    public class Nomination : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the nomination identifier.
        /// </summary>
        public string NominationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the nominated volume in barrels.
        /// </summary>
        public decimal NominatedVolume { get; set; }

        /// <summary>
        /// Gets or sets the delivery points.
        /// </summary>
        public List<string> DeliveryPoints { get; set; } = new();

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public NominationStatus Status { get; set; } = NominationStatus.Pending;

        /// <summary>
        /// Gets or sets the submission date.
        /// </summary>
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        public string? ApprovedBy { get; set; }
    }

    /// <summary>
    /// Represents an actual delivery.
    /// </summary>
    public class ActualDelivery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery identifier.
        /// </summary>
        public string DeliveryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        public decimal ActualVolume { get; set; }

        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        public string DeliveryPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        public string AllocationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        public string? RunTicketNumber { get; set; }
    }

    /// <summary>
    /// Represents an oil imbalance.
    /// </summary>
    public class OilImbalance : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the imbalance identifier.
        /// </summary>
        public string ImbalanceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the nominated volume in barrels.
        /// </summary>
        public decimal NominatedVolume { get; set; }

        /// <summary>
        /// Gets or sets the actual volume in barrels.
        /// </summary>
        public decimal ActualVolume { get; set; }

        /// <summary>
        /// Gets the imbalance amount (actual - nominated).
        /// </summary>
        public decimal ImbalanceAmount => ActualVolume - NominatedVolume;

        /// <summary>
        /// Gets the imbalance percentage.
        /// </summary>
        public decimal ImbalancePercentage => NominatedVolume > 0
            ? (ImbalanceAmount / NominatedVolume) * 100m
            : 0;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ImbalanceStatus Status { get; set; } = ImbalanceStatus.PendingReconciliation;

        /// <summary>
        /// Gets or sets the tolerance percentage (0-100).
        /// </summary>
        public decimal TolerancePercentage { get; set; } = 2.0m;

        /// <summary>
        /// Gets whether the imbalance is within tolerance.
        /// </summary>
        public bool IsWithinTolerance => Math.Abs(ImbalancePercentage) <= TolerancePercentage;
    }

    /// <summary>
    /// Represents an imbalance statement.
    /// </summary>
    public class ImbalanceStatement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the statement identifier.
        /// </summary>
        public string StatementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the statement period start date.
        /// </summary>
        public DateTime StatementPeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the statement period end date.
        /// </summary>
        public DateTime StatementPeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the nominations summary.
        /// </summary>
        public ImbalanceSummary Nominations { get; set; } = new();

        /// <summary>
        /// Gets or sets the actuals summary.
        /// </summary>
        public ImbalanceSummary Actuals { get; set; } = new();

        /// <summary>
        /// Gets or sets the imbalances.
        /// </summary>
        public List<OilImbalance> Imbalances { get; set; } = new();

        /// <summary>
        /// Gets or sets the reconciliation details.
        /// </summary>
        public ImbalanceReconciliation? Reconciliation { get; set; }
    }

    /// <summary>
    /// Represents an imbalance summary.
    /// </summary>
    public class ImbalanceSummary : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the total volume in barrels.
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions.
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// Gets or sets the average daily volume.
        /// </summary>
        public decimal AverageDailyVolume { get; set; }
    }

    /// <summary>
    /// Represents imbalance reconciliation.
    /// </summary>
    public class ImbalanceReconciliation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the reconciliation identifier.
        /// </summary>
        public string ReconciliationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reconciliation date.
        /// </summary>
        public DateTime ReconciliationDate { get; set; }

        /// <summary>
        /// Gets or sets the total imbalance before reconciliation.
        /// </summary>
        public decimal TotalImbalanceBefore { get; set; }

        /// <summary>
        /// Gets or sets the adjustments made.
        /// </summary>
        public List<ImbalanceAdjustment> Adjustments { get; set; } = new();

        /// <summary>
        /// Gets the total adjustments.
        /// </summary>
        public decimal TotalAdjustments => Adjustments.Sum(a => a.AdjustmentAmount);

        /// <summary>
        /// Gets the total imbalance after reconciliation.
        /// </summary>
        public decimal TotalImbalanceAfter => TotalImbalanceBefore + TotalAdjustments;

        /// <summary>
        /// Gets or sets the reconciled by.
        /// </summary>
        public string ReconciledBy { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any notes.
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents an imbalance adjustment.
    /// </summary>
    public class ImbalanceAdjustment : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the adjustment identifier.
        /// </summary>
        public string AdjustmentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the adjustment type.
        /// </summary>
        public string AdjustmentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the adjustment amount in barrels.
        /// </summary>
        public decimal AdjustmentAmount { get; set; }

        /// <summary>
        /// Gets or sets the reason for adjustment.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}





