using System;

namespace Beep.OilandGas.EconomicAnalysis.Models
{
    /// <summary>
    /// Represents a cash flow at a specific time period.
    /// </summary>
    public class CashFlow
    {
        /// <summary>
        /// Gets or sets the period number (0 = initial, 1+ = subsequent periods).
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Gets or sets the cash flow amount (negative for investment, positive for revenue).
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Gets or sets the description of the cash flow.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        public CashFlow() { }

        public CashFlow(int period, double amount, string description = "")
        {
            Period = period;
            Amount = amount;
            Description = description;
        }
    }

    /// <summary>
    /// Represents economic analysis results.
    /// </summary>
    public class EconomicResult
    {
        /// <summary>
        /// Gets or sets the Net Present Value.
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Gets or sets the Internal Rate of Return.
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Gets or sets the Modified Internal Rate of Return.
        /// </summary>
        public double MIRR { get; set; }

        /// <summary>
        /// Gets or sets the Profitability Index.
        /// </summary>
        public double ProfitabilityIndex { get; set; }

        /// <summary>
        /// Gets or sets the payback period in years.
        /// </summary>
        public double PaybackPeriod { get; set; }

        /// <summary>
        /// Gets or sets the discounted payback period in years.
        /// </summary>
        public double DiscountedPaybackPeriod { get; set; }

        /// <summary>
        /// Gets or sets the Return on Investment.
        /// </summary>
        public double ROI { get; set; }

        /// <summary>
        /// Gets or sets the total cash flow.
        /// </summary>
        public double TotalCashFlow { get; set; }

        /// <summary>
        /// Gets or sets the present value of cash flows.
        /// </summary>
        public double PresentValue { get; set; }

        /// <summary>
        /// Gets or sets the discount rate used.
        /// </summary>
        public double DiscountRate { get; set; }
    }

    /// <summary>
    /// Represents a point on an NPV profile.
    /// </summary>
    public class NPVProfilePoint
    {
        /// <summary>
        /// Gets or sets the discount rate.
        /// </summary>
        public double DiscountRate { get; set; }

        /// <summary>
        /// Gets or sets the NPV at this discount rate.
        /// </summary>
        public double NPV { get; set; }

        public NPVProfilePoint() { }

        public NPVProfilePoint(double discountRate, double npv)
        {
            DiscountRate = discountRate;
            NPV = npv;
        }
    }
}

