using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_PROPERTIES : ModelEntityBase {
        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private Decimal? DIAMETERValue;
        public Decimal? DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private Decimal? LENGTHValue;
        public Decimal? LENGTH
        {
            get { return this.LENGTHValue; }
            set { SetProperty(ref LENGTHValue, value); }
        }

        private Decimal? ROUGHNESSValue;
        public Decimal? ROUGHNESS
        {
            get { return this.ROUGHNESSValue; }
            set { SetProperty(ref ROUGHNESSValue, value); }
        }

        private Decimal? ELEVATION_CHANGEValue;
        public Decimal? ELEVATION_CHANGE
        {
            get { return this.ELEVATION_CHANGEValue; }
            set { SetProperty(ref ELEVATION_CHANGEValue, value); }
        }

        private Decimal? INLET_PRESSUREValue;
        public Decimal? INLET_PRESSURE
        {
            get { return this.INLET_PRESSUREValue; }
            set { SetProperty(ref INLET_PRESSUREValue, value); }
        }

        private Decimal? OUTLET_PRESSUREValue;
        public Decimal? OUTLET_PRESSURE
        {
            get { return this.OUTLET_PRESSUREValue; }
            set { SetProperty(ref OUTLET_PRESSUREValue, value); }
        }

        private Decimal? AVERAGE_TEMPERATUREValue;
        public Decimal? AVERAGE_TEMPERATURE
        {
            get { return this.AVERAGE_TEMPERATUREValue; }
            set { SetProperty(ref AVERAGE_TEMPERATUREValue, value); }
        }

        // Standard PPDM columns

    }
}


