
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Financial.Amortization
{
    /// <summary>
    /// Provides amortization calculations for oil and gas properties.
    /// </summary>
    public static class AmortizationCalculator
    {
        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        /// <param name="netCapitalizedCosts">Net capitalized costs (after accumulated amortization).</param>
        /// <param name="totalProvedReservesBOE">Total proved reserves in BOE.</param>
        /// <param name="productionBOE">Production for the period in BOE.</param>
        /// <returns>Amortization amount for the period.</returns>
        public static decimal CalculateUnitsOfProduction(
            decimal netCapitalizedCosts,
            decimal totalProvedReservesBOE,
            decimal productionBOE)
        {
            if (netCapitalizedCosts < 0)
                throw new InvalidAccountingDataException(nameof(netCapitalizedCosts), "Net capitalized costs cannot be negative.");

            if (totalProvedReservesBOE <= 0)
                throw new InsufficientReservesException("Total proved reserves must be greater than zero.");

            if (productionBOE < 0)
                throw new InvalidAccountingDataException(nameof(productionBOE), "Production cannot be negative.");

            if (productionBOE > totalProvedReservesBOE)
                throw new InvalidAccountingDataException(nameof(productionBOE), 
                    "Production cannot exceed total reserves.");

            if (netCapitalizedCosts == 0)
                return 0;

            decimal amortizationRate = productionBOE / totalProvedReservesBOE;
            return netCapitalizedCosts * amortizationRate;
        }

        /// <summary>
        /// Calculates amortization for acquisition costs (amortized over total proved reserves).
        /// </summary>
        public static decimal CalculateAcquisitionAmortization(
            decimal acquisitionCost,
            decimal totalProvedReservesBOE,
            decimal productionBOE)
        {
            return CalculateUnitsOfProduction(acquisitionCost, totalProvedReservesBOE, productionBOE);
        }

        /// <summary>
        /// Calculates amortization for exploration and development costs (amortized over proved developed reserves only).
        /// </summary>
        public static decimal CalculateExplorationDevelopmentAmortization(
            decimal explorationDevelopmentCosts,
            decimal provedDevelopedReservesBOE,
            decimal productionBOE)
        {
            return CalculateUnitsOfProduction(explorationDevelopmentCosts, provedDevelopedReservesBOE, productionBOE);
        }

        /// <summary>
        /// Converts gas reserves to BOE.
        /// </summary>
        public static decimal ConvertGasToBOE(decimal gasReservesMCF)
        {
            return gasReservesMCF / AccountingConstants.GasToOilEquivalent;
        }

        /// <summary>
        /// Converts production data to BOE.
        /// </summary>
        public static decimal ConvertProductionToBOE(ProductionData production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            return production.OilProduction + ConvertGasToBOE(production.GasProduction);
        }

        /// <summary>
        /// Converts reserves to BOE.
        /// </summary>
        public static decimal ConvertReservesToBOE(ProvedReserves reserves)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            return reserves.TotalProvedOilReserves + ConvertGasToBOE(reserves.TotalProvedGasReserves);
        }

        /// <summary>
        /// Converts proved developed reserves to BOE.
        /// </summary>
        public static decimal ConvertProvedDevelopedReservesToBOE(ProvedReserves reserves)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            return reserves.ProvedDevelopedOilReserves + 
                   ConvertGasToBOE(reserves.ProvedDevelopedGasReserves);
        }
    }
}
