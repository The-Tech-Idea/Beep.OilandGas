using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class COMPLETION_STRING : ModelEntityBase
    {
        private String COMPLETION_IDValue;
        public String COMPLETION_ID
        {
            get { return this.COMPLETION_IDValue; }
            set { SetProperty(ref COMPLETION_IDValue, value); }
        }

        private String WELL_CONSTRUCTION_IDValue;
        public String WELL_CONSTRUCTION_ID
        {
            get { return this.WELL_CONSTRUCTION_IDValue; }
            set { SetProperty(ref WELL_CONSTRUCTION_IDValue, value); }
        }

        private String COMPLETION_TYPEValue;
        public String COMPLETION_TYPE
        {
            get { return this.COMPLETION_TYPEValue; }
            set { SetProperty(ref COMPLETION_TYPEValue, value); }
        }

        private Decimal? TOP_DEPTHValue;
        public Decimal? TOP_DEPTH
        {
            get { return this.TOP_DEPTHValue; }
            set { SetProperty(ref TOP_DEPTHValue, value); }
        }

        private Decimal? BOTTOM_DEPTHValue;
        public Decimal? BOTTOM_DEPTH
        {
            get { return this.BOTTOM_DEPTHValue; }
            set { SetProperty(ref BOTTOM_DEPTHValue, value); }
        }

        private Decimal? DIAMETERValue;
        public Decimal? DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private String DIAMETER_UNITValue;
        public String DIAMETER_UNIT
        {
            get { return this.DIAMETER_UNITValue; }
            set { SetProperty(ref DIAMETER_UNITValue, value); }
        }

        // Standard PPDM columns

        public COMPLETION_STRING() { }
    }
}
