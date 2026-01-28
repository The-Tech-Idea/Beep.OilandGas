using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class GAS_COMPOSITION_COMPONENT : ModelEntityBase
    {
        private System.String GAS_COMPONENT_IDValue;
        public System.String GAS_COMPONENT_ID
        {
            get { return this.GAS_COMPONENT_IDValue; }
            set { SetProperty(ref GAS_COMPONENT_IDValue, value); }
        }

        private System.String GAS_COMPOSITION_IDValue;
        public System.String GAS_COMPOSITION_ID
        {
            get { return this.GAS_COMPOSITION_IDValue; }
            set { SetProperty(ref GAS_COMPOSITION_IDValue, value); }
        }

        private System.String COMPONENT_NAMEValue;
        public System.String COMPONENT_NAME
        {
            get { return this.COMPONENT_NAMEValue; }
            set { SetProperty(ref COMPONENT_NAMEValue, value); }
        }

        private System.Decimal? MOLE_FRACTIONValue;
        public System.Decimal? MOLE_FRACTION
        {
            get { return this.MOLE_FRACTIONValue; }
            set { SetProperty(ref MOLE_FRACTIONValue, value); }
        }

        private System.Decimal? MOLECULAR_WEIGHTValue;
        public System.Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
        }

        public DateTime COMPOSITION_DATE { get; set; }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
