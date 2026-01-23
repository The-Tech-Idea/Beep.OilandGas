using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COST_VARIANCE_REPORT : ModelEntityBase {
        private System.String COST_VARIANCE_REPORT_IDValue;
        public System.String COST_VARIANCE_REPORT_ID
        {
            get { return this.COST_VARIANCE_REPORT_IDValue; }
            set { SetProperty(ref COST_VARIANCE_REPORT_IDValue, value); }
        }

        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
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

        private System.Decimal? BUDGET_AMOUNTValue;
        public System.Decimal? BUDGET_AMOUNT
        {
            get { return this.BUDGET_AMOUNTValue; }
            set { SetProperty(ref BUDGET_AMOUNTValue, value); }
        }

        private System.Decimal? ACTUAL_AMOUNTValue;
        public System.Decimal? ACTUAL_AMOUNT
        {
            get { return this.ACTUAL_AMOUNTValue; }
            set { SetProperty(ref ACTUAL_AMOUNTValue, value); }
        }

        private System.Decimal? VARIANCE_AMOUNTValue;
        public System.Decimal? VARIANCE_AMOUNT
        {
            get { return this.VARIANCE_AMOUNTValue; }
            set { SetProperty(ref VARIANCE_AMOUNTValue, value); }
        }

        private System.Decimal? VARIANCE_PERCENTValue;
        public System.Decimal? VARIANCE_PERCENT
        {
            get { return this.VARIANCE_PERCENTValue; }
            set { SetProperty(ref VARIANCE_PERCENTValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


