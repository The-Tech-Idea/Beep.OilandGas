using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasProperties
{
    public partial class AVERAGE_GAS_PROPERTIES : ModelEntityBase {
        private String AVERAGE_GAS_PROPERTIES_IDValue;
        public String AVERAGE_GAS_PROPERTIES_ID
        {
            get { return this.AVERAGE_GAS_PROPERTIES_IDValue; }
            set { SetProperty(ref AVERAGE_GAS_PROPERTIES_IDValue, value); }
        }

        private String GAS_COMPOSITION_IDValue;
        public String GAS_COMPOSITION_ID
        {
            get { return this.GAS_COMPOSITION_IDValue; }
            set { SetProperty(ref GAS_COMPOSITION_IDValue, value); }
        }

        private Decimal? AVERAGE_PRESSUREValue;
        public Decimal? AVERAGE_PRESSURE
        {
            get { return this.AVERAGE_PRESSUREValue; }
            set { SetProperty(ref AVERAGE_PRESSUREValue, value); }
        }

        private Decimal? AVERAGE_TEMPERATUREValue;
        public Decimal? AVERAGE_TEMPERATURE
        {
            get { return this.AVERAGE_TEMPERATUREValue; }
            set { SetProperty(ref AVERAGE_TEMPERATUREValue, value); }
        }

        private Decimal? AVERAGE_Z_FACTORValue;
        public Decimal? AVERAGE_Z_FACTOR
        {
            get { return this.AVERAGE_Z_FACTORValue; }
            set { SetProperty(ref AVERAGE_Z_FACTORValue, value); }
        }

        private Decimal? AVERAGE_VISCOSITYValue;
        public Decimal? AVERAGE_VISCOSITY
        {
            get { return this.AVERAGE_VISCOSITYValue; }
            set { SetProperty(ref AVERAGE_VISCOSITYValue, value); }
        }

        // Standard PPDM columns

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

    }
}


