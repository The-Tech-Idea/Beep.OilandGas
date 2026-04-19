using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EMISSIONS_ALLOWANCE : ModelEntityBase
    {
        private string EMISSIONS_ALLOWANCE_IDValue;
        public string EMISSIONS_ALLOWANCE_ID
        {
            get { return this.EMISSIONS_ALLOWANCE_IDValue; }
            set { SetProperty(ref EMISSIONS_ALLOWANCE_IDValue, value); }
        }

        private string PROGRAM_CODEValue;
        public string PROGRAM_CODE
        {
            get { return this.PROGRAM_CODEValue; }
            set { SetProperty(ref PROGRAM_CODEValue, value); }
        }

        private string ALLOWANCE_TYPEValue;
        public string ALLOWANCE_TYPE
        {
            get { return this.ALLOWANCE_TYPEValue; }
            set { SetProperty(ref ALLOWANCE_TYPEValue, value); }
        }

        private decimal? VOLUMEValue;
        public decimal? VOLUME
        {
            get { return this.VOLUMEValue; }
            set { SetProperty(ref VOLUMEValue, value); }
        }

        private decimal? ACQUISITION_COSTValue;
        public decimal? ACQUISITION_COST
        {
            get { return this.ACQUISITION_COSTValue; }
            set { SetProperty(ref ACQUISITION_COSTValue, value); }
        }

        private decimal? FAIR_VALUEValue;
        public decimal? FAIR_VALUE
        {
            get { return this.FAIR_VALUEValue; }
            set { SetProperty(ref FAIR_VALUEValue, value); }
        }

        private DateTime? PERIOD_ENDValue;
        public DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private string CURRENCY_CODEValue;
        public string CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }
    }
}
