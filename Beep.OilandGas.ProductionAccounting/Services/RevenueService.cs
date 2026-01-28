using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Revenue Service - Recognizes revenue per ASC 606.
    /// Allocates revenue to interest owners based on their ownership percentages.
    /// </summary>
    public class RevenueService : IRevenueService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILeaseEconomicInterestService _leaseEconomicInterestService;
        private readonly ILogger<RevenueService> _logger;
        private const string ConnectionName = "PPDM39";

        public RevenueService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RevenueService> logger = null,
            ILeaseEconomicInterestService leaseEconomicInterestService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _leaseEconomicInterestService = leaseEconomicInterestService;
            _logger = logger;
        }

        /// <summary>
        /// Recognizes revenue from an allocation detail.
        /// Allocates revenue to interest owners based on their ownership percentages.
        /// </summary>
        public async Task<REVENUE_ALLOCATION> RecognizeRevenueAsync(
            ALLOCATION_DETAIL allocation,
            string userId,
            string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Recognizing revenue for allocation detail {DetailId}", allocation.ALLOCATION_DETAIL_ID);

            try
            {
                if (allocation.ALLOCATED_VOLUME == null || allocation.ALLOCATED_VOLUME <= 0)
                    throw new AccountingException("Allocated volume must be positive.");

                if (string.IsNullOrWhiteSpace(allocation.ALLOCATION_RESULT_ID))
                    throw new AccountingException("Allocation detail is missing ALLOCATION_RESULT_ID.");

                var ALLOCATION_RESULT = await GetAllocationResultAsync(allocation.ALLOCATION_RESULT_ID, cn);
                if (ALLOCATION_RESULT == null)
                    throw new AccountingException($"Allocation result not found: {allocation.ALLOCATION_RESULT_ID}");

                var RUN_TICKET = await GetRunTicketAsync(ALLOCATION_RESULT.ALLOCATION_REQUEST_ID, cn);
                if (RUN_TICKET == null)
                    throw new AccountingException($"Run ticket not found for allocation request {ALLOCATION_RESULT.ALLOCATION_REQUEST_ID}");

                if (_leaseEconomicInterestService != null && !string.IsNullOrWhiteSpace(RUN_TICKET.LEASE_ID))
                {
                    var interestsValid = await _leaseEconomicInterestService.ValidateEconomicInterestsAsync(
                        RUN_TICKET.LEASE_ID,
                        RUN_TICKET.TICKET_DATE_TIME,
                        cn);
                    if (!interestsValid)
                        throw new AccountingException($"Economic interest validation failed for lease {RUN_TICKET.LEASE_ID}");
                }

                var allocatedVolume = allocation.ALLOCATED_VOLUME.Value;
                var priceDate = RUN_TICKET.TICKET_DATE_TIME ?? DateTime.UtcNow;

                decimal commodityPrice = RUN_TICKET.PRICE_PER_BARREL > 0
                    ? RUN_TICKET.PRICE_PER_BARREL.Value
                    : await GetCommodityPriceAsync("OIL", priceDate, cn);

                var totalVolume = ALLOCATION_RESULT.ALLOCATED_VOLUME ?? ALLOCATION_RESULT.TOTAL_VOLUME ?? allocatedVolume;
                var grossRevenueTotal = totalVolume * commodityPrice;

                var totalRoyalty = await GetTotalRoyaltyAsync(ALLOCATION_RESULT.ALLOCATION_RESULT_ID, cn);
                var netRevenueTotal = grossRevenueTotal - totalRoyalty;
                if (netRevenueTotal < 0m)
                    netRevenueTotal = 0m;

                _logger?.LogDebug(
                    "Revenue calculation for {Entity}: {Volume} x ${Price} x {Interest}% = ${Revenue}",
                    allocation.ENTITY_NAME, allocatedVolume, commodityPrice,
                    allocation.ALLOCATION_PERCENTAGE ?? 100, grossRevenueTotal);

                var revenueTransaction = await GetOrCreateRevenueTransactionAsync(
                    ALLOCATION_RESULT,
                    RUN_TICKET,
                    priceDate,
                    commodityPrice,
                    grossRevenueTotal,
                    netRevenueTotal,
                    userId,
                    cn);

                var allocationRatio = allocation.ALLOCATION_PERCENTAGE.HasValue && allocation.ALLOCATION_PERCENTAGE.Value > 0
                    ? allocation.ALLOCATION_PERCENTAGE.Value / 100m
                    : (totalVolume > 0m ? allocatedVolume / totalVolume : 1m);

                var allocatedAmount = netRevenueTotal * allocationRatio;
                var allocationPercentage = allocation.ALLOCATION_PERCENTAGE ?? (allocationRatio * 100m);

                // Create revenue allocation record
                var revenueAllocation = new REVENUE_ALLOCATION
                {
                    REVENUE_ALLOCATION_ID = Guid.NewGuid().ToString(),
                    REVENUE_TRANSACTION_ID = revenueTransaction.REVENUE_TRANSACTION_ID,
                    AFE_ID = revenueTransaction.AFE_ID,
                    INTEREST_OWNER_BA_ID = allocation.ENTITY_ID,
                    INTEREST_PERCENTAGE = allocationPercentage,
                    ALLOCATED_AMOUNT = allocatedAmount,
                    ALLOCATION_METHOD = RevenueAllocationMethod.ProRata,
                    DESCRIPTION = $"Revenue allocation for {allocation.ENTITY_NAME}",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

                // Save to database
                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_ALLOCATION");

                await repo.InsertAsync(revenueAllocation, userId);

                _logger?.LogInformation(
                    "Revenue allocation created: {AllocationId} for entity {EntityId}",
                    revenueAllocation.REVENUE_ALLOCATION_ID, allocation.ENTITY_ID);

                return revenueAllocation;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recognizing revenue");
                throw;
            }
        }

        /// <summary>
        /// Validates a revenue allocation record.
        /// Checks: amount is positive, interest percentage valid, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(REVENUE_ALLOCATION allocation, string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Validating revenue allocation {AllocationId}", allocation.REVENUE_ALLOCATION_ID);

            try
            {
                // Validation 1: Interest percentage should be between 0 and 100
                if (allocation.INTEREST_PERCENTAGE.HasValue)
                {
                    if (allocation.INTEREST_PERCENTAGE < 0 || allocation.INTEREST_PERCENTAGE > 100)
                    {
                        _logger?.LogWarning(
                            "Revenue allocation {AllocationId}: Invalid interest percentage {Percentage}",
                            allocation.REVENUE_ALLOCATION_ID, allocation.INTEREST_PERCENTAGE);
                        throw new AccountingException(
                            $"Interest percentage must be between 0 and 100: {allocation.INTEREST_PERCENTAGE}");
                    }
                }

                // Validation 2: Allocated amount should be non-negative
                if (allocation.ALLOCATED_AMOUNT.HasValue && allocation.ALLOCATED_AMOUNT < 0)
                {
                    _logger?.LogWarning(
                        "Revenue allocation {AllocationId}: Negative allocated amount {Amount}",
                        allocation.REVENUE_ALLOCATION_ID, allocation.ALLOCATED_AMOUNT);
                    throw new AccountingException(
                        $"Allocated amount cannot be negative: {allocation.ALLOCATED_AMOUNT}");
                }

                // Validation 3: Interest owner ID should be set
                if (string.IsNullOrWhiteSpace(allocation.INTEREST_OWNER_BA_ID))
                {
                    _logger?.LogWarning("Revenue allocation {AllocationId}: Interest owner ID is required",
                        allocation.REVENUE_ALLOCATION_ID);
                    throw new AccountingException("Interest owner ID is required");
                }

                // Validation 4: Revenue transaction should be linked
                if (string.IsNullOrWhiteSpace(allocation.REVENUE_TRANSACTION_ID))
                {
                    _logger?.LogWarning("Revenue allocation {AllocationId}: Revenue transaction ID is required",
                        allocation.REVENUE_ALLOCATION_ID);
                    throw new AccountingException("Revenue transaction ID is required");
                }

                _logger?.LogInformation("Revenue allocation {AllocationId} validation passed",
                    allocation.REVENUE_ALLOCATION_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Revenue allocation validation failed");
                throw;
            }
        }

        /// <summary>
        /// Gets current commodity price for revenue calculation from PRICE_INDEX.
        /// </summary>
        private async Task<decimal> GetCommodityPriceAsync(string commodity, DateTime asOfDate, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRICE_INDEX");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PRICE_INDEX);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PRICE_INDEX");

                // Get latest oil price from PRICE_INDEX
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COMMODITY_TYPE", Operator = "=", FilterValue = commodity },
                    new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var prices = await repo.GetAsync(filters);
                var priceList = prices?.Cast<PRICE_INDEX>().OrderByDescending(p => p.PRICE_DATE).ToList() 
                    ?? new List<PRICE_INDEX>();

                if (priceList.Any())
                {
                    decimal price = priceList.First().PRICE_VALUE ?? 75.00m;
                    _logger?.LogDebug("Retrieved commodity price: ${Price}/BBL", price);
                    return price;
                }

                throw new AccountingException($"No price index found for commodity {commodity} as of {asOfDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving commodity price");
                throw;
            }
        }

        private async Task<decimal> GetTotalRoyaltyAsync(string allocationResultId, string cn)
        {
            if (string.IsNullOrWhiteSpace(allocationResultId))
                return 0m;

            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_CALCULATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_CALCULATION");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationResultId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var royalties = results?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();

            return royalties.Sum(r => r.ROYALTY_AMOUNT ?? 0m);
        }

        private async Task<REVENUE_TRANSACTION> GetOrCreateRevenueTransactionAsync(
            ALLOCATION_RESULT ALLOCATION_RESULT,
            RUN_TICKET RUN_TICKET,
            DateTime priceDate,
            decimal commodityPrice,
            decimal grossRevenueTotal,
            decimal netRevenueTotal,
            string userId,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(REVENUE_TRANSACTION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "REVENUE_TRANSACTION");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = ALLOCATION_RESULT.ALLOCATION_RESULT_ID },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var existing = await repo.GetAsync(filters);
            var existingTransaction = existing?.Cast<REVENUE_TRANSACTION>().FirstOrDefault();
            if (existingTransaction != null)
                return existingTransaction;

            var revenueTransaction = new REVENUE_TRANSACTION
            {
                REVENUE_TRANSACTION_ID = Guid.NewGuid().ToString(),
                ALLOCATION_RESULT_ID = ALLOCATION_RESULT.ALLOCATION_RESULT_ID,
                PROPERTY_ID = RUN_TICKET.LEASE_ID,
                WELL_ID = RUN_TICKET.WELL_ID,
                AFE_ID = GetAfeId(ALLOCATION_RESULT, RUN_TICKET),
                TRANSACTION_DATE = priceDate,
                REVENUE_TYPE = "OIL",
                GROSS_REVENUE = grossRevenueTotal,
                NET_REVENUE = netRevenueTotal,
                OIL_VOLUME = ALLOCATION_RESULT.ALLOCATED_VOLUME ?? ALLOCATION_RESULT.TOTAL_VOLUME ?? 0m,
                OIL_PRICE = commodityPrice,
                CURRENCY_CODE = "USD",
                DESCRIPTION = $"Revenue from allocation {ALLOCATION_RESULT.ALLOCATION_RESULT_ID}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            await repo.InsertAsync(revenueTransaction, userId);
            return revenueTransaction;
        }

        private async Task<ALLOCATION_RESULT?> GetAllocationResultAsync(string allocationResultId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_RESULT");

            var result = await repo.GetByIdAsync(allocationResultId);
            return result as ALLOCATION_RESULT;
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_REQUEST_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private static string GetAfeId(ALLOCATION_RESULT ALLOCATION_RESULT, RUN_TICKET RUN_TICKET)
        {
            if (!string.IsNullOrWhiteSpace(ALLOCATION_RESULT?.AFE_ID))
                return ALLOCATION_RESULT.AFE_ID;
            if (!string.IsNullOrWhiteSpace(RUN_TICKET?.AFE_ID))
                return RUN_TICKET.AFE_ID;
            return null;
        }
    }

    /// <summary>
    /// Revenue status constants per ASC 606.
    /// </summary>
    public static class RevenueStatus
    {
        public const string Deferred = "DEFERRED";
        public const string Recognized = "RECOGNIZED";
        public const string Billed = "BILLED";
        public const string Collected = "COLLECTED";
    }

    /// <summary>
    /// Allocation method constants for revenue allocation.
    /// </summary>
    public static class RevenueAllocationMethod
    {
        public const string ProRata = "PRO-RATA";
        public const string Equation = "EQUATION";
        public const string Custom = "CUSTOM";
    }
}
