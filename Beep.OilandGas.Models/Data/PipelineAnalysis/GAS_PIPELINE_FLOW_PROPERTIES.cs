using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class GAS_PIPELINE_FLOW_PROPERTIES : ModelEntityBase {
        private String GAS_PIPELINE_FLOW_PROPERTIES_IDValue;
        public String GAS_PIPELINE_FLOW_PROPERTIES_ID
        {
            get { return this.GAS_PIPELINE_FLOW_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_PIPELINE_FLOW_PROPERTIES_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private  decimal  GAS_FLOW_RATEValue;
        public  decimal  GAS_FLOW_RATE
        {
            get { return this.GAS_FLOW_RATEValue; }
            set { SetProperty(ref GAS_FLOW_RATEValue, value); }
        }

        private  decimal  GAS_SPECIFIC_GRAVITYValue;
        public  decimal  GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private  decimal  GAS_MOLECULAR_WEIGHTValue;
        public  decimal  GAS_MOLECULAR_WEIGHT
        {
            get { return this.GAS_MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref GAS_MOLECULAR_WEIGHTValue, value); }
        }

        private  decimal  BASE_PRESSUREValue;
        public  decimal  BASE_PRESSURE
        {
            get { return this.BASE_PRESSUREValue; }
            set { SetProperty(ref BASE_PRESSUREValue, value); }
        }

        private  decimal  BASE_TEMPERATUREValue;
        public  decimal  BASE_TEMPERATURE
        {
            get { return this.BASE_TEMPERATUREValue; }
            set { SetProperty(ref BASE_TEMPERATUREValue, value); }
        }

        private  decimal  Z_FACTORValue;
        public  decimal  Z_FACTOR
        {
            get { return this.Z_FACTORValue; }
            set { SetProperty(ref Z_FACTORValue, value); }
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
