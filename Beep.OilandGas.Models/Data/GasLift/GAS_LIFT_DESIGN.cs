using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_DESIGN : Entity
    {
        private string DESIGN_IDValue;
        public string DESIGN_ID
        {
            get { return this.DESIGN_IDValue; }
            set { SetProperty(ref DESIGN_IDValue, value); }
        }

        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private DateTime? DESIGN_DATEValue;
        public DateTime? DESIGN_DATE
        {
            get { return this.DESIGN_DATEValue; }
            set { SetProperty(ref DESIGN_DATEValue, value); }
        }

        private int? NUMBER_OF_VALVESValue;
        public int? NUMBER_OF_VALVES
        {
            get { return this.NUMBER_OF_VALVESValue; }
            set { SetProperty(ref NUMBER_OF_VALVESValue, value); }
        }

        private decimal? TOTAL_GAS_INJECTION_RATEValue;
        public decimal? TOTAL_GAS_INJECTION_RATE
        {
            get { return this.TOTAL_GAS_INJECTION_RATEValue; }
            set { SetProperty(ref TOTAL_GAS_INJECTION_RATEValue, value); }
        }

        private decimal? EXPECTED_PRODUCTION_RATEValue;
        public decimal? EXPECTED_PRODUCTION_RATE
        {
            get { return this.EXPECTED_PRODUCTION_RATEValue; }
            set { SetProperty(ref EXPECTED_PRODUCTION_RATEValue, value); }
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

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
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

