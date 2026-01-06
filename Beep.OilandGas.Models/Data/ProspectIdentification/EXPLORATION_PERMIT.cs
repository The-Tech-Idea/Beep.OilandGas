using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_PERMIT : Entity,IPPDMEntity
    {
        private System.String PERMIT_IDValue;
        public System.String PERMIT_ID
        {
            get
            {
                return this.PERMIT_IDValue;
            }
            set { SetProperty(ref PERMIT_IDValue, value); }
        }
        private System.String PERMIT_NUMBERValue;
        public System.String PERMIT_NUMBER
        {
            get
            {
                return this.PERMIT_NUMBERValue;
            }
            set { SetProperty(ref PERMIT_NUMBERValue, value); }
        }
        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get
            {
                return this.FIELD_IDValue;
            }
            set { SetProperty(ref FIELD_IDValue, value); }
        }
        private System.String PERMIT_HOLDERValue;
        public System.String PERMIT_HOLDER
        {
            get
            {
                return this.PERMIT_HOLDERValue;
            }
            set { SetProperty(ref PERMIT_HOLDERValue, value); }
        }
        private System.DateTime? PERMIT_START_DATEValue;
        public System.DateTime? PERMIT_START_DATE
        {
            get
            {
                return this.PERMIT_START_DATEValue;
            }
            set { SetProperty(ref PERMIT_START_DATEValue, value); }
        }
        private System.DateTime? PERMIT_END_DATEValue;
        public System.DateTime? PERMIT_END_DATE
        {
            get
            {
                return this.PERMIT_END_DATEValue;
            }
            set { SetProperty(ref PERMIT_END_DATEValue, value); }
        }
        private System.Decimal? PERMIT_AREAValue;
        public System.Decimal? PERMIT_AREA
        {
            get
            {
                return this.PERMIT_AREAValue;
            }
            set { SetProperty(ref PERMIT_AREAValue, value); }
        }
        private System.String PERMIT_AREA_OUOMValue;
        public System.String PERMIT_AREA_OUOM
        {
            get
            {
                return this.PERMIT_AREA_OUOMValue;
            }
            set { SetProperty(ref PERMIT_AREA_OUOMValue, value); }
        }
        private System.String PERMIT_TERMSValue;
        public System.String PERMIT_TERMS
        {
            get
            {
                return this.PERMIT_TERMSValue;
            }
            set { SetProperty(ref PERMIT_TERMSValue, value); }
        }
        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get
            {
                return this.DESCRIPTIONValue;
            }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get
            {
                return this.ACTIVE_INDValue;
            }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }
        private System.DateTime? EFFECTIVE_DATEValue;
        public System.DateTime? EFFECTIVE_DATE
        {
            get
            {
                return this.EFFECTIVE_DATEValue;
            }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }
        private System.DateTime? EXPIRY_DATEValue;
        public System.DateTime? EXPIRY_DATE
        {
            get
            {
                return this.EXPIRY_DATEValue;
            }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get
            {
                return this.PPDM_GUIDValue;
            }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }
        private System.String REMARKValue;
        public System.String REMARK
        {
            get
            {
                return this.REMARKValue;
            }
            set { SetProperty(ref REMARKValue, value); }
        }
        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get
            {
                return this.SOURCEValue;
            }
            set { SetProperty(ref SOURCEValue, value); }
        }
        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get
            {
                return this.ROW_CHANGED_BYValue;
            }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }
        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get
            {
                return this.ROW_CHANGED_DATEValue;
            }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }
        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get
            {
                return this.ROW_CREATED_BYValue;
            }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }
        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get
            {
                return this.ROW_CREATED_DATEValue;
            }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }
        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get
            {
                return this.ROW_EFFECTIVE_DATEValue;
            }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }
        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get
            {
                return this.ROW_EXPIRY_DATEValue;
            }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get
            {
                return this.ROW_QUALITYValue;
            }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        public EXPLORATION_PERMIT() { }
    }
}




