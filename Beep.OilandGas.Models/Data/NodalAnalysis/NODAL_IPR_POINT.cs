using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_IPR_POINT : ModelEntityBase {
        private String NODAL_IPR_POINT_IDValue;
        public String NODAL_IPR_POINT_ID
        {
            get { return this.NODAL_IPR_POINT_IDValue; }
            set { SetProperty(ref NODAL_IPR_POINT_IDValue, value); }
        }

        private String NODAL_ANALYSIS_IDValue;
        public String NODAL_ANALYSIS_ID
        {
            get { return this.NODAL_ANALYSIS_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_IDValue, value); }
        }

        private Decimal  FLOW_RATEValue;
        public Decimal  FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal  FLOWING_BOTTOMHOLE_PRESSUREValue;
        public Decimal  FLOWING_BOTTOMHOLE_PRESSURE
        {
            get { return this.FLOWING_BOTTOMHOLE_PRESSUREValue; }
            set { SetProperty(ref FLOWING_BOTTOMHOLE_PRESSUREValue, value); }
        }

        // Standard PPDM columns

    }
}
