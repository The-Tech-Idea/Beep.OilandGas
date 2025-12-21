using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Operational.Allocation;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Operational.Allocation
{
    /// <summary>
    /// Provides advanced allocation methods beyond basic pro-rata.
    /// </summary>
    public static class AdvancedAllocationMethods
    {
        /// <summary>
        /// Performs time-weighted allocation based on production history.
        /// </summary>
        public static AllocationResult AllocateTimeWeighted(
            decimal totalVolume,
            List<WellAllocationData> wells,
            Dictionary<string, List<(DateTime date, decimal production)>> productionHistory)
        {
            if (wells == null || wells.Count == 0)
                throw new AllocationException("Wells list cannot be null or empty.");

            if (productionHistory == null)
                throw new ArgumentNullException(nameof(productionHistory));

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = AllocationMethod.Estimated,
                TotalVolume = totalVolume
            };

            // Calculate time-weighted average production for each well
            var weightedAverages = new Dictionary<string, decimal>();

            foreach (var well in wells)
            {
                if (productionHistory.TryGetValue(well.WellId, out var history) && history.Count > 0)
                {
                    // Calculate weighted average (more recent production weighted higher)
                    decimal totalWeight = 0;
                    decimal weightedSum = 0;

                    for (int i = 0; i < history.Count; i++)
                    {
                        decimal weight = (i + 1); // More recent = higher weight
                        totalWeight += weight;
                        weightedSum += history[i].production * weight;
                    }

                    weightedAverages[well.WellId] = totalWeight > 0 ? weightedSum / totalWeight : 0;
                }
                else
                {
                    weightedAverages[well.WellId] = well.EstimatedProduction ?? 0;
                }
            }

            // Allocate based on weighted averages
            decimal totalWeighted = weightedAverages.Values.Sum();
            if (totalWeighted == 0)
                throw new AllocationException("Total weighted production cannot be zero.");

            foreach (var well in wells)
            {
                decimal weightedAvg = weightedAverages[well.WellId];
                decimal percentage = totalWeighted > 0 ? (weightedAvg / totalWeighted) * 100m : 0;
                decimal volume = totalVolume * (weightedAvg / totalWeighted);

                result.Details.Add(new AllocationDetail
                {
                    EntityId = well.WellId,
                    EntityName = well.WellId,
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = weightedAvg
                });
            }

            return result;
        }

        /// <summary>
        /// Performs decline-curve-based allocation.
        /// </summary>
        public static AllocationResult AllocateDeclineCurve(
            decimal totalVolume,
            List<WellAllocationData> wells,
            Dictionary<string, (decimal initialRate, decimal declineRate)> declineCurves)
        {
            if (wells == null || wells.Count == 0)
                throw new AllocationException("Wells list cannot be null or empty.");

            if (declineCurves == null)
                throw new ArgumentNullException(nameof(declineCurves));

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = AllocationMethod.Estimated,
                TotalVolume = totalVolume
            };

            // Calculate expected production from decline curves
            var expectedProduction = new Dictionary<string, decimal>();

            foreach (var well in wells)
            {
                if (declineCurves.TryGetValue(well.WellId, out var curve))
                {
                    // Calculate expected production using decline curve
                    // Simplified: use current rate from decline curve
                    decimal expectedRate = curve.initialRate * (decimal)Math.Exp(-(double)curve.declineRate);
                    expectedProduction[well.WellId] = expectedRate;
                }
                else
                {
                    expectedProduction[well.WellId] = well.EstimatedProduction ?? 0;
                }
            }

            // Allocate based on expected production
            decimal totalExpected = expectedProduction.Values.Sum();
            if (totalExpected == 0)
                throw new AllocationException("Total expected production cannot be zero.");

            foreach (var well in wells)
            {
                decimal expected = expectedProduction[well.WellId];
                decimal percentage = (expected / totalExpected) * 100m;
                decimal volume = totalVolume * (expected / totalExpected);

                result.Details.Add(new AllocationDetail
                {
                    EntityId = well.WellId,
                    EntityName = well.WellId,
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = expected
                });
            }

            return result;
        }

        /// <summary>
        /// Performs quality-based allocation (allocates based on API gravity or quality).
        /// </summary>
        public static AllocationResult AllocateQualityBased(
            decimal totalVolume,
            List<WellAllocationData> wells,
            Dictionary<string, decimal> qualityFactors)
        {
            if (wells == null || wells.Count == 0)
                throw new AllocationException("Wells list cannot be null or empty.");

            if (qualityFactors == null)
                throw new ArgumentNullException(nameof(qualityFactors));

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = AllocationMethod.Estimated,
                TotalVolume = totalVolume
            };

            // Calculate quality-weighted allocation
            var qualityWeights = new Dictionary<string, decimal>();

            foreach (var well in wells)
            {
                if (qualityFactors.TryGetValue(well.WellId, out var quality))
                {
                    qualityWeights[well.WellId] = quality;
                }
                else
                {
                    qualityWeights[well.WellId] = 1.0m; // Default weight
                }
            }

            decimal totalWeight = qualityWeights.Values.Sum();
            if (totalWeight == 0)
                throw new AllocationException("Total quality weight cannot be zero.");

            foreach (var well in wells)
            {
                decimal weight = qualityWeights[well.WellId];
                decimal percentage = (weight / totalWeight) * 100m;
                decimal volume = totalVolume * (weight / totalWeight);

                result.Details.Add(new AllocationDetail
                {
                    EntityId = well.WellId,
                    EntityName = well.WellId,
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = weight
                });
            }

            return result;
        }

        /// <summary>
        /// Performs multi-factor allocation combining multiple criteria.
        /// </summary>
        public static AllocationResult AllocateMultiFactor(
            decimal totalVolume,
            List<WellAllocationData> wells,
            Dictionary<string, decimal> workingInterestWeights,
            Dictionary<string, decimal> productionHistoryWeights,
            Dictionary<string, decimal> qualityWeights,
            decimal workingInterestFactor = 0.4m,
            decimal productionFactor = 0.4m,
            decimal qualityFactor = 0.2m)
        {
            if (wells == null || wells.Count == 0)
                throw new AllocationException("Wells list cannot be null or empty.");

            // Normalize factors to sum to 1.0
            decimal totalFactor = workingInterestFactor + productionFactor + qualityFactor;
            if (totalFactor != 1.0m)
            {
                workingInterestFactor /= totalFactor;
                productionFactor /= totalFactor;
                qualityFactor /= totalFactor;
            }

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = AllocationMethod.Estimated,
                TotalVolume = totalVolume
            };

            var compositeScores = new Dictionary<string, decimal>();

            foreach (var well in wells)
            {
                decimal wiScore = workingInterestWeights?.TryGetValue(well.WellId, out var wi) == true 
                    ? wi * workingInterestFactor 
                    : well.WorkingInterest * workingInterestFactor;

                decimal prodScore = productionHistoryWeights?.TryGetValue(well.WellId, out var prod) == true 
                    ? prod * productionFactor 
                    : (well.EstimatedProduction ?? 0) * productionFactor;

                decimal qualScore = qualityWeights?.TryGetValue(well.WellId, out var qual) == true 
                    ? qual * qualityFactor 
                    : 1.0m * qualityFactor;

                compositeScores[well.WellId] = wiScore + prodScore + qualScore;
            }

            decimal totalScore = compositeScores.Values.Sum();
            if (totalScore == 0)
                throw new AllocationException("Total composite score cannot be zero.");

            foreach (var well in wells)
            {
                decimal score = compositeScores[well.WellId];
                decimal percentage = (score / totalScore) * 100m;
                decimal volume = totalVolume * (score / totalScore);

                result.Details.Add(new AllocationDetail
                {
                    EntityId = well.WellId,
                    EntityName = well.WellId,
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = score
                });
            }

            return result;
        }
    }
}

