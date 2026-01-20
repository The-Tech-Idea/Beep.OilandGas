using System;

namespace Beep.OilandGas.Models.Data.Accounting.Pricing
{
    /// <summary>
    /// Request DTO for price index
    /// </summary>
    public class PriceIndexRequest : ModelEntityBase
    {
        public string IndexName { get; set; } = string.Empty;
        public DateTime IndexDate { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
    }

    /// <summary>
    /// Request DTO for valuing a run ticket
    /// </summary>
    public class ValueRunTicketRequest : ModelEntityBase
    {
        public string RunTicketNumber { get; set; } = string.Empty;
        public string PricingMethod { get; set; } = string.Empty;
        public decimal? FixedPrice { get; set; }
        public string? IndexName { get; set; }
        public decimal? Differential { get; set; }
    }
}




