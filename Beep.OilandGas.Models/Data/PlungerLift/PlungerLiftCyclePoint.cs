namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents a point in plunger lift cycle
    /// DTO for calculations - Entity class: PLUNGER_LIFT_CYCLE_POINT
    /// </summary>
    public class PlungerLiftCyclePoint : ModelEntityBase
    {
        /// <summary>
        /// Time in minutes from cycle start
        /// </summary>
        private decimal TimeValue;

        public decimal Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Cycle phase
        /// </summary>
        private PlungerLiftCyclePhase PhaseValue;

        public PlungerLiftCyclePhase Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        }

        /// <summary>
        /// Plunger depth in feet
        /// </summary>
        private decimal PlungerDepthValue;

        public decimal PlungerDepth

        {

            get { return this.PlungerDepthValue; }

            set { SetProperty(ref PlungerDepthValue, value); }

        }

        /// <summary>
        /// Casing pressure in psia
        /// </summary>
        private decimal CasingPressureValue;

        public decimal CasingPressure

        {

            get { return this.CasingPressureValue; }

            set { SetProperty(ref CasingPressureValue, value); }

        }

        /// <summary>
        /// Tubing pressure in psia
        /// </summary>
        private decimal TubingPressureValue;

        public decimal TubingPressure

        {

            get { return this.TubingPressureValue; }

            set { SetProperty(ref TubingPressureValue, value); }

        }

        /// <summary>
        /// Liquid production rate in bbl/day
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
    }
}




