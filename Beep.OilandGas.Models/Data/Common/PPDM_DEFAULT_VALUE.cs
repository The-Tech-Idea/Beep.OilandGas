using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Common
{
    /// <summary>
    /// Stores PPDM default values per database and per user
    /// Allows system defaults and user-specific overrides
    /// </summary>
    public partial class PPDM_DEFAULT_VALUE : Entity, IPPDMEntity
    {
        private System.String DEFAULT_VALUE_IDValue;
        public System.String DEFAULT_VALUE_ID
        {
            get { return this.DEFAULT_VALUE_IDValue; }
            set { SetProperty(ref DEFAULT_VALUE_IDValue, value); }
        }

        private System.String DEFAULT_KEYValue;
        public System.String DEFAULT_KEY
        {
            get { return this.DEFAULT_KEYValue; }
            set { SetProperty(ref DEFAULT_KEYValue, value); }
        }

        private System.String DEFAULT_VALUEValue;
        public System.String DEFAULT_VALUE
        {
            get { return this.DEFAULT_VALUEValue; }
            set { SetProperty(ref DEFAULT_VALUEValue, value); }
        }

        private System.String DEFAULT_CATEGORYValue;
        public System.String DEFAULT_CATEGORY
        {
            get { return this.DEFAULT_CATEGORYValue; }
            set { SetProperty(ref DEFAULT_CATEGORYValue, value); }
        }

        private System.String VALUE_TYPEValue;
        public System.String VALUE_TYPE
        {
            get { return this.VALUE_TYPEValue; }
            set { SetProperty(ref VALUE_TYPEValue, value); }
        }

        private System.String USER_IDValue;
        public System.String USER_ID
        {
            get { return this.USER_IDValue; }
            set { SetProperty(ref USER_IDValue, value); }
        }

        private System.String DATABASE_IDValue;
        public System.String DATABASE_ID
        {
            get { return this.DATABASE_IDValue; }
            set { SetProperty(ref DATABASE_IDValue, value); }
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

        private System.Int64? ROW_IDValue;
        public System.Int64? ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}



