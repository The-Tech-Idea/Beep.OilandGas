using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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
        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
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

        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}



