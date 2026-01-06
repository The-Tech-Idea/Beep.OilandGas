using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class PRODUCTION_FORECAST_POINT : Entity
    {
        private string FORECAST_POINT_IDValue;
        public string FORECAST_POINT_ID
        {
            get { return this.FORECAST_POINT_IDValue; }
            set { SetProperty(ref FORECAST_POINT_IDValue, value); }
        }

        private string FORECAST_IDValue;
        public string FORECAST_ID
        {
            get { return this.FORECAST_IDValue; }
            set { SetProperty(ref FORECAST_IDValue, value); }
        }

        private DateTime? FORECAST_DATEValue;
        public DateTime? FORECAST_DATE
        {
            get { return this.FORECAST_DATEValue; }
            set { SetProperty(ref FORECAST_DATEValue, value); }
        }

        private decimal? OIL_RATEValue;
        public decimal? OIL_RATE
        {
            get { return this.OIL_RATEValue; }
            set { SetProperty(ref OIL_RATEValue, value); }
        }

        private decimal? GAS_RATEValue;
        public decimal? GAS_RATE
        {
            get { return this.GAS_RATEValue; }
            set { SetProperty(ref GAS_RATEValue, value); }
        }

        private decimal? WATER_RATEValue;
        public decimal? WATER_RATE
        {
            get { return this.WATER_RATEValue; }
            set { SetProperty(ref WATER_RATEValue, value); }
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




