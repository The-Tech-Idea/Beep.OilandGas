using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOIB_LINE_ITEM : ModelEntityBase {
        private System.String JOIB_LINE_ITEM_IDValue;
        public System.String JOIB_LINE_ITEM_ID
        {
            get { return this.JOIB_LINE_ITEM_IDValue; }
            set { SetProperty(ref JOIB_LINE_ITEM_IDValue, value); }
        }

        private System.String JOINT_INTEREST_BILL_IDValue;
        public System.String JOINT_INTEREST_BILL_ID
        {
            get { return this.JOINT_INTEREST_BILL_IDValue; }
            set { SetProperty(ref JOINT_INTEREST_BILL_IDValue, value); }
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

        private System.Decimal? AMOUNTValue;
        public System.Decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
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


