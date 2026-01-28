using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class GAS_COMPOSITION : ModelEntityBase
    {
        private System.String GAS_COMPOSITION_IDValue;
        public System.String GAS_COMPOSITION_ID
        {
            get { return this.GAS_COMPOSITION_IDValue; }
            set { SetProperty(ref GAS_COMPOSITION_IDValue, value); }
        }

        private System.String COMPOSITION_NAMEValue;
        public System.String COMPOSITION_NAME
        {
            get { return this.COMPOSITION_NAMEValue; }
            set { SetProperty(ref COMPOSITION_NAMEValue, value); }
        }

        private System.DateTime? COMPOSITION_DATEValue;
        public System.DateTime? COMPOSITION_DATE
        {
            get { return this.COMPOSITION_DATEValue; }
            set { SetProperty(ref COMPOSITION_DATEValue, value); }
        }

        private System.Decimal? TOTAL_MOLE_FRACTIONValue;
        public System.Decimal? TOTAL_MOLE_FRACTION
        {
            get { return this.TOTAL_MOLE_FRACTIONValue; }
            set { SetProperty(ref TOTAL_MOLE_FRACTIONValue, value); }
        }

        private System.Decimal? MOLECULAR_WEIGHTValue;
        public System.Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
        }

        private System.Decimal? SPECIFIC_GRAVITYValue;
        public System.Decimal? SPECIFIC_GRAVITY
        {
            get { return this.SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref SPECIFIC_GRAVITYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
