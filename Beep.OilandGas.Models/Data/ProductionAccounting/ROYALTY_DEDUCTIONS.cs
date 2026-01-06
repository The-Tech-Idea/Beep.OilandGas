using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_DEDUCTIONS : Entity, IPPDMEntity
    {
        private System.String ROYALTY_DEDUCTIONS_IDValue;
        public System.String ROYALTY_DEDUCTIONS_ID
        {
            get { return this.ROYALTY_DEDUCTIONS_IDValue; }
            set { SetProperty(ref ROYALTY_DEDUCTIONS_IDValue, value); }
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




