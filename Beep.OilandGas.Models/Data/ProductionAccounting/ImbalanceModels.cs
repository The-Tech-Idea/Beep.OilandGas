using System;
using System.Collections.Generic;
using System.Linq;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Nomination status enumeration.
    /// </summary>


    /// <summary>
    /// Imbalance status enumeration.
    /// </summary>


    /// <summary>
    /// Represents production avails estimate (DTO for calculations/reporting).
    /// </summary>
    public class ProductionAvails : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the avails identifier.
        /// </summary>
        private string AvailsIdValue = string.Empty;

        public string AvailsId

        {

            get { return this.AvailsIdValue; }

            set { SetProperty(ref AvailsIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the estimated production in barrels.
        /// </summary>
        private decimal EstimatedProductionValue;

        public decimal EstimatedProduction

        {

            get { return this.EstimatedProductionValue; }

            set { SetProperty(ref EstimatedProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the available for delivery in barrels.
        /// </summary>
        private decimal AvailableForDeliveryValue;

        public decimal AvailableForDelivery

        {

            get { return this.AvailableForDeliveryValue; }

            set { SetProperty(ref AvailableForDeliveryValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocations.
        /// </summary>
        private List<AvailsAllocation> AllocationsValue = new();

        public List<AvailsAllocation> Allocations

        {

            get { return this.AllocationsValue; }

            set { SetProperty(ref AllocationsValue, value); }

        }
    }

    /// <summary>
    /// Represents an avails allocation.
    /// </summary>
    public class AvailsAllocation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the entity identifier (well, lease, etc.).
        /// </summary>
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocated volume in barrels.
        /// </summary>
        private decimal AllocatedVolumeValue;

        public decimal AllocatedVolume

        {

            get { return this.AllocatedVolumeValue; }

            set { SetProperty(ref AllocatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation percentage (0-100).
        /// </summary>
        private decimal AllocationPercentageValue;

        public decimal AllocationPercentage

        {

            get { return this.AllocationPercentageValue; }

            set { SetProperty(ref AllocationPercentageValue, value); }

        }
    }

    /// <summary>
    /// Represents a nomination.
    /// </summary>
    public class Nomination : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the nomination identifier.
        /// </summary>
        private string NominationIdValue = string.Empty;

        public string NominationId

        {

            get { return this.NominationIdValue; }

            set { SetProperty(ref NominationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the nominated volume in barrels.
        /// </summary>
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery points.
        /// </summary>
        private List<string> DeliveryPointsValue = new();

        public List<string> DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private NominationStatus StatusValue = NominationStatus.Pending;

        public NominationStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the submission date.
        /// </summary>
        private DateTime SubmissionDateValue = DateTime.Now;

        public DateTime SubmissionDate

        {

            get { return this.SubmissionDateValue; }

            set { SetProperty(ref SubmissionDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        private string? ApprovedByValue;

        public string? ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
    }

    /// <summary>
    /// Represents an actual delivery.
    /// </summary>
    public class ActualDelivery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery identifier.
        /// </summary>
        private string DeliveryIdValue = string.Empty;

        public string DeliveryId

        {

            get { return this.DeliveryIdValue; }

            set { SetProperty(ref DeliveryIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        private string DeliveryPointValue = string.Empty;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        private string AllocationMethodValue = string.Empty;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }

    /// <summary>
    /// Represents an oil imbalance.
    /// </summary>
    public class OilImbalance : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the imbalance identifier.
        /// </summary>
        private string ImbalanceIdValue = string.Empty;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the nominated volume in barrels.
        /// </summary>
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume in barrels.
        /// </summary>
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }

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
        private ImbalanceStatus StatusValue = ImbalanceStatus.PendingReconciliation;

        public ImbalanceStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the tolerance percentage (0-100).
        /// </summary>
        private decimal TolerancePercentageValue = 2.0m;

        public decimal TolerancePercentage

        {

            get { return this.TolerancePercentageValue; }

            set { SetProperty(ref TolerancePercentageValue, value); }

        }

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
        private string StatementIdValue = string.Empty;

        public string StatementId

        {

            get { return this.StatementIdValue; }

            set { SetProperty(ref StatementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the statement period start date.
        /// </summary>
        private DateTime StatementPeriodStartValue;

        public DateTime StatementPeriodStart

        {

            get { return this.StatementPeriodStartValue; }

            set { SetProperty(ref StatementPeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the statement period end date.
        /// </summary>
        private DateTime StatementPeriodEndValue;

        public DateTime StatementPeriodEnd

        {

            get { return this.StatementPeriodEndValue; }

            set { SetProperty(ref StatementPeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the nominations summary.
        /// </summary>
        private ImbalanceSummary NominationsValue = new();

        public ImbalanceSummary Nominations

        {

            get { return this.NominationsValue; }

            set { SetProperty(ref NominationsValue, value); }

        }

        /// <summary>
        /// Gets or sets the actuals summary.
        /// </summary>
        private ImbalanceSummary ActualsValue = new();

        public ImbalanceSummary Actuals

        {

            get { return this.ActualsValue; }

            set { SetProperty(ref ActualsValue, value); }

        }

        /// <summary>
        /// Gets or sets the imbalances.
        /// </summary>
        private List<OilImbalance> ImbalancesValue = new();

        public List<OilImbalance> Imbalances

        {

            get { return this.ImbalancesValue; }

            set { SetProperty(ref ImbalancesValue, value); }

        }

        /// <summary>
        /// Gets or sets the reconciliation details.
        /// </summary>
        private ImbalanceReconciliation? ReconciliationValue;

        public ImbalanceReconciliation? Reconciliation

        {

            get { return this.ReconciliationValue; }

            set { SetProperty(ref ReconciliationValue, value); }

        }
    }

    /// <summary>
    /// Represents an imbalance summary.
    /// </summary>
    public class ImbalanceSummary : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the total volume in barrels.
        /// </summary>
        private decimal TotalVolumeValue;

        public decimal TotalVolume

        {

            get { return this.TotalVolumeValue; }

            set { SetProperty(ref TotalVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the number of transactions.
        /// </summary>
        private int TransactionCountValue;

        public int TransactionCount

        {

            get { return this.TransactionCountValue; }

            set { SetProperty(ref TransactionCountValue, value); }

        }

        /// <summary>
        /// Gets or sets the average daily volume.
        /// </summary>
        private decimal AverageDailyVolumeValue;

        public decimal AverageDailyVolume

        {

            get { return this.AverageDailyVolumeValue; }

            set { SetProperty(ref AverageDailyVolumeValue, value); }

        }
    }

    /// <summary>
    /// Represents imbalance reconciliation.
    /// </summary>
    public class ImbalanceReconciliation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the reconciliation identifier.
        /// </summary>
        private string ReconciliationIdValue = string.Empty;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the reconciliation date.
        /// </summary>
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the total imbalance before reconciliation.
        /// </summary>
        private decimal TotalImbalanceBeforeValue;

        public decimal TotalImbalanceBefore

        {

            get { return this.TotalImbalanceBeforeValue; }

            set { SetProperty(ref TotalImbalanceBeforeValue, value); }

        }

        /// <summary>
        /// Gets or sets the adjustments made.
        /// </summary>
        private List<ImbalanceAdjustment> AdjustmentsValue = new();

        public List<ImbalanceAdjustment> Adjustments

        {

            get { return this.AdjustmentsValue; }

            set { SetProperty(ref AdjustmentsValue, value); }

        }

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
        private string ReconciledByValue = string.Empty;

        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }

        /// <summary>
        /// Gets or sets any notes.
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Represents an imbalance adjustment.
    /// </summary>
    public class ImbalanceAdjustment : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the adjustment identifier.
        /// </summary>
        private string AdjustmentIdValue = string.Empty;

        public string AdjustmentId

        {

            get { return this.AdjustmentIdValue; }

            set { SetProperty(ref AdjustmentIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the adjustment type.
        /// </summary>
        private string AdjustmentTypeValue = string.Empty;

        public string AdjustmentType

        {

            get { return this.AdjustmentTypeValue; }

            set { SetProperty(ref AdjustmentTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the adjustment amount in barrels.
        /// </summary>
        private decimal AdjustmentAmountValue;

        public decimal AdjustmentAmount

        {

            get { return this.AdjustmentAmountValue; }

            set { SetProperty(ref AdjustmentAmountValue, value); }

        }

        /// <summary>
        /// Gets or sets the reason for adjustment.
        /// </summary>
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }
}








