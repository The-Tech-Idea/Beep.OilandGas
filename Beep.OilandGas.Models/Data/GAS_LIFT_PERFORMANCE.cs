using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class GAS_LIFT_PERFORMANCE : Entity
    {
        private string PERFORMANCE_IDValue;
        public string PERFORMANCE_ID
        {
            get { return this.PERFORMANCE_IDValue; }
            set { SetProperty(ref PERFORMANCE_IDValue, value); }
        }

        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private DateTime? PERFORMANCE_DATEValue;
        public DateTime? PERFORMANCE_DATE
        {
            get { return this.PERFORMANCE_DATEValue; }
            set { SetProperty(ref PERFORMANCE_DATEValue, value); }
        }

        private decimal? GAS_INJECTION_RATEValue;
        public decimal? GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
        }

        private decimal? PRODUCTION_RATEValue;
        public decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private decimal? GAS_LIQUID_RATIOValue;
        public decimal? GAS_LIQUID_RATIO
        {
            get { return this.GAS_LIQUID_RATIOValue; }
            set { SetProperty(ref GAS_LIQUID_RATIOValue, value); }
        }

        private decimal? EFFICIENCYValue;
        public decimal? EFFICIENCY
        {
            get { return this.EFFICIENCYValue; }
            set { SetProperty(ref EFFICIENCYValue, value); }
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

