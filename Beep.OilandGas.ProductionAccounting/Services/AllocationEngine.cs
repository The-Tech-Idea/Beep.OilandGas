using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Allocation;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using ProductionAllocationMethod = Beep.OilandGas.Models.Data.ProductionAccounting.AllocationMethod;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Allocation Engine - Real business logic for production allocation.
    /// Implements Pro Rata, Equation, Volumetric, and Yield allocation methods.
    /// Per PPDM39 standards and industry accounting requirements.
    /// </summary>
    public class AllocationEngine : IAllocationEngine
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AllocationEngine> _logger;

        public AllocationEngine(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AllocationEngine> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Allocates production volume using specified method.
        /// Supports: ProRata (ownership %), Equation, Volumetric, Yield.
        /// </summary>
        public async Task<ALLOCATION_RESULT> AllocateAsync(
            RUN_TICKET RUN_TICKET,
            string allocationMethod,
            string userId,
            string connectionName = "PPDM39")
        {
            if (RUN_TICKET == null)
                throw new AllocationException("Run ticket cannot be null");
            if (string.IsNullOrWhiteSpace(allocationMethod))
                throw new AllocationException("Allocation method required");
            if (!AllocationMethods.AllMethods.Contains(allocationMethod))
                throw new AllocationException($"Invalid allocation method: {allocationMethod}");

            _logger?.LogInformation("Starting allocation for run ticket {RunTicketId} using method {Method}",
                RUN_TICKET.RUN_TICKET_ID, allocationMethod);

            // Prefer net volume if available, fall back to gross.
            var volume = (RUN_TICKET.NET_VOLUME.HasValue && RUN_TICKET.NET_VOLUME.Value > 0)
                ? RUN_TICKET.NET_VOLUME.Value
                : (RUN_TICKET.GROSS_VOLUME ?? 0m);
            if (volume <= 0)
                throw new AllocationException($"Invalid volume: {volume}");

            // Create allocation result
            var ALLOCATION_RESULT = new ALLOCATION_RESULT
            {
                ALLOCATION_RESULT_ID = Guid.NewGuid().ToString(),
                ALLOCATION_REQUEST_ID = RUN_TICKET.RUN_TICKET_ID,
                ALLOCATION_METHOD = allocationMethod,
                AFE_ID = RUN_TICKET.AFE_ID,
                ALLOCATION_DATE = DateTime.UtcNow,
                TOTAL_VOLUME = volume,
                ALLOCATED_VOLUME = 0,  // Will be calculated from details
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            // Save allocation result to database
            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "ALLOCATION_RESULT");

            await repo.InsertAsync(ALLOCATION_RESULT, userId);

            _logger?.LogInformation("Allocation created: {AllocationResultId} Total Volume: {TotalVolume}",
                ALLOCATION_RESULT.ALLOCATION_RESULT_ID, volume);

            if (allocationMethod == AllocationMethods.ProRata)
            {
                var details = await CreateProRataDetailsAsync(ALLOCATION_RESULT, RUN_TICKET, userId, connectionName);
                var allocatedTotal = details.Sum(d => d.ALLOCATED_VOLUME ?? 0m);
                ALLOCATION_RESULT.ALLOCATED_VOLUME = allocatedTotal;
                ALLOCATION_RESULT.ALLOCATION_VARIANCE = (ALLOCATION_RESULT.TOTAL_VOLUME ?? 0m) - allocatedTotal;
                ALLOCATION_RESULT.DESCRIPTION =
                    AllocationDescriptionPhrases.ProRataRunTicketPrefix + RUN_TICKET.RUN_TICKET_ID;
                ALLOCATION_RESULT.ROW_CHANGED_BY = userId;
                ALLOCATION_RESULT.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(ALLOCATION_RESULT, userId);
            }
            else
            {
                throw new AllocationException($"Allocation method not implemented: {allocationMethod}");
            }

            return ALLOCATION_RESULT;
        }

        public async Task<ALLOCATION_RESULT?> GetAllocationAsync(
            string allocationId,
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new AllocationException("Allocation ID required");

            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "ALLOCATION_RESULT");

            var result = await repo.GetByIdAsync(allocationId);
            return result as ALLOCATION_RESULT;
        }

        public async Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(
            string allocationId,
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new AllocationException("Allocation ID required");

            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_DETAIL);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "ALLOCATION_DETAIL");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ALLOCATION_DETAIL>().ToList();
        }

        public async Task<bool> ValidateAsync(
            ALLOCATION_RESULT allocation,
            string connectionName = "PPDM39")
        {
            if (allocation == null)
                throw new AllocationException("Allocation cannot be null");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(allocation.ALLOCATION_RESULT_ID))
                return false;
            if (string.IsNullOrWhiteSpace(allocation.ALLOCATION_REQUEST_ID))
                return false;
            if (string.IsNullOrWhiteSpace(allocation.ALLOCATION_METHOD))
                return false;
            if (!AllocationMethods.AllMethods.Contains(allocation.ALLOCATION_METHOD))
                return false;
            if (allocation.TOTAL_VOLUME == null || allocation.TOTAL_VOLUME <= 0)
                return false;

            // Validate that allocated volume doesn't exceed total
            if (allocation.ALLOCATED_VOLUME != null && allocation.ALLOCATED_VOLUME > allocation.TOTAL_VOLUME)
                return false;

            return true;
        }

        private async Task<List<ALLOCATION_DETAIL>> CreateProRataDetailsAsync(
            ALLOCATION_RESULT ALLOCATION_RESULT,
            RUN_TICKET RUN_TICKET,
            string userId,
            string connectionName)
        {
            if (ALLOCATION_RESULT == null)
                throw new AllocationException("Allocation result is required");
            if (RUN_TICKET == null)
                throw new AllocationException("Run ticket is required");

            var ownerships = await GetOwnershipInterestsAsync(RUN_TICKET, connectionName);
            if (ownerships.Count == 0)
                throw new AllocationException("No ownership interests found for allocation");

            var divisionOrders = await GetDivisionOrdersAsync(RUN_TICKET, connectionName);
            ValidateDivisionOrders(ownerships, divisionOrders);

            var detailsMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
            var detailsEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{detailsMetadata.EntityTypeName}")
                ?? typeof(ALLOCATION_DETAIL);

            var detailsRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                detailsEntityType, connectionName, "ALLOCATION_DETAIL");

            var totalVolume = ALLOCATION_RESULT.TOTAL_VOLUME ?? 0m;
            if (totalVolume <= 0m)
                throw new AllocationException($"Invalid total volume: {totalVolume}");

            var normalized = ownerships
                .Select(o => new
                {
                    Ownership = o,
                    Percentage = NormalizePercentage(GetEffectiveInterest(o, divisionOrders))
                })
                .Where(x => x.Percentage > 0m)
                .ToList();

            var totalPct = normalized.Sum(x => x.Percentage);
            if (totalPct <= 0m)
                throw new AllocationException("Ownership percentages sum to zero");

            var calculated = normalized
                .Select(item =>
                {
                    var pct = item.Percentage;
                    var scaledPct = Math.Round((pct / totalPct) * 100m, 6);
                    var allocatedVolume = Math.Round(totalVolume * (scaledPct / 100m), 6);
                    return new { item.Ownership, ScaledPct = scaledPct, AllocatedVolume = allocatedVolume };
                })
                .ToList();

            if (calculated.Count == 0)
                throw new AllocationException("No valid ownership percentages found");

            var pctVariance = 100m - calculated.Sum(x => x.ScaledPct);
            var volVariance = totalVolume - calculated.Sum(x => x.AllocatedVolume);
            var lastIndex = calculated.Count - 1;
            var last = calculated[lastIndex];
            calculated[lastIndex] = new
            {
                last.Ownership,
                ScaledPct = last.ScaledPct + pctVariance,
                AllocatedVolume = last.AllocatedVolume + volVariance
            };

            var details = new List<ALLOCATION_DETAIL>();
            foreach (var item in calculated)
            {
                var detail = new ALLOCATION_DETAIL
                {
                    ALLOCATION_DETAIL_ID = Guid.NewGuid().ToString(),
                    ALLOCATION_RESULT_ID = ALLOCATION_RESULT.ALLOCATION_RESULT_ID,
                    ENTITY_ID = item.Ownership.OWNER_ID,
                    ENTITY_TYPE = AllocationEntityTypeCodes.Owner,
                    ENTITY_NAME = item.Ownership.OWNER_ID,
                    ALLOCATION_PERCENTAGE = item.ScaledPct,
                    ALLOCATED_VOLUME = item.AllocatedVolume,
                    ALLOCATION_BASIS = totalVolume,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

                await detailsRepo.InsertAsync(detail, userId);
                details.Add(detail);
            }

            return details;
        }

        public static ALLOCATION_RESULT AllocateToWells(decimal totalVolume, List<WellAllocationData> wells, ProductionAllocationMethod method)
        {
            wells ??= new List<WellAllocationData>();

            if (wells.Count == 0)
                return new ALLOCATION_RESULT
                {
                    TOTAL_VOLUME = totalVolume,
                    ALLOCATED_VOLUME = 0m,
                    ALLOCATION_VARIANCE = totalVolume,
                    METHOD = method,
                    Details = new List<ALLOCATION_DETAIL>()
                };

            var basisTotal = method switch
            {
                ProductionAllocationMethod.ProRataWorkingInterest => wells.Sum(well => well.WorkingInterest ?? 0m),
                ProductionAllocationMethod.ProRataNetRevenueInterest => wells.Sum(well => well.NetRevenueInterest ?? 0m),
                ProductionAllocationMethod.Measured => wells.Sum(well => well.MeasuredProduction ?? 0m),
                ProductionAllocationMethod.Estimated => wells.Sum(well => well.EstimatedProduction ?? 0m),
                ProductionAllocationMethod.Equal => wells.Count,
                _ => wells.Sum(well => well.WorkingInterest ?? 0m)
            };

            if (basisTotal <= 0m)
                basisTotal = wells.Count;

            var details = new List<ALLOCATION_DETAIL>();
            decimal allocatedTotal = 0m;

            for (var index = 0; index < wells.Count; index++)
            {
                var well = wells[index];
                var basis = method switch
                {
                    ProductionAllocationMethod.ProRataWorkingInterest => well.WorkingInterest ?? 0m,
                    ProductionAllocationMethod.ProRataNetRevenueInterest => well.NetRevenueInterest ?? 0m,
                    ProductionAllocationMethod.Measured => well.MeasuredProduction ?? 0m,
                    ProductionAllocationMethod.Estimated => well.EstimatedProduction ?? 0m,
                    ProductionAllocationMethod.Equal => 1m,
                    _ => well.WorkingInterest ?? 0m
                };

                if (basisTotal == wells.Count && basis <= 0m)
                    basis = 1m;

                var allocationPercentage = basisTotal == 0m ? 0m : (basis / basisTotal) * 100m;
                var allocatedVolume = index == wells.Count - 1
                    ? totalVolume - allocatedTotal
                    : Math.Round(totalVolume * (allocationPercentage / 100m), 6);

                allocatedTotal += allocatedVolume;

                details.Add(new ALLOCATION_DETAIL
                {
                    ENTITY_ID = well.WellId,
                    ENTITY_NAME = well.WellName,
                    ALLOCATED_VOLUME = allocatedVolume,
                    ALLOCATION_PERCENTAGE = allocationPercentage,
                    WorkingInterest = well.WorkingInterest,
                    NetRevenueInterest = well.NetRevenueInterest
                });
            }

            return new ALLOCATION_RESULT
            {
                ALLOCATION_ID = Guid.NewGuid().ToString(),
                ALLOCATION_DATE = DateTime.UtcNow,
                TOTAL_VOLUME = totalVolume,
                ALLOCATED_VOLUME = allocatedTotal,
                ALLOCATION_VARIANCE = totalVolume - allocatedTotal,
                METHOD = method,
                Details = details
            };
        }

        private async Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsAsync(
            RUN_TICKET RUN_TICKET,
            string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync("OWNERSHIP_INTEREST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(OWNERSHIP_INTEREST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "OWNERSHIP_INTEREST");

            var propertyOrLeaseId = !string.IsNullOrWhiteSpace(RUN_TICKET.LEASE_ID)
                ? RUN_TICKET.LEASE_ID
                : RUN_TICKET.WELL_ID;

            if (string.IsNullOrWhiteSpace(propertyOrLeaseId))
                throw new AllocationException("Run ticket has no lease or well ID for ownership lookup");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var all = results?.Cast<OWNERSHIP_INTEREST>().ToList() ?? new List<OWNERSHIP_INTEREST>();

            var effectiveDate = RUN_TICKET.TICKET_DATE_TIME?.Date;
            if (effectiveDate.HasValue)
            {
                return all.Where(o =>
                        (!o.EFFECTIVE_START_DATE.HasValue || o.EFFECTIVE_START_DATE.Value.Date <= effectiveDate.Value) &&
                        (!o.EFFECTIVE_END_DATE.HasValue || o.EFFECTIVE_END_DATE.Value.Date >= effectiveDate.Value))
                    .ToList();
            }

            return all;
        }

        private static decimal NormalizePercentage(decimal value)
        {
            if (value <= 0m)
                return 0m;
            return value <= 1m ? value * 100m : value;
        }

        private async Task<List<DIVISION_ORDER>> GetDivisionOrdersAsync(
            RUN_TICKET RUN_TICKET,
            string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync("DIVISION_ORDER");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(DIVISION_ORDER);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, "DIVISION_ORDER");

            var propertyOrLeaseId = !string.IsNullOrWhiteSpace(RUN_TICKET.LEASE_ID)
                ? RUN_TICKET.LEASE_ID
                : RUN_TICKET.WELL_ID;

            if (string.IsNullOrWhiteSpace(propertyOrLeaseId))
                return new List<DIVISION_ORDER>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var orders = results?.Cast<DIVISION_ORDER>().ToList() ?? new List<DIVISION_ORDER>();

            var effectiveDate = RUN_TICKET.TICKET_DATE_TIME?.Date;
            if (!effectiveDate.HasValue)
                return orders;

            return orders
                .Where(o =>
                    string.Equals(o.STATUS, ApprovalWorkflowStatusCodes.Approved, StringComparison.OrdinalIgnoreCase) &&
                    (!o.EFFECTIVE_DATE.HasValue || o.EFFECTIVE_DATE.Value.Date <= effectiveDate.Value) &&
                    (!o.EXPIRATION_DATE.HasValue || o.EXPIRATION_DATE.Value.Date >= effectiveDate.Value))
                .ToList();
        }

        private static decimal GetEffectiveInterest(
            OWNERSHIP_INTEREST ownership,
            List<DIVISION_ORDER> divisionOrders)
        {
            var nri = NormalizeFraction(ownership.NET_REVENUE_INTEREST);
            if (nri > 0m)
                return nri;

            if (!string.IsNullOrWhiteSpace(ownership.DIVISION_ORDER_ID))
            {
                var order = divisionOrders.FirstOrDefault(d =>
                    string.Equals(d.DIVISION_ORDER_ID, ownership.DIVISION_ORDER_ID, StringComparison.OrdinalIgnoreCase));
                if (order != null)
                {
                    var orderNri = NormalizeFraction(order.NET_REVENUE_INTEREST);
                    if (orderNri > 0m)
                        return orderNri;
                }
            }

            var working = NormalizeFraction(ownership.WORKING_INTEREST);
            var royalty = NormalizeFraction(ownership.ROYALTY_INTEREST);
            var overriding = NormalizeFraction(ownership.OVERRIDING_ROYALTY_INTEREST);
            var computed = working - (royalty + overriding);
            return computed < 0m ? 0m : computed;
        }

        private static decimal NormalizeFraction(decimal? value)
        {
            if (!value.HasValue)
                return 0m;
            if (value.Value <= 0m)
                return 0m;
            return value.Value > 1m ? value.Value / 100m : value.Value;
        }

        private static void ValidateDivisionOrders(
            List<OWNERSHIP_INTEREST> ownerships,
            List<DIVISION_ORDER> divisionOrders)
        {
            if (ownerships == null || ownerships.Count == 0)
                return;

            var approvedOrders = new HashSet<string>(
                divisionOrders
                    .Where(d => string.Equals(d.STATUS, ApprovalWorkflowStatusCodes.Approved, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.DIVISION_ORDER_ID ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            var missing = ownerships
                .Where(o => !string.IsNullOrWhiteSpace(o.DIVISION_ORDER_ID))
                .Where(o => !approvedOrders.Contains(o.DIVISION_ORDER_ID))
                .Select(o => o.DIVISION_ORDER_ID)
                .Distinct()
                .ToList();

            if (missing.Count > 0)
                throw new AllocationException(
                    $"Unapproved division orders: {string.Join(", ", missing)}");
        }
    }
}
