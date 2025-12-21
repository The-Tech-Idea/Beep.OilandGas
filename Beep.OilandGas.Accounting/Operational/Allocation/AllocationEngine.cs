using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Operational.Allocation;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Operational.Allocation
{
    /// <summary>
    /// Provides allocation calculations for production volumes.
    /// </summary>
    public static class AllocationEngine
    {
        /// <summary>
        /// Allocates volume back to wells.
        /// </summary>
        public static AllocationResult AllocateToWells(
            decimal totalVolume,
            List<WellAllocationData> wells,
            AllocationMethod method)
        {
            if (wells == null || wells.Count == 0)
                throw new AllocationException("Wells list cannot be null or empty.");

            if (totalVolume < 0)
                throw new AllocationException("Total volume cannot be negative.");

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = method,
                TotalVolume = totalVolume
            };

            switch (method)
            {
                case AllocationMethod.Equal:
                    result.Details = AllocateEqual(wells, totalVolume, w => w.WellId, w => w.WellId);
                    break;

                case AllocationMethod.ProRataWorkingInterest:
                    result.Details = AllocateProRata(wells, totalVolume, w => w.WorkingInterest, w => w.WellId, w => w.WellId);
                    break;

                case AllocationMethod.ProRataNetRevenueInterest:
                    result.Details = AllocateProRata(wells, totalVolume, w => w.NetRevenueInterest, w => w.WellId, w => w.WellId);
                    break;

                case AllocationMethod.Measured:
                    result.Details = AllocateMeasured(wells, totalVolume, w => w.MeasuredProduction, w => w.WellId, w => w.WellId);
                    break;

                case AllocationMethod.Estimated:
                    result.Details = AllocateMeasured(wells, totalVolume, w => w.EstimatedProduction, w => w.WellId, w => w.WellId);
                    break;

                default:
                    throw new AllocationException($"Unsupported allocation method: {method}");
            }

            return result;
        }

        /// <summary>
        /// Allocates volume to leases.
        /// </summary>
        public static AllocationResult AllocateToLeases(
            decimal totalVolume,
            List<LeaseAllocationData> leases,
            AllocationMethod method)
        {
            if (leases == null || leases.Count == 0)
                throw new AllocationException("Leases list cannot be null or empty.");

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = method,
                TotalVolume = totalVolume
            };

            switch (method)
            {
                case AllocationMethod.Equal:
                    result.Details = AllocateEqual(leases, totalVolume, l => l.LeaseId, l => l.LeaseId);
                    break;

                case AllocationMethod.ProRataWorkingInterest:
                    result.Details = AllocateProRata(leases, totalVolume, l => l.WorkingInterest, l => l.LeaseId, l => l.LeaseId);
                    break;

                case AllocationMethod.ProRataNetRevenueInterest:
                    result.Details = AllocateProRata(leases, totalVolume, l => l.NetRevenueInterest, l => l.LeaseId, l => l.LeaseId);
                    break;

                case AllocationMethod.Measured:
                    result.Details = AllocateMeasured(leases, totalVolume, l => l.MeasuredProduction, l => l.LeaseId, l => l.LeaseId);
                    break;

                default:
                    throw new AllocationException($"Unsupported allocation method: {method}");
            }

            return result;
        }

        /// <summary>
        /// Allocates volume to tracts within a unit.
        /// </summary>
        public static AllocationResult AllocateToTracts(
            decimal totalVolume,
            List<TractAllocationData> tracts,
            AllocationMethod method)
        {
            if (tracts == null || tracts.Count == 0)
                throw new AllocationException("Tracts list cannot be null or empty.");

            var result = new AllocationResult
            {
                AllocationId = Guid.NewGuid().ToString(),
                AllocationDate = DateTime.Now,
                Method = method,
                TotalVolume = totalVolume
            };

            switch (method)
            {
                case AllocationMethod.Equal:
                    result.Details = AllocateEqual(tracts, totalVolume, t => t.TractId, t => t.TractId);
                    break;

                case AllocationMethod.ProRataWorkingInterest:
                    result.Details = AllocateProRata(tracts, totalVolume, t => t.WorkingInterest, t => t.TractId, t => t.TractId);
                    break;

                case AllocationMethod.ProRataNetRevenueInterest:
                    result.Details = AllocateProRata(tracts, totalVolume, t => t.NetRevenueInterest, t => t.TractId, t => t.TractId);
                    break;

                case AllocationMethod.Measured:
                    // For tracts, use participation percentage
                    result.Details = AllocateProRata(tracts, totalVolume, t => t.TractParticipation, t => t.TractId, t => t.TractId);
                    break;

                default:
                    throw new AllocationException($"Unsupported allocation method: {method}");
            }

            return result;
        }

        /// <summary>
        /// Performs equal allocation.
        /// </summary>
        private static List<AllocationDetail> AllocateEqual<T>(
            List<T> entities,
            decimal totalVolume,
            Func<T, string> getId,
            Func<T, string> getName)
        {
            if (entities.Count == 0)
                return new List<AllocationDetail>();

            decimal volumePerEntity = totalVolume / entities.Count;
            decimal percentage = 100m / entities.Count;

            return entities.Select(e => new AllocationDetail
            {
                EntityId = getId(e),
                EntityName = getName(e),
                AllocatedVolume = volumePerEntity,
                AllocationPercentage = percentage,
                AllocationBasis = 1.0m / entities.Count
            }).ToList();
        }

        /// <summary>
        /// Performs pro-rata allocation based on interest.
        /// </summary>
        private static List<AllocationDetail> AllocateProRata<T>(
            List<T> entities,
            decimal totalVolume,
            Func<T, decimal> getInterest,
            Func<T, string> getId,
            Func<T, string> getName)
        {
            if (entities.Count == 0)
                return new List<AllocationDetail>();

            decimal totalInterest = entities.Sum(e => getInterest(e));
            if (totalInterest == 0)
                throw new AllocationException("Total interest cannot be zero for pro-rata allocation.");

            return entities.Select(e =>
            {
                decimal interest = getInterest(e);
                decimal percentage = (interest / totalInterest) * 100m;
                decimal volume = totalVolume * (interest / totalInterest);

                return new AllocationDetail
                {
                    EntityId = getId(e),
                    EntityName = getName(e),
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = interest
                };
            }).ToList();
        }

        /// <summary>
        /// Performs measured allocation based on test data.
        /// </summary>
        private static List<AllocationDetail> AllocateMeasured<T>(
            List<T> entities,
            decimal totalVolume,
            Func<T, decimal?> getMeasured,
            Func<T, string> getId,
            Func<T, string> getName)
        {
            if (entities.Count == 0)
                return new List<AllocationDetail>();

            var entitiesWithMeasurements = entities
                .Where(e => getMeasured(e).HasValue)
                .ToList();

            if (entitiesWithMeasurements.Count == 0)
                throw new AllocationException("No measured production data available for measured allocation.");

            decimal totalMeasured = entitiesWithMeasurements.Sum(e => getMeasured(e)!.Value);
            if (totalMeasured == 0)
                throw new AllocationException("Total measured production cannot be zero.");

            return entitiesWithMeasurements.Select(e =>
            {
                decimal measured = getMeasured(e)!.Value;
                decimal percentage = (measured / totalMeasured) * 100m;
                decimal volume = totalVolume * (measured / totalMeasured);

                return new AllocationDetail
                {
                    EntityId = getId(e),
                    EntityName = getName(e),
                    AllocatedVolume = volume,
                    AllocationPercentage = percentage,
                    AllocationBasis = measured
                };
            }).ToList();
        }
    }
}

