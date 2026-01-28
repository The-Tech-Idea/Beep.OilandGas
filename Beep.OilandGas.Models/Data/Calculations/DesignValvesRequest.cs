using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DesignValvesRequest : ModelEntityBase
    {
        /// <summary>
        /// Well properties for valve design
        /// </summary>
        private GAS_LIFT_WELL_PROPERTIES WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public GAS_LIFT_WELL_PROPERTIES WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }

        /// <summary>
        /// Gas injection pressure (psia)
        /// </summary>
        private decimal GasInjectionPressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasInjectionPressure must be greater than or equal to 0")]
        public decimal GasInjectionPressure

        {

            get { return this.GasInjectionPressureValue; }

            set { SetProperty(ref GasInjectionPressureValue, value); }

        }

        /// <summary>
        /// Number of valves to design
        /// </summary>
        private int NumberOfValvesValue;

        [Range(1, 50, ErrorMessage = "NumberOfValves must be between 1 and 50")]
        public int NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }

        /// <summary>
        /// Whether to use SI units (false = use field units)
        /// </summary>
        private bool UseSIUnitsValue = false;

        public bool UseSIUnits

        {

            get { return this.UseSIUnitsValue; }

            set { SetProperty(ref UseSIUnitsValue, value); }

        }
    }
}
