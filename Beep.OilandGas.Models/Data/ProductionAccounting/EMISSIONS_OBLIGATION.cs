using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EMISSIONS_OBLIGATION : ModelEntityBase
    {
        private string EMISSIONS_OBLIGATION_IDValue;
        public string EMISSIONS_OBLIGATION_ID
        {
            get { return this.EMISSIONS_OBLIGATION_IDValue; }
            set { SetProperty(ref EMISSIONS_OBLIGATION_IDValue, value); }
        }

        private string PROGRAM_CODEValue;
        public string PROGRAM_CODE
        {
            get { return this.PROGRAM_CODEValue; }
            set { SetProperty(ref PROGRAM_CODEValue, value); }
        }

        private DateTime? PERIOD_ENDValue;
        public DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private decimal? EMISSIONS_VOLUMEValue;
        public decimal? EMISSIONS_VOLUME
        {
            get { return this.EMISSIONS_VOLUMEValue; }
            set { SetProperty(ref EMISSIONS_VOLUMEValue, value); }
        }

        private decimal? ALLOWANCE_PRICEValue;
        public decimal? ALLOWANCE_PRICE
        {
            get { return this.ALLOWANCE_PRICEValue; }
            set { SetProperty(ref ALLOWANCE_PRICEValue, value); }
        }

        private decimal? LIABILITY_AMOUNTValue;
        public decimal? LIABILITY_AMOUNT
        {
            get { return this.LIABILITY_AMOUNTValue; }
            set { SetProperty(ref LIABILITY_AMOUNTValue, value); }
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

