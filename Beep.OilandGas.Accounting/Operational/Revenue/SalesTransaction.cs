using System;
using System.Collections.Generic;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Operational.Production;
using Beep.OilandGas.Accounting.Operational.Pricing;

namespace Beep.OilandGas.Accounting.Operational.Revenue
{
    /// <summary>
    /// Represents a sales transaction.
    /// </summary>
    public class SalesTransaction
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sales agreement reference.
        /// </summary>
        public string? SalesAgreementId { get; set; }

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        public string RunTicketNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

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
        /// Gets or sets the delivery information.
        /// </summary>
        public DeliveryInformation Delivery { get; set; } = new();

        /// <summary>
        /// Gets or sets the production and marketing costs.
        /// </summary>
        public ProductionMarketingCosts Costs { get; set; } = new();

        /// <summary>
        /// Gets or sets the taxes.
        /// </summary>
        public List<ProductionTax> Taxes { get; set; } = new();

        /// <summary>
        /// Gets the net revenue (total value - costs - taxes).
        /// </summary>
        public decimal NetRevenue => TotalValue - Costs.TotalCosts - Taxes.Sum(t => t.Amount);
    }

    /// <summary>
    /// Represents delivery information.
    /// </summary>
    public class DeliveryInformation
    {
        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        public string DeliveryPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery method.
        /// </summary>
        public string DeliveryMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether title transferred at delivery point.
        /// </summary>
        public bool TitleTransferredAtDeliveryPoint { get; set; } = true;
    }

    /// <summary>
    /// Represents production and marketing costs.
    /// </summary>
    public class ProductionMarketingCosts
    {
        /// <summary>
        /// Gets or sets the lifting costs per barrel.
        /// </summary>
        public decimal LiftingCostsPerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the operating costs per barrel.
        /// </summary>
        public decimal OperatingCostsPerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the marketing costs per barrel.
        /// </summary>
        public decimal MarketingCostsPerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the transportation costs per barrel.
        /// </summary>
        public decimal TransportationCostsPerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the net volume.
        /// </summary>
        public decimal NetVolume { get; set; }

        /// <summary>
        /// Gets the total lifting costs.
        /// </summary>
        public decimal TotalLiftingCosts => LiftingCostsPerBarrel * NetVolume;

        /// <summary>
        /// Gets the total operating costs.
        /// </summary>
        public decimal TotalOperatingCosts => OperatingCostsPerBarrel * NetVolume;

        /// <summary>
        /// Gets the total marketing costs.
        /// </summary>
        public decimal TotalMarketingCosts => MarketingCostsPerBarrel * NetVolume;

        /// <summary>
        /// Gets the total transportation costs.
        /// </summary>
        public decimal TotalTransportationCosts => TransportationCostsPerBarrel * NetVolume;

        /// <summary>
        /// Gets the total costs.
        /// </summary>
        public decimal TotalCosts => TotalLiftingCosts + TotalOperatingCosts + TotalMarketingCosts + TotalTransportationCosts;
    }

    /// <summary>
    /// Represents a production tax.
    /// </summary>
    public class ProductionTax
    {
        /// <summary>
        /// Gets or sets the tax identifier.
        /// </summary>
        public string TaxId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tax type.
        /// </summary>
        public ProductionTaxType TaxType { get; set; }

        /// <summary>
        /// Gets or sets the tax rate (percentage or fixed amount).
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Gets or sets the tax amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the tax authority.
        /// </summary>
        public string TaxAuthority { get; set; } = string.Empty;
    }

    /// <summary>
    /// Production tax type enumeration.
    /// </summary>
    public enum ProductionTaxType
    {
        /// <summary>
        /// Severance tax.
        /// </summary>
        Severance,

        /// <summary>
        /// Ad valorem tax.
        /// </summary>
        AdValorem,

        /// <summary>
        /// State tax.
        /// </summary>
        State,

        /// <summary>
        /// Local tax.
        /// </summary>
        Local,

        /// <summary>
        /// Federal tax.
        /// </summary>
        Federal
    }
}

