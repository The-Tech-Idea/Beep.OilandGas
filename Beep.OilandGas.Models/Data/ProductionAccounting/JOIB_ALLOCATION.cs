using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOIB_ALLOCATION : ModelEntityBase {
        private System.String JOIB_ALLOCATION_IDValue;
        public System.String JOIB_ALLOCATION_ID
        {
            get { return this.JOIB_ALLOCATION_IDValue; }
            set { SetProperty(ref JOIB_ALLOCATION_IDValue, value); }
        }

        private System.String JOINT_INTEREST_BILL_IDValue;
        public System.String JOINT_INTEREST_BILL_ID
        {
            get { return this.JOINT_INTEREST_BILL_IDValue; }
            set { SetProperty(ref JOINT_INTEREST_BILL_IDValue, value); }
        }

        private System.String JOIB_LINE_ITEM_IDValue;
        public System.String JOIB_LINE_ITEM_ID
        {
            get { return this.JOIB_LINE_ITEM_IDValue; }
            set { SetProperty(ref JOIB_LINE_ITEM_IDValue, value); }
        }

        private System.String INTEREST_OWNER_BA_IDValue;
        public System.String INTEREST_OWNER_BA_ID
        {
            get { return this.INTEREST_OWNER_BA_IDValue; }
            set { SetProperty(ref INTEREST_OWNER_BA_IDValue, value); }
        }

        private System.Decimal  ALLOCATED_AMOUNTValue;
        public System.Decimal  ALLOCATED_AMOUNT
        {
            get { return this.ALLOCATED_AMOUNTValue; }
            set { SetProperty(ref ALLOCATED_AMOUNTValue, value); }
        }

        private System.Decimal  ALLOCATION_PERCENTAGEValue;
        public System.Decimal  ALLOCATION_PERCENTAGE
        {
            get { return this.ALLOCATION_PERCENTAGEValue; }
            set { SetProperty(ref ALLOCATION_PERCENTAGEValue, value); }
        }

        private System.String ALLOCATION_METHODValue;
        public System.String ALLOCATION_METHOD
        {
            get { return this.ALLOCATION_METHODValue; }
            set { SetProperty(ref ALLOCATION_METHODValue, value); }
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
