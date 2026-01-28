using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COST_CENTER : ModelEntityBase {
        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        private System.String COST_CENTER_NAMEValue;
        public System.String COST_CENTER_NAME
        {
            get { return this.COST_CENTER_NAMEValue; }
            set { SetProperty(ref COST_CENTER_NAMEValue, value); }
        }

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String ACCOUNTING_METHODValue;
        public System.String ACCOUNTING_METHOD
        {
            get { return this.ACCOUNTING_METHODValue; }
            set { SetProperty(ref ACCOUNTING_METHODValue, value); }
        }

        private System.Decimal? TOTAL_CAPITALIZED_COSTSValue;
        public System.Decimal? TOTAL_CAPITALIZED_COSTS
        {
            get { return this.TOTAL_CAPITALIZED_COSTSValue; }
            set { SetProperty(ref TOTAL_CAPITALIZED_COSTSValue, value); }
        }

        private System.Decimal? ACCUMULATED_AMORTIZATIONValue;
        public System.Decimal? ACCUMULATED_AMORTIZATION
        {
            get { return this.ACCUMULATED_AMORTIZATIONValue; }
            set { SetProperty(ref ACCUMULATED_AMORTIZATIONValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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
