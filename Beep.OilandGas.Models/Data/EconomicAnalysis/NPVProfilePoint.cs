namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    /// <summary>
    /// Represents a point on an NPV profile curve
    /// DTO for calculations - Entity class: NPV_PROFILE_POINT
    /// </summary>
    public class NPVProfilePoint : ModelEntityBase
    {
        /// <summary>
        /// Discount rate for this point
        /// </summary>
        private double RateValue;

        public double Rate

        {

            get { return this.RateValue; }

            set { SetProperty(ref RateValue, value); }

        }

        /// <summary>
        /// NPV at this discount rate
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NPVProfilePoint()
        {
        }

        /// <summary>
        /// Constructor with rate and NPV
        /// </summary>
        public NPVProfilePoint(double rate, double npv)
        {
            Rate = rate;
            NPV = npv;
        }
    }
}




