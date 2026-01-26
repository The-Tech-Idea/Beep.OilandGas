using System;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public partial class FLASH_CALCULATION_RESULT : ModelEntityBase {
        private string FLASH_CALCULATION_RESULT_IDValue;
        public string FLASH_CALCULATION_RESULT_ID
        {
            get { return this.FLASH_CALCULATION_RESULT_IDValue; }
            set { SetProperty(ref FLASH_CALCULATION_RESULT_IDValue, value); }
        }

        private string CALCULATION_IDValue;
        public string CALCULATION_ID
        {
            get { return this.CALCULATION_IDValue; }
            set { SetProperty(ref CALCULATION_IDValue, value); }
        }
        public string FLASH_CALCULATION_ID
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

        private string WELL_IDValue;
        public string WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private string FACILITY_IDValue;
        public string FACILITY_ID
        {
            get { return this.FACILITY_IDValue; }
            set { SetProperty(ref FACILITY_IDValue, value); }
        }

        private decimal? PRESSUREValue;
        public decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private decimal? TEMPERATUREValue;
        public decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
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

        private string FEED_COMPOSITION_JSONValue;
        public string FEED_COMPOSITION_JSON
        {
            get { return this.FEED_COMPOSITION_JSONValue; }
            set { SetProperty(ref FEED_COMPOSITION_JSONValue, value); }
        }

        private string VAPOR_COMPOSITION_JSONValue;
        public string VAPOR_COMPOSITION_JSON
        {
            get { return this.VAPOR_COMPOSITION_JSONValue; }
            set { SetProperty(ref VAPOR_COMPOSITION_JSONValue, value); }
        }

        private string LIQUID_COMPOSITION_JSONValue;
        public string LIQUID_COMPOSITION_JSON
        {
            get { return this.LIQUID_COMPOSITION_JSONValue; }
            set { SetProperty(ref LIQUID_COMPOSITION_JSONValue, value); }
        }

        private string K_VALUES_JSONValue;
        public string K_VALUES_JSON
        {
            get { return this.K_VALUES_JSONValue; }
            set { SetProperty(ref K_VALUES_JSONValue, value); }
        }

        private DateTime? CALCULATION_DATEValue;
        public DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private string ERROR_MESSAGEValue;
        public string ERROR_MESSAGE
        {
            get { return this.ERROR_MESSAGEValue; }
            set { SetProperty(ref ERROR_MESSAGEValue, value); }
        }

        // Standard PPDM columns
    }
}





