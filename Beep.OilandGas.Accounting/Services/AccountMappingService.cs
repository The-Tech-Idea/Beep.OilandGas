using System;
using System.Collections.Generic;
using Beep.OilandGas.Accounting.Constants;

namespace Beep.OilandGas.Accounting.Services
{
    public interface IAccountMappingService
    {
        string GetAccountId(string key);
    }

    public class AccountMappingService : IAccountMappingService
    {
        private readonly Dictionary<string, string> _mappings;

        public AccountMappingService(IDictionary<string, string>? overrides = null)
        {
            _mappings = BuildDefaultMappings();
            if (overrides == null)
                return;

            foreach (var kvp in overrides)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                _mappings[kvp.Key] = kvp.Value;
            }
        }

        public string GetAccountId(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_mappings.TryGetValue(key, out var accountId))
                return accountId;

            throw new InvalidOperationException($"No account mapping found for key '{key}'");
        }

        private static Dictionary<string, string> BuildDefaultMappings()
        {
            return new Dictionary<string, string>
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
}
