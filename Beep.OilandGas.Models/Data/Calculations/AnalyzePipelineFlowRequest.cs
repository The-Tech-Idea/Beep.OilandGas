using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class AnalyzePipelineFlowRequest : ModelEntityBase
    {
        /// <summary>
        /// Pipeline identifier
        /// </summary>
        private string PipelineIdValue = string.Empty;

        [Required(ErrorMessage = "PipelineId is required")]
        public string PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }

        /// <summary>
        /// Flow rate (Mscf/day for gas, bbl/day for liquid)
        /// </summary>
        private decimal FlowRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "FlowRate must be greater than or equal to 0")]
        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Inlet pressure (psia)
        /// </summary>
        private decimal InletPressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "InletPressure must be greater than or equal to 0")]
        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
    }
}
