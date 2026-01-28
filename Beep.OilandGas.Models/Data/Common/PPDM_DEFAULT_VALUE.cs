using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class PPDM_DEFAULT_VALUE : ModelEntityBase
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.Int64? ROW_IDValue;
        public System.Int64? ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
