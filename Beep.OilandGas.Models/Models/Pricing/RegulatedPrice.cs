#nullable enable

namespace Beep.OilandGas.Models.Models.Pricing
{
    /// <summary>
    /// Represents a regulated price in the oil and gas domain.
    /// </summary>
    public class RegulatedPrice
    {
        public string? RegulatedPriceId { get; set; }
        public string? RegulationName { get; set; }
        public decimal? Price { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? Commodity { get; set; }
        public string? Status { get; set; }
    }
}
