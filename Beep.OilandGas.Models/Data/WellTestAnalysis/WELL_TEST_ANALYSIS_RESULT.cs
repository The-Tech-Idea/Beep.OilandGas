using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class WELL_TEST_ANALYSIS_RESULT : ModelEntityBase {
        private String WELL_TEST_ANALYSIS_RESULT_IDValue;
        public String WELL_TEST_ANALYSIS_RESULT_ID
        {
            get { return this.WELL_TEST_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref WELL_TEST_ANALYSIS_RESULT_IDValue, value); }
        }

        private String WELL_TEST_DATA_IDValue;
        public String WELL_TEST_DATA_ID
        {
            get { return this.WELL_TEST_DATA_IDValue; }
            set { SetProperty(ref WELL_TEST_DATA_IDValue, value); }
        }

        private Decimal  PERMEABILITYValue;
        public Decimal  PERMEABILITY
        {
            get { return this.PERMEABILITYValue; }
            set { SetProperty(ref PERMEABILITYValue, value); }
        }

        private Decimal  SKIN_FACTORValue;
        public Decimal  SKIN_FACTOR
        {
            get { return this.SKIN_FACTORValue; }
            set { SetProperty(ref SKIN_FACTORValue, value); }
        }

        private Decimal  RESERVOIR_PRESSUREValue;
        public Decimal  RESERVOIR_PRESSURE
        {
            get { return this.RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref RESERVOIR_PRESSUREValue, value); }
        }

        private Decimal  PRODUCTIVITY_INDEXValue;
        public Decimal  PRODUCTIVITY_INDEX
        {
            get { return this.PRODUCTIVITY_INDEXValue; }
            set { SetProperty(ref PRODUCTIVITY_INDEXValue, value); }
        }

        private Decimal  FLOW_EFFICIENCYValue;
        public Decimal  FLOW_EFFICIENCY
        {
            get { return this.FLOW_EFFICIENCYValue; }
            set { SetProperty(ref FLOW_EFFICIENCYValue, value); }
        }

        private Decimal  DAMAGE_RATIOValue;
        public Decimal  DAMAGE_RATIO
        {
            get { return this.DAMAGE_RATIOValue; }
            set { SetProperty(ref DAMAGE_RATIOValue, value); }
        }

        private Decimal  RADIUS_OF_INVESTIGATIONValue;
        public Decimal  RADIUS_OF_INVESTIGATION
        {
            get { return this.RADIUS_OF_INVESTIGATIONValue; }
            set { SetProperty(ref RADIUS_OF_INVESTIGATIONValue, value); }
        }

        private String IDENTIFIED_MODELValue;
        public String IDENTIFIED_MODEL
        {
            get { return this.IDENTIFIED_MODELValue; }
            set { SetProperty(ref IDENTIFIED_MODELValue, value); }
        }

        private Decimal  R_SQUAREDValue;
        public Decimal  R_SQUARED
        {
            get { return this.R_SQUAREDValue; }
            set { SetProperty(ref R_SQUAREDValue, value); }
        }

        private String ANALYSIS_METHODValue;
        public String ANALYSIS_METHOD
        {
            get { return this.ANALYSIS_METHODValue; }
            set { SetProperty(ref ANALYSIS_METHODValue, value); }
        }




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


        private string ANALYSIS_IDValue;
        public string ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private string CALCULATION_IDValue;
        public string CALCULATION_ID
        {
            get { return this.CALCULATION_IDValue; }
            set { SetProperty(ref CALCULATION_IDValue, value); }
        }

        private string? WELL_IDValue;
        public string? WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private string? TEST_IDValue;
        public string? TEST_ID
        {
            get { return this.TEST_IDValue; }
            set { SetProperty(ref TEST_IDValue, value); }
        }

        private string? FIELD_IDValue;
        public string? FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private DateTime ANALYSIS_DATEValue;
        public DateTime ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private DateTime CALCULATION_DATEValue;
        public DateTime CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private string ANALYSIS_BY_USERValue;
        public string ANALYSIS_BY_USER
        {
            get { return this.ANALYSIS_BY_USERValue; }
            set { SetProperty(ref ANALYSIS_BY_USERValue, value); }
        }

        private string IDENTIFIED_MODEL_STRINGValue;
        public string IDENTIFIED_MODEL_STRING
        {
            get { return this.IDENTIFIED_MODEL_STRINGValue; }
            set { SetProperty(ref IDENTIFIED_MODEL_STRINGValue, value); }
        }

        private string ANALYSIS_TYPEValue;
        public string ANALYSIS_TYPE
        {
            get { return this.ANALYSIS_TYPEValue; }
            set { SetProperty(ref ANALYSIS_TYPEValue, value); }
        }

        private double FLOW_RATEValue;
        public double FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private double WELLBORE_RADIUSValue;
        public double WELLBORE_RADIUS
        {
            get { return this.WELLBORE_RADIUSValue; }
            set { SetProperty(ref WELLBORE_RADIUSValue, value); }
        }

        private double FORMATION_THICKNESSValue;
        public double FORMATION_THICKNESS
        {
            get { return this.FORMATION_THICKNESSValue; }
            set { SetProperty(ref FORMATION_THICKNESSValue, value); }
        }

        private double POROSITYValue;
        public double POROSITY
        {
            get { return this.POROSITYValue; }
            set { SetProperty(ref POROSITYValue, value); }
        }

        private double TOTAL_COMPRESSIBILITYValue;
        public double TOTAL_COMPRESSIBILITY
        {
            get { return this.TOTAL_COMPRESSIBILITYValue; }
            set { SetProperty(ref TOTAL_COMPRESSIBILITYValue, value); }
        }

        private double OIL_VISCOSITYValue;
        public double OIL_VISCOSITY
        {
            get { return this.OIL_VISCOSITYValue; }
            set { SetProperty(ref OIL_VISCOSITYValue, value); }
        }

        private double OIL_FORMATION_VOLUME_FACTORValue;
        public double OIL_FORMATION_VOLUME_FACTOR
        {
            get { return this.OIL_FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref OIL_FORMATION_VOLUME_FACTORValue, value); }
        }

        private double PRODUCTION_TIMEValue;
        public double PRODUCTION_TIME
        {
            get { return this.PRODUCTION_TIMEValue; }
            set { SetProperty(ref PRODUCTION_TIMEValue, value); }
        }

        private bool IS_GAS_WELLValue;
        public bool IS_GAS_WELL
        {
            get { return this.IS_GAS_WELLValue; }
            set { SetProperty(ref IS_GAS_WELLValue, value); }
        }

        private double GAS_SPECIFIC_GRAVITYValue;
        public double GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private double RESERVOIR_TEMPERATUREValue;
        public double RESERVOIR_TEMPERATURE
        {
            get { return this.RESERVOIR_TEMPERATUREValue; }
            set { SetProperty(ref RESERVOIR_TEMPERATUREValue, value); }
        }

        private double INITIAL_RESERVOIR_PRESSUREValue;
        public double INITIAL_RESERVOIR_PRESSURE
        {
            get { return this.INITIAL_RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref INITIAL_RESERVOIR_PRESSUREValue, value); }
        }

        private string? DIAGNOSTIC_DATA_JSONValue;
        public string? DIAGNOSTIC_DATA_JSON
        {
            get { return this.DIAGNOSTIC_DATA_JSONValue; }
            set { SetProperty(ref DIAGNOSTIC_DATA_JSONValue, value); }
        }

        private string? DERIVATIVE_DATA_JSONValue;
        public string? DERIVATIVE_DATA_JSON
        {
            get { return this.DERIVATIVE_DATA_JSONValue; }
            set { SetProperty(ref DERIVATIVE_DATA_JSONValue, value); }
        }

        private string? STATUSValue;
        public string? STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private string? ERROR_MESSAGEValue;
        public string? ERROR_MESSAGE
        {
            get { return this.ERROR_MESSAGEValue; }
            set { SetProperty(ref ERROR_MESSAGEValue, value); }
        }

        private string? USER_IDValue;
        public string? USER_ID
        {
            get { return this.USER_IDValue; }
            set { SetProperty(ref USER_IDValue, value); }
        }

        private bool IS_SUCCESSFULValue;
        public bool IS_SUCCESSFUL
        {
            get { return this.IS_SUCCESSFULValue; }
            set { SetProperty(ref IS_SUCCESSFULValue, value); }
        }

        public double RSQUARED { get; set; }
    }
}
