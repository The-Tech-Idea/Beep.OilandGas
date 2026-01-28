using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class NOMINATION : ModelEntityBase {
        private System.String NOMINATION_IDValue;
        public System.String NOMINATION_ID
        {
            get { return this.NOMINATION_IDValue; }
            set { SetProperty(ref NOMINATION_IDValue, value); }
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

        private System.String DELIVERY_POINTS_JSONValue;
        public System.String DELIVERY_POINTS_JSON
        {
            get { return this.DELIVERY_POINTS_JSONValue; }
            set { SetProperty(ref DELIVERY_POINTS_JSONValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? SUBMISSION_DATEValue;
        public System.DateTime? SUBMISSION_DATE
        {
            get { return this.SUBMISSION_DATEValue; }
            set { SetProperty(ref SUBMISSION_DATEValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        private System.String APPROVED_BYValue;
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
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
