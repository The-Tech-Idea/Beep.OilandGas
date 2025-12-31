using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class FLASH_CALCULATION_RESULT : Entity
    {
        private string CALCULATION_IDValue;
        public string CALCULATION_ID
        {
            get { return this.CALCULATION_IDValue; }
            set { SetProperty(ref CALCULATION_IDValue, value); }
        }

        private string COMPONENT_IDValue;
        public string COMPONENT_ID
        {
            get { return this.COMPONENT_IDValue; }
            set { SetProperty(ref COMPONENT_IDValue, value); }
        }

        private decimal? VAPOR_FRACTIONValue;
        public decimal? VAPOR_FRACTION
        {
            get { return this.VAPOR_FRACTIONValue; }
            set { SetProperty(ref VAPOR_FRACTIONValue, value); }
        }

        private decimal? LIQUID_FRACTIONValue;
        public decimal? LIQUID_FRACTION
        {
            get { return this.LIQUID_FRACTIONValue; }
            set { SetProperty(ref LIQUID_FRACTIONValue, value); }
        }

        private int? ITERATIONSValue;
        public int? ITERATIONS
        {
            get { return this.ITERATIONSValue; }
            set { SetProperty(ref ITERATIONSValue, value); }
        }

        private string CONVERGEDValue;
        public string CONVERGED
        {
            get { return this.CONVERGEDValue; }
            set { SetProperty(ref CONVERGEDValue, value); }
        }

        private decimal? CONVERGENCE_ERRORValue;
        public decimal? CONVERGENCE_ERROR
        {
            get { return this.CONVERGENCE_ERRORValue; }
            set { SetProperty(ref CONVERGENCE_ERRORValue, value); }
        }

        private DateTime? CALCULATION_DATEValue;
        public DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
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

