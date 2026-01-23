using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class OIL_COMPOSITION : ModelEntityBase
    {
        private string OIL_COMPOSITION_IDValue;
        public string OIL_COMPOSITION_ID
        {
            get { return this.OIL_COMPOSITION_IDValue; }
            set { SetProperty(ref OIL_COMPOSITION_IDValue, value); }
        }

        private string COMPOSITION_NAMEValue;
        public string COMPOSITION_NAME
        {
            get { return this.COMPOSITION_NAMEValue; }
            set { SetProperty(ref COMPOSITION_NAMEValue, value); }
        }

        private DateTime? COMPOSITION_DATEValue;
        public DateTime? COMPOSITION_DATE
        {
            get { return this.COMPOSITION_DATEValue; }
            set { SetProperty(ref COMPOSITION_DATEValue, value); }
        }

        private decimal? OIL_GRAVITYValue;
        public decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private decimal? GAS_OIL_RATIOValue;
        public decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private decimal? WATER_CUTValue;
        public decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private decimal? BUBBLE_POINT_PRESSUREValue;
        public decimal? BUBBLE_POINT_PRESSURE
        {
            get { return this.BUBBLE_POINT_PRESSUREValue; }
            set { SetProperty(ref BUBBLE_POINT_PRESSUREValue, value); }
        }

        // Standard PPDM columns

    }
}


