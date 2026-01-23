using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public partial class GAS_CHOKE_PROPERTIES : ModelEntityBase {
        private String GAS_CHOKE_PROPERTIES_IDValue;
        public String GAS_CHOKE_PROPERTIES_ID
        {
            get { return this.GAS_CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_CHOKE_PROPERTIES_IDValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? UPSTREAM_PRESSUREValue;
        public Decimal? UPSTREAM_PRESSURE
        {
            get { return this.UPSTREAM_PRESSUREValue; }
            set { SetProperty(ref UPSTREAM_PRESSUREValue, value); }
        }

        private Decimal? DOWNSTREAM_PRESSUREValue;
        public Decimal? DOWNSTREAM_PRESSURE
        {
            get { return this.DOWNSTREAM_PRESSUREValue; }
            set { SetProperty(ref DOWNSTREAM_PRESSUREValue, value); }
        }

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private Decimal? Z_FACTORValue;
        public Decimal? Z_FACTOR
        {
            get { return this.Z_FACTORValue; }
            set { SetProperty(ref Z_FACTORValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        // Standard PPDM columns

    }
}


