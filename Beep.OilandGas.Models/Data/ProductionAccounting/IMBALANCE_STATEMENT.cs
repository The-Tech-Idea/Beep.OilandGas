using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMBALANCE_STATEMENT : ModelEntityBase {
        private System.String IMBALANCE_STATEMENT_IDValue;
        public System.String IMBALANCE_STATEMENT_ID
        {
            get { return this.IMBALANCE_STATEMENT_IDValue; }
            set { SetProperty(ref IMBALANCE_STATEMENT_IDValue, value); }
        }

        private System.DateTime? STATEMENT_PERIOD_STARTValue;
        public System.DateTime? STATEMENT_PERIOD_START
        {
            get { return this.STATEMENT_PERIOD_STARTValue; }
            set { SetProperty(ref STATEMENT_PERIOD_STARTValue, value); }
        }

        private System.DateTime? STATEMENT_PERIOD_ENDValue;
        public System.DateTime? STATEMENT_PERIOD_END
        {
            get { return this.STATEMENT_PERIOD_ENDValue; }
            set { SetProperty(ref STATEMENT_PERIOD_ENDValue, value); }
        }

        private System.String IMBALANCE_RECONCILIATION_IDValue;
        public System.String IMBALANCE_RECONCILIATION_ID
        {
            get { return this.IMBALANCE_RECONCILIATION_IDValue; }
            set { SetProperty(ref IMBALANCE_RECONCILIATION_IDValue, value); }
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

        private string STATEMENT_IDValue;
        public string STATEMENT_ID
        {
            get { return this.STATEMENT_IDValue; }
            set { SetProperty(ref STATEMENT_IDValue, value); }
        }

     
    }
}
