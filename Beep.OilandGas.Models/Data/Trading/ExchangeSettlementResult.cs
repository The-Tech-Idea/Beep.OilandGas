using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class ExchangeSettlementResult : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime SettlementDateValue;

        public DateTime SettlementDate

        {

            get { return this.SettlementDateValue; }

            set { SetProperty(ref SettlementDateValue, value); }

        }
        private decimal TotalReceiptValueValue;

        public decimal TotalReceiptValue

        {

            get { return this.TotalReceiptValueValue; }

            set { SetProperty(ref TotalReceiptValueValue, value); }

        }
        private decimal TotalDeliveryValueValue;

        public decimal TotalDeliveryValue

        {

            get { return this.TotalDeliveryValueValue; }

            set { SetProperty(ref TotalDeliveryValueValue, value); }

        }
        private decimal NetSettlementAmountValue;

        public decimal NetSettlementAmount

        {

            get { return this.NetSettlementAmountValue; }

            set { SetProperty(ref NetSettlementAmountValue, value); }

        }
        private int TransactionCountValue;

        public int TransactionCount

        {

            get { return this.TransactionCountValue; }

            set { SetProperty(ref TransactionCountValue, value); }

        }
        private bool IsSettledValue;

        public bool IsSettled

        {

            get { return this.IsSettledValue; }

            set { SetProperty(ref IsSettledValue, value); }

        }
    }
}
