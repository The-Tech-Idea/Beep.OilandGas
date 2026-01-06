using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class PRODUCTION_FORECAST : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
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

        private String FORECAST_TYPEValue;
        public String FORECAST_TYPE
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
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

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

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}



