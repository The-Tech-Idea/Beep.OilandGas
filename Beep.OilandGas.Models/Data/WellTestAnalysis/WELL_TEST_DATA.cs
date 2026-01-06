using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class WELL_TEST_DATA : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private String WELL_TEST_DATA_IDValue;
        public String WELL_TEST_DATA_ID
        {
            get { return this.WELL_TEST_DATA_IDValue; }
            set { SetProperty(ref WELL_TEST_DATA_IDValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
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

        private Decimal? FORMATION_THICKNESSValue;
        public Decimal? FORMATION_THICKNESS
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

        private Decimal? RESERVOIR_TEMPERATUREValue;
        public Decimal? RESERVOIR_TEMPERATURE
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
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
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

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}



