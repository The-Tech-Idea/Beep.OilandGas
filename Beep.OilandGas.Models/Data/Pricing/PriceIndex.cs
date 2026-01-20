#nullable enable

namespace Beep.OilandGas.Models.Data.Pricing
{
    /// <summary>
    /// Represents a price index reference in the oil and gas domain.
    /// </summary>
    public class PriceIndex : ModelEntityBase
    {
        public string? PriceIndexId { get; set; }
        public string? IndexName { get; set; }
        public string? IndexCode { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
        public DateTime? PriceDate { get; set; }
        public string? Unit { get; set; }
        public string? Source { get; set; }
    }
}
