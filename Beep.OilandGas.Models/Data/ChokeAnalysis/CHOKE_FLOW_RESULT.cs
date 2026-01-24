using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public partial class CHOKE_FLOW_RESULT : ModelEntityBase {
        private String CHOKE_FLOW_RESULT_IDValue;
        public String CHOKE_FLOW_RESULT_ID
        {
            get { return this.CHOKE_FLOW_RESULT_IDValue; }
            set { SetProperty(ref CHOKE_FLOW_RESULT_IDValue, value); }
        }

        private String CHOKE_PROPERTIES_IDValue;
        public String CHOKE_PROPERTIES_ID
        {
            get { return this.CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref CHOKE_PROPERTIES_IDValue, value); }
        }

        private String GAS_CHOKE_PROPERTIES_IDValue;
        public String GAS_CHOKE_PROPERTIES_ID
        {
            get { return this.GAS_CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_CHOKE_PROPERTIES_IDValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal? DOWNSTREAM_PRESSUREValue;
        public Decimal? DOWNSTREAM_PRESSURE
        {
            get { return this.DOWNSTREAM_PRESSUREValue; }
            set { SetProperty(ref DOWNSTREAM_PRESSUREValue, value); }
        }

        private Decimal? UPSTREAM_PRESSUREValue;
        public Decimal? UPSTREAM_PRESSURE
        {
            get { return this.UPSTREAM_PRESSUREValue; }
            set { SetProperty(ref UPSTREAM_PRESSUREValue, value); }
        }

        private Decimal? PRESSURE_RATIOValue;
        public Decimal? PRESSURE_RATIO
        {
            get { return this.PRESSURE_RATIOValue; }
            set { SetProperty(ref PRESSURE_RATIOValue, value); }
        }

        private FlowRegime FLOW_REGIMEValue;
        public FlowRegime FLOW_REGIME
        {
            get { return this.FLOW_REGIMEValue; }
            set { SetProperty(ref FLOW_REGIMEValue, value); }
        }

        private Decimal? CRITICAL_PRESSURE_RATIOValue;
        public Decimal? CRITICAL_PRESSURE_RATIO
        {
            get { return this.CRITICAL_PRESSURE_RATIOValue; }
            set { SetProperty(ref CRITICAL_PRESSURE_RATIOValue, value); }
        }

        // Standard PPDM columns

    }
}


