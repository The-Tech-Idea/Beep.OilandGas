using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.FullCost
{
    /// <summary>
    /// Provides Full Cost accounting calculations per industry practice.
    /// </summary>
    public class FullCostAccounting
    {
        private readonly Dictionary<string, CostCenter> costCenters = new();
        private readonly List<ExplorationCosts> allExplorationCosts = new();
        private readonly List<DevelopmentCosts> allDevelopmentCosts = new();
        private readonly List<UnprovedProperty> allAcquisitionCosts = new();

        /// <summary>
        /// Creates or gets a cost center.
        /// </summary>
        public CostCenter GetOrCreateCostCenter(string costCenterId, string name = "")
        {
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));

            if (!costCenters.ContainsKey(costCenterId))
            {
                costCenters[costCenterId] = new CostCenter
                {
                    CostCenterId = costCenterId,
                    Name = name
                };
            }

            return costCenters[costCenterId];
        }

        /// <summary>
        /// Records exploration costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordExplorationCosts(string costCenterId, ExplorationCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            var costCenter = GetOrCreateCostCenter(costCenterId);
            costCenter.ExplorationCosts.Add(costs);
            allExplorationCosts.Add(costs);
        }

        /// <summary>
        /// Records development costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordDevelopmentCosts(string costCenterId, DevelopmentCosts costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            var costCenter = GetOrCreateCostCenter(costCenterId);
            costCenter.DevelopmentCosts.Add(costs);
            allDevelopmentCosts.Add(costs);
        }

        /// <summary>
        /// Records acquisition costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordAcquisitionCosts(string costCenterId, UnprovedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var costCenter = GetOrCreateCostCenter(costCenterId);
            costCenter.AcquisitionCosts.Add(property);
            allAcquisitionCosts.Add(property);
        }

        /// <summary>
        /// Calculates total capitalized costs for a cost center.
        /// </summary>
        public decimal CalculateTotalCapitalizedCosts(string costCenterId)
        {
            if (!costCenters.ContainsKey(costCenterId))
                return 0;

            var costCenter = costCenters[costCenterId];

            decimal acquisition = costCenter.AcquisitionCosts.Sum(p => p.AcquisitionCost);
            decimal exploration = costCenter.ExplorationCosts.Sum(c => c.TotalExplorationCosts);
            decimal development = costCenter.DevelopmentCosts.Sum(c => c.TotalDevelopmentCosts);

            return acquisition + exploration + development;
        }

        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        public decimal CalculateAmortization(string costCenterId, ProvedReserves reserves, ProductionData production)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (production == null)
                throw new ArgumentNullException(nameof(production));

            decimal totalCapitalizedCosts = CalculateTotalCapitalizedCosts(costCenterId);
            decimal accumulatedAmortization = GetAccumulatedAmortization(costCenterId);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            if (netCapitalizedCosts <= 0)
                return 0;

            // Calculate total reserves in BOE
            decimal totalReservesBOE = reserves.TotalProvedOilReserves + 
                                      (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent);

            if (totalReservesBOE <= 0)
                throw new InsufficientReservesException("Total reserves must be greater than zero for amortization.");

            // Calculate production in BOE
            decimal productionBOE = production.OilProduction + 
                                   (production.GasProduction / AccountingConstants.GasToOilEquivalent);

            // Units-of-production amortization
            decimal amortizationRate = productionBOE / totalReservesBOE;
            decimal amortization = netCapitalizedCosts * amortizationRate;

            return amortization;
        }

        /// <summary>
        /// Performs ceiling test to determine if impairment is needed.
        /// </summary>
        public CeilingTestResult PerformCeilingTest(string costCenterId, ProvedReserves reserves, decimal discountRate = 0.10m)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            decimal totalCapitalizedCosts = CalculateTotalCapitalizedCosts(costCenterId);
            decimal accumulatedAmortization = GetAccumulatedAmortization(costCenterId);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            // Calculate present value of future net revenues
            decimal futureNetRevenues = CalculateFutureNetRevenues(reserves, discountRate);

            // Ceiling is the lower of: (1) net capitalized costs, (2) present value of future net revenues
            decimal ceiling = Math.Min(netCapitalizedCosts, futureNetRevenues);

            bool impairmentNeeded = netCapitalizedCosts > ceiling;
            decimal impairmentAmount = impairmentNeeded ? netCapitalizedCosts - ceiling : 0;

            return new CeilingTestResult
            {
                CostCenterId = costCenterId,
                NetCapitalizedCosts = netCapitalizedCosts,
                PresentValueOfFutureNetRevenues = futureNetRevenues,
                Ceiling = ceiling,
                ImpairmentNeeded = impairmentNeeded,
                ImpairmentAmount = impairmentAmount
            };
        }

        /// <summary>
        /// Calculates present value of future net revenues.
        /// </summary>
        private decimal CalculateFutureNetRevenues(ProvedReserves reserves, decimal discountRate)
        {
            // Simplified calculation - full implementation would use production forecast
            decimal oilRevenue = reserves.TotalProvedOilReserves * reserves.OilPrice;
            decimal gasRevenue = (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent) * reserves.GasPrice;
            decimal totalRevenue = oilRevenue + gasRevenue;

            // Apply discount (simplified - assumes immediate production)
            // Full implementation would discount over production period
            decimal presentValue = totalRevenue / (1 + discountRate);

            return presentValue;
        }

        /// <summary>
        /// Gets accumulated amortization for a cost center.
        /// </summary>
        public decimal GetAccumulatedAmortization(string costCenterId)
        {
            if (!costCenters.ContainsKey(costCenterId))
                return 0;

            return costCenters[costCenterId].AccumulatedAmortization;
        }

        /// <summary>
        /// Records amortization for a cost center.
        /// </summary>
        public void RecordAmortization(string costCenterId, decimal amortizationAmount)
        {
            var costCenter = GetOrCreateCostCenter(costCenterId);
            costCenter.AccumulatedAmortization += amortizationAmount;
        }

        /// <summary>
        /// Gets all cost centers.
        /// </summary>
        public IEnumerable<CostCenter> GetCostCenters()
        {
            return costCenters.Values;
        }
    }

    /// <summary>
    /// Represents a cost center for Full Cost accounting.
    /// </summary>
    public class CostCenter
    {
        /// <summary>
        /// Gets or sets the cost center identifier.
        /// </summary>
        public string CostCenterId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cost center name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the acquisition costs.
        /// </summary>
        public List<UnprovedProperty> AcquisitionCosts { get; set; } = new();

        /// <summary>
        /// Gets or sets the exploration costs.
        /// </summary>
        public List<ExplorationCosts> ExplorationCosts { get; set; } = new();

        /// <summary>
        /// Gets or sets the development costs.
        /// </summary>
        public List<DevelopmentCosts> DevelopmentCosts { get; set; } = new();

        /// <summary>
        /// Gets or sets the accumulated amortization.
        /// </summary>
        public decimal AccumulatedAmortization { get; set; }
    }

    /// <summary>
    /// Represents the result of a ceiling test.
    /// </summary>
    public class CeilingTestResult
    {
        /// <summary>
        /// Gets or sets the cost center identifier.
        /// </summary>
        public string CostCenterId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the net capitalized costs.
        /// </summary>
        public decimal NetCapitalizedCosts { get; set; }

        /// <summary>
        /// Gets or sets the present value of future net revenues.
        /// </summary>
        public decimal PresentValueOfFutureNetRevenues { get; set; }

        /// <summary>
        /// Gets or sets the ceiling amount.
        /// </summary>
        public decimal Ceiling { get; set; }

        /// <summary>
        /// Gets or sets whether impairment is needed.
        /// </summary>
        public bool ImpairmentNeeded { get; set; }

        /// <summary>
        /// Gets or sets the impairment amount.
        /// </summary>
        public decimal ImpairmentAmount { get; set; }
    }
}

