using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Represents a sales statement.
    /// </summary>
    public class SalesStatement
    {
        /// <summary>
        /// Gets or sets the statement identifier.
        /// </summary>
        public string StatementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the statement period start date.
        /// </summary>
        public DateTime StatementPeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the statement period end date.
        /// </summary>
        public DateTime StatementPeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string? PropertyOrLeaseId { get; set; }

        /// <summary>
        /// Gets or sets the sales summary.
        /// </summary>
        public SalesSummary Summary { get; set; } = new();

        /// <summary>
        /// Gets or sets the volume details.
        /// </summary>
        public List<VolumeDetail> VolumeDetails { get; set; } = new();

        /// <summary>
        /// Gets or sets the pricing details.
        /// </summary>
        public List<PricingDetail> PricingDetails { get; set; } = new();

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        public List<SalesTransaction> Transactions { get; set; } = new();
    }

    /// <summary>
    /// Represents a sales summary.
    /// </summary>
    public class SalesSummary
    {
        /// <summary>
        /// Gets or sets the total net volume in barrels.
        /// </summary>
        public decimal TotalNetVolume { get; set; }

        /// <summary>
        /// Gets or sets the average price per barrel.
        /// </summary>
        public decimal AveragePricePerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the total gross revenue.
        /// </summary>
        public decimal TotalGrossRevenue { get; set; }

        /// <summary>
        /// Gets or sets the total costs.
        /// </summary>
        public decimal TotalCosts { get; set; }

        /// <summary>
        /// Gets or sets the total taxes.
        /// </summary>
        public decimal TotalTaxes { get; set; }

        /// <summary>
        /// Gets the total net revenue.
        /// </summary>
        public decimal TotalNetRevenue => TotalGrossRevenue - TotalCosts - TotalTaxes;

        /// <summary>
        /// Gets or sets the number of transactions.
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Represents volume details.
    /// </summary>
    public class VolumeDetail
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        public decimal NetVolume { get; set; }

        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        public string? RunTicketNumber { get; set; }
    }

    /// <summary>
    /// Represents pricing details.
    /// </summary>
    public class PricingDetail
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        public decimal PricePerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the pricing method.
        /// </summary>
        public string PricingMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price index (if applicable).
        /// </summary>
        public string? PriceIndex { get; set; }
    }
}

