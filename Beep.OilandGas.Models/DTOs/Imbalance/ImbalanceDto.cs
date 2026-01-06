using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Imbalance
{
    public class CreateProductionAvailRequest
    {
        public string PropertyId { get; set; }
        public DateTime AvailDate { get; set; }
        public decimal EstimatedVolume { get; set; }
        public decimal? AvailableForDelivery { get; set; }
    }

    public class CreateNominationRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal NominatedVolume { get; set; }
        public List<string> DeliveryPoints { get; set; } = new();
    }

    public class CreateActualDeliveryRequest
    {
        public string NominationId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal ActualVolume { get; set; }
        public string DeliveryPoint { get; set; }
        public string AllocationMethod { get; set; }
        public string RunTicketNumber { get; set; }
    }

    public class ImbalanceReconciliationResult
    {
        public string ReconciliationId { get; set; }
        public string ImbalanceId { get; set; }
        public decimal ImbalanceBefore { get; set; }
        public decimal ImbalanceAfter { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime ReconciliationDate { get; set; } = DateTime.UtcNow;
        public string ReconciledBy { get; set; }
    }

    public class ImbalanceSettlementResult
    {
        public string SettlementId { get; set; }
        public string ImbalanceId { get; set; }
        public DateTime SettlementDate { get; set; }
        public decimal SettlementAmount { get; set; }
        public string Status { get; set; }
        public string SettledBy { get; set; }
    }

    public class ImbalanceSummary
    {
        public string PeriodStart { get; set; }
        public string PeriodEnd { get; set; }
        public decimal TotalNominatedVolume { get; set; }
        public decimal TotalActualVolume { get; set; }
        public decimal TotalImbalanceAmount { get; set; }
        public int ImbalanceCount { get; set; }
        public int BalancedCount { get; set; }
        public int OverDeliveredCount { get; set; }
        public int UnderDeliveredCount { get; set; }
    }
}




