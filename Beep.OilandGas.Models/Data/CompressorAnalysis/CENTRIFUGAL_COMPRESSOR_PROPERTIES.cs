using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class CENTRIFUGAL_COMPRESSOR_PROPERTIES : ModelEntityBase {
        private String CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue;
        public String CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID
        {
            get { return this.CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue; }
            set { SetProperty(ref CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? POLYTROPIC_EFFICIENCYValue;
        public Decimal? POLYTROPIC_EFFICIENCY
        {
            get { return this.POLYTROPIC_EFFICIENCYValue; }
            set { SetProperty(ref POLYTROPIC_EFFICIENCYValue, value); }
        }

        private Decimal? SPECIFIC_HEAT_RATIOValue;
        public Decimal? SPECIFIC_HEAT_RATIO
        {
            get { return this.SPECIFIC_HEAT_RATIOValue; }
            set { SetProperty(ref SPECIFIC_HEAT_RATIOValue, value); }
        }

        private Int32? NUMBER_OF_STAGESValue;
        public Int32? NUMBER_OF_STAGES
        {
            get { return this.NUMBER_OF_STAGESValue; }
            set { SetProperty(ref NUMBER_OF_STAGESValue, value); }
        }

        private Decimal? SPEEDValue;
        public Decimal? SPEED
        {
            get { return this.SPEEDValue; }
            set { SetProperty(ref SPEEDValue, value); }
        }

        // Standard PPDM columns

    }
}
