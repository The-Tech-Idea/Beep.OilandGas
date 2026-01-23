using System;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_ANALYSIS_RESULT : ModelEntityBase {
        private string ANALYSIS_IDValue;
        public string ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private string PIPELINE_IDValue;
        public string PIPELINE_ID
        {
            get { return this.PIPELINE_IDValue; }
            set { SetProperty(ref PIPELINE_IDValue, value); }
        }

        private DateTime? ANALYSIS_DATEValue;
        public DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private decimal? FLOW_RATEValue;
        public decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private decimal? PRESSURE_DROPValue;
        public decimal? PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private decimal? VELOCITYValue;
        public decimal? VELOCITY
        {
            get { return this.VELOCITYValue; }
            set { SetProperty(ref VELOCITYValue, value); }
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





