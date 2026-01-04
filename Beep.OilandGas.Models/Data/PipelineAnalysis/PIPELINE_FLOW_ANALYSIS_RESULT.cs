using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_FLOW_ANALYSIS_RESULT : Entity, IPPDMEntity
    {
        private String PIPELINE_FLOW_ANALYSIS_RESULT_IDValue;
        public String PIPELINE_FLOW_ANALYSIS_RESULT_ID
        {
            get { return this.PIPELINE_FLOW_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref PIPELINE_FLOW_ANALYSIS_RESULT_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal? PRESSURE_DROPValue;
        public Decimal? PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private Decimal? FLOW_VELOCITYValue;
        public Decimal? FLOW_VELOCITY
        {
            get { return this.FLOW_VELOCITYValue; }
            set { SetProperty(ref FLOW_VELOCITYValue, value); }
        }

        private Decimal? REYNOLDS_NUMBERValue;
        public Decimal? REYNOLDS_NUMBER
        {
            get { return this.REYNOLDS_NUMBERValue; }
            set { SetProperty(ref REYNOLDS_NUMBERValue, value); }
        }

        private Decimal? FRICTION_FACTORValue;
        public Decimal? FRICTION_FACTOR
        {
            get { return this.FRICTION_FACTORValue; }
            set { SetProperty(ref FRICTION_FACTORValue, value); }
        }

        private Decimal? PRESSURE_GRADIENTValue;
        public Decimal? PRESSURE_GRADIENT
        {
            get { return this.PRESSURE_GRADIENTValue; }
            set { SetProperty(ref PRESSURE_GRADIENTValue, value); }
        }

        private Decimal? OUTLET_PRESSUREValue;
        public Decimal? OUTLET_PRESSURE
        {
            get { return this.OUTLET_PRESSUREValue; }
            set { SetProperty(ref OUTLET_PRESSUREValue, value); }
        }

        private String FLOW_REGIMEValue;
        public String FLOW_REGIME
        {
            get { return this.FLOW_REGIMEValue; }
            set { SetProperty(ref FLOW_REGIMEValue, value); }
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
