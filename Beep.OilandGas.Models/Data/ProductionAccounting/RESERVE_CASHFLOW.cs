using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RESERVE_CASHFLOW : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private System.String RESERVE_CASHFLOW_IDValue;
        public System.String RESERVE_CASHFLOW_ID
        {
            get { return this.RESERVE_CASHFLOW_IDValue; }
            set { SetProperty(ref RESERVE_CASHFLOW_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.DateTime? PERIOD_END_DATEValue;
        public System.DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private System.Decimal? OIL_VOLUMEValue;
        public System.Decimal? OIL_VOLUME
        {
            get { return this.OIL_VOLUMEValue; }
            set { SetProperty(ref OIL_VOLUMEValue, value); }
        }

        private System.Decimal? GAS_VOLUMEValue;
        public System.Decimal? GAS_VOLUME
        {
            get { return this.GAS_VOLUMEValue; }
            set { SetProperty(ref GAS_VOLUMEValue, value); }
        }

        private System.Decimal? OIL_PRICEValue;
        public System.Decimal? OIL_PRICE
        {
            get { return this.OIL_PRICEValue; }
            set { SetProperty(ref OIL_PRICEValue, value); }
        }

        private System.Decimal? GAS_PRICEValue;
        public System.Decimal? GAS_PRICE
        {
            get { return this.GAS_PRICEValue; }
            set { SetProperty(ref GAS_PRICEValue, value); }
        }

        private System.Decimal? OPERATING_COSTValue;
        public System.Decimal? OPERATING_COST
        {
            get { return this.OPERATING_COSTValue; }
            set { SetProperty(ref OPERATING_COSTValue, value); }
        }

        private System.Decimal? DEVELOPMENT_COSTValue;
        public System.Decimal? DEVELOPMENT_COST
        {
            get { return this.DEVELOPMENT_COSTValue; }
            set { SetProperty(ref DEVELOPMENT_COSTValue, value); }
        }

        private System.Decimal? ABANDONMENT_COSTValue;
        public System.Decimal? ABANDONMENT_COST
        {
            get { return this.ABANDONMENT_COSTValue; }
            set { SetProperty(ref ABANDONMENT_COSTValue, value); }
        }

        private System.Decimal? TAX_RATEValue;
        public System.Decimal? TAX_RATE
        {
            get { return this.TAX_RATEValue; }
            set { SetProperty(ref TAX_RATEValue, value); }
        }

        private System.Decimal? DISCOUNT_RATEValue;
        public System.Decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }

        private System.Decimal? NET_CASH_FLOWValue;
        public System.Decimal? NET_CASH_FLOW
        {
            get { return this.NET_CASH_FLOWValue; }
            set { SetProperty(ref NET_CASH_FLOWValue, value); }
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
