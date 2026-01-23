namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod load analysis results
    /// DTO for calculations - Entity class: SUCKER_ROD_LOAD_RESULT
    /// </summary>
    public class SuckerRodLoadResult : ModelEntityBase
    {
        /// <summary>
        /// Peak load in pounds
        /// </summary>
        private decimal PeakLoadValue;

        public decimal PeakLoad

        {

            get { return this.PeakLoadValue; }

            set { SetProperty(ref PeakLoadValue, value); }

        }

        /// <summary>
        /// Minimum load in pounds
        /// </summary>
        private decimal MinimumLoadValue;

        public decimal MinimumLoad

        {

            get { return this.MinimumLoadValue; }

            set { SetProperty(ref MinimumLoadValue, value); }

        }

        /// <summary>
        /// Polished rod load in pounds
        /// </summary>
        private decimal PolishedRodLoadValue;

        public decimal PolishedRodLoad

        {

            get { return this.PolishedRodLoadValue; }

            set { SetProperty(ref PolishedRodLoadValue, value); }

        }

        /// <summary>
        /// Rod string weight in pounds
        /// </summary>
        private decimal RodStringWeightValue;

        public decimal RodStringWeight

        {

            get { return this.RodStringWeightValue; }

            set { SetProperty(ref RodStringWeightValue, value); }

        }

        /// <summary>
        /// Fluid load in pounds
        /// </summary>
        private decimal FluidLoadValue;

        public decimal FluidLoad

        {

            get { return this.FluidLoadValue; }

            set { SetProperty(ref FluidLoadValue, value); }

        }

        /// <summary>
        /// Dynamic load in pounds
        /// </summary>
        private decimal DynamicLoadValue;

        public decimal DynamicLoad

        {

            get { return this.DynamicLoadValue; }

            set { SetProperty(ref DynamicLoadValue, value); }

        }

        /// <summary>
        /// Load range in pounds
        /// </summary>
        private decimal LoadRangeValue;

        public decimal LoadRange

        {

            get { return this.LoadRangeValue; }

            set { SetProperty(ref LoadRangeValue, value); }

        }

        /// <summary>
        /// Stress range in psi
        /// </summary>
        private decimal StressRangeValue;

        public decimal StressRange

        {

            get { return this.StressRangeValue; }

            set { SetProperty(ref StressRangeValue, value); }

        }

        /// <summary>
        /// Maximum stress in psi
        /// </summary>
        private decimal MaximumStressValue;

        public decimal MaximumStress

        {

            get { return this.MaximumStressValue; }

            set { SetProperty(ref MaximumStressValue, value); }

        }

        /// <summary>
        /// Load factor (safety factor)
        /// </summary>
        private decimal LoadFactorValue;

        public decimal LoadFactor

        {

            get { return this.LoadFactorValue; }

            set { SetProperty(ref LoadFactorValue, value); }

        }
    }
}




