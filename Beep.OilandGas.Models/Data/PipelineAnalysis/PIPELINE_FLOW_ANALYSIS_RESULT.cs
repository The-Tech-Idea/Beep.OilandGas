using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_FLOW_ANALYSIS_RESULT : ModelEntityBase {
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

    }
}
