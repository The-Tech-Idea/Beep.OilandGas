using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_COMMITMENT : ModelEntityBase {
        private System.String COMMITMENT_IDValue;
        public System.String COMMITMENT_ID
        {
            get { return this.COMMITMENT_IDValue; }
            set { SetProperty(ref COMMITMENT_IDValue, value); }
        }

        private System.String CONTRACT_IDValue;
        public System.String CONTRACT_ID
        {
            get { return this.CONTRACT_IDValue; }
            set { SetProperty(ref CONTRACT_IDValue, value); }
        }

        private System.String COMMITMENT_TYPEValue;
        public System.String COMMITMENT_TYPE
        {
            get { return this.COMMITMENT_TYPEValue; }
            set { SetProperty(ref COMMITMENT_TYPEValue, value); }
        }

        private System.Decimal? COMMITTED_VOLUMEValue;
        public System.Decimal? COMMITTED_VOLUME
        {
            get { return this.COMMITTED_VOLUMEValue; }
            set { SetProperty(ref COMMITTED_VOLUMEValue, value); }
        }

        private System.DateTime? DELIVERY_PERIOD_STARTValue;
        public System.DateTime? DELIVERY_PERIOD_START
        {
            get { return this.DELIVERY_PERIOD_STARTValue; }
            set { SetProperty(ref DELIVERY_PERIOD_STARTValue, value); }
        }

        private System.DateTime? DELIVERY_PERIOD_ENDValue;
        public System.DateTime? DELIVERY_PERIOD_END
        {
            get { return this.DELIVERY_PERIOD_ENDValue; }
            set { SetProperty(ref DELIVERY_PERIOD_ENDValue, value); }
        }

        private System.Decimal? ACTUAL_VOLUME_DELIVEREDValue;
        public System.Decimal? ACTUAL_VOLUME_DELIVERED
        {
            get { return this.ACTUAL_VOLUME_DELIVEREDValue; }
            set { SetProperty(ref ACTUAL_VOLUME_DELIVEREDValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.Decimal? REMAINING_COMMITMENTValue;
        public System.Decimal? REMAINING_COMMITMENT
        {
            get { return this.REMAINING_COMMITMENTValue; }
            set { SetProperty(ref REMAINING_COMMITMENTValue, value); }
        }

        private System.Decimal? FULFILLMENT_PERCENTAGEValue;
        public System.Decimal? FULFILLMENT_PERCENTAGE
        {
            get { return this.FULFILLMENT_PERCENTAGEValue; }
            set { SetProperty(ref FULFILLMENT_PERCENTAGEValue, value); }
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
