using System;
using Beep.OilandGas.Accounting.Operational.Production;
using Beep.OilandGas.Accounting.Operational.Pricing;

namespace Beep.OilandGas.Accounting.Operational.Revenue
{
    /// <summary>
    /// Represents a wellhead sale (sale at wellhead, no storage).
    /// </summary>
    public class WellheadSale
    {
        /// <summary>
        /// Gets or sets the sale identifier.
        /// </summary>
        public string SaleId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        public string LeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sale date.
        /// </summary>
        public DateTime SaleDate { get; set; }

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        public string Purchaser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        public decimal NetVolume { get; set; }

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        public decimal PricePerBarrel { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => NetVolume * PricePerBarrel;

        /// <summary>
        /// Gets or sets the measurement method.
        /// </summary>
        public Measurement.MeasurementMethod MeasurementMethod { get; set; }

        /// <summary>
        /// Gets or sets whether a run ticket was created.
        /// </summary>
        public bool RunTicketCreated { get; set; }
    }

    /// <summary>
    /// Provides wellhead sale accounting functionality.
    /// </summary>
    public static class WellheadSaleAccounting
    {
        /// <summary>
        /// Creates accounting entries for a wellhead sale.
        /// </summary>
        public static List<SalesJournalEntry> CreateAccountingEntries(WellheadSale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            // Wellhead sales are similar to regular sales but typically don't go through storage
            var transaction = new SalesTransaction
            {
                TransactionId = sale.SaleId,
                RunTicketNumber = sale.RunTicketCreated ? $"WT-{sale.SaleId}" : string.Empty,
                TransactionDate = sale.SaleDate,
                Purchaser = sale.Purchaser,
                NetVolume = sale.NetVolume,
                PricePerBarrel = sale.PricePerBarrel
            };

            return SalesJournalEntryGenerator.CreateEntries(transaction);
        }

        /// <summary>
        /// Creates a run ticket from a wellhead sale.
        /// </summary>
        public static RunTicket CreateRunTicket(WellheadSale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            return new RunTicket
            {
                RunTicketNumber = $"WT-{sale.SaleId}",
                TicketDateTime = sale.SaleDate,
                LeaseId = sale.LeaseId,
                WellId = sale.WellId,
                GrossVolume = sale.NetVolume, // Assume no BS&W for wellhead sales
                BSWVolume = 0,
                BSWPercentage = 0,
                DispositionType = Production.DispositionType.Sale,
                Purchaser = sale.Purchaser,
                PricePerBarrel = sale.PricePerBarrel,
                MeasurementMethod = sale.MeasurementMethod
            };
        }
    }
}

