using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOINT_INTEREST_BILL : ModelEntityBase {
        private String JIB_IDValue;
        public String JIB_ID
        {
            get { return this.JIB_IDValue; }
            set { SetProperty(ref JIB_IDValue, value); }
        }

        private String INTEREST_SET_IDValue;
        public String INTEREST_SET_ID
        {
            get { return this.INTEREST_SET_IDValue; }
            set { SetProperty(ref INTEREST_SET_IDValue, value); }
        }

        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private String OPERATOR_IDValue;
        public String OPERATOR_ID
        {
            get { return this.OPERATOR_IDValue; }
            set { SetProperty(ref OPERATOR_IDValue, value); }
        }

        private DateTime? BILL_PERIOD_START_DATEValue;
        public DateTime? BILL_PERIOD_START_DATE
        {
            get { return this.BILL_PERIOD_START_DATEValue; }
            set { SetProperty(ref BILL_PERIOD_START_DATEValue, value); }
        }

        private DateTime? BILL_PERIOD_END_DATEValue;
        public DateTime? BILL_PERIOD_END_DATE
        {
            get { return this.BILL_PERIOD_END_DATEValue; }
            set { SetProperty(ref BILL_PERIOD_END_DATEValue, value); }
        }

        private Decimal TOTAL_BILL_AMOUNTValue;
        public Decimal TOTAL_BILL_AMOUNT
        {
            get { return this.TOTAL_BILL_AMOUNTValue; }
            set { SetProperty(ref TOTAL_BILL_AMOUNTValue, value); }
        }

        private Decimal? WORKING_INTEREST_SHAREValue;
        public Decimal? WORKING_INTEREST_SHARE
        {
            get { return this.WORKING_INTEREST_SHAREValue; }
            set { SetProperty(ref WORKING_INTEREST_SHAREValue, value); }
        }

        private Decimal? NET_AMOUNT_DUEValue;
        public Decimal? NET_AMOUNT_DUE
        {
            get { return this.NET_AMOUNT_DUEValue; }
            set { SetProperty(ref NET_AMOUNT_DUEValue, value); }
        }

        private String BILL_STATUSValue;
        public String BILL_STATUS
        {
            get { return this.BILL_STATUSValue; }
            set { SetProperty(ref BILL_STATUSValue, value); }
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
        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}





