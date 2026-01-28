using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class DRILLING_REPORT : ModelEntityBase
    {
        private String DRILLING_REPORT_IDValue;
        public String DRILLING_REPORT_ID
        {
            get { return this.DRILLING_REPORT_IDValue; }
            set { SetProperty(ref DRILLING_REPORT_IDValue, value); }
        }

        private String DRILLING_OPERATION_IDValue;
        public String DRILLING_OPERATION_ID
        {
            get { return this.DRILLING_OPERATION_IDValue; }
            set { SetProperty(ref DRILLING_OPERATION_IDValue, value); }
        }

        private DateTime? REPORT_DATEValue;
        public DateTime? REPORT_DATE
        {
            get { return this.REPORT_DATEValue; }
            set { SetProperty(ref REPORT_DATEValue, value); }
        }

        private Decimal? DEPTHValue;
        public Decimal? DEPTH
        {
            get { return this.DEPTHValue; }
            set { SetProperty(ref DEPTHValue, value); }
        }

        private String ACTIVITYValue;
        public String ACTIVITY
        {
            get { return this.ACTIVITYValue; }
            set { SetProperty(ref ACTIVITYValue, value); }
        }

        private Decimal? HOURSValue;
        public Decimal? HOURS
        {
            get { return this.HOURSValue; }
            set { SetProperty(ref HOURSValue, value); }
        }

        private String REPORTED_BYValue;
        public String REPORTED_BY
        {
            get { return this.REPORTED_BYValue; }
            set { SetProperty(ref REPORTED_BYValue, value); }
        }

        // Standard PPDM columns

        public DRILLING_REPORT() { }
    }
}
