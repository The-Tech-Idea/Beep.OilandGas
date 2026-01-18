#nullable enable

namespace Beep.OilandGas.Models.DTOs.Allocation
{
    /// <summary>
    /// Represents well allocation data for production allocation calculations.
    /// </summary>
    public class WellAllocationDataDto
    {
        public string? WellId { get; set; }
        public string? WellName { get; set; }
        public decimal? AllocationPercentage { get; set; }
        public decimal? ProductionVolume { get; set; }
        public string? AllocationMethod { get; set; }
        public DateTime? AllocationDate { get; set; }
    }
}
