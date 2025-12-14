using System;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Validation
{
    /// <summary>
    /// Validates accounting data.
    /// </summary>
    public static class AccountingDataValidator
    {
        /// <summary>
        /// Validates unproved property.
        /// </summary>
        public static void ValidateUnprovedProperty(UnprovedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(property.PropertyId))
                throw new InvalidAccountingDataException(nameof(property.PropertyId), "Property ID cannot be null or empty.");

            if (property.AcquisitionCost < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(property.AcquisitionCost), "Acquisition cost cannot be negative.");

            if (property.AcquisitionCost > AccountingConstants.MaxCost)
                throw new InvalidAccountingDataException(nameof(property.AcquisitionCost), "Acquisition cost exceeds maximum allowed.");

            ValidateInterest(property.WorkingInterest, nameof(property.WorkingInterest));
            ValidateInterest(property.NetRevenueInterest, nameof(property.NetRevenueInterest));
        }

        /// <summary>
        /// Validates proved reserves.
        /// </summary>
        public static void ValidateProvedReserves(ProvedReserves reserves)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (reserves.ProvedDevelopedOilReserves < AccountingConstants.MinReserves)
                throw new InvalidAccountingDataException(nameof(reserves.ProvedDevelopedOilReserves), 
                    "Proved developed oil reserves cannot be negative.");

            if (reserves.ProvedUndevelopedOilReserves < AccountingConstants.MinReserves)
                throw new InvalidAccountingDataException(nameof(reserves.ProvedUndevelopedOilReserves), 
                    "Proved undeveloped oil reserves cannot be negative.");

            if (reserves.ProvedDevelopedGasReserves < AccountingConstants.MinReserves)
                throw new InvalidAccountingDataException(nameof(reserves.ProvedDevelopedGasReserves), 
                    "Proved developed gas reserves cannot be negative.");

            if (reserves.ProvedUndevelopedGasReserves < AccountingConstants.MinReserves)
                throw new InvalidAccountingDataException(nameof(reserves.ProvedUndevelopedGasReserves), 
                    "Proved undeveloped gas reserves cannot be negative.");

            if (reserves.OilPrice < 0)
                throw new InvalidAccountingDataException(nameof(reserves.OilPrice), "Oil price cannot be negative.");

            if (reserves.GasPrice < 0)
                throw new InvalidAccountingDataException(nameof(reserves.GasPrice), "Gas price cannot be negative.");
        }

        /// <summary>
        /// Validates production data.
        /// </summary>
        public static void ValidateProductionData(ProductionData production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            if (production.OilProduction < AccountingConstants.MinProduction)
                throw new InvalidAccountingDataException(nameof(production.OilProduction), 
                    "Oil production cannot be negative.");

            if (production.GasProduction < AccountingConstants.MinProduction)
                throw new InvalidAccountingDataException(nameof(production.GasProduction), 
                    "Gas production cannot be negative.");
        }

        /// <summary>
        /// Validates exploration costs.
        /// </summary>
        public static void ValidateExplorationCosts(ExplorationCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            if (costs.GeologicalGeophysicalCosts < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.GeologicalGeophysicalCosts), 
                    "G&G costs cannot be negative.");

            if (costs.ExploratoryDrillingCosts < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.ExploratoryDrillingCosts), 
                    "Exploratory drilling costs cannot be negative.");

            if (costs.ExploratoryWellEquipment < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.ExploratoryWellEquipment), 
                    "Exploratory well equipment costs cannot be negative.");
        }

        /// <summary>
        /// Validates development costs.
        /// </summary>
        public static void ValidateDevelopmentCosts(DevelopmentCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            if (costs.DevelopmentWellDrillingCosts < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.DevelopmentWellDrillingCosts), 
                    "Development well drilling costs cannot be negative.");

            if (costs.DevelopmentWellEquipment < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.DevelopmentWellEquipment), 
                    "Development well equipment costs cannot be negative.");

            if (costs.SupportEquipmentAndFacilities < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(costs.SupportEquipmentAndFacilities), 
                    "Support equipment and facilities costs cannot be negative.");
        }

        /// <summary>
        /// Validates interest percentage.
        /// </summary>
        private static void ValidateInterest(decimal interest, string parameterName)
        {
            if (interest < AccountingConstants.MinWorkingInterest || 
                interest > AccountingConstants.MaxWorkingInterest)
                throw new InvalidAccountingDataException(parameterName, 
                    $"Interest must be between {AccountingConstants.MinWorkingInterest} and {AccountingConstants.MaxWorkingInterest}.");
        }
    }
}

