using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Represents a royalty statement.
    /// </summary>
    public class RoyaltyStatement
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
        /// Gets or sets the royalty owner identifier.
        /// </summary>
        public string RoyaltyOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string PropertyOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the production summary.
        /// </summary>
        public ProductionSummary Production { get; set; } = new();

        /// <summary>
        /// Gets or sets the revenue summary.
        /// </summary>
        public RevenueSummary Revenue { get; set; } = new();

        /// <summary>
        /// Gets or sets the deductions summary.
        /// </summary>
        public DeductionsSummary Deductions { get; set; } = new();

        /// <summary>
        /// Gets or sets the royalty calculation.
        /// </summary>
        public RoyaltyCalculation Calculation { get; set; } = new();

        /// <summary>
        /// Gets or sets the payment information.
        /// </summary>
        public RoyaltyPayment? Payment { get; set; }
        public decimal TotalRoyaltyAmount { get; internal set; }
    }

    /// <summary>
    /// Represents production summary.
    /// </summary>
    public class ProductionSummary
    {
        /// <summary>
        /// Gets or sets the total oil production in barrels.
        /// </summary>
        public decimal TotalOilProduction { get; set; }

        /// <summary>
        /// Gets or sets the total gas production in MCF.
        /// </summary>
        public decimal TotalGasProduction { get; set; }

        /// <summary>
        /// Gets or sets the number of producing days.
        /// </summary>
        public int ProducingDays { get; set; }
    }

    /// <summary>
    /// Represents revenue summary.
    /// </summary>
    public class RevenueSummary
    {
        /// <summary>
        /// Gets or sets the gross revenue.
        /// </summary>
        public decimal GrossRevenue { get; set; }

        /// <summary>
        /// Gets or sets the average price per barrel.
        /// </summary>
        public decimal AveragePricePerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the number of sales transactions.
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Represents deductions summary.
    /// </summary>
    public class DeductionsSummary
    {
        /// <summary>
        /// Gets or sets the total production taxes.
        /// </summary>
        public decimal TotalProductionTaxes { get; set; }

        /// <summary>
        /// Gets or sets the total transportation costs.
        /// </summary>
        public decimal TotalTransportationCosts { get; set; }

        /// <summary>
        /// Gets or sets the total processing costs.
        /// </summary>
        public decimal TotalProcessingCosts { get; set; }

        /// <summary>
        /// Gets or sets the total other deductions.
        /// </summary>
        public decimal TotalOtherDeductions { get; set; }

        /// <summary>
        /// Gets the total deductions.
        /// </summary>
        public decimal TotalDeductions => 
            TotalProductionTaxes + 
            TotalTransportationCosts + 
            TotalProcessingCosts + 
            TotalOtherDeductions;
    }
}

