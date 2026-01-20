using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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

        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }
    }
}

