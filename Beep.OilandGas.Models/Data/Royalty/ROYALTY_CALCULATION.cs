using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Royalty
{
    public partial class ROYALTY_CALCULATION : Entity
    {
        private System.String ROYALTY_CALCULATION_IDValue;
        public System.String ROYALTY_CALCULATION_ID
        {
            get { return this.ROYALTY_CALCULATION_IDValue; }
            set { SetProperty(ref ROYALTY_CALCULATION_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String REVENUE_TRANSACTION_IDValue;
        public System.String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.Decimal? GROSS_REVENUEValue;
        public System.Decimal? GROSS_REVENUE
        {
            get { return this.GROSS_REVENUEValue; }
            set { SetProperty(ref GROSS_REVENUEValue, value); }
        }

        private System.Decimal? PRODUCTION_TAXESValue;
        public System.Decimal? PRODUCTION_TAXES
        {
            get { return this.PRODUCTION_TAXESValue; }
            set { SetProperty(ref PRODUCTION_TAXESValue, value); }
        }

        private System.Decimal? TRANSPORTATION_COSTSValue;
        public System.Decimal? TRANSPORTATION_COSTS
        {
            get { return this.TRANSPORTATION_COSTSValue; }
            set { SetProperty(ref TRANSPORTATION_COSTSValue, value); }
        }

        private System.Decimal? PROCESSING_COSTSValue;
        public System.Decimal? PROCESSING_COSTS
        {
            get { return this.PROCESSING_COSTSValue; }
            set { SetProperty(ref PROCESSING_COSTSValue, value); }
        }

        private System.Decimal? MARKETING_COSTSValue;
        public System.Decimal? MARKETING_COSTS
        {
            get { return this.MARKETING_COSTSValue; }
            set { SetProperty(ref MARKETING_COSTSValue, value); }
        }

        private System.Decimal? OTHER_DEDUCTIONSValue;
        public System.Decimal? OTHER_DEDUCTIONS
        {
            get { return this.OTHER_DEDUCTIONSValue; }
            set { SetProperty(ref OTHER_DEDUCTIONSValue, value); }
        }

        private System.Decimal? TOTAL_DEDUCTIONSValue;
        public System.Decimal? TOTAL_DEDUCTIONS
        {
            get { return this.TOTAL_DEDUCTIONSValue; }
            set { SetProperty(ref TOTAL_DEDUCTIONSValue, value); }
        }

        private System.Decimal? NET_REVENUEValue;
        public System.Decimal? NET_REVENUE
        {
            get { return this.NET_REVENUEValue; }
            set { SetProperty(ref NET_REVENUEValue, value); }
        }

        private System.Decimal? ROYALTY_INTERESTValue;
        public System.Decimal? ROYALTY_INTEREST
        {
            get { return this.ROYALTY_INTERESTValue; }
            set { SetProperty(ref ROYALTY_INTERESTValue, value); }
        }

        private System.Decimal? ROYALTY_AMOUNTValue;
        public System.Decimal? ROYALTY_AMOUNT
        {
            get { return this.ROYALTY_AMOUNTValue; }
            set { SetProperty(ref ROYALTY_AMOUNTValue, value); }
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

