using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_CONTRACT : ModelEntityBase
    {
        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private string LESSOR_BA_IDValue;
        public string LESSOR_BA_ID
        {
            get { return this.LESSOR_BA_IDValue; }
            set { SetProperty(ref LESSOR_BA_IDValue, value); }
        }

        private DateTime? COMMENCEMENT_DATEValue;
        public DateTime? COMMENCEMENT_DATE
        {
            get { return this.COMMENCEMENT_DATEValue; }
            set { SetProperty(ref COMMENCEMENT_DATEValue, value); }
        }

        private int? TERM_MONTHSValue;
        public int? TERM_MONTHS
        {
            get { return this.TERM_MONTHSValue; }
            set { SetProperty(ref TERM_MONTHSValue, value); }
        }

        private decimal? DISCOUNT_RATEValue;
        public decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }

        private string CURRENCY_CODEValue;
        public string CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }
    }
}
