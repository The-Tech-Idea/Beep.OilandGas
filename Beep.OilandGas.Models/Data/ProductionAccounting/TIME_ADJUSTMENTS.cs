using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TIME_ADJUSTMENTS : ModelEntityBase {
        private System.String TIME_ADJUSTMENTS_IDValue;
        public System.String TIME_ADJUSTMENTS_ID
        {
            get { return this.TIME_ADJUSTMENTS_IDValue; }
            set { SetProperty(ref TIME_ADJUSTMENTS_IDValue, value); }
        }

        private System.Decimal? TIME_DIFFERENTIALValue;
        public System.Decimal? TIME_DIFFERENTIAL
        {
            get { return this.TIME_DIFFERENTIALValue; }
            set { SetProperty(ref TIME_DIFFERENTIALValue, value); }
        }

        private System.Decimal? INTEREST_ADJUSTMENTValue;
        public System.Decimal? INTEREST_ADJUSTMENT
        {
            get { return this.INTEREST_ADJUSTMENTValue; }
            set { SetProperty(ref INTEREST_ADJUSTMENTValue, value); }
        }

        private System.Decimal? TOTAL_ADJUSTMENTValue;
        public System.Decimal? TOTAL_ADJUSTMENT
        {
            get { return this.TOTAL_ADJUSTMENTValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


