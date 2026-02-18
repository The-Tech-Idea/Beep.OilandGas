using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    public interface IAccountMappingService
    {
        string GetAccountId(string key);
        Task InitializeAsync(string userId);
    }

    public class AccountMappingService : IAccountMappingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AccountMappingService> _logger;
        private const string ConnectionName = "PPDM39";
        private const string TableName = "GL_ACCOUNT_MAPPING";

        // In-memory cache for fast lookups
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private bool _isInitialized = false;

        public AccountMappingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AccountMappingService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetAccountId(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var accountId))
                return accountId;

            // Fallback for immediate startup before async init completes, or if key missing in DB
            if (DefaultMappings.TryGetValue(key, out var defaultId))
            {
                _logger?.LogWarning("Mapping key '{Key}' not found in cache, using default '{DefaultId}'", key, defaultId);
                return defaultId;
            }

            throw new InvalidOperationException($"No account mapping found for key '{key}'");
        }

        public async Task InitializeAsync(string userId)
        {
            if (_isInitialized) return;

            try
            {
                await EnsureTableExistsAsync();
                await LoadMappingsFromDbAsync();
                await SeedMissingMappingsAsync(userId);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize account mappings");
                // Don't throw, allow valid fallback to defaults
            }
        }

        private async Task EnsureTableExistsAsync()
        {
             // In a real scenario, we might create the table here if DME supports it.
             // For now, we assume the table exists or will be created by the PPDM system.
             // If we needed to create it, we'd use _editor.DataDefinition.CreateTable(...)
             await Task.CompletedTask;
        }

        private async Task LoadMappingsFromDbAsync()
        {
            try
            {
                var repo = await GetRepoAsync<GLAccountMapping>(TableName);
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                if (results != null)
                {
                    foreach (var item in results.Cast<GLAccountMapping>())
                    {
                        if (!string.IsNullOrWhiteSpace(item.MAPPING_KEY) && !string.IsNullOrWhiteSpace(item.GL_ACCOUNT_NUMBER))
                        {
                            _cache[item.MAPPING_KEY] = item.GL_ACCOUNT_NUMBER;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not load mappings from DB, using defaults.");
            }
        }

        private async Task SeedMissingMappingsAsync(string userId)
        {
            var repo = await GetRepoAsync<GLAccountMapping>(TableName);
            var madeChanges = false;

            foreach (var kvp in DefaultMappings)
            {
                if (!_cache.ContainsKey(kvp.Key))
                {
                    // Key is missing in DB (and cache), so insert it
                    var mapping = new GLAccountMapping
                    {
                        GL_ACCOUNT_MAPPING_ID = Guid.NewGuid().ToString(),
                        MAPPING_KEY = kvp.Key,
                        GL_ACCOUNT_NUMBER = kvp.Value,
                        DESCRIPTION = $"Default mapping for {kvp.Key}",
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    };

                    try
                    {
                        await repo.InsertAsync(mapping, userId);
                        _cache[kvp.Key] = kvp.Value; // Update cache immediately
                        madeChanges = true;
                    }
                    catch (Exception ex)
                    {
                         _logger?.LogError(ex, "Failed to seed mapping for key {Key}", kvp.Key);
                    }
                }
            }

            if (madeChanges)
            {
                _logger?.LogInformation("Seeded missing GL account mappings to database.");
            }
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName)
        {
             // We need to handle the case where the table metadata might not exist yet
             // In a production system, we'd check _metadata.GetTableMetadataAsync first.
             // For this refactor, we assume the table creation is handled or we use a generic approach.
             
             // Note: PPDMGenericRepository usually requires valid metadata.
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = typeof(T); 
                
                return new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, tableName);
            }
            catch
            {
                 // Fallback: If table doesn't exist in metadata, we might need a more dynamic repo 
                 // or just fail gracefully (which falls back to defaults).
                 throw new InvalidOperationException($"Table {tableName} not found in metadata.");
            }
        }

        private static readonly Dictionary<string, string> DefaultMappings = new Dictionary<string, string>
        {
                { AccountMappingKeys.Cash, DefaultGlAccounts.Cash },
                { AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable },
                { AccountMappingKeys.Inventory, DefaultGlAccounts.Inventory },
                { AccountMappingKeys.AccountsPayable, DefaultGlAccounts.AccountsPayable },
                { AccountMappingKeys.RetainedEarnings, DefaultGlAccounts.RetainedEarnings },
                { AccountMappingKeys.Revenue, DefaultGlAccounts.Revenue },
                { AccountMappingKeys.CostOfGoodsSold, DefaultGlAccounts.CostOfGoodsSold },
                { AccountMappingKeys.OperatingExpense, DefaultGlAccounts.OperatingExpense },
                { AccountMappingKeys.FixedAssets, DefaultGlAccounts.FixedAssets },
                { AccountMappingKeys.AccumulatedDepreciation, DefaultGlAccounts.AccumulatedDepreciation },
                { AccountMappingKeys.DepreciationExpense, DefaultGlAccounts.DepreciationExpense },
                { AccountMappingKeys.AssetRetirementCost, DefaultGlAccounts.AssetRetirementCost },
                { AccountMappingKeys.AssetRetirementObligation, DefaultGlAccounts.AssetRetirementObligation },
                { AccountMappingKeys.AccretionExpense, DefaultGlAccounts.AccretionExpense },
                { AccountMappingKeys.ContractAsset, DefaultGlAccounts.ContractAsset },
                { AccountMappingKeys.ContractLiability, DefaultGlAccounts.ContractLiability },
                { AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.RightOfUseAsset },
                { AccountMappingKeys.LeaseLiability, DefaultGlAccounts.LeaseLiability },
                { AccountMappingKeys.LeaseInterestExpense, DefaultGlAccounts.LeaseInterestExpense },
                { AccountMappingKeys.LeaseAmortizationExpense, DefaultGlAccounts.LeaseAmortizationExpense },
                { AccountMappingKeys.IncomeTaxExpense, DefaultGlAccounts.IncomeTaxExpense },
                { AccountMappingKeys.IncomeTaxPayable, DefaultGlAccounts.IncomeTaxPayable },
                { AccountMappingKeys.DeferredTaxAsset, DefaultGlAccounts.DeferredTaxAsset },
                { AccountMappingKeys.DeferredTaxLiability, DefaultGlAccounts.DeferredTaxLiability },
                { AccountMappingKeys.ForeignExchangeGain, DefaultGlAccounts.ForeignExchangeGain },
                { AccountMappingKeys.ForeignExchangeLoss, DefaultGlAccounts.ForeignExchangeLoss },
                { AccountMappingKeys.ImpairmentLoss, DefaultGlAccounts.ImpairmentLoss },
                { AccountMappingKeys.ImpairmentAllowance, DefaultGlAccounts.ImpairmentAllowance },
                { AccountMappingKeys.IntangibleAssets, DefaultGlAccounts.IntangibleAssets },
                { AccountMappingKeys.AccumulatedAmortization, DefaultGlAccounts.AccumulatedAmortization },
                { AccountMappingKeys.AmortizationExpense, DefaultGlAccounts.AmortizationExpense },
                { AccountMappingKeys.EmployeeBenefitExpense, DefaultGlAccounts.EmployeeBenefitExpense },
                { AccountMappingKeys.EmployeeBenefitLiability, DefaultGlAccounts.EmployeeBenefitLiability },
                { AccountMappingKeys.GrantReceivable, DefaultGlAccounts.GrantReceivable },
                { AccountMappingKeys.DeferredGrantLiability, DefaultGlAccounts.DeferredGrantLiability },
                { AccountMappingKeys.GrantIncome, DefaultGlAccounts.GrantIncome },
                { AccountMappingKeys.BorrowingCostExpense, DefaultGlAccounts.BorrowingCostExpense },
                { AccountMappingKeys.InterestPayable, DefaultGlAccounts.InterestPayable },
                { AccountMappingKeys.RetirementPlanAsset, DefaultGlAccounts.RetirementPlanAsset },
                { AccountMappingKeys.RetirementPlanExpense, DefaultGlAccounts.RetirementPlanExpense },
                { AccountMappingKeys.InvestmentProperty, DefaultGlAccounts.InvestmentProperty },
                { AccountMappingKeys.InvestmentPropertyFairValueGain, DefaultGlAccounts.InvestmentPropertyFairValueGain },
                { AccountMappingKeys.InvestmentPropertyFairValueLoss, DefaultGlAccounts.InvestmentPropertyFairValueLoss },
                { AccountMappingKeys.BiologicalAssets, DefaultGlAccounts.BiologicalAssets },
                { AccountMappingKeys.BiologicalAssetGain, DefaultGlAccounts.BiologicalAssetGain },
                { AccountMappingKeys.BiologicalAssetLoss, DefaultGlAccounts.BiologicalAssetLoss },
                { AccountMappingKeys.FinancialInstrumentAsset, DefaultGlAccounts.FinancialInstrumentAsset },
                { AccountMappingKeys.FinancialInstrumentLiability, DefaultGlAccounts.FinancialInstrumentLiability },
                { AccountMappingKeys.FinancialInstrumentGain, DefaultGlAccounts.FinancialInstrumentGain },
                { AccountMappingKeys.FinancialInstrumentLoss, DefaultGlAccounts.FinancialInstrumentLoss },
                { AccountMappingKeys.AssociateInvestment, DefaultGlAccounts.AssociateInvestment },
                { AccountMappingKeys.JointVentureInvestment, DefaultGlAccounts.JointVentureInvestment },
                { AccountMappingKeys.EquityMethodEarnings, DefaultGlAccounts.EquityMethodEarnings },
                { AccountMappingKeys.EquityMethodLoss, DefaultGlAccounts.EquityMethodLoss },
                { AccountMappingKeys.RestatementReserve, DefaultGlAccounts.RestatementReserve },
                { AccountMappingKeys.InflationGain, DefaultGlAccounts.InflationGain },
                { AccountMappingKeys.InflationLoss, DefaultGlAccounts.InflationLoss },
                { AccountMappingKeys.InsuranceContractLiability, DefaultGlAccounts.InsuranceContractLiability },
                { AccountMappingKeys.ReinsuranceAsset, DefaultGlAccounts.ReinsuranceAsset },
                { AccountMappingKeys.ContractualServiceMargin, DefaultGlAccounts.ContractualServiceMargin },
                { AccountMappingKeys.InsuranceRevenue, DefaultGlAccounts.InsuranceRevenue },
                { AccountMappingKeys.InsuranceServiceExpense, DefaultGlAccounts.InsuranceServiceExpense },
                { AccountMappingKeys.InsuranceFinanceExpense, DefaultGlAccounts.InsuranceFinanceExpense },
                { AccountMappingKeys.ExpectedCreditLossExpense, DefaultGlAccounts.ExpectedCreditLossExpense },
                { AccountMappingKeys.LossAllowance, DefaultGlAccounts.LossAllowance },
                { AccountMappingKeys.FairValueGain, DefaultGlAccounts.FairValueGain },
                { AccountMappingKeys.FairValueLoss, DefaultGlAccounts.FairValueLoss },
                { AccountMappingKeys.ShareBasedCompExpense, DefaultGlAccounts.ShareBasedCompExpense },
                { AccountMappingKeys.ShareBasedCompEquity, DefaultGlAccounts.ShareBasedCompEquity },
                { AccountMappingKeys.Goodwill, DefaultGlAccounts.Goodwill },
                { AccountMappingKeys.BusinessCombinationGain, DefaultGlAccounts.BusinessCombinationGain },
                { AccountMappingKeys.HeldForSaleAsset, DefaultGlAccounts.HeldForSaleAsset },
                { AccountMappingKeys.HeldForSaleLiability, DefaultGlAccounts.HeldForSaleLiability },
                { AccountMappingKeys.HeldForSaleImpairment, DefaultGlAccounts.HeldForSaleImpairment },
                { AccountMappingKeys.ExplorationAsset, DefaultGlAccounts.ExplorationAsset },
                { AccountMappingKeys.ExplorationExpense, DefaultGlAccounts.ExplorationExpense },
                { AccountMappingKeys.IntercompanyReceivable, DefaultGlAccounts.IntercompanyReceivable },
                { AccountMappingKeys.IntercompanyPayable, DefaultGlAccounts.IntercompanyPayable },
                { AccountMappingKeys.RegulatoryDeferralAsset, DefaultGlAccounts.RegulatoryDeferralAsset },
                { AccountMappingKeys.RegulatoryDeferralLiability, DefaultGlAccounts.RegulatoryDeferralLiability },
                { AccountMappingKeys.RegulatoryIncome, DefaultGlAccounts.RegulatoryIncome },
                { AccountMappingKeys.RegulatoryExpense, DefaultGlAccounts.RegulatoryExpense },
                { AccountMappingKeys.CeclAllowance, DefaultGlAccounts.CeclAllowance },
                { AccountMappingKeys.CeclExpense, DefaultGlAccounts.CeclExpense },
                { AccountMappingKeys.GaapContractAsset, DefaultGlAccounts.GaapContractAsset },
                { AccountMappingKeys.GaapContractLiability, DefaultGlAccounts.GaapContractLiability },
                { AccountMappingKeys.GaapLeaseLiability, DefaultGlAccounts.GaapLeaseLiability },
                { AccountMappingKeys.GaapRightOfUseAsset, DefaultGlAccounts.GaapRightOfUseAsset }
        };
    }
}
