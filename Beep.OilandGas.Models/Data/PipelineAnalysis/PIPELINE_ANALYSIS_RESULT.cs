using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_ANALYSIS_RESULT : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
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
        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }
    }
}




