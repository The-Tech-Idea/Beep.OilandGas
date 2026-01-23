using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Common
{
    /// <summary>
    /// Generic List of Value table for storing custom/non-PPDM LOVs
    /// </summary>
    public partial class LIST_OF_VALUE : ModelEntityBase
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


