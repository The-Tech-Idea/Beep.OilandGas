namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    /// <summary>
    /// Result of comprehensive economic analysis
    /// DTO for calculations - Entity class: ECONOMIC_ANALYSIS_RESULT
    /// </summary>
    public class EconomicResult : ModelEntityBase
    {
        /// <summary>
        /// Net Present Value
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return (as decimal, e.g., 0.15 for 15%)
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Modified Internal Rate of Return
        /// </summary>
        public double MIRR { get; set; }

        /// <summary>
        /// Profitability Index
        /// </summary>
        public double ProfitabilityIndex { get; set; }

        /// <summary>
        /// Payback period (in periods)
        /// </summary>
        public double PaybackPeriod { get; set; }

        /// <summary>
        /// Discounted payback period (in periods)
        /// </summary>
        public double DiscountedPaybackPeriod { get; set; }

        /// <summary>
        /// Return on Investment (as percentage)
        /// </summary>
        public double ROI { get; set; }

        /// <summary>
        /// Total cash flow
        /// </summary>
        public double TotalCashFlow { get; set; }

        /// <summary>
        /// Present value
        /// </summary>
        public double PresentValue { get; set; }

        /// <summary>
        /// Discount rate used for calculations
        /// </summary>
        public double DiscountRate { get; set; }
    }
}



