using System;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class PRODUCTION_FORECAST_POINT : ModelEntityBase {
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
    }
}





