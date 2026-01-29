using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class PERFORATION : ModelEntityBase
    {
        private String PERFORATION_IDValue;
        public String PERFORATION_ID
        {
            get { return this.PERFORATION_IDValue; }
            set { SetProperty(ref PERFORATION_IDValue, value); }
        }

        private String COMPLETION_IDValue;
        public String COMPLETION_ID
        {
            get { return this.COMPLETION_IDValue; }
            set { SetProperty(ref COMPLETION_IDValue, value); }
        }

        private Decimal  TOP_DEPTHValue;
        public Decimal  TOP_DEPTH
        {
            get { return this.TOP_DEPTHValue; }
            set { SetProperty(ref TOP_DEPTHValue, value); }
        }

        private Decimal  BOTTOM_DEPTHValue;
        public Decimal  BOTTOM_DEPTH
        {
            get { return this.BOTTOM_DEPTHValue; }
            set { SetProperty(ref BOTTOM_DEPTHValue, value); }
        }

        private Int32? SHOTS_PER_FOOTValue;
        public Int32? SHOTS_PER_FOOT
        {
            get { return this.SHOTS_PER_FOOTValue; }
            set { SetProperty(ref SHOTS_PER_FOOTValue, value); }
        }

        private String PERFORATION_TYPEValue;
        public String PERFORATION_TYPE
        {
            get { return this.PERFORATION_TYPEValue; }
            set { SetProperty(ref PERFORATION_TYPEValue, value); }
        }

        private DateTime? PERFORATION_DATEValue;
        public DateTime? PERFORATION_DATE
        {
            get { return this.PERFORATION_DATEValue; }
            set { SetProperty(ref PERFORATION_DATEValue, value); }
        }

        // Standard PPDM columns

        public PERFORATION() { }
    }
}


