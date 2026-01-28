using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class AnalyzeGasLiftPotentialRequest : ModelEntityBase
    {
        /// <summary>
        /// Well properties for gas lift analysis
        /// </summary>
        private GAS_LIFT_WELL_PROPERTIES WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public GAS_LIFT_WELL_PROPERTIES WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }

        /// <summary>
        /// Minimum gas injection rate (Mscf/day)
        /// </summary>
        private decimal MinGasInjectionRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinGasInjectionRate must be greater than or equal to 0")]
        public decimal MinGasInjectionRate

        {

            get { return this.MinGasInjectionRateValue; }

            set { SetProperty(ref MinGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Maximum gas injection rate (Mscf/day)
        /// </summary>
        private decimal MaxGasInjectionRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaxGasInjectionRate must be greater than or equal to 0")]
        public decimal MaxGasInjectionRate

        {

            get { return this.MaxGasInjectionRateValue; }

            set { SetProperty(ref MaxGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Number of points for performance curve
        /// </summary>
        private int NumberOfPointsValue = 50;

        [Range(2, 1000, ErrorMessage = "NumberOfPoints must be between 2 and 1000")]
        public int NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        }
    }
}
