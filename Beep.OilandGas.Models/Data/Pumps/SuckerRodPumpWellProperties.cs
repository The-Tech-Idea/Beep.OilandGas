using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class SuckerRodPumpWellProperties : ModelEntityBase
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
        /// Producing fluid level in feet
        /// </summary>
        private decimal ProducingFluidLevelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ProducingFluidLevel

        {

            get { return this.ProducingFluidLevelValue; }

            set { SetProperty(ref ProducingFluidLevelValue, value); }

        }

        /// <summary>
        /// API gravity of produced fluid
        /// </summary>
        private decimal APIGravityValue;

        [Required]
        [Range(0, 60, ErrorMessage = "APIGravity must be between 0 and 60")]
        public decimal APIGravity

        {

            get { return this.APIGravityValue; }

            set { SetProperty(ref APIGravityValue, value); }

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

        /// <summary>
        /// Gas-Oil Ratio (GOR) in scf/stb
        /// </summary>
        private decimal GasOilRatioValue;

        [Range(0, double.MaxValue)]
        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }

        /// <summary>
        /// Fluid density in lb/ft³
        /// </summary>
        private decimal FluidDensityValue;

        [Range(0, double.MaxValue)]
        public decimal FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        }

        /// <summary>
        /// Pump type/size designation
        /// </summary>
        private string? PumpTypeDesignationValue;

        public string? PumpTypeDesignation

        {

            get { return this.PumpTypeDesignationValue; }

            set { SetProperty(ref PumpTypeDesignationValue, value); }

        }
    }
}
