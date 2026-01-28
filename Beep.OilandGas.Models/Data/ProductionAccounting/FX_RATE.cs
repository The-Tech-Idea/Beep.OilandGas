using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class FX_RATE : ModelEntityBase
    {
        private string FX_RATE_IDValue;
        public string FX_RATE_ID
        {
            get { return this.FX_RATE_IDValue; }
            set { SetProperty(ref FX_RATE_IDValue, value); }
        }

        private string FROM_CURRENCYValue;
        public string FROM_CURRENCY
        {
            get { return this.FROM_CURRENCYValue; }
            set { SetProperty(ref FROM_CURRENCYValue, value); }
        }

        private string TO_CURRENCYValue;
        public string TO_CURRENCY
        {
            get { return this.TO_CURRENCYValue; }
            set { SetProperty(ref TO_CURRENCYValue, value); }
        }

        private DateTime? RATE_DATEValue;
        public DateTime? RATE_DATE
        {
            get { return this.RATE_DATEValue; }
            set { SetProperty(ref RATE_DATEValue, value); }
        }

        private decimal? RATEValue;
        public decimal? RATE
        {
            get { return this.RATEValue; }
            set { SetProperty(ref RATEValue, value); }
        }

    }
}
