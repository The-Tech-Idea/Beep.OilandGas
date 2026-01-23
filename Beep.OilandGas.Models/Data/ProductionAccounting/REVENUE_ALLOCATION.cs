using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class REVENUE_ALLOCATION : ModelEntityBase {
        private System.String REVENUE_ALLOCATION_IDValue;
        public System.String REVENUE_ALLOCATION_ID
        {
            get { return this.REVENUE_ALLOCATION_IDValue; }
            set { SetProperty(ref REVENUE_ALLOCATION_IDValue, value); }
        }

        private System.String REVENUE_TRANSACTION_IDValue;
        public System.String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.String INTEREST_OWNER_BA_IDValue;
        public System.String INTEREST_OWNER_BA_ID
        {
            get { return this.INTEREST_OWNER_BA_IDValue; }
            set { SetProperty(ref INTEREST_OWNER_BA_IDValue, value); }
        }

        private System.Decimal? INTEREST_PERCENTAGEValue;
        public System.Decimal? INTEREST_PERCENTAGE
        {
            get { return this.INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref INTEREST_PERCENTAGEValue, value); }
        }

        private System.Decimal? ALLOCATED_AMOUNTValue;
        public System.Decimal? ALLOCATED_AMOUNT
        {
            get { return this.ALLOCATED_AMOUNTValue; }
            set { SetProperty(ref ALLOCATED_AMOUNTValue, value); }
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


