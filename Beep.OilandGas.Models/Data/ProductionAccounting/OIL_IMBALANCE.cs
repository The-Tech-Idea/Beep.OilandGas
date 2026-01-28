using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class OIL_IMBALANCE : ModelEntityBase {
        private System.String IMBALANCE_IDValue;
        public System.String IMBALANCE_ID
        {
            get { return this.IMBALANCE_IDValue; }
            set { SetProperty(ref IMBALANCE_IDValue, value); }
        }

        private System.DateTime? PERIOD_STARTValue;
        public System.DateTime? PERIOD_START
        {
            get { return this.PERIOD_STARTValue; }
            set { SetProperty(ref PERIOD_STARTValue, value); }
        }

        private System.DateTime? PERIOD_ENDValue;
        public System.DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private System.Decimal? NOMINATED_VOLUMEValue;
        public System.Decimal? NOMINATED_VOLUME
        {
            get { return this.NOMINATED_VOLUMEValue; }
            set { SetProperty(ref NOMINATED_VOLUMEValue, value); }
        }

        private System.Decimal? ACTUAL_VOLUMEValue;
        public System.Decimal? ACTUAL_VOLUME
        {
            get { return this.ACTUAL_VOLUMEValue; }
            set { SetProperty(ref ACTUAL_VOLUMEValue, value); }
        }

        private System.Decimal? IMBALANCE_AMOUNTValue;
        public System.Decimal? IMBALANCE_AMOUNT
        {
            get { return this.IMBALANCE_AMOUNTValue; }
            set { SetProperty(ref IMBALANCE_AMOUNTValue, value); }
        }

        private System.Decimal? IMBALANCE_PERCENTAGEValue;
        public System.Decimal? IMBALANCE_PERCENTAGE
        {
            get { return this.IMBALANCE_PERCENTAGEValue; }
            set { SetProperty(ref IMBALANCE_PERCENTAGEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.Decimal? TOLERANCE_PERCENTAGEValue;
        public System.Decimal? TOLERANCE_PERCENTAGE
        {
            get { return this.TOLERANCE_PERCENTAGEValue; }
            set { SetProperty(ref TOLERANCE_PERCENTAGEValue, value); }
        }

        private System.String IS_WITHIN_TOLERANCE_INDValue;
        public System.String IS_WITHIN_TOLERANCE_IND
        {
            get { return this.IS_WITHIN_TOLERANCE_INDValue; }
            set { SetProperty(ref IS_WITHIN_TOLERANCE_INDValue, value); }
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
