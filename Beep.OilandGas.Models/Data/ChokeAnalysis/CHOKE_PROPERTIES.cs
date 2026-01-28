using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public partial class CHOKE_PROPERTIES : ModelEntityBase {
        private String CHOKE_PROPERTIES_IDValue;
        public String CHOKE_PROPERTIES_ID
        {
            get { return this.CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref CHOKE_PROPERTIES_IDValue, value); }
        }

        private Decimal? CHOKE_DIAMETERValue;
        public Decimal? CHOKE_DIAMETER
        {
            get { return this.CHOKE_DIAMETERValue; }
            set { SetProperty(ref CHOKE_DIAMETERValue, value); }
        }

        private String CHOKE_TYPEValue;
        public String CHOKE_TYPE
        {
            get { return this.CHOKE_TYPEValue; }
            set { SetProperty(ref CHOKE_TYPEValue, value); }
        }

        private Decimal? DISCHARGE_COEFFICIENTValue;
        public Decimal? DISCHARGE_COEFFICIENT
        {
            get { return this.DISCHARGE_COEFFICIENTValue; }
            set { SetProperty(ref DISCHARGE_COEFFICIENTValue, value); }
        }

        // Standard PPDM columns

    }
}
