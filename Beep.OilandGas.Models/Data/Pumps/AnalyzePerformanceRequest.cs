using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class AnalyzePerformanceRequest : ModelEntityBase
    {
        /// <summary>
        /// Pump identifier or Well UWI (depending on pump type)
        /// </summary>
        private string PumpIdValue = string.Empty;

        [Required(ErrorMessage = "PumpId or WellUWI is required")]
        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
    }
}
