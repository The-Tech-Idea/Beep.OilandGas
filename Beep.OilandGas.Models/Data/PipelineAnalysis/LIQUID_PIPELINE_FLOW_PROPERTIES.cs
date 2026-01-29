using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class LIQUID_PIPELINE_FLOW_PROPERTIES : ModelEntityBase {
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

        private Decimal  LIQUID_FLOW_RATEValue;
        public Decimal  LIQUID_FLOW_RATE
        {
            get { return this.LIQUID_FLOW_RATEValue; }
            set { SetProperty(ref LIQUID_FLOW_RATEValue, value); }
        }

        private Decimal  LIQUID_SPECIFIC_GRAVITYValue;
        public Decimal  LIQUID_SPECIFIC_GRAVITY
        {
            get { return this.LIQUID_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref LIQUID_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal  LIQUID_VISCOSITYValue;
        public Decimal  LIQUID_VISCOSITY
        {
            get { return this.LIQUID_VISCOSITYValue; }
            set { SetProperty(ref LIQUID_VISCOSITYValue, value); }
        }

        private Decimal  LIQUID_DENSITYValue;
        public Decimal  LIQUID_DENSITY
        {
            get { return this.LIQUID_DENSITYValue; }
            set { SetProperty(ref LIQUID_DENSITYValue, value); }
        }

        private PIPELINE_PROPERTIES pIPELINE_PROPERTIESValue;
        public PIPELINE_PROPERTIES PIPELINE_PROPERTIES
        {
            get { return this.pIPELINE_PROPERTIESValue; }
            set { SetProperty(ref pIPELINE_PROPERTIESValue, value); }
        }
        // Standard PPDM columns


        private PIPELINE_PROPERTIES PIPELINEValue;
        public PIPELINE_PROPERTIES PIPELINE
        {
            get { return this.PIPELINEValue; }
            set { SetProperty(ref PIPELINEValue, value); }
        }
    }
}
