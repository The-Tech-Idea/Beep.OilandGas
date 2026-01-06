using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AMORTIZATION_RECORD : Entity,IPPDMEntity
    {
        private System.String AMORTIZATION_RECORD_IDValue;
        public System.String AMORTIZATION_RECORD_ID
        {
            get { return this.AMORTIZATION_RECORD_IDValue; }
            set { SetProperty(ref AMORTIZATION_RECORD_IDValue, value); }
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

        private System.DateTime? PERIOD_START_DATEValue;
        public System.DateTime? PERIOD_START_DATE
        {
            get { return this.PERIOD_START_DATEValue; }
            set { SetProperty(ref PERIOD_START_DATEValue, value); }
        }

        private System.DateTime? PERIOD_END_DATEValue;
        public System.DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private System.Decimal? NET_CAPITALIZED_COSTSValue;
        public System.Decimal? NET_CAPITALIZED_COSTS
        {
            get { return this.NET_CAPITALIZED_COSTSValue; }
            set { SetProperty(ref NET_CAPITALIZED_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_RESERVES_BOEValue;
        public System.Decimal? TOTAL_RESERVES_BOE
        {
            get { return this.TOTAL_RESERVES_BOEValue; }
            set { SetProperty(ref TOTAL_RESERVES_BOEValue, value); }
        }

        private System.Decimal? PRODUCTION_BOEValue;
        public System.Decimal? PRODUCTION_BOE
        {
            get { return this.PRODUCTION_BOEValue; }
            set { SetProperty(ref PRODUCTION_BOEValue, value); }
        }

        private System.Decimal? AMORTIZATION_AMOUNTValue;
        public System.Decimal? AMORTIZATION_AMOUNT
        {
            get { return this.AMORTIZATION_AMOUNTValue; }
            set { SetProperty(ref AMORTIZATION_AMOUNTValue, value); }
        }

        private System.String ACCOUNTING_METHODValue;
        public System.String ACCOUNTING_METHOD
        {
            get { return this.ACCOUNTING_METHODValue; }
            set { SetProperty(ref ACCOUNTING_METHODValue, value); }
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

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}





