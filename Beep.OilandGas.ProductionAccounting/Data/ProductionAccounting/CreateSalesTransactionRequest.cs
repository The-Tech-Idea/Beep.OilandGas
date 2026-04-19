using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
