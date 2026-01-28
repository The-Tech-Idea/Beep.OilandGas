using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Export
{
    public partial class EXPORT_HISTORY : ModelEntityBase
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
