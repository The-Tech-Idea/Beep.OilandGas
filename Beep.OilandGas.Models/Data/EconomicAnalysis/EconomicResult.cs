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
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return (as decimal, e.g., 0.15 for 15%)
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Modified Internal Rate of Return
        /// </summary>
        private double MIRRValue;

        public double MIRR

        {

            get { return this.MIRRValue; }

            set { SetProperty(ref MIRRValue, value); }

        }

        /// <summary>
        /// Profitability Index
        /// </summary>
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Payback period (in periods)
        /// </summary>
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Discounted payback period (in periods)
        /// </summary>
        private double DiscountedPaybackPeriodValue;

        public double DiscountedPaybackPeriod

        {

            get { return this.DiscountedPaybackPeriodValue; }

            set { SetProperty(ref DiscountedPaybackPeriodValue, value); }

        }

        /// <summary>
        /// Return on Investment (as percentage)
        /// </summary>
        private double ROIValue;

        public double ROI

        {

            get { return this.ROIValue; }

            set { SetProperty(ref ROIValue, value); }

        }

        /// <summary>
        /// Total cash flow
        /// </summary>
        private double TotalCashFlowValue;

        public double TotalCashFlow

        {

            get { return this.TotalCashFlowValue; }

            set { SetProperty(ref TotalCashFlowValue, value); }

        }

        /// <summary>
        /// Present value
        /// </summary>
        private double PresentValueValue;

        public double PresentValue

        {

            get { return this.PresentValueValue; }

            set { SetProperty(ref PresentValueValue, value); }

        }

        /// <summary>
        /// Discount rate used for calculations
        /// </summary>
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
    }
}




