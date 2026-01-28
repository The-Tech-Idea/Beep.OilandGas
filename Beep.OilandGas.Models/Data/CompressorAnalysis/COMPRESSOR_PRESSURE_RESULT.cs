using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class COMPRESSOR_PRESSURE_RESULT : ModelEntityBase {
        private String COMPRESSOR_PRESSURE_RESULT_IDValue;
        public String COMPRESSOR_PRESSURE_RESULT_ID
        {
            get { return this.COMPRESSOR_PRESSURE_RESULT_IDValue; }
            set { SetProperty(ref COMPRESSOR_PRESSURE_RESULT_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? REQUIRED_DISCHARGE_PRESSUREValue;
        public Decimal? REQUIRED_DISCHARGE_PRESSURE
        {
            get { return this.REQUIRED_DISCHARGE_PRESSUREValue; }
            set { SetProperty(ref REQUIRED_DISCHARGE_PRESSUREValue, value); }
        }

        private Decimal? COMPRESSION_RATIOValue;
        public Decimal? COMPRESSION_RATIO
        {
            get { return this.COMPRESSION_RATIOValue; }
            set { SetProperty(ref COMPRESSION_RATIOValue, value); }
        }

        private Decimal? REQUIRED_POWERValue;
        public Decimal? REQUIRED_POWER
        {
            get { return this.REQUIRED_POWERValue; }
            set { SetProperty(ref REQUIRED_POWERValue, value); }
        }

        private Decimal? DISCHARGE_TEMPERATUREValue;
        public Decimal? DISCHARGE_TEMPERATURE
        {
            get { return this.DISCHARGE_TEMPERATUREValue; }
            set { SetProperty(ref DISCHARGE_TEMPERATUREValue, value); }
        }

        private Boolean? IS_FEASIBLEValue;
        public Boolean? IS_FEASIBLE
        {
            get { return this.IS_FEASIBLEValue; }
            set { SetProperty(ref IS_FEASIBLEValue, value); }
        }

        // Standard PPDM columns

    }
}
