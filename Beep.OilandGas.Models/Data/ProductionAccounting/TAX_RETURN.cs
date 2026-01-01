using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAX_RETURN : Entity,IPPDMEntity
    {
        private System.String TAX_RETURN_IDValue;
        public System.String TAX_RETURN_ID
        {
            get { return this.TAX_RETURN_IDValue; }
            set { SetProperty(ref TAX_RETURN_IDValue, value); }
        }

        private System.String RETURN_NUMBERValue;
        public System.String RETURN_NUMBER
        {
            get { return this.RETURN_NUMBERValue; }
            set { SetProperty(ref RETURN_NUMBERValue, value); }
        }

        private System.String TAX_TYPEValue;
        public System.String TAX_TYPE
        {
            get { return this.TAX_TYPEValue; }
            set { SetProperty(ref TAX_TYPEValue, value); }
        }

        private System.DateTime? RETURN_PERIOD_STARTValue;
        public System.DateTime? RETURN_PERIOD_START
        {
            get { return this.RETURN_PERIOD_STARTValue; }
            set { SetProperty(ref RETURN_PERIOD_STARTValue, value); }
        }

        private System.DateTime? RETURN_PERIOD_ENDValue;
        public System.DateTime? RETURN_PERIOD_END
        {
            get { return this.RETURN_PERIOD_ENDValue; }
            set { SetProperty(ref RETURN_PERIOD_ENDValue, value); }
        }

        private System.DateTime? FILING_DATEValue;
        public System.DateTime? FILING_DATE
        {
            get { return this.FILING_DATEValue; }
            set { SetProperty(ref FILING_DATEValue, value); }
        }

        private System.Decimal? TOTAL_TAX_LIABILITYValue;
        public System.Decimal? TOTAL_TAX_LIABILITY
        {
            get { return this.TOTAL_TAX_LIABILITYValue; }
            set { SetProperty(ref TOTAL_TAX_LIABILITYValue, value); }
        }

        private System.Decimal? TAX_PAIDValue;
        public System.Decimal? TAX_PAID
        {
            get { return this.TAX_PAIDValue; }
            set { SetProperty(ref TAX_PAIDValue, value); }
        }

        private System.Decimal? TAX_DUEValue;
        public System.Decimal? TAX_DUE
        {
            get { return this.TAX_DUEValue; }
            set { SetProperty(ref TAX_DUEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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

