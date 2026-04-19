using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class DesignPumpSystemRequest : ModelEntityBase
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
        /// Pump type (e.g., "ESP", "SuckerRod", "Hydraulic", "PlungerLift")
        /// </summary>
        private string PumpTypeValue = string.Empty;

        [Required(ErrorMessage = "PumpType is required")]
        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }

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
        /// Desired flow rate (bbl/day)
        /// </summary>
        private decimal DesiredFlowRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "DesiredFlowRate must be greater than or equal to 0")]
        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }

        public SuckerRodPumpWellProperties WellProperties

        {

            get

            {

                return new SuckerRodPumpWellProperties

                {

                    WellDepth = WellDepth,

                    DesiredProductionRate = DesiredFlowRate,

                    TubingSize = 2.375m,

                    CasingSize = 5.5m,

                    StaticFluidLevel = WellDepth > 0m ? WellDepth * 0.50m : 0m,

                    ProducingFluidLevel = WellDepth > 0m ? WellDepth * 0.65m : 0m,

                    APIGravity = 35m,

                    GasOilRatio = 0m,

                    FluidDensity = 50m,

                    PumpTypeDesignation = PumpType

                };

            }

            set

            {

                if (value == null)

                    return;

                WellDepth = value.WellDepth;

                DesiredFlowRate = value.DesiredProductionRate;

                if (!string.IsNullOrWhiteSpace(value.PumpTypeDesignation))

                    PumpType = value.PumpTypeDesignation;

            }

        }
    }
}
