using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CONTRACT_PERFORMANCE_OBLIGATION : ModelEntityBase {
        private System.String CONTRACT_PERFORMANCE_OBLIGATION_IDValue;
        public System.String CONTRACT_PERFORMANCE_OBLIGATION_ID
        {
            get { return this.CONTRACT_PERFORMANCE_OBLIGATION_IDValue; }
            set { SetProperty(ref CONTRACT_PERFORMANCE_OBLIGATION_IDValue, value); }
        }

        private System.String SALES_CONTRACT_IDValue;
        public System.String SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private System.String OBLIGATION_TYPEValue;
        public System.String OBLIGATION_TYPE
        {
            get { return this.OBLIGATION_TYPEValue; }
            set { SetProperty(ref OBLIGATION_TYPEValue, value); }
        }

        private System.String OBLIGATION_DESCRIPTIONValue;
        public System.String OBLIGATION_DESCRIPTION
        {
            get { return this.OBLIGATION_DESCRIPTIONValue; }
            set { SetProperty(ref OBLIGATION_DESCRIPTIONValue, value); }
        }

        private System.Decimal? ALLOCATED_PRICEValue;
        public System.Decimal? ALLOCATED_PRICE
        {
            get { return this.ALLOCATED_PRICEValue; }
            set { SetProperty(ref ALLOCATED_PRICEValue, value); }
        }

        private System.DateTime? SATISFIED_DATEValue;
        public System.DateTime? SATISFIED_DATE
        {
            get { return this.SATISFIED_DATEValue; }
            set { SetProperty(ref SATISFIED_DATEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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
    }
}
