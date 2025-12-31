
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Financial.Amortization
{
    /// <summary>
    /// Provides interest capitalization calculations per FASB standards.
    /// </summary>
    public static class InterestCapitalizationCalculator
    {
        /// <summary>
        /// Calculates interest to be capitalized for a period.
        /// </summary>
        /// <param name="data">Interest capitalization data.</param>
        /// <returns>Interest amount to capitalize.</returns>
        public static decimal CalculateInterestCapitalization(InterestCapitalizationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.InterestRate < AccountingConstants.MinInterestRate || 
                data.InterestRate > AccountingConstants.MaxInterestRate)
                throw new InvalidAccountingDataException(nameof(data.InterestRate), 
                    "Interest rate must be between 0 and 1.");

            if (data.CapitalizationPeriodMonths < 0)
                throw new InvalidAccountingDataException(nameof(data.CapitalizationPeriodMonths), 
                    "Capitalization period cannot be negative.");

            // Calculate interest to capitalize
            decimal averageExpenditures = data.AverageAccumulatedExpenditures;
            decimal interestRate = data.InterestRate;
            decimal periodYears = data.CapitalizationPeriodMonths / 12.0m;

            decimal interestToCapitalize = averageExpenditures * interestRate * periodYears;

            // Cannot exceed actual interest costs
            return Math.Min(interestToCapitalize, data.ActualInterestCosts);
        }

        /// <summary>
        /// Calculates average accumulated expenditures.
        /// </summary>
        public static decimal CalculateAverageAccumulatedExpenditures(
            decimal beginningBalance,
            decimal endingBalance)
        {
            return (beginningBalance + endingBalance) / 2.0m;
        }

        /// <summary>
        /// Determines if interest capitalization period has started.
        /// </summary>
        public static bool HasCapitalizationPeriodStarted(
            bool expendituresMade,
            bool activitiesInProgress,
            bool interestCostsIncurred)
        {
            return expendituresMade && activitiesInProgress && interestCostsIncurred;
        }

        /// <summary>
        /// Determines if interest capitalization period should end.
        /// </summary>
        public static bool ShouldCapitalizationPeriodEnd(bool assetSubstantiallyComplete)
        {
            return assetSubstantiallyComplete;
        }
    }
}
