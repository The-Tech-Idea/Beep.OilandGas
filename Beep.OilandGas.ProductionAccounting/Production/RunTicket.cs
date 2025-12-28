using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Measurement;
using Beep.OilandGas.ProductionAccounting.Models;

namespace Beep.OilandGas.ProductionAccounting.Production
{
    /// <summary>
    /// Represents a run ticket (delivery ticket).
    /// </summary>
    public class RunTicket
    {
        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        public string RunTicketNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ticket date and time.
        /// </summary>
        public DateTime TicketDateTime { get; set; }

        /// <summary>
        /// Gets or sets the lease or property identifier.
        /// </summary>
        public string LeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the well identifier (if applicable).
        /// </summary>
        public string? WellId { get; set; }

        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        public string? TankBatteryId { get; set; }

        /// <summary>
        /// Gets or sets the gross volume in barrels.
        /// </summary>
        public decimal GrossVolume { get; set; }

        /// <summary>
        /// Gets or sets the BS&W volume in barrels.
        /// </summary>
        public decimal BSWVolume { get; set; }

        /// <summary>
        /// Gets or sets the BS&W percentage (0-100).
        /// </summary>
        public decimal BSWPercentage { get; set; }

        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        public decimal NetVolume => GrossVolume - BSWVolume;

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        public decimal Temperature { get; set; } = 60m;

        /// <summary>
        /// Gets or sets the API gravity.
        /// </summary>
        public decimal? ApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the crude oil properties.
        /// </summary>
        public CrudeOilProperties? Properties { get; set; }

        /// <summary>
        /// Gets or sets the disposition type.
        /// </summary>
        public DispositionType DispositionType { get; set; }

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        public string Purchaser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        public decimal? PricePerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the total value.
        /// </summary>
        public decimal? TotalValue => PricePerBarrel.HasValue ? NetVolume * PricePerBarrel.Value : null;

        /// <summary>
        /// Gets or sets the measurement method.
        /// </summary>
        public Measurement.MeasurementMethod MeasurementMethod { get; set; }

        /// <summary>
        /// Gets or sets the measurement record reference.
        /// </summary>
        public string? MeasurementRecordId { get; set; }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets whether the ticket has been processed.
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Gets or sets the processing date.
        /// </summary>
        public DateTime? ProcessedDate { get; set; }
    }

    /// <summary>
    /// Represents tank battery stock inventory.
    /// </summary>
    public class TankInventory
    {
        /// <summary>
        /// Gets or sets the inventory identifier.
        /// </summary>
        public string InventoryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the inventory date.
        /// </summary>
        public DateTime InventoryDate { get; set; }

        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        public string TankBatteryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the opening inventory in barrels.
        /// </summary>
        public decimal OpeningInventory { get; set; }

        /// <summary>
        /// Gets or sets the receipts (additions) in barrels.
        /// </summary>
        public decimal Receipts { get; set; }

        /// <summary>
        /// Gets or sets the deliveries (dispositions) in barrels.
        /// </summary>
        public decimal Deliveries { get; set; }

        /// <summary>
        /// Gets or sets the adjustments in barrels.
        /// </summary>
        public decimal Adjustments { get; set; }

        /// <summary>
        /// Gets or sets the shrinkage in barrels.
        /// </summary>
        public decimal Shrinkage { get; set; }

        /// <summary>
        /// Gets or sets the theft/loss in barrels.
        /// </summary>
        public decimal TheftLoss { get; set; }

        /// <summary>
        /// Gets the calculated closing inventory.
        /// </summary>
        public decimal ClosingInventory => OpeningInventory + Receipts - Deliveries + Adjustments - Shrinkage - TheftLoss;

        /// <summary>
        /// Gets or sets the actual closing inventory (from measurement).
        /// </summary>
        public decimal? ActualClosingInventory { get; set; }

        /// <summary>
        /// Gets the inventory variance.
        /// </summary>
        public decimal? InventoryVariance => ActualClosingInventory.HasValue 
            ? ActualClosingInventory.Value - ClosingInventory 
            : null;
    }
}

