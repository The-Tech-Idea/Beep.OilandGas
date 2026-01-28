using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasProperties
{
    public partial class PSEUDO_PRESSURE_RESULT : ModelEntityBase {
        private String PSEUDO_PRESSURE_RESULT_IDValue;
        public String PSEUDO_PRESSURE_RESULT_ID
        {
            get { return this.PSEUDO_PRESSURE_RESULT_IDValue; }
            set { SetProperty(ref PSEUDO_PRESSURE_RESULT_IDValue, value); }
        }

        private String GAS_COMPOSITION_IDValue;
        public String GAS_COMPOSITION_ID
        {
            get { return this.GAS_COMPOSITION_IDValue; }
            set { SetProperty(ref GAS_COMPOSITION_IDValue, value); }
        }

        private Decimal? PRESSUREValue;
        public Decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private Decimal? PSEUDO_PRESSUREValue;
        public Decimal? PSEUDO_PRESSURE
        {
            get { return this.PSEUDO_PRESSUREValue; }
            set { SetProperty(ref PSEUDO_PRESSUREValue, value); }
        }

        private Decimal? Z_FACTORValue;
        public Decimal? Z_FACTOR
        {
            get { return this.Z_FACTORValue; }
            set { SetProperty(ref Z_FACTORValue, value); }
        }

        private Decimal? VISCOSITYValue;
        public Decimal? VISCOSITY
        {
            get { return this.VISCOSITYValue; }
            set { SetProperty(ref VISCOSITYValue, value); }
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
