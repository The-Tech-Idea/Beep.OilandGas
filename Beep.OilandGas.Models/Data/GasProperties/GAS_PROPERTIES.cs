using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasProperties
{
    public partial class GAS_PROPERTIES : ModelEntityBase {
        private String GAS_PROPERTIES_IDValue;
        public String GAS_PROPERTIES_ID
        {
            get { return this.GAS_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_PROPERTIES_IDValue, value); }
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

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
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

        private Decimal? DENSITYValue;
        public Decimal? DENSITY
        {
            get { return this.DENSITYValue; }
            set { SetProperty(ref DENSITYValue, value); }
        }

        private Decimal? SPECIFIC_GRAVITYValue;
        public Decimal? SPECIFIC_GRAVITY
        {
            get { return this.SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? MOLECULAR_WEIGHTValue;
        public Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
        }

        private Decimal? PSEUDO_REDUCED_PRESSUREValue;
        public Decimal? PSEUDO_REDUCED_PRESSURE
        {
            get { return this.PSEUDO_REDUCED_PRESSUREValue; }
            set { SetProperty(ref PSEUDO_REDUCED_PRESSUREValue, value); }
        }

        private Decimal? PSEUDO_REDUCED_TEMPERATUREValue;
        public Decimal? PSEUDO_REDUCED_TEMPERATURE
        {
            get { return this.PSEUDO_REDUCED_TEMPERATUREValue; }
            set { SetProperty(ref PSEUDO_REDUCED_TEMPERATUREValue, value); }
        }

        private Decimal? PSEUDO_CRITICAL_PRESSUREValue;
        public Decimal? PSEUDO_CRITICAL_PRESSURE
        {
            get { return this.PSEUDO_CRITICAL_PRESSUREValue; }
            set { SetProperty(ref PSEUDO_CRITICAL_PRESSUREValue, value); }
        }

        private Decimal? PSEUDO_CRITICAL_TEMPERATUREValue;
        public Decimal? PSEUDO_CRITICAL_TEMPERATURE
        {
            get { return this.PSEUDO_CRITICAL_TEMPERATUREValue; }
            set { SetProperty(ref PSEUDO_CRITICAL_TEMPERATUREValue, value); }
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
