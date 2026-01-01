using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Common
{
    /// <summary>
    /// Generic List of Value table for storing custom/non-PPDM LOVs
    /// </summary>
    public partial class LIST_OF_VALUE : Entity,IPPDMEntity
    {
        private System.String LIST_OF_VALUE_IDValue;
        public System.String LIST_OF_VALUE_ID
        {
            get { return this.LIST_OF_VALUE_IDValue; }
            set { SetProperty(ref LIST_OF_VALUE_IDValue, value); }
        }

        private System.String VALUE_TYPEValue;
        public System.String VALUE_TYPE
        {
            get { return this.VALUE_TYPEValue; }
            set { SetProperty(ref VALUE_TYPEValue, value); }
        }

        private System.String VALUE_CODEValue;
        public System.String VALUE_CODE
        {
            get { return this.VALUE_CODEValue; }
            set { SetProperty(ref VALUE_CODEValue, value); }
        }

        private System.String VALUE_NAMEValue;
        public System.String VALUE_NAME
        {
            get { return this.VALUE_NAMEValue; }
            set { SetProperty(ref VALUE_NAMEValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private System.String CATEGORYValue;
        public System.String CATEGORY
        {
            get { return this.CATEGORYValue; }
            set { SetProperty(ref CATEGORYValue, value); }
        }

        private System.String MODULEValue;
        public System.String MODULE
        {
            get { return this.MODULEValue; }
            set { SetProperty(ref MODULEValue, value); }
        }

        private System.Int32? SORT_ORDERValue;
        public System.Int32? SORT_ORDER
        {
            get { return this.SORT_ORDERValue; }
            set { SetProperty(ref SORT_ORDERValue, value); }
        }

        private System.String PARENT_VALUE_IDValue;
        public System.String PARENT_VALUE_ID
        {
            get { return this.PARENT_VALUE_IDValue; }
            set { SetProperty(ref PARENT_VALUE_IDValue, value); }
        }

        private System.String IS_DEFAULTValue;
        public System.String IS_DEFAULT
        {
            get { return this.IS_DEFAULTValue; }
            set { SetProperty(ref IS_DEFAULTValue, value); }
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

        private System.Int64? ROW_IDValue;
        public System.Int64? ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

