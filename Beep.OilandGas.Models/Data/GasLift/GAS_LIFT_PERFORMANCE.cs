using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_PERFORMANCE : ModelEntityBase {
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

        private string? STATUSValue;
        public string? STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }
    }
}
