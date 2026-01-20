using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GL_ACCOUNT : ModelEntityBase {
        private System.String GL_ACCOUNT_IDValue;
        public System.String GL_ACCOUNT_ID
        {
            get { return this.GL_ACCOUNT_IDValue; }
            set { SetProperty(ref GL_ACCOUNT_IDValue, value); }
        }

        private System.String ACCOUNT_NUMBERValue;
        public System.String ACCOUNT_NUMBER
        {
            get { return this.ACCOUNT_NUMBERValue; }
            set { SetProperty(ref ACCOUNT_NUMBERValue, value); }
        }

        private System.String ACCOUNT_NAMEValue;
        public System.String ACCOUNT_NAME
        {
            get { return this.ACCOUNT_NAMEValue; }
            set { SetProperty(ref ACCOUNT_NAMEValue, value); }
        }

        private System.String ACCOUNT_TYPEValue;
        public System.String ACCOUNT_TYPE
        {
            get { return this.ACCOUNT_TYPEValue; }
            set { SetProperty(ref ACCOUNT_TYPEValue, value); }
        }

        private System.String PARENT_ACCOUNT_IDValue;
        public System.String PARENT_ACCOUNT_ID
        {
            get { return this.PARENT_ACCOUNT_IDValue; }
            set { SetProperty(ref PARENT_ACCOUNT_IDValue, value); }
        }

        private System.String NORMAL_BALANCEValue;
        public System.String NORMAL_BALANCE
        {
            get { return this.NORMAL_BALANCEValue; }
            set { SetProperty(ref NORMAL_BALANCEValue, value); }
        }

        private System.Decimal? OPENING_BALANCEValue;
        public System.Decimal? OPENING_BALANCE
        {
            get { return this.OPENING_BALANCEValue; }
            set { SetProperty(ref OPENING_BALANCEValue, value); }
        }

        private System.Decimal? CURRENT_BALANCEValue;
        public System.Decimal? CURRENT_BALANCE
        {
            get { return this.CURRENT_BALANCEValue; }
            set { SetProperty(ref CURRENT_BALANCEValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}





