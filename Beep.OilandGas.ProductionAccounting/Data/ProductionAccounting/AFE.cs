using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AFE : ModelEntityBase {
        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.String AFE_NUMBERValue;
        public System.String AFE_NUMBER
        {
            get { return this.AFE_NUMBERValue; }
            set { SetProperty(ref AFE_NUMBERValue, value); }
        }

        private System.String AFE_NAMEValue;
        public System.String AFE_NAME
        {
            get { return this.AFE_NAMEValue; }
            set { SetProperty(ref AFE_NAMEValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.Decimal? ESTIMATED_COSTValue;
        public System.Decimal? ESTIMATED_COST
        {
            get { return this.ESTIMATED_COSTValue; }
            set { SetProperty(ref ESTIMATED_COSTValue, value); }
        }

        private System.Decimal? ACTUAL_COSTValue;
        public System.Decimal? ACTUAL_COST
        {
            get { return this.ACTUAL_COSTValue; }
            set { SetProperty(ref ACTUAL_COSTValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
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

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
