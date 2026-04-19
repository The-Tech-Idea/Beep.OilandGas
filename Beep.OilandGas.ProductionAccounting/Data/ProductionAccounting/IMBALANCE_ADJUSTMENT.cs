using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMBALANCE_ADJUSTMENT : ModelEntityBase {
        private System.String IMBALANCE_ADJUSTMENT_IDValue;
        public System.String IMBALANCE_ADJUSTMENT_ID
        {
            get { return this.IMBALANCE_ADJUSTMENT_IDValue; }
            set { SetProperty(ref IMBALANCE_ADJUSTMENT_IDValue, value); }
        }

        private System.String IMBALANCE_RECONCILIATION_IDValue;
        public System.String IMBALANCE_RECONCILIATION_ID
        {
            get { return this.IMBALANCE_RECONCILIATION_IDValue; }
            set { SetProperty(ref IMBALANCE_RECONCILIATION_IDValue, value); }
        }

        private System.String ADJUSTMENT_TYPEValue;
        public System.String ADJUSTMENT_TYPE
        {
            get { return this.ADJUSTMENT_TYPEValue; }
            set { SetProperty(ref ADJUSTMENT_TYPEValue, value); }
        }

        private System.Decimal?  ADJUSTMENT_AMOUNTValue;
        public System.Decimal?  ADJUSTMENT_AMOUNT
        {
            get { return this.ADJUSTMENT_AMOUNTValue; }
            set { SetProperty(ref ADJUSTMENT_AMOUNTValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.String REASONValue;
        public System.String REASON
        {
            get { return this.REASONValue; }
            set { SetProperty(ref REASONValue, value); }
        }

        // Standard PPDM columns

       

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private string ADJUSTMENT_IDValue;
        public string ADJUSTMENT_ID
        {
            get { return this.ADJUSTMENT_IDValue; }
            set { SetProperty(ref ADJUSTMENT_IDValue, value); }
        }

      
    }
}
