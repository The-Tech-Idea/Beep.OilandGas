using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_CAPACITY_RESULT : ModelEntityBase {
        private String PIPELINE_CAPACITY_RESULT_IDValue;
        public String PIPELINE_CAPACITY_RESULT_ID
        {
            get { return this.PIPELINE_CAPACITY_RESULT_IDValue; }
            set { SetProperty(ref PIPELINE_CAPACITY_RESULT_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private  decimal  MAXIMUM_FLOW_RATEValue;
        public  decimal  MAXIMUM_FLOW_RATE
        {
            get { return this.MAXIMUM_FLOW_RATEValue; }
            set { SetProperty(ref MAXIMUM_FLOW_RATEValue, value); }
        }

        private  decimal  PRESSURE_DROPValue;
        public  decimal  PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private  decimal  FLOW_VELOCITYValue;
        public  decimal  FLOW_VELOCITY
        {
            get { return this.FLOW_VELOCITYValue; }
            set { SetProperty(ref FLOW_VELOCITYValue, value); }
        }

        private  decimal  REYNOLDS_NUMBERValue;
        public  decimal  REYNOLDS_NUMBER
        {
            get { return this.REYNOLDS_NUMBERValue; }
            set { SetProperty(ref REYNOLDS_NUMBERValue, value); }
        }

        private  decimal  FRICTION_FACTORValue;
        public  decimal  FRICTION_FACTOR
        {
            get { return this.FRICTION_FACTORValue; }
            set { SetProperty(ref FRICTION_FACTORValue, value); }
        }

        private  decimal  PRESSURE_GRADIENTValue;
        public  decimal  PRESSURE_GRADIENT
        {
            get { return this.PRESSURE_GRADIENTValue; }
            set { SetProperty(ref PRESSURE_GRADIENTValue, value); }
        }

        private  decimal  OUTLET_PRESSUREValue;
        public  decimal  OUTLET_PRESSURE
        {
            get { return this.OUTLET_PRESSUREValue; }
            set { SetProperty(ref OUTLET_PRESSUREValue, value); }
        }

        // Standard PPDM columns

    }
}
