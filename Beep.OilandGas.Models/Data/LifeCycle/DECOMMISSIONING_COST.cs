using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public partial class DECOMMISSIONING_COST : ModelEntityBase
    {
        private System.String COST_IDValue;
        public System.String COST_ID
        {
            get => COST_IDValue;
            set => SetProperty(ref COST_IDValue, value);
        }

        private System.String ENTITY_TYPEValue;
        public System.String ENTITY_TYPE
        {
            get => ENTITY_TYPEValue;
            set => SetProperty(ref ENTITY_TYPEValue, value);
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get => ENTITY_IDValue;
            set => SetProperty(ref ENTITY_IDValue, value);
        }

        private System.String COST_TYPEValue;
        public System.String COST_TYPE
        {
            get => COST_TYPEValue;
            set => SetProperty(ref COST_TYPEValue, value);
        }

        private System.Decimal? ESTIMATED_COSTValue;
        public System.Decimal? ESTIMATED_COST
        {
            get => ESTIMATED_COSTValue;
            set => SetProperty(ref ESTIMATED_COSTValue, value);
        }

        private System.Decimal? ACTUAL_COSTValue;
        public System.Decimal? ACTUAL_COST
        {
            get => ACTUAL_COSTValue;
            set => SetProperty(ref ACTUAL_COSTValue, value);
        }

        private System.String CURRENCYValue;
        public System.String CURRENCY
        {
            get => CURRENCYValue;
            set => SetProperty(ref CURRENCYValue, value);
        }

        private System.DateTime? ESTIMATE_DATEValue;
        public System.DateTime? ESTIMATE_DATE
        {
            get => ESTIMATE_DATEValue;
            set => SetProperty(ref ESTIMATE_DATEValue, value);
        }

        private System.DateTime? ACTUAL_COST_DATEValue;
        public System.DateTime? ACTUAL_COST_DATE
        {
            get => ACTUAL_COST_DATEValue;
            set => SetProperty(ref ACTUAL_COST_DATEValue, value);
        }

        private System.String COST_CATEGORYValue;
        public System.String COST_CATEGORY
        {
            get => COST_CATEGORYValue;
            set => SetProperty(ref COST_CATEGORYValue, value);
        }

        private System.String VENDOR_IDValue;
        public System.String VENDOR_ID
        {
            get => VENDOR_IDValue;
            set => SetProperty(ref VENDOR_IDValue, value);
        }

        private System.String INVOICE_REFERENCEValue;
        public System.String INVOICE_REFERENCE
        {
            get => INVOICE_REFERENCEValue;
            set => SetProperty(ref INVOICE_REFERENCEValue, value);
        }

        private System.String COST_STATUSValue;
        public System.String COST_STATUS
        {
            get => COST_STATUSValue;
            set => SetProperty(ref COST_STATUSValue, value);
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get => REMARKValue;
            set => SetProperty(ref REMARKValue, value);
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get => SOURCEValue;
            set => SetProperty(ref SOURCEValue, value);
        }

        public DECOMMISSIONING_COST() { }
    }
}
