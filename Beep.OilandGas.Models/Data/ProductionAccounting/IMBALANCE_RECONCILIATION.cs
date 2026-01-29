using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMBALANCE_RECONCILIATION : ModelEntityBase {
        private System.String IMBALANCE_RECONCILIATION_IDValue;
        public System.String IMBALANCE_RECONCILIATION_ID
        {
            get { return this.IMBALANCE_RECONCILIATION_IDValue; }
            set { SetProperty(ref IMBALANCE_RECONCILIATION_IDValue, value); }
        }

        private System.DateTime? RECONCILIATION_DATEValue;
        public System.DateTime? RECONCILIATION_DATE
        {
            get { return this.RECONCILIATION_DATEValue; }
            set { SetProperty(ref RECONCILIATION_DATEValue, value); }
        }

        private System.Decimal  TOTAL_IMBALANCE_BEFOREValue;
        public System.Decimal  TOTAL_IMBALANCE_BEFORE
        {
            get { return this.TOTAL_IMBALANCE_BEFOREValue; }
            set { SetProperty(ref TOTAL_IMBALANCE_BEFOREValue, value); }
        }

        private System.Decimal  TOTAL_ADJUSTMENTSValue;
        public System.Decimal  TOTAL_ADJUSTMENTS
        {
            get { return this.TOTAL_ADJUSTMENTSValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTSValue, value); }
        }

        private System.Decimal  TOTAL_IMBALANCE_AFTERValue;
        public System.Decimal  TOTAL_IMBALANCE_AFTER
        {
            get { return this.TOTAL_IMBALANCE_AFTERValue; }
            set { SetProperty(ref TOTAL_IMBALANCE_AFTERValue, value); }
        }

        private System.String RECONCILED_BYValue;
        public System.String RECONCILED_BY
        {
            get { return this.RECONCILED_BYValue; }
            set { SetProperty(ref RECONCILED_BYValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        // Standard PPDM columns

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private string RECONCILIATION_IDValue;
        public string RECONCILIATION_ID
        {
            get { return this.RECONCILIATION_IDValue; }
            set { SetProperty(ref RECONCILIATION_IDValue, value); }
        }

   
    }
}
