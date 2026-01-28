using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMPAIRMENT_RECORD : ModelEntityBase {
        private System.String IMPAIRMENT_RECORD_IDValue;
        public System.String IMPAIRMENT_RECORD_ID
        {
            get { return this.IMPAIRMENT_RECORD_IDValue; }
            set { SetProperty(ref IMPAIRMENT_RECORD_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        private System.DateTime? IMPAIRMENT_DATEValue;
        public System.DateTime? IMPAIRMENT_DATE
        {
            get { return this.IMPAIRMENT_DATEValue; }
            set { SetProperty(ref IMPAIRMENT_DATEValue, value); }
        }

        private System.Decimal? IMPAIRMENT_AMOUNTValue;
        public System.Decimal? IMPAIRMENT_AMOUNT
        {
            get { return this.IMPAIRMENT_AMOUNTValue; }
            set { SetProperty(ref IMPAIRMENT_AMOUNTValue, value); }
        }

        private System.String REASONValue;
        public System.String REASON
        {
            get { return this.REASONValue; }
            set { SetProperty(ref REASONValue, value); }
        }

        private System.String IMPAIRMENT_TYPEValue;
        public System.String IMPAIRMENT_TYPE
        {
            get { return this.IMPAIRMENT_TYPEValue; }
            set { SetProperty(ref IMPAIRMENT_TYPEValue, value); }
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
