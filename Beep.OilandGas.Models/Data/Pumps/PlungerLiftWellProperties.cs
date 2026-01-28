using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PlungerLiftWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        private decimal WellDepthValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Tubing size in inches
        /// </summary>
        private decimal TubingSizeValue;

        [Required]
        [Range(0.5, 10, ErrorMessage = "TubingSize must be between 0.5 and 10 inches")]
        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }

        /// <summary>
        /// Casing size in inches
        /// </summary>
        private decimal CasingSizeValue;

        [Required]
        [Range(0.5, 20, ErrorMessage = "CasingSize must be between 0.5 and 20 inches")]
        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }

        /// <summary>
        /// Static fluid level in feet
        /// </summary>
        private decimal StaticFluidLevelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal StaticFluidLevel

        {

            get { return this.StaticFluidLevelValue; }

            set { SetProperty(ref StaticFluidLevelValue, value); }

        }

        /// <summary>
        /// Reservoir pressure in psi
        /// </summary>
        private decimal ReservoirPressureValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Desired production rate in bbl/day
        /// </summary>
        private decimal DesiredProductionRateValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        }
    }
}
