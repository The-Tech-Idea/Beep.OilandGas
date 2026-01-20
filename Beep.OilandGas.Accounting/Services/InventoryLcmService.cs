using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 2 lower-of-cost-or-market adjustments for inventory.
    /// </summary>
    public class InventoryLcmService : IInventoryLcmService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService? _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<InventoryLcmService> _logger;
        private const string ConnectionName = "PPDM39";

        public InventoryLcmService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InventoryLcmService> logger,
            AccountingBasisPostingService? basisPosting = null,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basisPosting = basisPosting;
            _accountMapping = accountMapping;
        }

        public async Task<INVENTORY_ADJUSTMENT?> ApplyLowerOfCostOrMarketAsync(
            string inventoryItemId,
            DateTime valuationDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var itemRepo = await CreateRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM", cn);
            var itemObj = await itemRepo.GetByIdAsync(inventoryItemId);
            if (itemObj is not INVENTORY_ITEM item)
                throw new InvalidOperationException($"Inventory item not found: {inventoryItemId}");

            var valuation = await GetLatestValuationAsync(inventoryItemId, valuationDate, cn);
            var quantity = valuation?.QUANTITY ?? item.QUANTITY_ON_HAND ?? 0m;
            var unitCost = valuation?.UNIT_COST ?? item.UNIT_COST ?? 0m;
            var costValue = valuation?.TOTAL_VALUE ?? (quantity * unitCost);

            if (quantity <= 0m || costValue <= 0m)
            {
                _logger?.LogInformation(
                    "LCM skipped for inventory {InventoryItemId}: quantity {Quantity} cost {CostValue}",
                    inventoryItemId, quantity, costValue);
                return null;
            }

            var marketValue = await GetMarketValueAsync(inventoryItemId, valuationDate, cn);
            if (marketValue <= 0m || marketValue >= costValue)
            {
                _logger?.LogInformation(
                    "LCM not required for inventory {InventoryItemId}: cost {Cost} market {Market}",
                    inventoryItemId, costValue, marketValue);
                return null;
            }

            var newUnitCost = marketValue / quantity;
            var unitCostAdjustment = newUnitCost - unitCost;
            var writeDownAmount = costValue - marketValue;

            var adjustment = new INVENTORY_ADJUSTMENT
            {
                INVENTORY_ADJUSTMENT_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = inventoryItemId,
                ADJUSTMENT_DATE = valuationDate,
                ADJUSTMENT_TYPE = "LCM_WRITEDOWN",
                QUANTITY_ADJUSTMENT = 0m,
                UNIT_COST_ADJUSTMENT = unitCostAdjustment,
                REASON = "Lower of cost or market",
                DESCRIPTION = $"LCM write-down from {unitCost:0.####} to {newUnitCost:0.####}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var adjustmentRepo = await CreateRepoAsync<INVENTORY_ADJUSTMENT>("INVENTORY_ADJUSTMENT", cn);
            await adjustmentRepo.InsertAsync(adjustment, userId);

            item.UNIT_COST = newUnitCost;
            item.TOTAL_VALUE = marketValue;
            item.ROW_CHANGED_BY = userId;
            item.ROW_CHANGED_DATE = DateTime.UtcNow;
            await itemRepo.UpdateAsync(item, userId);

            if (_basisPosting != null)
            {
                var inventoryAccount = GetAccountId(AccountMappingKeys.Inventory, DefaultGlAccounts.Inventory);
                var writeDownAccount = GetAccountId(AccountMappingKeys.CostOfGoodsSold, DefaultGlAccounts.CostOfGoodsSold);

                await _basisPosting.PostBalancedEntryByAccountAsync(
                    writeDownAccount,
                    inventoryAccount,
                    writeDownAmount,
                    $"Inventory LCM write-down for {item.ITEM_NUMBER ?? inventoryItemId}",
                    userId,
                    cn);
            }

            _logger?.LogWarning(
                "LCM write-down recorded for inventory {InventoryItemId}: cost {Cost} market {Market}",
                inventoryItemId, costValue, marketValue);

            return adjustment;
        }

        public async Task<decimal> GetMarketValueAsync(
            string inventoryItemId,
            DateTime valuationDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));

            var itemRepo = await CreateRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM", cn);
            var itemObj = await itemRepo.GetByIdAsync(inventoryItemId);
            if (itemObj is not INVENTORY_ITEM item)
                return 0m;

            var quantity = item.QUANTITY_ON_HAND ?? 0m;
            if (quantity <= 0m)
                return 0m;

            var commodity = NormalizeCommodity(item.ITEM_TYPE) ?? NormalizeCommodity(item.ITEM_NAME);
            if (string.IsNullOrWhiteSpace(commodity))
                commodity = "OIL";

            var price = await GetPriceAsync(commodity, valuationDate, cn);
            var adjustments = GetNrvAdjustments(item.REMARK);
            var netPrice = price - adjustments.transportCost - adjustments.qualityDeduction;
            if (netPrice < 0m)
                netPrice = 0m;

            return netPrice * quantity;
        }

        private async Task<INVENTORY_VALUATION?> GetLatestValuationAsync(string inventoryItemId, DateTime valuationDate, string cn)
        {
            var repo = await CreateRepoAsync<INVENTORY_VALUATION>("INVENTORY_VALUATION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = inventoryItemId },
                new AppFilter { FieldName = "VALUATION_DATE", Operator = "<=", FilterValue = valuationDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVENTORY_VALUATION>()
                .OrderByDescending(v => v.VALUATION_DATE ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        private async Task<decimal> GetPriceAsync(string commodityType, DateTime asOfDate, string cn)
        {
            var repo = await CreateRepoAsync<PRICE_INDEX>("PRICE_INDEX", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COMMODITY_TYPE", Operator = "=", FilterValue = commodityType },
                new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var latest = results?.Cast<PRICE_INDEX>()
                .OrderByDescending(p => p.PRICE_DATE ?? DateTime.MinValue)
                .FirstOrDefault();

            return latest?.PRICE_VALUE ?? 0m;
        }

        private static string NormalizeCommodity(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var upper = input.Trim().ToUpperInvariant();
            if (upper.Contains("GAS"))
                return "GAS";
            if (upper.Contains("NGL") || upper.Contains("LIQUID"))
                return "NGL";
            if (upper.Contains("OIL") || upper.Contains("CRUDE"))
                return "OIL";

            return upper;
        }

        private static (decimal transportCost, decimal qualityDeduction) GetNrvAdjustments(string remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return (0m, 0m);

            decimal transport = 0m;
            decimal quality = 0m;
            var tokens = remark.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (parts[0].Equals("TRANSPORT_COST", StringComparison.OrdinalIgnoreCase) &&
                    decimal.TryParse(parts[1], out var t))
                    transport = t;
                if ((parts[0].Equals("QUALITY_DED", StringComparison.OrdinalIgnoreCase) ||
                     parts[0].Equals("QUALITY_ADJ", StringComparison.OrdinalIgnoreCase)) &&
                    decimal.TryParse(parts[1], out var q))
                    quality = q;
            }

            return (transport, quality);
        }

        private async Task<PPDMGenericRepository> CreateRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}

