using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public partial class ECONOMIC_ANALYSIS_RESULT : Entity
    {
        private System.String ANALYSIS_IDValue;
        public System.String ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private System.DateTime? ANALYSIS_DATEValue;
        public System.DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private System.Decimal? NPVValue;
        public System.Decimal? NPV
        {
            get { return this.NPVValue; }
            set { SetProperty(ref NPVValue, value); }
        }

        private System.Decimal? IRRValue;
        public System.Decimal? IRR
        {
            get { return this.IRRValue; }
            set { SetProperty(ref IRRValue, value); }
        }

        private System.Decimal? PAYBACK_PERIODValue;
        public System.Decimal? PAYBACK_PERIOD
        {
            get { return this.PAYBACK_PERIODValue; }
            set { SetProperty(ref PAYBACK_PERIODValue, value); }
        }

        private System.Decimal? DISCOUNT_RATEValue;
        public System.Decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
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

