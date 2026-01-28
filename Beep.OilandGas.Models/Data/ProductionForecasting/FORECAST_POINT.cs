using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class FORECAST_POINT : ModelEntityBase {
        private String FORECAST_POINT_IDValue;
        public String FORECAST_POINT_ID
        {
            get { return this.FORECAST_POINT_IDValue; }
            set { SetProperty(ref FORECAST_POINT_IDValue, value); }
        }

        private String PRODUCTION_FORECAST_IDValue;
        public String PRODUCTION_FORECAST_ID
        {
            get { return this.PRODUCTION_FORECAST_IDValue; }
            set { SetProperty(ref PRODUCTION_FORECAST_IDValue, value); }
        }

        private Decimal? TIMEValue;
        public Decimal? TIME
        {
            get { return this.TIMEValue; }
            set { SetProperty(ref TIMEValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private Decimal? CUMULATIVE_PRODUCTIONValue;
        public Decimal? CUMULATIVE_PRODUCTION
        {
            get { return this.CUMULATIVE_PRODUCTIONValue; }
            set { SetProperty(ref CUMULATIVE_PRODUCTIONValue, value); }
        }

        private Decimal? RESERVOIR_PRESSUREValue;
        public Decimal? RESERVOIR_PRESSURE
        {
            get { return this.RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref RESERVOIR_PRESSUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Int32? POINT_ORDERValue;
        public Int32? POINT_ORDER
        {
            get { return this.POINT_ORDERValue; }
            set { SetProperty(ref POINT_ORDERValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        public DateTime FORECAST_DATE { get; set; }

        private decimal? DECLINE_EXPONENTValue;
        public decimal? DECLINE_EXPONENT
        {
            get { return this.DECLINE_EXPONENTValue; }
            set { SetProperty(ref DECLINE_EXPONENTValue, value); }
        }

        private string FORECAST_METHODValue;
        public string FORECAST_METHOD
        {
            get { return this.FORECAST_METHODValue; }
            set { SetProperty(ref FORECAST_METHODValue, value); }
        }
    }
}
