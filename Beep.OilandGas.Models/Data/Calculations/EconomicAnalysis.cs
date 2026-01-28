using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EconomicAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Net present value (NPV)
        /// </summary>
        private decimal NPVValue;

        public decimal NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal rate of return (IRR) (%)
        /// </summary>
        private decimal IRRValue;

        public decimal IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Profitability index
        /// </summary>
        private decimal ProfitabilityIndexValue;

        public decimal ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Payback period (years)
        /// </summary>
        private decimal PaybackPeriodValue;

        public decimal PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Break-even price ($/unit)
        /// </summary>
        private decimal BreakEvenPriceValue;

        public decimal BreakEvenPrice

        {

            get { return this.BreakEvenPriceValue; }

            set { SetProperty(ref BreakEvenPriceValue, value); }

        }

        /// <summary>
        /// Discount rate used (%)
        /// </summary>
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }

        /// <summary>
        /// Economic limit (STB/day)
        /// </summary>
        private decimal EconomicLimitValue;

        public decimal EconomicLimit

        {

            get { return this.EconomicLimitValue; }

            set { SetProperty(ref EconomicLimitValue, value); }

        }

        /// <summary>
        /// Cash flow projections
        /// </summary>
        private List<CashFlowPoint> CashFlowsValue = new();

        public List<CashFlowPoint> CashFlows

        {

            get { return this.CashFlowsValue; }

            set { SetProperty(ref CashFlowsValue, value); }

        }
    }
}
