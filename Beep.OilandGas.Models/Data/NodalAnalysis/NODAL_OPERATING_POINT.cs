using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_OPERATING_POINT : ModelEntityBase {
        private String NODAL_OPERATING_POINT_IDValue;
        public String NODAL_OPERATING_POINT_ID
        {
            get { return this.NODAL_OPERATING_POINT_IDValue; }
            set { SetProperty(ref NODAL_OPERATING_POINT_IDValue, value); }
        }

        private String NODAL_ANALYSIS_RESULT_IDValue;
        public String NODAL_ANALYSIS_RESULT_ID
        {
            get { return this.NODAL_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_RESULT_IDValue, value); }
        }

        private Decimal  FLOW_RATEValue;
        public Decimal  FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal  BOTTOMHOLE_PRESSUREValue;
        public Decimal  BOTTOMHOLE_PRESSURE
        {
            get { return this.BOTTOMHOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOMHOLE_PRESSUREValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

    }
}
