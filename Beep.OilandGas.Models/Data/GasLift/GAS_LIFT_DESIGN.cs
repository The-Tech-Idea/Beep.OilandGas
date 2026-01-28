using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_DESIGN : ModelEntityBase {
        private string DESIGN_IDValue;
        public string DESIGN_ID
        {
            get { return this.DESIGN_IDValue; }
            set { SetProperty(ref DESIGN_IDValue, value); }
        }

        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private DateTime? DESIGN_DATEValue;
        public DateTime? DESIGN_DATE
        {
            get { return this.DESIGN_DATEValue; }
            set { SetProperty(ref DESIGN_DATEValue, value); }
        }

        private int? NUMBER_OF_VALVESValue;
        public int? NUMBER_OF_VALVES
        {
            get { return this.NUMBER_OF_VALVESValue; }
            set { SetProperty(ref NUMBER_OF_VALVESValue, value); }
        }

        private decimal? TOTAL_GAS_INJECTION_RATEValue;
        public decimal? TOTAL_GAS_INJECTION_RATE
        {
            get { return this.TOTAL_GAS_INJECTION_RATEValue; }
            set { SetProperty(ref TOTAL_GAS_INJECTION_RATEValue, value); }
        }

        private decimal? EXPECTED_PRODUCTION_RATEValue;
        public decimal? EXPECTED_PRODUCTION_RATE
        {
            get { return this.EXPECTED_PRODUCTION_RATEValue; }
            set { SetProperty(ref EXPECTED_PRODUCTION_RATEValue, value); }
        }

        // Standard PPDM columns


        private decimal GAS_INJECTION_PRESSUREValue;
        public decimal GAS_INJECTION_PRESSURE
        {
            get { return this.GAS_INJECTION_PRESSUREValue; }
            set { SetProperty(ref GAS_INJECTION_PRESSUREValue, value); }
        }

        private decimal SYSTEM_EFFICIENCYValue;
        public decimal SYSTEM_EFFICIENCY
        {
            get { return this.SYSTEM_EFFICIENCYValue; }
            set { SetProperty(ref SYSTEM_EFFICIENCYValue, value); }
        }

        private string? STATUSValue;
        public string? STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }
    }
}
