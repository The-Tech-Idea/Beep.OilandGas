using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.ProductionAccounting.Storage;

namespace Beep.OilandGas.ProductionAccounting.Production
{
    /// <summary>
    /// Manages production data, run tickets, and dispositions.
    /// </summary>
    public class ProductionManager
    {
        private readonly Dictionary<string, RunTicket> runTickets = new();
        private readonly Dictionary<string, TankInventory> inventories = new();
        private readonly Dictionary<string, List<RunTicket>> ticketsByLease = new();

        /// <summary>
        /// Creates a run ticket from a measurement record.
        /// </summary>
        public RunTicket CreateRunTicket(
            string leaseId,
            string? wellId,
            string? tankBatteryId,
            Measurement.MeasurementRecord measurement,
            DispositionType dispositionType,
            string purchaser)
        {
            if (measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty.", nameof(leaseId));

            var ticket = new RunTicket
            {
                RunTicketNumber = GenerateRunTicketNumber(),
                TicketDateTime = measurement.MeasurementDateTime,
                LeaseId = leaseId,
                WellId = wellId,
                TankBatteryId = tankBatteryId,
                GrossVolume = measurement.GrossVolume,
                BSWVolume = measurement.GrossVolume * (measurement.BSW / 100m),
                BSWPercentage = measurement.BSW,
                Temperature = measurement.Temperature,
                ApiGravity = measurement.ApiGravity,
                Properties = measurement.Properties,
                DispositionType = dispositionType,
                Purchaser = purchaser,
                MeasurementMethod = measurement.Method,
                MeasurementRecordId = measurement.MeasurementId
            };

            runTickets[ticket.RunTicketNumber] = ticket;

            if (!ticketsByLease.ContainsKey(leaseId))
                ticketsByLease[leaseId] = new List<RunTicket>();
            ticketsByLease[leaseId].Add(ticket);

            return ticket;
        }

        /// <summary>
        /// Gets a run ticket by number.
        /// </summary>
        public RunTicket? GetRunTicket(string runTicketNumber)
        {
            return runTickets.TryGetValue(runTicketNumber, out var ticket) ? ticket : null;
        }

        /// <summary>
        /// Gets all run tickets for a lease.
        /// </summary>
        public IEnumerable<RunTicket> GetRunTicketsByLease(string leaseId)
        {
            return ticketsByLease.TryGetValue(leaseId, out var tickets) 
                ? tickets 
                : Enumerable.Empty<RunTicket>();
        }

        /// <summary>
        /// Gets run tickets by date range.
        /// </summary>
        public IEnumerable<RunTicket> GetRunTicketsByDateRange(DateTime startDate, DateTime endDate)
        {
            return runTickets.Values
                .Where(t => t.TicketDateTime >= startDate && t.TicketDateTime <= endDate);
        }

        /// <summary>
        /// Creates a tank inventory record.
        /// </summary>
        public TankInventory CreateTankInventory(
            string tankBatteryId,
            DateTime inventoryDate,
            decimal openingInventory,
            decimal receipts,
            decimal deliveries,
            decimal? actualClosingInventory = null)
        {
            var inventory = new TankInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                InventoryDate = inventoryDate,
                TankBatteryId = tankBatteryId,
                OpeningInventory = openingInventory,
                Receipts = receipts,
                Deliveries = deliveries,
                ActualClosingInventory = actualClosingInventory
            };

            inventories[inventory.InventoryId] = inventory;
            return inventory;
        }

        /// <summary>
        /// Gets tank inventory by ID.
        /// </summary>
        public TankInventory? GetTankInventory(string inventoryId)
        {
            return inventories.TryGetValue(inventoryId, out var inventory) ? inventory : null;
        }

        /// <summary>
        /// Calculates total production for a lease in a date range.
        /// </summary>
        public decimal CalculateTotalProduction(string leaseId, DateTime startDate, DateTime endDate)
        {
            return GetRunTicketsByLease(leaseId)
                .Where(t => t.TicketDateTime >= startDate && t.TicketDateTime <= endDate)
                .Sum(t => t.NetVolume);
        }

        /// <summary>
        /// Calculates total dispositions by type.
        /// </summary>
        public Dictionary<DispositionType, decimal> CalculateDispositionsByType(DateTime startDate, DateTime endDate)
        {
            var tickets = GetRunTicketsByDateRange(startDate, endDate);
            return tickets
                .GroupBy(t => t.DispositionType)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NetVolume));
        }

        /// <summary>
        /// Generates a unique run ticket number.
        /// </summary>
        private string GenerateRunTicketNumber()
        {
            return $"RT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}

