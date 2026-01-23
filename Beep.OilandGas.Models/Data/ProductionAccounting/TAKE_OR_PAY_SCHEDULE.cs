using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAKE_OR_PAY_SCHEDULE : ModelEntityBase {
        private string TAKE_OR_PAY_SCHEDULE_IDValue;
        public string TAKE_OR_PAY_SCHEDULE_ID
        {
            get { return this.TAKE_OR_PAY_SCHEDULE_IDValue; }
            set { SetProperty(ref TAKE_OR_PAY_SCHEDULE_IDValue, value); }
        }

        private string SALES_CONTRACT_IDValue;
        public string SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private DateTime? PERIOD_STARTValue;
        public DateTime? PERIOD_START
        {
            get { return this.PERIOD_STARTValue; }
            set { SetProperty(ref PERIOD_STARTValue, value); }
        }

        private DateTime? PERIOD_ENDValue;
        public DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private decimal? MIN_VOLUMEValue;
        public decimal? MIN_VOLUME
        {
            get { return this.MIN_VOLUMEValue; }
            set { SetProperty(ref MIN_VOLUMEValue, value); }
        }

        private decimal? PRICEValue;
        public decimal? PRICE
        {
            get { return this.PRICEValue; }
            set { SetProperty(ref PRICEValue, value); }
        }

        private decimal? MAKEUP_LIMITValue;
        public decimal? MAKEUP_LIMIT
        {
            get { return this.MAKEUP_LIMITValue; }
            set { SetProperty(ref MAKEUP_LIMITValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


