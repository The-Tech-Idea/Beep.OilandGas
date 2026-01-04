using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class REGULATED_PRICE : Entity, IPPDMEntity
    {
        private System.String REGULATED_PRICE_IDValue;
        public System.String REGULATED_PRICE_ID
        {
            get { return this.REGULATED_PRICE_IDValue; }
            set { SetProperty(ref REGULATED_PRICE_IDValue, value); }
        }

        private System.String REGULATORY_AUTHORITYValue;
        public System.String REGULATORY_AUTHORITY
        {
            get { return this.REGULATORY_AUTHORITYValue; }
            set { SetProperty(ref REGULATORY_AUTHORITYValue, value); }
        }

        private System.String PRICE_FORMULAValue;
        public System.String PRICE_FORMULA
        {
            get { return this.PRICE_FORMULAValue; }
            set { SetProperty(ref PRICE_FORMULAValue, value); }
        }

        private System.DateTime? EFFECTIVE_START_DATEValue;
        public System.DateTime? EFFECTIVE_START_DATE
        {
            get { return this.EFFECTIVE_START_DATEValue; }
            set { SetProperty(ref EFFECTIVE_START_DATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_END_DATEValue;
        public System.DateTime? EFFECTIVE_END_DATE
        {
            get { return this.EFFECTIVE_END_DATEValue; }
            set { SetProperty(ref EFFECTIVE_END_DATEValue, value); }
        }

        private System.Decimal? PRICE_CAPValue;
        public System.Decimal? PRICE_CAP
        {
            get { return this.PRICE_CAPValue; }
            set { SetProperty(ref PRICE_CAPValue, value); }
        }

        private System.Decimal? PRICE_FLOORValue;
        public System.Decimal? PRICE_FLOOR
        {
            get { return this.PRICE_FLOORValue; }
            set { SetProperty(ref PRICE_FLOORValue, value); }
        }

        private System.Decimal? BASE_PRICEValue;
        public System.Decimal? BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String ADJUSTMENT_FACTORS_JSONValue;
        public System.String ADJUSTMENT_FACTORS_JSON
        {
            get { return this.ADJUSTMENT_FACTORS_JSONValue; }
            set { SetProperty(ref ADJUSTMENT_FACTORS_JSONValue, value); }
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
