using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class PRODUCTION_FORECAST : ModelEntityBase {
        private String PRODUCTION_FORECAST_IDValue;
        public String PRODUCTION_FORECAST_ID
        {
            get { return this.PRODUCTION_FORECAST_IDValue; }
            set { SetProperty(ref PRODUCTION_FORECAST_IDValue, value); }
        }

        private String RESERVOIR_FORECAST_PROPERTIES_IDValue;
        public String RESERVOIR_FORECAST_PROPERTIES_ID
        {
            get { return this.RESERVOIR_FORECAST_PROPERTIES_IDValue; }
            set { SetProperty(ref RESERVOIR_FORECAST_PROPERTIES_IDValue, value); }
        }

        private ForecastType FORECAST_TYPEValue;
        public ForecastType FORECAST_TYPE
        {
            get { return this.FORECAST_TYPEValue; }
            set { SetProperty(ref FORECAST_TYPEValue, value); }
        }

        private Decimal? FORECAST_DURATIONValue;
        public Decimal? FORECAST_DURATION
        {
            get { return this.FORECAST_DURATIONValue; }
            set { SetProperty(ref FORECAST_DURATIONValue, value); }
        }

        private Decimal? INITIAL_PRODUCTION_RATEValue;
        public Decimal? INITIAL_PRODUCTION_RATE
        {
            get { return this.INITIAL_PRODUCTION_RATEValue; }
            set { SetProperty(ref INITIAL_PRODUCTION_RATEValue, value); }
        }

        private Decimal? FINAL_PRODUCTION_RATEValue;
        public Decimal? FINAL_PRODUCTION_RATE
        {
            get { return this.FINAL_PRODUCTION_RATEValue; }
            set { SetProperty(ref FINAL_PRODUCTION_RATEValue, value); }
        }

        private Decimal? TOTAL_CUMULATIVE_PRODUCTIONValue;
        public Decimal? TOTAL_CUMULATIVE_PRODUCTION
        {
            get { return this.TOTAL_CUMULATIVE_PRODUCTIONValue; }
            set { SetProperty(ref TOTAL_CUMULATIVE_PRODUCTIONValue, value); }
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

        private string FORECAST_IDValue;

        public string FORECAST_ID

        {

            get { return this.FORECAST_IDValue; }

            set { SetProperty(ref FORECAST_IDValue, value); }

        }
        private string WELL_UWIValue;

        public string WELL_UWI

        {

            get { return this.WELL_UWIValue; }

            set { SetProperty(ref WELL_UWIValue, value); }

        }
        private string FIELD_IDValue;

        public string FIELD_ID

        {

            get { return this.FIELD_IDValue; }

            set { SetProperty(ref FIELD_IDValue, value); }

        }
        private string FORECAST_NAMEValue;

        public string FORECAST_NAME

        {

            get { return this.FORECAST_NAMEValue; }

            set { SetProperty(ref FORECAST_NAMEValue, value); }

        }
        private DateTime FORECAST_START_DATEValue;

        public DateTime FORECAST_START_DATE

        {

            get { return this.FORECAST_START_DATEValue; }

            set { SetProperty(ref FORECAST_START_DATEValue, value); }

        }
        private List<FORECAST_POINT>? ForecastPointsValue = new List<FORECAST_POINT>();

        public List<FORECAST_POINT>? FORECAST_POINTS
        {
            get { return this.ForecastPointsValue; }
            set { SetProperty(ref ForecastPointsValue, value); }
        }

      
        
    }
}


