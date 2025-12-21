using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Imbalance
{
    /// <summary>
    /// Manages oil imbalance calculations and reconciliation.
    /// </summary>
    public class ImbalanceManager
    {
        private readonly Dictionary<string, ProductionAvails> avails = new();
        private readonly Dictionary<string, Nomination> nominations = new();
        private readonly Dictionary<string, ActualDelivery> actualDeliveries = new();
        private readonly Dictionary<string, OilImbalance> imbalances = new();
        private readonly Dictionary<string, ImbalanceStatement> statements = new();

        /// <summary>
        /// Creates a production avails estimate.
        /// </summary>
        public ProductionAvails CreateProductionAvails(
            DateTime periodStart,
            DateTime periodEnd,
            decimal estimatedProduction)
        {
            var avails = new ProductionAvails
            {
                AvailsId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                EstimatedProduction = estimatedProduction,
                AvailableForDelivery = estimatedProduction // Simplified - would subtract inventory, etc.
            };

            this.avails[avails.AvailsId] = avails;
            return avails;
        }

        /// <summary>
        /// Creates a nomination.
        /// </summary>
        public Nomination CreateNomination(
            DateTime periodStart,
            DateTime periodEnd,
            decimal nominatedVolume,
            List<string> deliveryPoints)
        {
            var nomination = new Nomination
            {
                NominationId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                NominatedVolume = nominatedVolume,
                DeliveryPoints = deliveryPoints,
                Status = NominationStatus.Pending
            };

            nominations[nomination.NominationId] = nomination;
            return nomination;
        }

        /// <summary>
        /// Approves a nomination.
        /// </summary>
        public void ApproveNomination(string nominationId, string approvedBy)
        {
            if (!nominations.TryGetValue(nominationId, out var nomination))
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            nomination.Status = NominationStatus.Approved;
            nomination.ApprovalDate = DateTime.Now;
            nomination.ApprovedBy = approvedBy;
        }

        /// <summary>
        /// Records an actual delivery.
        /// </summary>
        public void RecordActualDelivery(
            DateTime deliveryDate,
            decimal actualVolume,
            string deliveryPoint,
            string allocationMethod,
            string? runTicketNumber = null)
        {
            var delivery = new ActualDelivery
            {
                DeliveryId = Guid.NewGuid().ToString(),
                DeliveryDate = deliveryDate,
                ActualVolume = actualVolume,
                DeliveryPoint = deliveryPoint,
                AllocationMethod = allocationMethod,
                RunTicketNumber = runTicketNumber
            };

            actualDeliveries[delivery.DeliveryId] = delivery;
        }

        /// <summary>
        /// Calculates imbalance for a period.
        /// </summary>
        public OilImbalance CalculateImbalance(
            string nominationId,
            DateTime periodStart,
            DateTime periodEnd,
            decimal tolerancePercentage = 2.0m)
        {
            if (!nominations.TryGetValue(nominationId, out var nomination))
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            var periodDeliveries = actualDeliveries.Values
                .Where(d => d.DeliveryDate >= periodStart && d.DeliveryDate <= periodEnd)
                .ToList();

            decimal actualVolume = periodDeliveries.Sum(d => d.ActualVolume);

            var imbalance = new OilImbalance
            {
                ImbalanceId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                NominatedVolume = nomination.NominatedVolume,
                ActualVolume = actualVolume,
                TolerancePercentage = tolerancePercentage
            };

            // Determine status
            if (imbalance.IsWithinTolerance)
                imbalance.Status = ImbalanceStatus.Balanced;
            else if (imbalance.ImbalanceAmount > 0)
                imbalance.Status = ImbalanceStatus.OverDelivered;
            else
                imbalance.Status = ImbalanceStatus.UnderDelivered;

            imbalances[imbalance.ImbalanceId] = imbalance;
            return imbalance;
        }

        /// <summary>
        /// Generates an imbalance statement.
        /// </summary>
        public ImbalanceStatement GenerateStatement(
            DateTime periodStart,
            DateTime periodEnd)
        {
            var periodNominations = nominations.Values
                .Where(n => n.PeriodStart >= periodStart && n.PeriodEnd <= periodEnd)
                .ToList();

            var periodDeliveries = actualDeliveries.Values
                .Where(d => d.DeliveryDate >= periodStart && d.DeliveryDate <= periodEnd)
                .ToList();

            var periodImbalances = imbalances.Values
                .Where(i => i.PeriodStart >= periodStart && i.PeriodEnd <= periodEnd)
                .ToList();

            var statement = new ImbalanceStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = periodStart,
                StatementPeriodEnd = periodEnd,
                Nominations = new ImbalanceSummary
                {
                    TotalVolume = periodNominations.Sum(n => n.NominatedVolume),
                    TransactionCount = periodNominations.Count,
                    AverageDailyVolume = periodNominations.Count > 0
                        ? periodNominations.Sum(n => n.NominatedVolume) / (periodEnd - periodStart).Days
                        : 0
                },
                Actuals = new ImbalanceSummary
                {
                    TotalVolume = periodDeliveries.Sum(d => d.ActualVolume),
                    TransactionCount = periodDeliveries.Count,
                    AverageDailyVolume = periodDeliveries.Count > 0
                        ? periodDeliveries.Sum(d => d.ActualVolume) / (periodEnd - periodStart).Days
                        : 0
                },
                Imbalances = periodImbalances
            };

            statements[statement.StatementId] = statement;
            return statement;
        }

        /// <summary>
        /// Reconciles an imbalance.
        /// </summary>
        public ImbalanceReconciliation ReconcileImbalance(
            string imbalanceId,
            List<ImbalanceAdjustment> adjustments,
            string reconciledBy,
            string? notes = null)
        {
            if (!imbalances.TryGetValue(imbalanceId, out var imbalance))
                throw new ArgumentException($"Imbalance {imbalanceId} not found.", nameof(imbalanceId));

            var reconciliation = new ImbalanceReconciliation
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                ReconciliationDate = DateTime.Now,
                TotalImbalanceBefore = imbalance.ImbalanceAmount,
                Adjustments = adjustments,
                ReconciledBy = reconciledBy,
                Notes = notes
            };

            // Update imbalance status
            if (reconciliation.TotalImbalanceAfter == 0 || Math.Abs(reconciliation.TotalImbalanceAfter) <= imbalance.TolerancePercentage / 100m * imbalance.NominatedVolume)
                imbalance.Status = ImbalanceStatus.Reconciled;
            else
                imbalance.Status = ImbalanceStatus.PendingReconciliation;

            // Add reconciliation to statement if exists
            var statement = statements.Values.FirstOrDefault(s => 
                s.StatementPeriodStart <= imbalance.PeriodStart && 
                s.StatementPeriodEnd >= imbalance.PeriodEnd);
            
            if (statement != null)
                statement.Reconciliation = reconciliation;

            return reconciliation;
        }

        /// <summary>
        /// Gets imbalances by status.
        /// </summary>
        public IEnumerable<OilImbalance> GetImbalancesByStatus(ImbalanceStatus status)
        {
            return imbalances.Values.Where(i => i.Status == status);
        }

        /// <summary>
        /// Gets all imbalances requiring reconciliation.
        /// </summary>
        public IEnumerable<OilImbalance> GetImbalancesRequiringReconciliation()
        {
            return imbalances.Values.Where(i => 
                i.Status == ImbalanceStatus.PendingReconciliation ||
                (!i.IsWithinTolerance && i.Status != ImbalanceStatus.Reconciled));
        }
    }
}

