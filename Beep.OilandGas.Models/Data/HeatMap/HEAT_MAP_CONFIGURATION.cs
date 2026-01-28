using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public partial class HEAT_MAP_CONFIGURATION : ModelEntityBase {
        private string HEAT_MAP_IDValue;
        public string HEAT_MAP_ID
        {
            get { return this.HEAT_MAP_IDValue; }
            set { SetProperty(ref HEAT_MAP_IDValue, value); }
        }

        private string CONFIGURATION_NAMEValue;
        public string CONFIGURATION_NAME
        {
            get { return this.CONFIGURATION_NAMEValue; }
            set { SetProperty(ref CONFIGURATION_NAMEValue, value); }
        }

        // Standard PPDM columns

        private string COLOR_SCHEME_TYPEValue;
        public string COLOR_SCHEME_TYPE
        {
            get { return this.COLOR_SCHEME_TYPEValue; }
            set { SetProperty(ref COLOR_SCHEME_TYPEValue, value); }
        }

        private int COLOR_STEPSValue;
        public int COLOR_STEPS
        {
            get { return this.COLOR_STEPSValue; }
            set { SetProperty(ref COLOR_STEPSValue, value); }
        }

        private bool SHOW_LEGENDValue;
        public bool SHOW_LEGEND
        {
            get { return this.SHOW_LEGENDValue; }
            set { SetProperty(ref SHOW_LEGENDValue, value); }
        }

        private bool USE_INTERPOLATIONValue;
        public bool USE_INTERPOLATION
        {
            get { return this.USE_INTERPOLATIONValue; }
            set { SetProperty(ref USE_INTERPOLATIONValue, value); }
        }

        private string INTERPOLATION_METHODValue;
        public string INTERPOLATION_METHOD
        {
            get { return this.INTERPOLATION_METHODValue; }
            set { SetProperty(ref INTERPOLATION_METHODValue, value); }
        }

        private double INTERPOLATION_CELL_SIZEValue;
        public double INTERPOLATION_CELL_SIZE
        {
            get { return this.INTERPOLATION_CELL_SIZEValue; }
            set { SetProperty(ref INTERPOLATION_CELL_SIZEValue, value); }
        }
    }
}
