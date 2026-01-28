using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class InterestCapitalizationData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the beginning accumulated expenditures.
        /// </summary>
        private decimal BeginningAccumulatedExpendituresValue;

        public decimal BeginningAccumulatedExpenditures

        {

            get { return this.BeginningAccumulatedExpendituresValue; }

            set { SetProperty(ref BeginningAccumulatedExpendituresValue, value); }

        }

        /// <summary>
        /// Gets or sets the ending accumulated expenditures.
        /// </summary>
        private decimal EndingAccumulatedExpendituresValue;

        public decimal EndingAccumulatedExpenditures

        {

            get { return this.EndingAccumulatedExpendituresValue; }

            set { SetProperty(ref EndingAccumulatedExpendituresValue, value); }

        }

        /// <summary>
        /// Gets the average accumulated expenditures.
        /// </summary>
        public decimal AverageAccumulatedExpenditures =>
            (BeginningAccumulatedExpenditures + EndingAccumulatedExpenditures) / 2.0m;

        /// <summary>
        /// Gets or sets the interest rate (as decimal, e.g., 0.10 for 10%).
        /// </summary>
        private decimal InterestRateValue;

        public decimal InterestRate

        {

            get { return this.InterestRateValue; }

            set { SetProperty(ref InterestRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the capitalization period in months.
        /// </summary>
        private decimal CapitalizationPeriodMonthsValue;

        public decimal CapitalizationPeriodMonths

        {

            get { return this.CapitalizationPeriodMonthsValue; }

            set { SetProperty(ref CapitalizationPeriodMonthsValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual interest costs incurred.
        /// </summary>
        private decimal ActualInterestCostsValue;

        public decimal ActualInterestCosts

        {

            get { return this.ActualInterestCostsValue; }

            set { SetProperty(ref ActualInterestCostsValue, value); }

        }
    }
}
