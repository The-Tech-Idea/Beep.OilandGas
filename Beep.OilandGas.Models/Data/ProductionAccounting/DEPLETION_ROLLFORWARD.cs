using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DEPLETION_ROLLFORWARD : ModelEntityBase {
        private System.String DEPLETION_ROLLFORWARD_IDValue;
        public System.String DEPLETION_ROLLFORWARD_ID
        {
            get { return this.DEPLETION_ROLLFORWARD_IDValue; }
            set { SetProperty(ref DEPLETION_ROLLFORWARD_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.DateTime? PERIOD_START_DATEValue;
        public System.DateTime? PERIOD_START_DATE
        {
            get { return this.PERIOD_START_DATEValue; }
            set { SetProperty(ref PERIOD_START_DATEValue, value); }
        }

        private System.DateTime? PERIOD_END_DATEValue;
        public System.DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private System.Decimal? OPENING_NET_CAPITALIZEDValue;
        public System.Decimal? OPENING_NET_CAPITALIZED
        {
            get { return this.OPENING_NET_CAPITALIZEDValue; }
            set { SetProperty(ref OPENING_NET_CAPITALIZEDValue, value); }
        }

        private System.Decimal? ADDITIONSValue;
        public System.Decimal? ADDITIONS
        {
            get { return this.ADDITIONSValue; }
            set { SetProperty(ref ADDITIONSValue, value); }
        }

        private System.Decimal? DEPLETIONValue;
        public System.Decimal? DEPLETION
        {
            get { return this.DEPLETIONValue; }
            set { SetProperty(ref DEPLETIONValue, value); }
        }

        private System.Decimal? IMPAIRMENTValue;
        public System.Decimal? IMPAIRMENT
        {
            get { return this.IMPAIRMENTValue; }
            set { SetProperty(ref IMPAIRMENTValue, value); }
        }

        private System.Decimal? CLOSING_NET_CAPITALIZEDValue;
        public System.Decimal? CLOSING_NET_CAPITALIZED
        {
            get { return this.CLOSING_NET_CAPITALIZEDValue; }
            set { SetProperty(ref CLOSING_NET_CAPITALIZEDValue, value); }
        }

        private System.Decimal? OPENING_RESERVESValue;
        public System.Decimal? OPENING_RESERVES
        {
            get { return this.OPENING_RESERVESValue; }
            set { SetProperty(ref OPENING_RESERVESValue, value); }
        }

        private System.Decimal? RESERVE_ADJUSTMENTSValue;
        public System.Decimal? RESERVE_ADJUSTMENTS
        {
            get { return this.RESERVE_ADJUSTMENTSValue; }
            set { SetProperty(ref RESERVE_ADJUSTMENTSValue, value); }
        }

        private System.Decimal? CLOSING_RESERVESValue;
        public System.Decimal? CLOSING_RESERVES
        {
            get { return this.CLOSING_RESERVESValue; }
            set { SetProperty(ref CLOSING_RESERVESValue, value); }
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


