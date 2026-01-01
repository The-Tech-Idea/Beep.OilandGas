using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class PRODUCTION_FORECAST : Entity
    {
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

        private string FORECAST_TYPEValue;
        public string FORECAST_TYPE
        {
            get { return this.FORECAST_TYPEValue; }
            set { SetProperty(ref FORECAST_TYPEValue, value); }
        }

        private DateTime? FORECAST_START_DATEValue;
        public DateTime? FORECAST_START_DATE
        {
            get { return this.FORECAST_START_DATEValue; }
            set { SetProperty(ref FORECAST_START_DATEValue, value); }
        }

        private DateTime? FORECAST_END_DATEValue;
        public DateTime? FORECAST_END_DATE
        {
            get { return this.FORECAST_END_DATEValue; }
            set { SetProperty(ref FORECAST_END_DATEValue, value); }
        }

        // Standard PPDM columns
        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
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

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }
    }
}

