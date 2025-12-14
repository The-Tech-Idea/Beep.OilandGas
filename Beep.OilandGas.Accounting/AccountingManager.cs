using Beep.OilandGas.Accounting.SuccessfulEfforts;
using Beep.OilandGas.Accounting.FullCost;
using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Models;

namespace Beep.OilandGas.Accounting
{
    /// <summary>
    /// Main manager class for oil and gas accounting operations.
    /// </summary>
    public static class AccountingManager
    {
        /// <summary>
        /// Creates a new Successful Efforts accounting instance.
        /// </summary>
        public static SuccessfulEffortsAccounting CreateSuccessfulEffortsAccounting()
        {
            return new SuccessfulEffortsAccounting();
        }

        /// <summary>
        /// Creates a new Full Cost accounting instance.
        /// </summary>
        public static FullCostAccounting CreateFullCostAccounting()
        {
            return new FullCostAccounting();
        }

        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        public static decimal CalculateAmortization(
            decimal netCapitalizedCosts,
            decimal totalProvedReservesBOE,
            decimal productionBOE)
        {
            return AmortizationCalculator.CalculateUnitsOfProduction(
                netCapitalizedCosts,
                totalProvedReservesBOE,
                productionBOE);
        }

        /// <summary>
        /// Calculates interest capitalization.
        /// </summary>
        public static decimal CalculateInterestCapitalization(InterestCapitalizationData data)
        {
            return InterestCapitalizationCalculator.CalculateInterestCapitalization(data);
        }

        /// <summary>
        /// Converts production to BOE.
        /// </summary>
        public static decimal ConvertProductionToBOE(ProductionData production)
        {
            return AmortizationCalculator.ConvertProductionToBOE(production);
        }

        /// <summary>
        /// Converts reserves to BOE.
        /// </summary>
        public static decimal ConvertReservesToBOE(ProvedReserves reserves)
        {
            return AmortizationCalculator.ConvertReservesToBOE(reserves);
        }
    }
}

