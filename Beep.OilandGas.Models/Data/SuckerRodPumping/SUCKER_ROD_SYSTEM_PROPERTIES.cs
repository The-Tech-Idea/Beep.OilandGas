using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class SUCKER_ROD_SYSTEM_PROPERTIES : ModelEntityBase {
        private String SUCKER_ROD_SYSTEM_PROPERTIES_IDValue;
        public String SUCKER_ROD_SYSTEM_PROPERTIES_ID
        {
            get { return this.SUCKER_ROD_SYSTEM_PROPERTIES_IDValue; }
            set { SetProperty(ref SUCKER_ROD_SYSTEM_PROPERTIES_IDValue, value); }
        }

        private Decimal? WELL_DEPTHValue;
        public Decimal? WELL_DEPTH
        {
            get { return this.WELL_DEPTHValue; }
            set { SetProperty(ref WELL_DEPTHValue, value); }
        }

        private Decimal? TUBING_DIAMETERValue;
        public Decimal? TUBING_DIAMETER
        {
            get { return this.TUBING_DIAMETERValue; }
            set { SetProperty(ref TUBING_DIAMETERValue, value); }
        }

        private Decimal? ROD_DIAMETERValue;
        public Decimal? ROD_DIAMETER
        {
            get { return this.ROD_DIAMETERValue; }
            set { SetProperty(ref ROD_DIAMETERValue, value); }
        }

        private Decimal? PUMP_DIAMETERValue;
        public Decimal? PUMP_DIAMETER
        {
            get { return this.PUMP_DIAMETERValue; }
            set { SetProperty(ref PUMP_DIAMETERValue, value); }
        }

        private Decimal? STROKE_LENGTHValue;
        public Decimal? STROKE_LENGTH
        {
            get { return this.STROKE_LENGTHValue; }
            set { SetProperty(ref STROKE_LENGTHValue, value); }
        }

        private Decimal? STROKES_PER_MINUTEValue;
        public Decimal? STROKES_PER_MINUTE
        {
            get { return this.STROKES_PER_MINUTEValue; }
            set { SetProperty(ref STROKES_PER_MINUTEValue, value); }
        }

        private Decimal? WELLHEAD_PRESSUREValue;
        public Decimal? WELLHEAD_PRESSURE
        {
            get { return this.WELLHEAD_PRESSUREValue; }
            set { SetProperty(ref WELLHEAD_PRESSUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Decimal? OIL_GRAVITYValue;
        public Decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal? WATER_CUTValue;
        public Decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal? GAS_OIL_RATIOValue;
        public Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? ROD_DENSITYValue;
        public Decimal? ROD_DENSITY
        {
            get { return this.ROD_DENSITYValue; }
            set { SetProperty(ref ROD_DENSITYValue, value); }
        }

        private Decimal? PUMP_EFFICIENCYValue;
        public Decimal? PUMP_EFFICIENCY
        {
            get { return this.PUMP_EFFICIENCYValue; }
            set { SetProperty(ref PUMP_EFFICIENCYValue, value); }
        }

        private Decimal? FLUID_LEVELValue;
        public Decimal? FLUID_LEVEL
        {
            get { return this.FLUID_LEVELValue; }
            set { SetProperty(ref FLUID_LEVELValue, value); }
        }

        private Decimal? FLUID_DENSITYValue;
        public Decimal? FLUID_DENSITY
        {
            get { return this.FLUID_DENSITYValue; }
            set { SetProperty(ref FLUID_DENSITYValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
    }
}



