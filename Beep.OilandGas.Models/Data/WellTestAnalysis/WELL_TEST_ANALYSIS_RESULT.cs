using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class WELL_TEST_ANALYSIS_RESULT : Entity, Core.Interfaces.IPPDMEntity
    {
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

        private Decimal? PERMEABILITYValue;
        public Decimal? PERMEABILITY
        {
            get { return this.PERMEABILITYValue; }
            set { SetProperty(ref PERMEABILITYValue, value); }
        }

        private Decimal? SKIN_FACTORValue;
        public Decimal? SKIN_FACTOR
        {
            get { return this.SKIN_FACTORValue; }
            set { SetProperty(ref SKIN_FACTORValue, value); }
        }

        private Decimal? RESERVOIR_PRESSUREValue;
        public Decimal? RESERVOIR_PRESSURE
        {
            get { return this.RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref RESERVOIR_PRESSUREValue, value); }
        }

        private Decimal? PRODUCTIVITY_INDEXValue;
        public Decimal? PRODUCTIVITY_INDEX
        {
            get { return this.PRODUCTIVITY_INDEXValue; }
            set { SetProperty(ref PRODUCTIVITY_INDEXValue, value); }
        }

        private Decimal? FLOW_EFFICIENCYValue;
        public Decimal? FLOW_EFFICIENCY
        {
            get { return this.FLOW_EFFICIENCYValue; }
            set { SetProperty(ref FLOW_EFFICIENCYValue, value); }
        }

        private Decimal? DAMAGE_RATIOValue;
        public Decimal? DAMAGE_RATIO
        {
            get { return this.DAMAGE_RATIOValue; }
            set { SetProperty(ref DAMAGE_RATIOValue, value); }
        }

        private Decimal? RADIUS_OF_INVESTIGATIONValue;
        public Decimal? RADIUS_OF_INVESTIGATION
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

        private Decimal? R_SQUAREDValue;
        public Decimal? R_SQUARED
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
