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
        public double Rate { get; set; }

        /// <summary>
        /// NPV at this discount rate
        /// </summary>
        public double NPV { get; set; }
        public double DiscountRate { get; set; }

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



