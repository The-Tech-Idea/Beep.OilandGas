using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RUN_TICKET_VALUATION : ModelEntityBase {
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

        private System.Decimal  BASE_PRICEValue;
        public System.Decimal  BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String QUALITY_ADJUSTMENTS_JSONValue;
        public System.String QUALITY_ADJUSTMENTS_JSON
        {
            get { return this.QUALITY_ADJUSTMENTS_JSONValue; }
            set { SetProperty(ref QUALITY_ADJUSTMENTS_JSONValue, value); }
        }

        private System.String LOCATION_ADJUSTMENTS_JSONValue;
        public System.String LOCATION_ADJUSTMENTS_JSON
        {
            get { return this.LOCATION_ADJUSTMENTS_JSONValue; }
            set { SetProperty(ref LOCATION_ADJUSTMENTS_JSONValue, value); }
        }

        private System.String TIME_ADJUSTMENTS_JSONValue;
        public System.String TIME_ADJUSTMENTS_JSON
        {
            get { return this.TIME_ADJUSTMENTS_JSONValue; }
            set { SetProperty(ref TIME_ADJUSTMENTS_JSONValue, value); }
        }

        private System.Decimal  TOTAL_ADJUSTMENTSValue;
        public System.Decimal  TOTAL_ADJUSTMENTS
        {
            get { return this.TOTAL_ADJUSTMENTSValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTSValue, value); }
        }

        private System.Decimal  ADJUSTED_PRICEValue;
        public System.Decimal  ADJUSTED_PRICE
        {
            get { return this.ADJUSTED_PRICEValue; }
            set { SetProperty(ref ADJUSTED_PRICEValue, value); }
        }

        private System.Decimal  NET_VOLUMEValue;
        public System.Decimal  NET_VOLUME
        {
            get { return this.NET_VOLUMEValue; }
            set { SetProperty(ref NET_VOLUMEValue, value); }
        }

        private System.Decimal  TOTAL_VALUEValue;
        public System.Decimal  TOTAL_VALUE
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

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private decimal? API_GRAVITY_ADJUSTMENTValue;

        public decimal? API_GRAVITY_ADJUSTMENT

        {

            get { return this.API_GRAVITY_ADJUSTMENTValue; }

            set { SetProperty(ref API_GRAVITY_ADJUSTMENTValue, value); }

        }
        private decimal? SULFUR_ADJUSTMENTValue;

        public decimal? SULFUR_ADJUSTMENT

        {

            get { return this.SULFUR_ADJUSTMENTValue; }

            set { SetProperty(ref SULFUR_ADJUSTMENTValue, value); }

        }
        private decimal? BSW_ADJUSTMENTValue;

        public decimal? BSW_ADJUSTMENT

        {

            get { return this.BSW_ADJUSTMENTValue; }

            set { SetProperty(ref BSW_ADJUSTMENTValue, value); }

        }
        private decimal? LOCATION_DIFFERENTIALValue;

        public decimal? LOCATION_DIFFERENTIAL

        {

            get { return this.LOCATION_DIFFERENTIALValue; }

            set { SetProperty(ref LOCATION_DIFFERENTIALValue, value); }

        }
        private decimal? OTHER_QUALITY_ADJUSTMENTValue;

        public decimal? OTHER_QUALITY_ADJUSTMENT

        {

            get { return this.OTHER_QUALITY_ADJUSTMENTValue; }

            set { SetProperty(ref OTHER_QUALITY_ADJUSTMENTValue, value); }

        }
        private decimal? TRANSPORTATION_ADJUSTMENTValue;

        public decimal? TRANSPORTATION_ADJUSTMENT

        {

            get { return this.TRANSPORTATION_ADJUSTMENTValue; }

            set { SetProperty(ref TRANSPORTATION_ADJUSTMENTValue, value); }

        }
        private decimal? TIME_DIFFERENTIALValue;

        public decimal? TIME_DIFFERENTIAL

        {

            get { return this.TIME_DIFFERENTIALValue; }

            set { SetProperty(ref TIME_DIFFERENTIALValue, value); }

        }
        private decimal? INTEREST_ADJUSTMENTValue;

        public decimal? INTEREST_ADJUSTMENT

        {

            get { return this.INTEREST_ADJUSTMENTValue; }

            set { SetProperty(ref INTEREST_ADJUSTMENTValue, value); }

        }
    }
}
