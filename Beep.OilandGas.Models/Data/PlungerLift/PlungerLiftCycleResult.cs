namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents plunger lift cycle analysis results
    /// DTO for calculations - Entity class: PLUNGER_LIFT_CYCLE_RESULT
    /// </summary>
    public class PlungerLiftCycleResult : ModelEntityBase
    {
        /// <summary>
        /// Cycle time in minutes
        /// </summary>
        private decimal CycleTimeValue;

        public decimal CycleTime

        {

            get { return this.CycleTimeValue; }

            set { SetProperty(ref CycleTimeValue, value); }

        }

        /// <summary>
        /// Plunger fall time in minutes
        /// </summary>
        private decimal FallTimeValue;

        public decimal FallTime

        {

            get { return this.FallTimeValue; }

            set { SetProperty(ref FallTimeValue, value); }

        }

        /// <summary>
        /// Plunger rise time in minutes
        /// </summary>
        private decimal RiseTimeValue;

        public decimal RiseTime

        {

            get { return this.RiseTimeValue; }

            set { SetProperty(ref RiseTimeValue, value); }

        }

        /// <summary>
        /// Shut-in time in minutes
        /// </summary>
        private decimal ShutInTimeValue;

        public decimal ShutInTime

        {

            get { return this.ShutInTimeValue; }

            set { SetProperty(ref ShutInTimeValue, value); }

        }

        /// <summary>
        /// Plunger fall velocity in ft/min
        /// </summary>
        private decimal FallVelocityValue;

        public decimal FallVelocity

        {

            get { return this.FallVelocityValue; }

            set { SetProperty(ref FallVelocityValue, value); }

        }

        /// <summary>
        /// Plunger rise velocity in ft/min
        /// </summary>
        private decimal RiseVelocityValue;

        public decimal RiseVelocity

        {

            get { return this.RiseVelocityValue; }

            set { SetProperty(ref RiseVelocityValue, value); }

        }

        /// <summary>
        /// Liquid slug size in bbl
        /// </summary>
        private decimal LiquidSlugSizeValue;

        public decimal LiquidSlugSize

        {

            get { return this.LiquidSlugSizeValue; }

            set { SetProperty(ref LiquidSlugSizeValue, value); }

        }

        /// <summary>
        /// Production per cycle in bbl
        /// </summary>
        private decimal ProductionPerCycleValue;

        public decimal ProductionPerCycle

        {

            get { return this.ProductionPerCycleValue; }

            set { SetProperty(ref ProductionPerCycleValue, value); }

        }

        /// <summary>
        /// Daily production rate in bbl/day
        /// </summary>
        private decimal DailyProductionRateValue;

        public decimal DailyProductionRate

        {

            get { return this.DailyProductionRateValue; }

            set { SetProperty(ref DailyProductionRateValue, value); }

        }

        /// <summary>
        /// Cycles per day
        /// </summary>
        private decimal CyclesPerDayValue;

        public decimal CyclesPerDay

        {

            get { return this.CyclesPerDayValue; }

            set { SetProperty(ref CyclesPerDayValue, value); }

        }
    }
}




