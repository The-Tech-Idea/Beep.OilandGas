using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EMISSIONS_SETTLEMENT : ModelEntityBase
    {
        private string EMISSIONS_SETTLEMENT_IDValue;
        public string EMISSIONS_SETTLEMENT_ID
        {
            get { return this.EMISSIONS_SETTLEMENT_IDValue; }
            set { SetProperty(ref EMISSIONS_SETTLEMENT_IDValue, value); }
        }

        private string EMISSIONS_OBLIGATION_IDValue;
        public string EMISSIONS_OBLIGATION_ID
        {
            get { return this.EMISSIONS_OBLIGATION_IDValue; }
            set { SetProperty(ref EMISSIONS_OBLIGATION_IDValue, value); }
        }

        private DateTime? SETTLEMENT_DATEValue;
        public DateTime? SETTLEMENT_DATE
        {
            get { return this.SETTLEMENT_DATEValue; }
            set { SetProperty(ref SETTLEMENT_DATEValue, value); }
        }

        private decimal? ALLOWANCES_SURRENDEREDValue;
        public decimal? ALLOWANCES_SURRENDERED
        {
            get { return this.ALLOWANCES_SURRENDEREDValue; }
            set { SetProperty(ref ALLOWANCES_SURRENDEREDValue, value); }
        }

        private decimal? SETTLEMENT_VALUEValue;
        public decimal? SETTLEMENT_VALUE
        {
            get { return this.SETTLEMENT_VALUEValue; }
            set { SetProperty(ref SETTLEMENT_VALUEValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }
    }
}


