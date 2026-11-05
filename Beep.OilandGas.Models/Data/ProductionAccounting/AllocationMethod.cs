
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum AllocationMethod
    {
        /// <summary>
        /// Equal allocation to all parties.
        /// </summary>
        Equal,

        /// <summary>
        /// Pro-rata allocation based on working interest.
        /// </summary>
        ProRataWorkingInterest,

        /// <summary>
        /// Pro-rata allocation based on net revenue interest.
        /// </summary>
        ProRataNetRevenueInterest,

        /// <summary>
        /// Measured allocation based on test data.
        /// </summary>
        Measured,

        /// <summary>
        /// Estimated allocation based on production history.
        /// </summary>
        Estimated,
        Reciprocal,
        ActivityBasedCosting,
        StepDown,
        DirectAllocation
    }
}
