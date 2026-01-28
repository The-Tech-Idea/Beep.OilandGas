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
    }
}
