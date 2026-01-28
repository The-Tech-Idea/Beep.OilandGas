using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
