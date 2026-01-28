using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class SalesTransactionResponse : ModelEntityBase
    {
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

        }
        private string RunTicketIdValue;

        public string RunTicketId

        {

            get { return this.RunTicketIdValue; }

            set { SetProperty(ref RunTicketIdValue, value); }

        }
        private string? SalesAgreementIdValue;

        public string? SalesAgreementId

        {

            get { return this.SalesAgreementIdValue; }

            set { SetProperty(ref SalesAgreementIdValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime SalesDateValue;

        public DateTime SalesDate

        {

            get { return this.SalesDateValue; }

            set { SetProperty(ref SalesDateValue, value); }

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
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? TotalCostsValue;

        public decimal? TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
        private decimal? TotalTaxesValue;

        public decimal? TotalTaxes

        {

            get { return this.TotalTaxesValue; }

            set { SetProperty(ref TotalTaxesValue, value); }

        }
        private decimal? NetRevenueValue;

        public decimal? NetRevenue

        {

            get { return this.NetRevenueValue; }

            set { SetProperty(ref NetRevenueValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ApprovalStatusValue;

        public string ApprovalStatus

        {

            get { return this.ApprovalStatusValue; }

            set { SetProperty(ref ApprovalStatusValue, value); }

        }
    }
}
