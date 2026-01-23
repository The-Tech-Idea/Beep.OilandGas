using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class FINANCIAL_INSTRUMENT : ModelEntityBase
    {
        private string INSTRUMENT_IDValue;
        public string INSTRUMENT_ID
        {
            get { return this.INSTRUMENT_IDValue; }
            set { SetProperty(ref INSTRUMENT_IDValue, value); }
        }

        private string INSTRUMENT_TYPEValue;
        public string INSTRUMENT_TYPE
        {
            get { return this.INSTRUMENT_TYPEValue; }
            set { SetProperty(ref INSTRUMENT_TYPEValue, value); }
        }

        private decimal? NOTIONAL_AMOUNTValue;
        public decimal? NOTIONAL_AMOUNT
        {
            get { return this.NOTIONAL_AMOUNTValue; }
            set { SetProperty(ref NOTIONAL_AMOUNTValue, value); }
        }

        private string CURRENCY_CODEValue;
        public string CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private decimal? FAIR_VALUEValue;
        public decimal? FAIR_VALUE
        {
            get { return this.FAIR_VALUEValue; }
            set { SetProperty(ref FAIR_VALUEValue, value); }
        }

        private DateTime? VALUATION_DATEValue;
        public DateTime? VALUATION_DATE
        {
            get { return this.VALUATION_DATEValue; }
            set { SetProperty(ref VALUATION_DATEValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }
    }
}


