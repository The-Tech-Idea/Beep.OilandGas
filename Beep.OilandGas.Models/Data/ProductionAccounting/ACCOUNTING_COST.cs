using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ACCOUNTING_COST : ModelEntityBase {
        private String ACCOUNTING_COST_IDValue;
        public String ACCOUNTING_COST_ID
        {
            get { return this.ACCOUNTING_COST_IDValue; }
            set { SetProperty(ref ACCOUNTING_COST_IDValue, value); }
        }

        private String FINANCE_IDValue;
        public String FINANCE_ID
        {
            get { return this.FINANCE_IDValue; }
            set { SetProperty(ref FINANCE_IDValue, value); }
        }
        private String PROPERTY_IDValue;
        public String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private String WELL_IDValue;
        public String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private String POOL_IDValue;
        public String POOL_ID
        {
            get { return this.POOL_IDValue; }
            set { SetProperty(ref POOL_IDValue, value); }
        }

        private String COST_TYPEValue;
        public String COST_TYPE
        {
            get { return this.COST_TYPEValue; }
            set { SetProperty(ref COST_TYPEValue, value); }
        }

        private String AFE_IDValue;
        public String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private String COST_CATEGORYValue;
        public String COST_CATEGORY
        {
            get { return this.COST_CATEGORYValue; }
            set { SetProperty(ref COST_CATEGORYValue, value); }
        }

        private Decimal AMOUNTValue;
        public Decimal AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private DateTime? COST_DATEValue;
        public DateTime? COST_DATE
        {
            get { return this.COST_DATEValue; }
            set { SetProperty(ref COST_DATEValue, value); }
        }

        private String IS_CAPITALIZEDValue;
        public String IS_CAPITALIZED
        {
            get { return this.IS_CAPITALIZEDValue; }
            set { SetProperty(ref IS_CAPITALIZEDValue, value); }
        }

        private String IS_EXPENSEDValue;
        public String IS_EXPENSED
        {
            get { return this.IS_EXPENSEDValue; }
            set { SetProperty(ref IS_EXPENSEDValue, value); }
        }

        private String DRY_HOLE_FLAGValue;
        public String DRY_HOLE_FLAG
        {
            get { return this.DRY_HOLE_FLAGValue; }
            set { SetProperty(ref DRY_HOLE_FLAGValue, value); }
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


