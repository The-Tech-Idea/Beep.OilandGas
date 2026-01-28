using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_PAYMENT : ModelEntityBase
    {
        private string LEASE_PAYMENT_IDValue;
        public string LEASE_PAYMENT_ID
        {
            get { return this.LEASE_PAYMENT_IDValue; }
            set { SetProperty(ref LEASE_PAYMENT_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private DateTime? PAYMENT_DATEValue;
        public DateTime? PAYMENT_DATE
        {
            get { return this.PAYMENT_DATEValue; }
            set { SetProperty(ref PAYMENT_DATEValue, value); }
        }

        private decimal? PAYMENT_AMOUNTValue;
        public decimal? PAYMENT_AMOUNT
        {
            get { return this.PAYMENT_AMOUNTValue; }
            set { SetProperty(ref PAYMENT_AMOUNTValue, value); }
        }

        private string CURRENCY_CODEValue;
        public string CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }
    }
}
