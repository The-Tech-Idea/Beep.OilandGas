using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class NODAL_ANALYSIS_RUN_METADATA : ModelEntityBase
    {
        private string NODAL_ANALYSIS_RUN_METADATA_IDValue;
        public string NODAL_ANALYSIS_RUN_METADATA_ID
        {
            get { return this.NODAL_ANALYSIS_RUN_METADATA_IDValue; }
            set { SetProperty(ref NODAL_ANALYSIS_RUN_METADATA_IDValue, value); }
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

        private string SNAPSHOT_INCLUDED_INDValue;
        public string SNAPSHOT_INCLUDED_IND
        {
            get { return this.SNAPSHOT_INCLUDED_INDValue; }
            set { SetProperty(ref SNAPSHOT_INCLUDED_INDValue, value); }
        }

        private int? IPR_POINT_COUNTValue;
        public int? IPR_POINT_COUNT
        {
            get { return this.IPR_POINT_COUNTValue; }
            set { SetProperty(ref IPR_POINT_COUNTValue, value); }
        }

        private int? VLP_POINT_COUNTValue;
        public int? VLP_POINT_COUNT
        {
            get { return this.VLP_POINT_COUNTValue; }
            set { SetProperty(ref VLP_POINT_COUNTValue, value); }
        }

        private string EXECUTION_STATUSValue;
        public string EXECUTION_STATUS
        {
            get { return this.EXECUTION_STATUSValue; }
            set { SetProperty(ref EXECUTION_STATUSValue, value); }
        }

        private DateTime? ANALYSIS_DATEValue;
        public DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }
    }
}
