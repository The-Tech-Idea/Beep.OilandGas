using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AFE_LINE_ITEM : ModelEntityBase {
        private System.String AFE_LINE_ITEM_IDValue;
        public System.String AFE_LINE_ITEM_ID
        {
            get { return this.AFE_LINE_ITEM_IDValue; }
            set { SetProperty(ref AFE_LINE_ITEM_IDValue, value); }
        }

        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.String LINE_ITEM_NUMBERValue;
        public System.String LINE_ITEM_NUMBER
        {
            get { return this.LINE_ITEM_NUMBERValue; }
            set { SetProperty(ref LINE_ITEM_NUMBERValue, value); }
        }

        private System.String COST_CATEGORYValue;
        public System.String COST_CATEGORY
        {
            get { return this.COST_CATEGORYValue; }
            set { SetProperty(ref COST_CATEGORYValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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

        private System.Decimal? VARIANCEValue;
        public System.Decimal? VARIANCE
        {
            get { return this.VARIANCEValue; }
            set { SetProperty(ref VARIANCEValue, value); }
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
