using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public partial class RUN_TICKET_VALUATION : Entity
    {
        private System.String VALUATION_IDValue;
        public System.String VALUATION_ID
        {
            get { return this.VALUATION_IDValue; }
            set { SetProperty(ref VALUATION_IDValue, value); }
        }

        private System.String RUN_TICKET_NUMBERValue;
        public System.String RUN_TICKET_NUMBER
        {
            get { return this.RUN_TICKET_NUMBERValue; }
            set { SetProperty(ref RUN_TICKET_NUMBERValue, value); }
        }

        private System.DateTime? VALUATION_DATEValue;
        public System.DateTime? VALUATION_DATE
        {
            get { return this.VALUATION_DATEValue; }
            set { SetProperty(ref VALUATION_DATEValue, value); }
        }

        private System.Decimal? BASE_PRICEValue;
        public System.Decimal? BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.Decimal? ADJUSTED_PRICEValue;
        public System.Decimal? ADJUSTED_PRICE
        {
            get { return this.ADJUSTED_PRICEValue; }
            set { SetProperty(ref ADJUSTED_PRICEValue, value); }
        }

        private System.Decimal? NET_VOLUMEValue;
        public System.Decimal? NET_VOLUME
        {
            get { return this.NET_VOLUMEValue; }
            set { SetProperty(ref NET_VOLUMEValue, value); }
        }

        private System.Decimal? TOTAL_VALUEValue;
        public System.Decimal? TOTAL_VALUE
        {
            get { return this.TOTAL_VALUEValue; }
            set { SetProperty(ref TOTAL_VALUEValue, value); }
        }

        private System.String PRICING_METHODValue;
        public System.String PRICING_METHOD
        {
            get { return this.PRICING_METHODValue; }
            set { SetProperty(ref PRICING_METHODValue, value); }
        }

        // Quality adjustments
        private System.Decimal? API_GRAVITY_ADJUSTMENTValue;
        public System.Decimal? API_GRAVITY_ADJUSTMENT
        {
            get { return this.API_GRAVITY_ADJUSTMENTValue; }
            set { SetProperty(ref API_GRAVITY_ADJUSTMENTValue, value); }
        }

        private System.Decimal? SULFUR_ADJUSTMENTValue;
        public System.Decimal? SULFUR_ADJUSTMENT
        {
            get { return this.SULFUR_ADJUSTMENTValue; }
            set { SetProperty(ref SULFUR_ADJUSTMENTValue, value); }
        }

        private System.Decimal? BSW_ADJUSTMENTValue;
        public System.Decimal? BSW_ADJUSTMENT
        {
            get { return this.BSW_ADJUSTMENTValue; }
            set { SetProperty(ref BSW_ADJUSTMENTValue, value); }
        }

        private System.Decimal? OTHER_QUALITY_ADJUSTMENTValue;
        public System.Decimal? OTHER_QUALITY_ADJUSTMENT
        {
            get { return this.OTHER_QUALITY_ADJUSTMENTValue; }
            set { SetProperty(ref OTHER_QUALITY_ADJUSTMENTValue, value); }
        }

        // Location adjustments
        private System.Decimal? LOCATION_DIFFERENTIALValue;
        public System.Decimal? LOCATION_DIFFERENTIAL
        {
            get { return this.LOCATION_DIFFERENTIALValue; }
            set { SetProperty(ref LOCATION_DIFFERENTIALValue, value); }
        }

        private System.Decimal? TRANSPORTATION_ADJUSTMENTValue;
        public System.Decimal? TRANSPORTATION_ADJUSTMENT
        {
            get { return this.TRANSPORTATION_ADJUSTMENTValue; }
            set { SetProperty(ref TRANSPORTATION_ADJUSTMENTValue, value); }
        }

        // Time adjustments
        private System.Decimal? TIME_DIFFERENTIALValue;
        public System.Decimal? TIME_DIFFERENTIAL
        {
            get { return this.TIME_DIFFERENTIALValue; }
            set { SetProperty(ref TIME_DIFFERENTIALValue, value); }
        }

        private System.Decimal? INTEREST_ADJUSTMENTValue;
        public System.Decimal? INTEREST_ADJUSTMENT
        {
            get { return this.INTEREST_ADJUSTMENTValue; }
            set { SetProperty(ref INTEREST_ADJUSTMENTValue, value); }
        }

        private System.Decimal? TOTAL_ADJUSTMENTSValue;
        public System.Decimal? TOTAL_ADJUSTMENTS
        {
            get { return this.TOTAL_ADJUSTMENTSValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTSValue, value); }
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




