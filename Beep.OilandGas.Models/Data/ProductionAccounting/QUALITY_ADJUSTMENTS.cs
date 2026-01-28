using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class QUALITY_ADJUSTMENTS : ModelEntityBase {
        private System.String QUALITY_ADJUSTMENTS_IDValue;
        public System.String QUALITY_ADJUSTMENTS_ID
        {
            get { return this.QUALITY_ADJUSTMENTS_IDValue; }
            set { SetProperty(ref QUALITY_ADJUSTMENTS_IDValue, value); }
        }

        private System.Decimal? API_GRAVITY_ADJUSTMENTValue;
        public System.Decimal? API_GRAVITY_ADJUSTMENT
        {
            get { return this.API_GRAVITY_ADJUSTMENTValue; }
            set { SetProperty(ref API_GRAVITY_ADJUSTMENTValue, value); }
        }

        private System.Decimal? SULFUR_ADJUSTMENTValue;
        public System.Decimal? SULFUR_ADJUSTMENT
        {
            get { return this.SULFUR_ADJUSTMENTValue; }
            set { SetProperty(ref SULFUR_ADJUSTMENTValue, value); }
        }

        private System.Decimal? BSW_ADJUSTMENTValue;
        public System.Decimal? BSW_ADJUSTMENT
        {
            get { return this.BSW_ADJUSTMENTValue; }
            set { SetProperty(ref BSW_ADJUSTMENTValue, value); }
        }

        private System.Decimal? OTHER_ADJUSTMENTSValue;
        public System.Decimal? OTHER_ADJUSTMENTS
        {
            get { return this.OTHER_ADJUSTMENTSValue; }
            set { SetProperty(ref OTHER_ADJUSTMENTSValue, value); }
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

      

        private decimal BSWADJUSTMENTValue;
        public decimal BSWADJUSTMENT
        {
            get { return this.BSWADJUSTMENTValue; }
            set { SetProperty(ref BSWADJUSTMENTValue, value); }
        }

       
    }
}
