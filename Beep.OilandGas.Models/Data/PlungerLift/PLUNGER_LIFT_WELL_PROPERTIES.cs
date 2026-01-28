using Beep.OilandGas.Models.Data;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PLUNGER_LIFT_WELL_PROPERTIES : ModelEntityBase {


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

        public decimal GasOilRatio { get; set; }
        private String PLUNGER_LIFT_WELL_PROPERTIES_IDValue;
        public String PLUNGER_LIFT_WELL_PROPERTIES_ID
        {
            get { return this.PLUNGER_LIFT_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? WELL_DEPTHValue;
        public Decimal? WELL_DEPTH
        {
            get { return this.WELL_DEPTHValue; }
            set { SetProperty(ref WELL_DEPTHValue, value); }
        }

        private Decimal? TUBING_DIAMETERValue;
        public Decimal? TUBING_DIAMETER
        {
            get { return this.TUBING_DIAMETERValue; }
            set { SetProperty(ref TUBING_DIAMETERValue, value); }
        }

        private Decimal? PLUNGER_DIAMETERValue;
        public Decimal? PLUNGER_DIAMETER
        {
            get { return this.PLUNGER_DIAMETERValue; }
            set { SetProperty(ref PLUNGER_DIAMETERValue, value); }
        }

        private Decimal? WELLHEAD_PRESSUREValue;
        public Decimal? WELLHEAD_PRESSURE
        {
            get { return this.WELLHEAD_PRESSUREValue; }
            set { SetProperty(ref WELLHEAD_PRESSUREValue, value); }
        }

        private Decimal? CASING_PRESSUREValue;
        public Decimal? CASING_PRESSURE
        {
            get { return this.CASING_PRESSUREValue; }
            set { SetProperty(ref CASING_PRESSUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Decimal? WELLHEAD_TEMPERATUREValue;
        public Decimal? WELLHEAD_TEMPERATURE
        {
            get { return this.WELLHEAD_TEMPERATUREValue; }
            set { SetProperty(ref WELLHEAD_TEMPERATUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_TEMPERATUREValue;
        public Decimal? BOTTOM_HOLE_TEMPERATURE
        {
            get { return this.BOTTOM_HOLE_TEMPERATUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_TEMPERATUREValue, value); }
        }

        private Decimal? OIL_GRAVITYValue;
        public Decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal? WATER_CUTValue;
        public Decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal? GAS_OIL_RATIOValue;
        public Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? LIQUID_PRODUCTION_RATEValue;
        public Decimal? LIQUID_PRODUCTION_RATE
        {
            get { return this.LIQUID_PRODUCTION_RATEValue; }
            set { SetProperty(ref LIQUID_PRODUCTION_RATEValue, value); }
        }

        // Standard PPDM columns

    }
}
