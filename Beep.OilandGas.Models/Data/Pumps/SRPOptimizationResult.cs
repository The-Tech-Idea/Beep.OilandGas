using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class SRPOptimizationResult : ModelEntityBase
    {
        /// <summary>
        /// Optimization analysis date
        /// </summary>
        private System.DateTime OptimizationDateValue;

        public System.DateTime OptimizationDate

        {

            get { return this.OptimizationDateValue; }

            set { SetProperty(ref OptimizationDateValue, value); }

        }

        /// <summary>
        /// User who performed optimization
        /// </summary>
        private string OptimizedByUserValue = string.Empty;

        public string OptimizedByUser

        {

            get { return this.OptimizedByUserValue; }

            set { SetProperty(ref OptimizedByUserValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Current production rate in bbl/day
        /// </summary>
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }

        /// <summary>
        /// Target production rate in bbl/day
        /// </summary>
        private decimal TargetProductionValue;

        public decimal TargetProduction

        {

            get { return this.TargetProductionValue; }

            set { SetProperty(ref TargetProductionValue, value); }

        }

        /// <summary>
        /// Production increase percentage
        /// </summary>
        private decimal ProductionIncreasePercentValue;

        public decimal ProductionIncreasePercent

        {

            get { return this.ProductionIncreasePercentValue; }

            set { SetProperty(ref ProductionIncreasePercentValue, value); }

        }

        /// <summary>
        /// Additional daily revenue impact
        /// </summary>
        private decimal AdditionalDailyRevenueValue;

        public decimal AdditionalDailyRevenue

        {

            get { return this.AdditionalDailyRevenueValue; }

            set { SetProperty(ref AdditionalDailyRevenueValue, value); }

        }

        /// <summary>
        /// Annual revenue increase
        /// </summary>
        private decimal AnnualRevenueIncreaseValue;

        public decimal AnnualRevenueIncrease

        {

            get { return this.AnnualRevenueIncreaseValue; }

            set { SetProperty(ref AnnualRevenueIncreaseValue, value); }

        }

        /// <summary>
        /// Flag indicating economic feasibility
        /// </summary>
        private bool IsEconomicallyFeasibleValue;

        public bool IsEconomicallyFeasible

        {

            get { return this.IsEconomicallyFeasibleValue; }

            set { SetProperty(ref IsEconomicallyFeasibleValue, value); }

        }

        /// <summary>
        /// Optimization recommendations
        /// </summary>
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }
}
