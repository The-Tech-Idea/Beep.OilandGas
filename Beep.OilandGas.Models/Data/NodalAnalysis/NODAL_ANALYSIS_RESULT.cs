using System;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_ANALYSIS_RESULT : ModelEntityBase {
        private string NODAL_ANALYSIS_RESULT_IDValue;
        public string NODAL_ANALYSIS_RESULT_ID
        {
            get { return this.NODAL_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_RESULT_IDValue, value); }
        }

        private string ANALYSIS_IDValue;
        public string ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private DateTime? ANALYSIS_DATEValue;
        public DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private decimal? OPERATING_FLOW_RATEValue;
        public decimal? OPERATING_FLOW_RATE
        {
            get { return this.OPERATING_FLOW_RATEValue; }
            set { SetProperty(ref OPERATING_FLOW_RATEValue, value); }
        }

        private decimal? OPERATING_PRESSUREValue;
        public decimal? OPERATING_PRESSURE
        {
            get { return this.OPERATING_PRESSUREValue; }
            set { SetProperty(ref OPERATING_PRESSUREValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        // Standard PPDM columns
    }
}





