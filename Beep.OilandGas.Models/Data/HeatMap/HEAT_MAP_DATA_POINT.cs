using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.HeatMap
{
    public partial class HEAT_MAP_DATA_POINT : ModelEntityBase {
        private String HEAT_MAP_DATA_POINT_IDValue;
        public String HEAT_MAP_DATA_POINT_ID
        {
            get { return this.HEAT_MAP_DATA_POINT_IDValue; }
            set { SetProperty(ref HEAT_MAP_DATA_POINT_IDValue, value); }
        }

        private String HEAT_MAP_IDValue;
        public String HEAT_MAP_ID
        {
            get { return this.HEAT_MAP_IDValue; }
            set { SetProperty(ref HEAT_MAP_IDValue, value); }
        }

        private Decimal? XValue;
        public Decimal? X
        {
            get { return this.XValue; }
            set { SetProperty(ref XValue, value); }
        }

        private Decimal? YValue;
        public Decimal? Y
        {
            get { return this.YValue; }
            set { SetProperty(ref YValue, value); }
        }

        private Decimal? ORIGINAL_XValue;
        public Decimal? ORIGINAL_X
        {
            get { return this.ORIGINAL_XValue; }
            set { SetProperty(ref ORIGINAL_XValue, value); }
        }

        private Decimal? ORIGINAL_YValue;
        public Decimal? ORIGINAL_Y
        {
            get { return this.ORIGINAL_YValue; }
            set { SetProperty(ref ORIGINAL_YValue, value); }
        }

        private Decimal? VALUEValue;
        public Decimal? VALUE
        {
            get { return this.VALUEValue; }
            set { SetProperty(ref VALUEValue, value); }
        }

        private String LABELValue;
        public String LABEL
        {
            get { return this.LABELValue; }
            set { SetProperty(ref LABELValue, value); }
        }

        // Standard PPDM columns

    }
}


