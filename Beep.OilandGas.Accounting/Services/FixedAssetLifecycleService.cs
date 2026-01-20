using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 16 fixed asset lifecycle: capitalization, depreciation posting, and disposal.
    /// </summary>
    public class FixedAssetLifecycleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<FixedAssetLifecycleService> _logger;
        private const string ConnectionName = "PPDM39";

        public FixedAssetLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<FixedAssetLifecycleService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<EQUIPMENT> RegisterAssetAsync(
            string assetName,
            string assetType,
            decimal acquisitionCost,
            DateTime purchaseDate,
            string userId,
            string? description = null,
            string? propertyId = null,
            string? fieldId = null,
            string? offsetAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentNullException(nameof(assetName));
            if (string.IsNullOrWhiteSpace(assetType))
                throw new ArgumentNullException(nameof(assetType));
            if (acquisitionCost <= 0m)
                throw new InvalidOperationException("Acquisition cost must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var equipment = new EQUIPMENT
            {
                EQUIPMENT_ID = Guid.NewGuid().ToString(),
                EQUIPMENT_NAME = assetName,
                EQUIPMENT_TYPE = assetType,
                PURCHASE_DATE = purchaseDate,
                ACQUIRE_DATE_NEW = purchaseDate,
                COMMISSION_DATE = purchaseDate,
                EFFECTIVE_DATE = purchaseDate,
                DESCRIPTION = description,
                SOURCE = "IAS16",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var equipmentRepo = await GetRepoAsync<EQUIPMENT>("EQUIPMENT", cn);
            await equipmentRepo.InsertAsync(equipment, userId);

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = equipment.EQUIPMENT_ID,
                PROPERTY_ID = propertyId,
                FIELD_ID = fieldId,
                COST_TYPE = "ASSET_CAPITALIZATION",
                COST_CATEGORY = "FIXED_ASSET",
                AMOUNT = acquisitionCost,
                COST_DATE = purchaseDate,
                IS_CAPITALIZED = "Y",
                IS_EXPENSED = "N",
                DESCRIPTION = $"Capitalized asset {assetName}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var costRepo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await costRepo.InsertAsync(cost, userId);

            var debitAccount = GetAccountId(AccountMappingKeys.FixedAssets, DefaultGlAccounts.FixedAssets);
            var creditAccount = string.IsNullOrWhiteSpace(offsetAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : offsetAccountId;

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                acquisitionCost,
                $"Asset acquisition {assetName}",
                userId,
                cn);

            _logger?.LogInformation("Registered fixed asset {AssetId} with cost {Cost}",
                equipment.EQUIPMENT_ID, acquisitionCost);

            return equipment;
        }

        public async Task<JOURNAL_ENTRY> PostDepreciationAsync(
            string equipmentId,
            decimal depreciationAmount,
            DateTime depreciationDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentNullException(nameof(equipmentId));
            if (depreciationAmount <= 0m)
                throw new InvalidOperationException("Depreciation amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var equipment = await GetEquipmentAsync(equipmentId, cn);
            if (equipment == null)
                throw new InvalidOperationException($"Equipment not found: {equipmentId}");

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = equipmentId,
                COST_TYPE = "DEPRECIATION",
                COST_CATEGORY = "FIXED_ASSET",
                AMOUNT = depreciationAmount,
                COST_DATE = depreciationDate,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = $"Depreciation for {equipment.EQUIPMENT_NAME}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var costRepo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await costRepo.InsertAsync(cost, userId);

            var debitAccount = GetAccountId(AccountMappingKeys.DepreciationExpense, DefaultGlAccounts.DepreciationExpense);
            var creditAccount = GetAccountId(AccountMappingKeys.AccumulatedDepreciation, DefaultGlAccounts.AccumulatedDepreciation);

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = debitAccount,
                    DEBIT_AMOUNT = depreciationAmount,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Depreciation expense {equipment.EQUIPMENT_NAME}"
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = creditAccount,
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = depreciationAmount,
                    DESCRIPTION = $"Accumulated depreciation {equipment.EQUIPMENT_NAME}"
                }
            };

            var result = await _basisPosting.PostEntryAsync(
                depreciationDate,
                $"Depreciation {equipment.EQUIPMENT_NAME}",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "DEPRECIATION");
            var entry = result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");
            return entry;
        }

        public async Task<(JOURNAL_ENTRY DisposalEntry, decimal GainOrLoss)> DisposeAssetAsync(
            string equipmentId,
            decimal saleProceeds,
            DateTime disposalDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentNullException(nameof(equipmentId));
            if (saleProceeds < 0m)
                throw new InvalidOperationException("Sale proceeds cannot be negative");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var equipment = await GetEquipmentAsync(equipmentId, cn);
            if (equipment == null)
                throw new InvalidOperationException($"Equipment not found: {equipmentId}");

            var costBasis = await GetAssetCostAsync(equipmentId, cn, "ASSET_CAPITALIZATION");
            var accumulatedDepreciation = await GetAssetCostAsync(equipmentId, cn, "DEPRECIATION");
            var carryingAmount = costBasis - accumulatedDepreciation;

            var lines = new List<JOURNAL_ENTRY_LINE>();
            var cashAccount = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash);
            var assetAccount = GetAccountId(AccountMappingKeys.FixedAssets, DefaultGlAccounts.FixedAssets);
            var accumDepAccount = GetAccountId(AccountMappingKeys.AccumulatedDepreciation, DefaultGlAccounts.AccumulatedDepreciation);

            if (saleProceeds > 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = cashAccount,
                    DEBIT_AMOUNT = saleProceeds,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Asset disposal proceeds {equipment.EQUIPMENT_NAME}"
                });
            }

            if (accumulatedDepreciation > 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = accumDepAccount,
                    DEBIT_AMOUNT = accumulatedDepreciation,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Reverse accumulated depreciation {equipment.EQUIPMENT_NAME}"
                });
            }

            if (costBasis > 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = assetAccount,
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = costBasis,
                    DESCRIPTION = $"Dispose asset {equipment.EQUIPMENT_NAME}"
                });
            }

            var gainOrLoss = saleProceeds - carryingAmount;
            if (gainOrLoss > 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.GainOnDisposal, DefaultGlAccounts.Revenue),
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = gainOrLoss,
                    DESCRIPTION = $"Gain on disposal {equipment.EQUIPMENT_NAME}"
                });
            }
            else if (gainOrLoss < 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.LossOnDisposal, DefaultGlAccounts.OperatingExpense),
                    DEBIT_AMOUNT = Math.Abs(gainOrLoss),
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Loss on disposal {equipment.EQUIPMENT_NAME}"
                });
            }

            var result = await _basisPosting.PostEntryAsync(
                disposalDate,
                $"Asset disposal {equipment.EQUIPMENT_NAME}",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "DISPOSAL");
            var entry = result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");

            await RecordDisposalCostAsync(equipmentId, saleProceeds, disposalDate, userId, cn, equipment.EQUIPMENT_NAME);
            await DecommissionEquipmentAsync(equipment, disposalDate, userId, cn);

            return (entry, gainOrLoss);
        }

        public async Task<EQUIPMENT_COMPONENT> AddAssetComponentAsync(
            string equipmentId,
            decimal componentObsNo,
            string componentType,
            DateTime effectiveDate,
            string userId,
            string? remark = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentNullException(nameof(equipmentId));
            if (componentObsNo <= 0m)
                throw new InvalidOperationException("Component observation number must be positive");
            if (string.IsNullOrWhiteSpace(componentType))
                throw new ArgumentNullException(nameof(componentType));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var equipment = await GetEquipmentAsync(equipmentId, cn);
            if (equipment == null)
                throw new InvalidOperationException($"Equipment not found: {equipmentId}");

            var component = new EQUIPMENT_COMPONENT
            {
                EQUIPMENT_ID = equipmentId,
                COMPONENT_OBS_NO = componentObsNo,
                EQUIPMENT_COMPONENT_TYPE = componentType,
                EFFECTIVE_DATE = effectiveDate,
                FINANCE_ID = equipmentId,
                REMARK = remark,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<EQUIPMENT_COMPONENT>("EQUIPMENT_COMPONENT", cn);
            await repo.InsertAsync(component, userId);
            return component;
        }

        private async Task<decimal> GetAssetCostAsync(string equipmentId, string cn, string costType)
        {
            var costRepo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FINANCE_ID", Operator = "=", FilterValue = equipmentId },
                new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = costType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await costRepo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            return costs.Sum(c => c.AMOUNT);
        }

        private async Task<EQUIPMENT?> GetEquipmentAsync(string equipmentId, string cn)
        {
            var equipmentRepo = await GetRepoAsync<EQUIPMENT>("EQUIPMENT", cn);
            var equipment = await equipmentRepo.GetByIdAsync(equipmentId);
            return equipment as EQUIPMENT;
        }

        private async Task RecordDisposalCostAsync(
            string equipmentId,
            decimal saleProceeds,
            DateTime disposalDate,
            string userId,
            string cn,
            string assetName)
        {
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = equipmentId,
                COST_TYPE = "ASSET_DISPOSAL",
                COST_CATEGORY = "FIXED_ASSET",
                AMOUNT = saleProceeds,
                COST_DATE = disposalDate,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "N",
                DESCRIPTION = $"Asset disposal {assetName}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var costRepo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await costRepo.InsertAsync(cost, userId);
        }

        private async Task DecommissionEquipmentAsync(
            EQUIPMENT equipment,
            DateTime disposalDate,
            string userId,
            string cn)
        {
            equipment.DECOMMISSION_DATE = disposalDate;
            equipment.EXPIRY_DATE = disposalDate;
            equipment.ACTIVE_IND = _defaults.GetActiveIndicatorNo();
            equipment.ROW_CHANGED_BY = userId;
            equipment.ROW_CHANGED_DATE = DateTime.UtcNow;

            var equipmentRepo = await GetRepoAsync<EQUIPMENT>("EQUIPMENT", cn);
            await equipmentRepo.UpdateAsync(equipment, userId);
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
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



