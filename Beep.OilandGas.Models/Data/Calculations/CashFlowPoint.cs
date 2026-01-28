using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CashFlowPoint : ModelEntityBase
    {
        /// <summary>
        /// Period date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Revenue ($)
        /// </summary>
        private decimal RevenueValue;

        public decimal Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }

        /// <summary>
        /// Operating costs ($)
        /// </summary>
        private decimal OperatingCostsValue;

        public decimal OperatingCosts

        {

            get { return this.OperatingCostsValue; }

            set { SetProperty(ref OperatingCostsValue, value); }

        }

        /// <summary>
        /// Capital costs ($)
        /// </summary>
        private decimal CapitalCostsValue;

        public decimal CapitalCosts

        {

            get { return this.CapitalCostsValue; }

            set { SetProperty(ref CapitalCostsValue, value); }

        }

        /// <summary>
        /// Net cash flow ($)
        /// </summary>
        private decimal NetCashFlowValue;

        public decimal NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }

        /// <summary>
        /// Cumulative cash flow ($)
        /// </summary>
        private decimal CumulativeCashFlowValue;

        public decimal CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
    }
}
