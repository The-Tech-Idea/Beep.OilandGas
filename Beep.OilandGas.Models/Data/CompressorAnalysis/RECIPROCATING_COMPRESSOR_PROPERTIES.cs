using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class RECIPROCATING_COMPRESSOR_PROPERTIES : ModelEntityBase {
        private String RECIPROCATING_COMPRESSOR_PROPERTIES_IDValue;
        public String RECIPROCATING_COMPRESSOR_PROPERTIES_ID
        {
            get { return this.RECIPROCATING_COMPRESSOR_PROPERTIES_IDValue; }
            set { SetProperty(ref RECIPROCATING_COMPRESSOR_PROPERTIES_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? CYLINDER_DIAMETERValue;
        public Decimal? CYLINDER_DIAMETER
        {
            get { return this.CYLINDER_DIAMETERValue; }
            set { SetProperty(ref CYLINDER_DIAMETERValue, value); }
        }

        private Decimal? STROKE_LENGTHValue;
        public Decimal? STROKE_LENGTH
        {
            get { return this.STROKE_LENGTHValue; }
            set { SetProperty(ref STROKE_LENGTHValue, value); }
        }

        private Decimal? ROTATIONAL_SPEEDValue;
        public Decimal? ROTATIONAL_SPEED
        {
            get { return this.ROTATIONAL_SPEEDValue; }
            set { SetProperty(ref ROTATIONAL_SPEEDValue, value); }
        }

        private Int32? NUMBER_OF_CYLINDERSValue;
        public Int32? NUMBER_OF_CYLINDERS
        {
            get { return this.NUMBER_OF_CYLINDERSValue; }
            set { SetProperty(ref NUMBER_OF_CYLINDERSValue, value); }
        }

        private Decimal? VOLUMETRIC_EFFICIENCYValue;
        public Decimal? VOLUMETRIC_EFFICIENCY
        {
            get { return this.VOLUMETRIC_EFFICIENCYValue; }
            set { SetProperty(ref VOLUMETRIC_EFFICIENCYValue, value); }
        }

        private Decimal? CLEARANCE_FACTORValue;
        public Decimal? CLEARANCE_FACTOR
        {
            get { return this.CLEARANCE_FACTORValue; }
            set { SetProperty(ref CLEARANCE_FACTORValue, value); }
        }

        // Standard PPDM columns

    }
}
