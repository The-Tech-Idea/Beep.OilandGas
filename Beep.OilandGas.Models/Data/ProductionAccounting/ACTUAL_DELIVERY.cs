using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ACTUAL_DELIVERY : ModelEntityBase {
        private System.String ACTUAL_DELIVERY_IDValue;
        public System.String ACTUAL_DELIVERY_ID
        {
            get { return this.ACTUAL_DELIVERY_IDValue; }
            set { SetProperty(ref ACTUAL_DELIVERY_IDValue, value); }
        }

        private System.DateTime? DELIVERY_DATEValue;
        public System.DateTime? DELIVERY_DATE
        {
            get { return this.DELIVERY_DATEValue; }
            set { SetProperty(ref DELIVERY_DATEValue, value); }
        }

        private System.Decimal? ACTUAL_VOLUMEValue;
        public System.Decimal? ACTUAL_VOLUME
        {
            get { return this.ACTUAL_VOLUMEValue; }
            set { SetProperty(ref ACTUAL_VOLUMEValue, value); }
        }

        private System.String DELIVERY_POINTValue;
        public System.String DELIVERY_POINT
        {
            get { return this.DELIVERY_POINTValue; }
            set { SetProperty(ref DELIVERY_POINTValue, value); }
        }

        private System.String ALLOCATION_METHODValue;
        public System.String ALLOCATION_METHOD
        {
            get { return this.ALLOCATION_METHODValue; }
            set { SetProperty(ref ALLOCATION_METHODValue, value); }
        }

        private System.String RUN_TICKET_NUMBERValue;
        public System.String RUN_TICKET_NUMBER
        {
            get { return this.RUN_TICKET_NUMBERValue; }
            set { SetProperty(ref RUN_TICKET_NUMBERValue, value); }
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
