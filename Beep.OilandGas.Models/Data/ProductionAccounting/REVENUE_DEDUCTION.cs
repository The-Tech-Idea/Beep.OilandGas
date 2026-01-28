using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class REVENUE_DEDUCTION : ModelEntityBase {
        private String REVENUE_DEDUCTION_IDValue;
        public String REVENUE_DEDUCTION_ID
        {
            get { return this.REVENUE_DEDUCTION_IDValue; }
            set { SetProperty(ref REVENUE_DEDUCTION_IDValue, value); }
        }

        private String REVENUE_TRANSACTION_IDValue;
        public String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private String DEDUCTION_TYPEValue;
        public String DEDUCTION_TYPE
        {
            get { return this.DEDUCTION_TYPEValue; }
            set { SetProperty(ref DEDUCTION_TYPEValue, value); }
        }

        private Decimal AMOUNTValue;
        public Decimal AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
