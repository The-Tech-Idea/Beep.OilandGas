using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_RESERVOIR_PROPERTIES : Entity, Core.Interfaces.IPPDMEntity
    {
        private String NODAL_RESERVOIR_PROPERTIES_IDValue;
        public String NODAL_RESERVOIR_PROPERTIES_ID
        {
            get { return this.NODAL_RESERVOIR_PROPERTIES_IDValue; }
            set { SetProperty(ref NODAL_RESERVOIR_PROPERTIES_IDValue, value); }
        }

        private String NODAL_ANALYSIS_RESULT_IDValue;
        public String NODAL_ANALYSIS_RESULT_ID
        {
            get { return this.NODAL_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_RESULT_IDValue, value); }
        }

        private Decimal? RESERVOIR_PRESSUREValue;
        public Decimal? RESERVOIR_PRESSURE
        {
            get { return this.RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref RESERVOIR_PRESSUREValue, value); }
        }

        private Decimal? BUBBLE_POINT_PRESSUREValue;
        public Decimal? BUBBLE_POINT_PRESSURE
        {
            get { return this.BUBBLE_POINT_PRESSUREValue; }
            set { SetProperty(ref BUBBLE_POINT_PRESSUREValue, value); }
        }

        private Decimal? PRODUCTIVITY_INDEXValue;
        public Decimal? PRODUCTIVITY_INDEX
        {
            get { return this.PRODUCTIVITY_INDEXValue; }
            set { SetProperty(ref PRODUCTIVITY_INDEXValue, value); }
        }

        private Decimal? WATER_CUTValue;
        public Decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal? GAS_OIL_RATIOValue;
        public Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal? OIL_GRAVITYValue;
        public Decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal? FORMATION_VOLUME_FACTORValue;
        public Decimal? FORMATION_VOLUME_FACTOR
        {
            get { return this.FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref FORMATION_VOLUME_FACTORValue, value); }
        }

        private Decimal? OIL_VISCOSITYValue;
        public Decimal? OIL_VISCOSITY
        {
            get { return this.OIL_VISCOSITYValue; }
            set { SetProperty(ref OIL_VISCOSITYValue, value); }
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
