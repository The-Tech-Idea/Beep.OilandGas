using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class DesignPlungerLiftSystemRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Well properties for plunger lift design
        /// </summary>
        private PLUNGER_LIFT_WELL_PROPERTIES WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public PLUNGER_LIFT_WELL_PROPERTIES WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }
    }
}
