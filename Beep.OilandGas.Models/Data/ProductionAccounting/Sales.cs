using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for sales transaction
    /// </summary>
    public class SalesTransaction : ModelEntityBase
    {
        private string TransactionIdValue = string.Empty;

        public string TransactionId

        {

            get { return this.TransactionIdValue; }

            set { SetProperty(ref TransactionIdValue, value); }

        }
        private string? SalesAgreementIdValue;

        public string? SalesAgreementId

        {

            get { return this.SalesAgreementIdValue; }

            set { SetProperty(ref SalesAgreementIdValue, value); }

        }
        private string RunTicketNumberValue = string.Empty;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string PurchaserValue = string.Empty;

        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal PricePerBarrelValue;

        public decimal PricePerBarrel

        {

            get { return this.PricePerBarrelValue; }

            set { SetProperty(ref PricePerBarrelValue, value); }

        }
        private decimal TotalValueValue;

        public decimal TotalValue

        {

            get { return this.TotalValueValue; }

            set { SetProperty(ref TotalValueValue, value); }

        }
        private DeliveryInformation? DeliveryValue;

        public DeliveryInformation? Delivery

        {

            get { return this.DeliveryValue; }

            set { SetProperty(ref DeliveryValue, value); }

        }
        private ProductionMarketingCosts? CostsValue;

        public ProductionMarketingCosts? Costs

        {

            get { return this.CostsValue; }

            set { SetProperty(ref CostsValue, value); }

        }
        private List<ProductionTax> TaxesValue = new();

        public List<ProductionTax> Taxes

        {

            get { return this.TaxesValue; }

            set { SetProperty(ref TaxesValue, value); }

        }
        private decimal NetRevenueValue;

        public decimal NetRevenue

        {

            get { return this.NetRevenueValue; }

            set { SetProperty(ref NetRevenueValue, value); }

        }
    }

    /// <summary>
    /// DTO for delivery information
    /// </summary>
    public class DeliveryInformation : ModelEntityBase
    {
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }
        private string DeliveryPointValue = string.Empty;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }
        private string DeliveryMethodValue = string.Empty;

        public string DeliveryMethod

        {

            get { return this.DeliveryMethodValue; }

            set { SetProperty(ref DeliveryMethodValue, value); }

        }
        private bool TitleTransferredAtDeliveryPointValue = true;

        public bool TitleTransferredAtDeliveryPoint

        {

            get { return this.TitleTransferredAtDeliveryPointValue; }

            set { SetProperty(ref TitleTransferredAtDeliveryPointValue, value); }

        }
    }

    /// <summary>
    /// DTO for production and marketing costs
    /// </summary>
    public class ProductionMarketingCosts : ModelEntityBase
    {
        private decimal LiftingCostsValue;

        public decimal LiftingCosts

        {

            get { return this.LiftingCostsValue; }

            set { SetProperty(ref LiftingCostsValue, value); }

        }
        private decimal TransportationCostsValue;

        public decimal TransportationCosts

        {

            get { return this.TransportationCostsValue; }

            set { SetProperty(ref TransportationCostsValue, value); }

        }
        private decimal ProcessingCostsValue;

        public decimal ProcessingCosts

        {

            get { return this.ProcessingCostsValue; }

            set { SetProperty(ref ProcessingCostsValue, value); }

        }
        private decimal MarketingCostsValue;

        public decimal MarketingCosts

        {

            get { return this.MarketingCostsValue; }

            set { SetProperty(ref MarketingCostsValue, value); }

        }
        private decimal OtherCostsValue;

        public decimal OtherCosts

        {

            get { return this.OtherCostsValue; }

            set { SetProperty(ref OtherCostsValue, value); }

        }
        private decimal TotalCostsValue;

        public decimal TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
    }

    /// <summary>
    /// DTO for production tax
    /// </summary>
    public class ProductionTax : ModelEntityBase
    {
        private string TaxTypeValue = string.Empty;

        public string TaxType

        {

            get { return this.TaxTypeValue; }

            set { SetProperty(ref TaxTypeValue, value); }

        }
        private string TaxNameValue = string.Empty;

        public string TaxName

        {

            get { return this.TaxNameValue; }

            set { SetProperty(ref TaxNameValue, value); }

        }
        private decimal TaxRateValue;

        public decimal TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }

    /// <summary>
    /// DTO for receivable
    /// </summary>
    public class Receivable : ModelEntityBase
    {
        private string ReceivableIdValue = string.Empty;

        public string ReceivableId

        {

            get { return this.ReceivableIdValue; }

            set { SetProperty(ref ReceivableIdValue, value); }

        }
        private string TransactionIdValue = string.Empty;

        public string TransactionId

        {

            get { return this.TransactionIdValue; }

            set { SetProperty(ref TransactionIdValue, value); }

        }
        private string PurchaserValue = string.Empty;

        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }
        private DateTime DueDateValue;

        public DateTime DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private decimal AmountPaidValue;

        public decimal AmountPaid

        {

            get { return this.AmountPaidValue; }

            set { SetProperty(ref AmountPaidValue, value); }

        }
        private decimal BalanceValue;

        public decimal Balance

        {

            get { return this.BalanceValue; }

            set { SetProperty(ref BalanceValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for inventory
    /// </summary>
    public class Inventory : ModelEntityBase
    {
        private string InventoryIdValue = string.Empty;

        public string InventoryId

        {

            get { return this.InventoryIdValue; }

            set { SetProperty(ref InventoryIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? TankBatteryIdValue;

        public string? TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private DateTime InventoryDateValue;

        public DateTime InventoryDate

        {

            get { return this.InventoryDateValue; }

            set { SetProperty(ref InventoryDateValue, value); }

        }
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal BSWVolumeValue;

        public decimal BSWVolume

        {

            get { return this.BSWVolumeValue; }

            set { SetProperty(ref BSWVolumeValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
    }

    /// <summary>
    /// Request to create sales transaction
    /// </summary>
    public class CreateSalesTransactionRequest : ModelEntityBase
    {
        private string RunTicketNumberValue = string.Empty;

        [Required]
        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string? SalesAgreementIdValue;

        public string? SalesAgreementId

        {

            get { return this.SalesAgreementIdValue; }

            set { SetProperty(ref SalesAgreementIdValue, value); }

        }
        private DateTime TransactionDateValue;

        [Required]
        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string PurchaserValue = string.Empty;

        [Required]
        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }
        private decimal NetVolumeValue;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal PricePerBarrelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerBarrel

        {

            get { return this.PricePerBarrelValue; }

            set { SetProperty(ref PricePerBarrelValue, value); }

        }
        private DeliveryInformation? DeliveryValue;

        public DeliveryInformation? Delivery

        {

            get { return this.DeliveryValue; }

            set { SetProperty(ref DeliveryValue, value); }

        }
        private ProductionMarketingCosts? CostsValue;

        public ProductionMarketingCosts? Costs

        {

            get { return this.CostsValue; }

            set { SetProperty(ref CostsValue, value); }

        }
        private List<ProductionTax>? TaxesValue;

        public List<ProductionTax>? Taxes

        {

            get { return this.TaxesValue; }

            set { SetProperty(ref TaxesValue, value); }

        }
    }
}








