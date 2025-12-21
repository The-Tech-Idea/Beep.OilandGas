using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Financial.SuccessfulEfforts
{
    /// <summary>
    /// Provides Successful Efforts accounting calculations per FASB Statement No. 19.
    /// </summary>
    public class SuccessfulEffortsAccounting
    {
        private readonly Dictionary<string, UnprovedProperty> unprovedProperties = new();
        private readonly Dictionary<string, ProvedProperty> provedProperties = new();
        private readonly Dictionary<string, List<ExplorationCosts>> explorationCosts = new();
        private readonly Dictionary<string, List<DevelopmentCosts>> developmentCosts = new();
        private readonly Dictionary<string, List<ProductionCosts>> productionCosts = new();

        /// <summary>
        /// Records acquisition of an unproved property.
        /// </summary>
        public void RecordAcquisition(UnprovedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(property.PropertyId))
                throw new InvalidAccountingDataException(nameof(property.PropertyId), "Property ID cannot be null or empty.");

            if (property.AcquisitionCost < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(property.AcquisitionCost), "Acquisition cost cannot be negative.");

            unprovedProperties[property.PropertyId] = property;
        }

        /// <summary>
        /// Records exploration costs. G&G costs are expensed, drilling costs are capitalized.
        /// </summary>
        public void RecordExplorationCosts(ExplorationCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            if (!explorationCosts.ContainsKey(costs.PropertyId))
                explorationCosts[costs.PropertyId] = new List<ExplorationCosts>();

            explorationCosts[costs.PropertyId].Add(costs);
        }

        /// <summary>
        /// Records development costs. All development costs are capitalized.
        /// </summary>
        public void RecordDevelopmentCosts(DevelopmentCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            if (!developmentCosts.ContainsKey(costs.PropertyId))
                developmentCosts[costs.PropertyId] = new List<DevelopmentCosts>();

            developmentCosts[costs.PropertyId].Add(costs);
        }

        /// <summary>
        /// Records production costs (lifting costs). These are expensed as incurred.
        /// </summary>
        public void RecordProductionCosts(ProductionCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            if (!productionCosts.ContainsKey(costs.PropertyId))
                productionCosts[costs.PropertyId] = new List<ProductionCosts>();

            productionCosts[costs.PropertyId].Add(costs);
        }

        /// <summary>
        /// Records a dry hole expense for an exploratory well.
        /// </summary>
        public void RecordDryHole(ExplorationCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            costs.IsDryHole = true;
            costs.FoundProvedReserves = false;

            // All costs of dry hole are expensed
            RecordExplorationCosts(costs);
        }

        /// <summary>
        /// Classifies an unproved property as proved when reserves are discovered.
        /// </summary>
        public void ClassifyAsProved(UnprovedProperty unprovedProperty, ProvedReserves reserves)
        {
            if (unprovedProperty == null)
                throw new ArgumentNullException(nameof(unprovedProperty));

            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (!unprovedProperties.ContainsKey(unprovedProperty.PropertyId))
                throw new InvalidAccountingDataException(nameof(unprovedProperty), "Property not found in unproved properties.");

            // Create proved property
            var provedProperty = new ProvedProperty
            {
                PropertyId = unprovedProperty.PropertyId,
                AcquisitionCost = unprovedProperty.AcquisitionCost,
                ExplorationCosts = GetTotalExplorationCosts(unprovedProperty.PropertyId),
                DevelopmentCosts = GetTotalDevelopmentCosts(unprovedProperty.PropertyId),
                Reserves = reserves,
                ProvedDate = DateTime.Now
            };

            provedProperties[unprovedProperty.PropertyId] = provedProperty;
            unprovedProperty.IsProved = true;
            unprovedProperty.ProvedDate = DateTime.Now;
        }

        /// <summary>
        /// Records impairment of an unproved property.
        /// </summary>
        public void RecordImpairment(string propertyId, decimal impairmentAmount)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            if (!unprovedProperties.ContainsKey(propertyId))
                throw new InvalidAccountingDataException(nameof(propertyId), "Property not found.");

            var property = unprovedProperties[propertyId];
            property.AccumulatedImpairment += impairmentAmount;

            // Impairment cannot exceed acquisition cost
            if (property.AccumulatedImpairment > property.AcquisitionCost)
                property.AccumulatedImpairment = property.AcquisitionCost;
        }

        /// <summary>
        /// Calculates amortization for a proved property using units-of-production method.
        /// </summary>
        public decimal CalculateAmortization(ProvedProperty property, ProvedReserves reserves, ProductionData production)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (production == null)
                throw new ArgumentNullException(nameof(production));

            // Calculate net capitalized costs
            decimal netCapitalizedCosts = property.AcquisitionCost + 
                                         property.ExplorationCosts + 
                                         property.DevelopmentCosts - 
                                         property.AccumulatedAmortization;

            if (netCapitalizedCosts <= 0)
                return 0;

            // Calculate total reserves in BOE
            decimal totalReservesBOE = reserves.TotalProvedOilReserves + 
                                      (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent);

            if (totalReservesBOE <= 0)
                return 0;

            // Calculate production in BOE
            decimal productionBOE = production.OilProduction + 
                                   (production.GasProduction / AccountingConstants.GasToOilEquivalent);

            // Units-of-production amortization
            decimal amortizationRate = productionBOE / totalReservesBOE;
            decimal amortization = netCapitalizedCosts * amortizationRate;

            return amortization;
        }

        /// <summary>
        /// Calculates interest capitalization for qualifying assets.
        /// </summary>
        public decimal CalculateInterestCapitalization(InterestCapitalizationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.InterestRate < AccountingConstants.MinInterestRate || 
                data.InterestRate > AccountingConstants.MaxInterestRate)
                throw new InvalidAccountingDataException(nameof(data.InterestRate), 
                    "Interest rate must be between 0 and 1.");

            // Calculate interest to capitalize
            decimal interestToCapitalize = data.AverageAccumulatedExpenditures * 
                                          data.InterestRate * 
                                          (data.CapitalizationPeriodMonths / 12.0m);

            // Cannot exceed actual interest costs
            return Math.Min(interestToCapitalize, data.ActualInterestCosts);
        }

        /// <summary>
        /// Gets total exploration costs for a property.
        /// </summary>
        public decimal GetTotalExplorationCosts(string propertyId)
        {
            if (!explorationCosts.ContainsKey(propertyId))
                return 0;

            // Only capitalize costs of successful wells (not dry holes)
            return explorationCosts[propertyId]
                .Where(c => !c.IsDryHole && c.FoundProvedReserves)
                .Sum(c => c.ExploratoryDrillingCosts + c.ExploratoryWellEquipment);
        }

        /// <summary>
        /// Gets total development costs for a property.
        /// </summary>
        public decimal GetTotalDevelopmentCosts(string propertyId)
        {
            if (!developmentCosts.ContainsKey(propertyId))
                return 0;

            return developmentCosts[propertyId].Sum(c => c.TotalDevelopmentCosts);
        }

        /// <summary>
        /// Gets total G&G costs expensed for a property.
        /// </summary>
        public decimal GetTotalGGCostsExpensed(string propertyId)
        {
            if (!explorationCosts.ContainsKey(propertyId))
                return 0;

            // G&G costs are always expensed
            return explorationCosts[propertyId].Sum(c => c.GeologicalGeophysicalCosts);
        }

        /// <summary>
        /// Gets total dry hole costs expensed for a property.
        /// </summary>
        public decimal GetTotalDryHoleCostsExpensed(string propertyId)
        {
            if (!explorationCosts.ContainsKey(propertyId))
                return 0;

            return explorationCosts[propertyId]
                .Where(c => c.IsDryHole)
                .Sum(c => c.TotalExplorationCosts);
        }

        /// <summary>
        /// Gets all unproved properties.
        /// </summary>
        public IEnumerable<UnprovedProperty> GetUnprovedProperties()
        {
            return unprovedProperties.Values;
        }

        /// <summary>
        /// Gets all proved properties.
        /// </summary>
        public IEnumerable<ProvedProperty> GetProvedProperties()
        {
            return provedProperties.Values;
        }
    }
}

