using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Export
{
    public partial class EXPORT_HISTORY : Entity
    {
        private System.String EXPORT_HISTORY_IDValue;
        public System.String EXPORT_HISTORY_ID
        {
            get { return this.EXPORT_HISTORY_IDValue; }
            set { SetProperty(ref EXPORT_HISTORY_IDValue, value); }
        }

        private System.String EXPORT_TYPEValue;
        public System.String EXPORT_TYPE
        {
            get { return this.EXPORT_TYPEValue; }
            set { SetProperty(ref EXPORT_TYPEValue, value); }
        }

        private System.String EXPORT_FORMATValue;
        public System.String EXPORT_FORMAT
        {
            get { return this.EXPORT_FORMATValue; }
            set { SetProperty(ref EXPORT_FORMATValue, value); }
        }

        private System.DateTime? EXPORT_DATEValue;
        public System.DateTime? EXPORT_DATE
        {
            get { return this.EXPORT_DATEValue; }
            set { SetProperty(ref EXPORT_DATEValue, value); }
        }

        private System.String EXPORTED_BYValue;
        public System.String EXPORTED_BY
        {
            get { return this.EXPORTED_BYValue; }
            set { SetProperty(ref EXPORTED_BYValue, value); }
        }

        private System.String FILE_PATHValue;
        public System.String FILE_PATH
        {
            get { return this.FILE_PATHValue; }
            set { SetProperty(ref FILE_PATHValue, value); }
        }

        private System.Int32? RECORD_COUNTValue;
        public System.Int32? RECORD_COUNT
        {
            get { return this.RECORD_COUNTValue; }
            set { SetProperty(ref RECORD_COUNTValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
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
    }
}

