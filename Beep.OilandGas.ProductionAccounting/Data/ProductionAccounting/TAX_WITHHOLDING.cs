using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAX_WITHHOLDING : ModelEntityBase {
        private System.String TAX_WITHHOLDING_IDValue;
        public System.String TAX_WITHHOLDING_ID
        {
            get { return this.TAX_WITHHOLDING_IDValue; }
            set { SetProperty(ref TAX_WITHHOLDING_IDValue, value); }
        }

        private System.String ROYALTY_PAYMENT_IDValue;
        public System.String ROYALTY_PAYMENT_ID
        {
            get { return this.ROYALTY_PAYMENT_IDValue; }
            set { SetProperty(ref ROYALTY_PAYMENT_IDValue, value); }
        }

        private System.String WITHHOLDING_TYPEValue;
        public System.String WITHHOLDING_TYPE
        {
            get { return this.WITHHOLDING_TYPEValue; }
            set { SetProperty(ref WITHHOLDING_TYPEValue, value); }
        }

        private System.Decimal  WITHHOLDING_RATEValue;
        public System.Decimal  WITHHOLDING_RATE
        {
            get { return this.WITHHOLDING_RATEValue; }
            set { SetProperty(ref WITHHOLDING_RATEValue, value); }
        }

        private System.Decimal  AMOUNTValue;
        public System.Decimal  AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private System.String REASONValue;
        public System.String REASON
        {
            get { return this.REASONValue; }
            set { SetProperty(ref REASONValue, value); }
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
