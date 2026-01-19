using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Handles drilling-specific accounting scenarios (dry hole, sidetrack, plug-back).
    /// </summary>
    public class DrillingScenarioAccountingService : IDrillingScenarioAccountingService
    {
        private static readonly string[] KnownScenarios =
        {
            "SUCCESSFUL",
            "SUCCESS",
            "DRY_HOLE",
            "DRY",
            "SIDETRACK",
            "PLUG_BACK",
            "ABANDONED",
            "FAILED"
        };

        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<DrillingScenarioAccountingService> _logger;
        private const string ConnectionName = "PPDM39";

        public DrillingScenarioAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<DrillingScenarioAccountingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public Task<bool> ValidateScenarioAsync(string scenario)
        {
            if (string.IsNullOrWhiteSpace(scenario))
                return Task.FromResult(false);

            var normalized = scenario.Trim().ToUpperInvariant();
            return Task.FromResult(KnownScenarios.Any(s => normalized.Contains(s)));
        }

        public async Task<ACCOUNTING_COST> RecordDrillingCostAsync(
            string wellId,
            decimal cost,
            string scenario,
            DateTime costDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (cost <= 0m)
                throw new AccountingException("Drilling cost must be positive.");

            var normalized = scenario?.Trim().ToUpperInvariant() ?? "UNKNOWN";
            var isDry = normalized.Contains("DRY") || normalized.Contains("FAILED") || normalized.Contains("ABANDON");
            var category = GetScenarioCategory(normalized);

            var record = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                WELL_ID = wellId,
                AMOUNT = cost,
                COST_DATE = costDate,
                COST_TYPE = isDry ? CostTypes.Exploration : CostTypes.Development,
                COST_CATEGORY = category,
                IS_CAPITALIZED = isDry ? "N" : "Y",
                IS_EXPENSED = isDry ? "Y" : "N",
                DESCRIPTION = $"Drilling scenario: {normalized}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(record, userId);

            _logger?.LogInformation(
                "Recorded drilling cost {Cost} for well {WellId} scenario {Scenario} (capitalized={Capitalized})",
                cost, wellId, normalized, record.IS_CAPITALIZED);

            return record;
        }

        public async Task<ACCOUNTING_COST> RecordSalvageRecoveryAsync(
            string wellId,
            decimal salvageAmount,
            DateTime salvageDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (salvageAmount <= 0m)
                throw new AccountingException("Salvage amount must be positive.");

            var record = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                WELL_ID = wellId,
                AMOUNT = -salvageAmount,
                COST_DATE = salvageDate,
                COST_TYPE = CostTypes.Production,
                COST_CATEGORY = CostCategories.Salvage,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = "Dry hole salvage recovery",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(record, userId);

            _logger?.LogInformation(
                "Recorded salvage recovery {Amount} for well {WellId}",
                salvageAmount, wellId);

            return record;
        }

        public async Task<ACCOUNTING_COST> RecordTestWellContributionAsync(
            string wellId,
            decimal cost,
            DateTime costDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (cost <= 0m)
                throw new AccountingException("Test well contribution must be positive.");

            var record = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                WELL_ID = wellId,
                AMOUNT = cost,
                COST_DATE = costDate,
                COST_TYPE = CostTypes.Exploration,
                COST_CATEGORY = CostCategories.TestWell,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = "Test well contribution",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(record, userId);

            _logger?.LogInformation(
                "Recorded test well contribution {Amount} for well {WellId}",
                cost, wellId);

            return record;
        }

        private static string GetScenarioCategory(string scenario)
        {
            if (scenario.Contains("SIDETRACK"))
                return CostCategories.Sidetrack;
            if (scenario.Contains("PLUG_BACK"))
                return CostCategories.PlugBack;
            return CostCategories.Drilling;
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
    }
}
