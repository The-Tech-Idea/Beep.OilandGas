using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public class ValueRunTicketRequest : ModelEntityBase
    {
        public string RunTicketNumber { get; set; }
        public string PricingMethod { get; set; } // Fixed, IndexBased, PostedPrice, Regulated
        public decimal? FixedPrice { get; set; }
        public string IndexName { get; set; }
        public decimal? Differential { get; set; }
        public string RegulatoryAuthority { get; set; }
    }

    public class CreatePriceIndexRequest : ModelEntityBase
    {
        public string IndexName { get; set; }
        public string CommodityType { get; set; }
        public DateTime PriceDate { get; set; }
        public decimal PriceValue { get; set; }
        public string CurrencyCode { get; set; }
        public string PricingPoint { get; set; }
        public string Unit { get; set; }
        public string Source { get; set; }
    }

    public class PricingReconciliationRequest : ModelEntityBase
    {
        public string RunTicketNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class PricingReconciliationResult : ModelEntityBase
    {
        public string ReconciliationId { get; set; }
        public string RunTicketNumber { get; set; }
        public decimal ExpectedValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal Variance { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime ReconciliationDate { get; set; } = DateTime.UtcNow;
    }

    public class PricingApproval : ModelEntityBase
    {
        public string ValuationId { get; set; }
        public string RunTicketNumber { get; set; }
        public decimal TotalValue { get; set; }
        public string PricingMethod { get; set; }
        public string Status { get; set; }
        public DateTime ValuationDate { get; set; }
    }

    public class PricingApprovalResult : ModelEntityBase
    {
        public string ValuationId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }
}





