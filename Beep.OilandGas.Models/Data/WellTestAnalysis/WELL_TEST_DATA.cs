using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class WELL_TEST_DATA : ModelEntityBase
    {
        private String WELL_TEST_DATA_IDValue;
        public String WELL_TEST_DATA_ID
        {
            get { return this.WELL_TEST_DATA_IDValue; }
            set { SetProperty(ref WELL_TEST_DATA_IDValue, value); }
        }

        private Decimal  FLOW_RATEValue;
        public Decimal  FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal? WELLBORE_RADIUSValue;
        public Decimal? WELLBORE_RADIUS
        {
            get { return this.WELLBORE_RADIUSValue; }
            set { SetProperty(ref WELLBORE_RADIUSValue, value); }
        }

        private Decimal  FORMATION_THICKNESSValue;
        public Decimal  FORMATION_THICKNESS
        {
            get { return this.FORMATION_THICKNESSValue; }
            set { SetProperty(ref FORMATION_THICKNESSValue, value); }
        }

        private Decimal? POROSITYValue;
        public Decimal? POROSITY
        {
            get { return this.POROSITYValue; }
            set { SetProperty(ref POROSITYValue, value); }
        }

        private Decimal? TOTAL_COMPRESSIBILITYValue;
        public Decimal? TOTAL_COMPRESSIBILITY
        {
            get { return this.TOTAL_COMPRESSIBILITYValue; }
            set { SetProperty(ref TOTAL_COMPRESSIBILITYValue, value); }
        }

        private Decimal? OIL_VISCOSITYValue;
        public Decimal? OIL_VISCOSITY
        {
            get { return this.OIL_VISCOSITYValue; }
            set { SetProperty(ref OIL_VISCOSITYValue, value); }
        }

        private Decimal? OIL_FORMATION_VOLUME_FACTORValue;
        public Decimal? OIL_FORMATION_VOLUME_FACTOR
        {
            get { return this.OIL_FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref OIL_FORMATION_VOLUME_FACTORValue, value); }
        }

        private String TEST_TYPEValue;
        public String TEST_TYPE
        {
            get { return this.TEST_TYPEValue; }
            set { SetProperty(ref TEST_TYPEValue, value); }
        }

        private Decimal? PRODUCTION_TIMEValue;
        public Decimal? PRODUCTION_TIME
        {
            get { return this.PRODUCTION_TIMEValue; }
            set { SetProperty(ref PRODUCTION_TIMEValue, value); }
        }

        private Boolean? IS_GAS_WELLValue;
        public Boolean? IS_GAS_WELL
        {
            get { return this.IS_GAS_WELLValue; }
            set { SetProperty(ref IS_GAS_WELLValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal  RESERVOIR_TEMPERATUREValue;
        public Decimal  RESERVOIR_TEMPERATURE
        {
            get { return this.RESERVOIR_TEMPERATUREValue; }
            set { SetProperty(ref RESERVOIR_TEMPERATUREValue, value); }
        }

        private Decimal? INITIAL_RESERVOIR_PRESSUREValue;
        public Decimal? INITIAL_RESERVOIR_PRESSURE
        {
            get { return this.INITIAL_RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref INITIAL_RESERVOIR_PRESSUREValue, value); }
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

        // Analysis Compatibility Properties
        public System.Collections.Generic.List<double> Time { get; set; } = new System.Collections.Generic.List<double>();
        public System.Collections.Generic.List<double> Pressure { get; set; } = new System.Collections.Generic.List<double>();
    }
}


