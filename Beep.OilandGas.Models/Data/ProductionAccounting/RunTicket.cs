using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for run ticket data
    /// </summary>
    public class RunTicket : ModelEntityBase
    {
        public string RunTicketNumber { get; set; } = string.Empty;
        public DateTime TicketDateTime { get; set; }
        public string LeaseId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? TankBatteryId { get; set; }
        public decimal GrossVolume { get; set; }
        public decimal BSWVolume { get; set; }
        public decimal BSWPercentage { get; set; }
        public decimal NetVolume { get; set; }
        public decimal Temperature { get; set; }
        public decimal? ApiGravity { get; set; }
        public CrudeOilProperties? Properties { get; set; }
        public DispositionType DispositionType { get; set; }
        public string Purchaser { get; set; } = string.Empty;
        public MeasurementMethod? MeasurementMethod { get; set; }
    }

    /// <summary>
    /// DTO for measurement method
    /// </summary>
    public class MeasurementMethod : ModelEntityBase
    {
        public string Method { get; set; } = string.Empty;
        public string Standard { get; set; } = string.Empty;
    }

    /// <summary>
    /// Type of disposition.
    /// </summary>
    public enum DispositionType
    {
        /// <summary>
        /// Sale to purchaser.
        /// </summary>
        Sale,

        /// <summary>
        /// Transfer to another location.
        /// </summary>
        Transfer,

        /// <summary>
        /// Exchange transaction.
        /// </summary>
        Exchange,

        /// <summary>
        /// Inventory (not disposed).
        /// </summary>
        Inventory,

        /// <summary>
        /// Royalty in kind.
        /// </summary>
        RoyaltyInKind,

        /// <summary>
        /// Working interest in kind.
        /// </summary>
        WorkingInterestInKind
    }

    /// <summary>
    /// Request to create a run ticket
    /// </summary>
    public class CreateRunTicketRequest : ModelEntityBase
    {
        [Required]
        public string LeaseId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? TankBatteryId { get; set; }
        [Required]
        public decimal GrossVolume { get; set; }
        [Range(0, 100)]
        public decimal BSWPercentage { get; set; }
        public decimal Temperature { get; set; } = 60m;
        public decimal? ApiGravity { get; set; }
        public CrudeOilProperties? Properties { get; set; }
        [Required]
        public DispositionType DispositionType { get; set; }
        [Required]
        public string Purchaser { get; set; } = string.Empty;
        public DateTime? TicketDateTime { get; set; }
        public MeasurementMethod MeasurementMethod { get; set; }
    }
}





