using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Represents a sales transaction.
    /// </summary>
    public partial class SalesTransaction : Entity
    {
        private System.String TransactionIdValue;
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public System.String TransactionId
        {
            get { return this.TransactionIdValue; }
            set { SetProperty(ref TransactionIdValue, value); }
        }

        private System.String? SalesAgreementIdValue;
        /// <summary>
        /// Gets or sets the sales agreement reference.
        /// </summary>
        public System.String? SalesAgreementId
        {
            get { return this.SalesAgreementIdValue; }
            set { SetProperty(ref SalesAgreementIdValue, value); }
        }

        private System.String RunTicketNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        public System.String RunTicketNumber
        {
            get { return this.RunTicketNumberValue; }
            set { SetProperty(ref RunTicketNumberValue, value); }
        }

        private System.DateTime TransactionDateValue;
        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public System.DateTime TransactionDate
        {
            get { return this.TransactionDateValue; }
            set { SetProperty(ref TransactionDateValue, value); }
        }

        private System.String PurchaserValue = string.Empty;
        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        public System.String Purchaser
        {
            get { return this.PurchaserValue; }
            set { SetProperty(ref PurchaserValue, value); }
        }

        private System.Decimal NetVolumeValue;
        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        private System.Decimal PricePerBarrelValue;
        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        public System.Decimal PricePerBarrel
        {
            get { return this.PricePerBarrelValue; }
            set { SetProperty(ref PricePerBarrelValue, value); }
        }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public System.Decimal TotalValue => NetVolume * PricePerBarrel;

        private DeliveryInformation DeliveryValue = new DeliveryInformation();
        /// <summary>
        /// Gets or sets the delivery information.
        /// </summary>
        public DeliveryInformation Delivery
        {
            get { return this.DeliveryValue; }
            set { SetProperty(ref DeliveryValue, value); }
        }

        private ProductionMarketingCosts CostsValue = new ProductionMarketingCosts();
        /// <summary>
        /// Gets or sets the production and marketing costs.
        /// </summary>
        public ProductionMarketingCosts Costs
        {
            get { return this.CostsValue; }
            set { SetProperty(ref CostsValue, value); }
        }

        private List<ProductionTax> TaxesValue = new List<ProductionTax>();
        /// <summary>
        /// Gets or sets the taxes.
        /// </summary>
        public List<ProductionTax> Taxes
        {
            get { return this.TaxesValue; }
            set { SetProperty(ref TaxesValue, value); }
        }

        /// <summary>
        /// Gets the net revenue (total value - costs - taxes).
        /// </summary>
        public System.Decimal NetRevenue => TotalValue - Costs.TotalCosts - Taxes.Sum(t => t.Amount);
    }

    /// <summary>
    /// Represents delivery information.
    /// </summary>
    public partial class DeliveryInformation : Entity
    {
        private System.DateTime DeliveryDateValue;
        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        public System.DateTime DeliveryDate
        {
            get { return this.DeliveryDateValue; }
            set { SetProperty(ref DeliveryDateValue, value); }
        }

        private System.String DeliveryPointValue = string.Empty;
        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        public System.String DeliveryPoint
        {
            get { return this.DeliveryPointValue; }
            set { SetProperty(ref DeliveryPointValue, value); }
        }

        private System.String DeliveryMethodValue = string.Empty;
        /// <summary>
        /// Gets or sets the delivery method.
        /// </summary>
        public System.String DeliveryMethod
        {
            get { return this.DeliveryMethodValue; }
            set { SetProperty(ref DeliveryMethodValue, value); }
        }

        private System.Boolean TitleTransferredAtDeliveryPointValue = true;
        /// <summary>
        /// Gets or sets whether title transferred at delivery point.
        /// </summary>
        public System.Boolean TitleTransferredAtDeliveryPoint
        {
            get { return this.TitleTransferredAtDeliveryPointValue; }
            set { SetProperty(ref TitleTransferredAtDeliveryPointValue, value); }
        }
    }

    /// <summary>
    /// Represents production and marketing costs.
    /// </summary>
    public partial class ProductionMarketingCosts : Entity
    {
        private System.Decimal LiftingCostsPerBarrelValue;
        /// <summary>
        /// Gets or sets the lifting costs per barrel.
        /// </summary>
        public System.Decimal LiftingCostsPerBarrel
        {
            get { return this.LiftingCostsPerBarrelValue; }
            set { SetProperty(ref LiftingCostsPerBarrelValue, value); }
        }

        private System.Decimal OperatingCostsPerBarrelValue;
        /// <summary>
        /// Gets or sets the operating costs per barrel.
        /// </summary>
        public System.Decimal OperatingCostsPerBarrel
        {
            get { return this.OperatingCostsPerBarrelValue; }
            set { SetProperty(ref OperatingCostsPerBarrelValue, value); }
        }

        private System.Decimal MarketingCostsPerBarrelValue;
        /// <summary>
        /// Gets or sets the marketing costs per barrel.
        /// </summary>
        public System.Decimal MarketingCostsPerBarrel
        {
            get { return this.MarketingCostsPerBarrelValue; }
            set { SetProperty(ref MarketingCostsPerBarrelValue, value); }
        }

        private System.Decimal TransportationCostsPerBarrelValue;
        /// <summary>
        /// Gets or sets the transportation costs per barrel.
        /// </summary>
        public System.Decimal TransportationCostsPerBarrel
        {
            get { return this.TransportationCostsPerBarrelValue; }
            set { SetProperty(ref TransportationCostsPerBarrelValue, value); }
        }

        private System.Decimal NetVolumeValue;
        /// <summary>
        /// Gets or sets the net volume.
        /// </summary>
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        /// <summary>
        /// Gets the total lifting costs.
        /// </summary>
        public System.Decimal TotalLiftingCosts => LiftingCostsPerBarrel * NetVolume;
        /// <summary>
        /// Gets the total operating costs.
        /// </summary>
        public System.Decimal TotalOperatingCosts => OperatingCostsPerBarrel * NetVolume;
        /// <summary>
        /// Gets the total marketing costs.
        /// </summary>
        public System.Decimal TotalMarketingCosts => MarketingCostsPerBarrel * NetVolume;
        /// <summary>
        /// Gets the total transportation costs.
        /// </summary>
        public System.Decimal TotalTransportationCosts => TransportationCostsPerBarrel * NetVolume;
        /// <summary>
        /// Gets the total costs.
        /// </summary>
        public System.Decimal TotalCosts => TotalLiftingCosts + TotalOperatingCosts + TotalMarketingCosts + TotalTransportationCosts;
    }

    /// <summary>
    /// Represents a production tax.
    /// </summary>
    public partial class ProductionTax : Entity
    {
        private System.String TaxIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the tax identifier.
        /// </summary>
        public System.String TaxId
        {
            get { return this.TaxIdValue; }
            set { SetProperty(ref TaxIdValue, value); }
        }

        private ProductionTaxType TaxTypeValue;
        /// <summary>
        /// Gets or sets the tax type.
        /// </summary>
        public ProductionTaxType TaxType
        {
            get { return this.TaxTypeValue; }
            set { SetProperty(ref TaxTypeValue, value); }
        }

        private System.Decimal TaxRateValue;
        /// <summary>
        /// Gets or sets the tax rate (percentage or fixed amount).
        /// </summary>
        public System.Decimal TaxRate
        {
            get { return this.TaxRateValue; }
            set { SetProperty(ref TaxRateValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets the tax amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.DateTime DueDateValue;
        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        public System.DateTime DueDate
        {
            get { return this.DueDateValue; }
            set { SetProperty(ref DueDateValue, value); }
        }

        private System.String TaxAuthorityValue = string.Empty;
        /// <summary>
        /// Gets or sets the tax authority.
        /// </summary>
        public System.String TaxAuthority
        {
            get { return this.TaxAuthorityValue; }
            set { SetProperty(ref TaxAuthorityValue, value); }
        }
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

