using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LOCATION_ADJUSTMENTS : ModelEntityBase {
        private System.String LOCATION_ADJUSTMENTS_IDValue;
        public System.String LOCATION_ADJUSTMENTS_ID
        {
            get { return this.LOCATION_ADJUSTMENTS_IDValue; }
            set { SetProperty(ref LOCATION_ADJUSTMENTS_IDValue, value); }
        }

        private System.Decimal? LOCATION_DIFFERENTIALValue;
        public System.Decimal? LOCATION_DIFFERENTIAL
        {
            get { return this.LOCATION_DIFFERENTIALValue; }
            set { SetProperty(ref LOCATION_DIFFERENTIALValue, value); }
        }

        private System.Decimal? TRANSPORTATION_ADJUSTMENTValue;
        public System.Decimal? TRANSPORTATION_ADJUSTMENT
        {
            get { return this.TRANSPORTATION_ADJUSTMENTValue; }
            set { SetProperty(ref TRANSPORTATION_ADJUSTMENTValue, value); }
        }

        private System.Decimal? TOTAL_ADJUSTMENTValue;
        public System.Decimal? TOTAL_ADJUSTMENT
        {
            get { return this.TOTAL_ADJUSTMENTValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTValue, value); }
        }

        // Standard PPDM columns

    

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

      
    }
}
