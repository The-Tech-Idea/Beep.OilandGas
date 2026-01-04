using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class LIQUID_PIPELINE_FLOW_PROPERTIES : Entity, IPPDMEntity
    {
        private String LIQUID_PIPELINE_FLOW_PROPERTIES_IDValue;
        public String LIQUID_PIPELINE_FLOW_PROPERTIES_ID
        {
            get { return this.LIQUID_PIPELINE_FLOW_PROPERTIES_IDValue; }
            set { SetProperty(ref LIQUID_PIPELINE_FLOW_PROPERTIES_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private Decimal? LIQUID_FLOW_RATEValue;
        public Decimal? LIQUID_FLOW_RATE
        {
            get { return this.LIQUID_FLOW_RATEValue; }
            set { SetProperty(ref LIQUID_FLOW_RATEValue, value); }
        }

        private Decimal? LIQUID_SPECIFIC_GRAVITYValue;
        public Decimal? LIQUID_SPECIFIC_GRAVITY
        {
            get { return this.LIQUID_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref LIQUID_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? LIQUID_VISCOSITYValue;
        public Decimal? LIQUID_VISCOSITY
        {
            get { return this.LIQUID_VISCOSITYValue; }
            set { SetProperty(ref LIQUID_VISCOSITYValue, value); }
        }

        private Decimal? LIQUID_DENSITYValue;
        public Decimal? LIQUID_DENSITY
        {
            get { return this.LIQUID_DENSITYValue; }
            set { SetProperty(ref LIQUID_DENSITYValue, value); }
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
    }
}
