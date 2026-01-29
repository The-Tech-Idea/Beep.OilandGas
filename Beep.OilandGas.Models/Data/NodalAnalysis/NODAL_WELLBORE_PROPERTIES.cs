using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_WELLBORE_PROPERTIES : ModelEntityBase {
        private String NODAL_WELLBORE_PROPERTIES_IDValue;
        public String NODAL_WELLBORE_PROPERTIES_ID
        {
            get { return this.NODAL_WELLBORE_PROPERTIES_IDValue; }
            set { SetProperty(ref NODAL_WELLBORE_PROPERTIES_IDValue, value); }
        }

        private String NODAL_ANALYSIS_RESULT_IDValue;
        public String NODAL_ANALYSIS_RESULT_ID
        {
            get { return this.NODAL_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_RESULT_IDValue, value); }
        }

        private Decimal  TUBING_DIAMETERValue;
        public Decimal  TUBING_DIAMETER
        {
            get { return this.TUBING_DIAMETERValue; }
            set { SetProperty(ref TUBING_DIAMETERValue, value); }
        }

        private Decimal  TUBING_LENGTHValue;
        public Decimal  TUBING_LENGTH
        {
            get { return this.TUBING_LENGTHValue; }
            set { SetProperty(ref TUBING_LENGTHValue, value); }
        }

        private Decimal  WELLHEAD_PRESSUREValue;
        public Decimal  WELLHEAD_PRESSURE
        {
            get { return this.WELLHEAD_PRESSUREValue; }
            set { SetProperty(ref WELLHEAD_PRESSUREValue, value); }
        }

        private Decimal  WATER_CUTValue;
        public Decimal  WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal  GAS_OIL_RATIOValue;
        public Decimal  GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal  OIL_GRAVITYValue;
        public Decimal  OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal  GAS_SPECIFIC_GRAVITYValue;
        public Decimal  GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal  WELLHEAD_TEMPERATUREValue;
        public Decimal  WELLHEAD_TEMPERATURE
        {
            get { return this.WELLHEAD_TEMPERATUREValue; }
            set { SetProperty(ref WELLHEAD_TEMPERATUREValue, value); }
        }

        private Decimal  BOTTOMHOLE_TEMPERATUREValue;
        public Decimal  BOTTOMHOLE_TEMPERATURE
        {
            get { return this.BOTTOMHOLE_TEMPERATUREValue; }
            set { SetProperty(ref BOTTOMHOLE_TEMPERATUREValue, value); }
        }

        private Decimal  TUBING_ROUGHNESSValue;
        public Decimal  TUBING_ROUGHNESS
        {
            get { return this.TUBING_ROUGHNESSValue; }
            set { SetProperty(ref TUBING_ROUGHNESSValue, value); }
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
