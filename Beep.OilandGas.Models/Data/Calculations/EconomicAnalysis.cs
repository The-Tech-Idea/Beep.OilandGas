using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for Economic Analysis advanced operations.
    /// Supports Monte Carlo simulation, real options analysis, decision trees, DCF valuation, and sensitivity analysis.
    /// </summary>

    /// <summary>
    /// Result DTO for Monte Carlo probabilistic simulation analysis.
    /// Provides NPV and IRR distributions with statistical measures.
    /// </summary>
    public class MonteCarloSimulationResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Number of simulations performed
        /// </summary>
        private int SimulationCountValue;

        public int SimulationCount

        {

            get { return this.SimulationCountValue; }

            set { SetProperty(ref SimulationCountValue, value); }

        }

        /// <summary>
        /// Distribution of NPV values from all simulations
        /// </summary>
        private List<double> NPVDistributionValue;

        public List<double> NPVDistribution

        {

            get { return this.NPVDistributionValue; }

            set { SetProperty(ref NPVDistributionValue, value); }

        }

        /// <summary>
        /// Distribution of IRR values from all simulations
        /// </summary>
        private List<double> IRRDistributionValue;

        public List<double> IRRDistribution

        {

            get { return this.IRRDistributionValue; }

            set { SetProperty(ref IRRDistributionValue, value); }

        }

        /// <summary>
        /// Mean (average) NPV across all simulations
        /// </summary>
        private double MeanNPVValue;

        public double MeanNPV

        {

            get { return this.MeanNPVValue; }

            set { SetProperty(ref MeanNPVValue, value); }

        }

        /// <summary>
        /// Mean (average) IRR across all simulations
        /// </summary>
        private double MeanIRRValue;

        public double MeanIRR

        {

            get { return this.MeanIRRValue; }

            set { SetProperty(ref MeanIRRValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV distribution (volatility measure)
        /// </summary>
        private double StdDevNPVValue;

        public double StdDevNPV

        {

            get { return this.StdDevNPVValue; }

            set { SetProperty(ref StdDevNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of IRR distribution (volatility measure)
        /// </summary>
        private double StdDevIRRValue;

        public double StdDevIRR

        {

            get { return this.StdDevIRRValue; }

            set { SetProperty(ref StdDevIRRValue, value); }

        }

        /// <summary>
        /// 10th percentile NPV (downside scenario)
        /// </summary>
        private double P10NPVValue;

        public double P10NPV

        {

            get { return this.P10NPVValue; }

            set { SetProperty(ref P10NPVValue, value); }

        }

        /// <summary>
        /// 50th percentile NPV (base case)
        /// </summary>
        private double P50NPVValue;

        public double P50NPV

        {

            get { return this.P50NPVValue; }

            set { SetProperty(ref P50NPVValue, value); }

        }

        /// <summary>
        /// 90th percentile NPV (upside scenario)
        /// </summary>
        private double P90NPVValue;

        public double P90NPV

        {

            get { return this.P90NPVValue; }

            set { SetProperty(ref P90NPVValue, value); }

        }

        /// <summary>
        /// Minimum NPV from all simulations
        /// </summary>
        private double MinNPVValue;

        public double MinNPV

        {

            get { return this.MinNPVValue; }

            set { SetProperty(ref MinNPVValue, value); }

        }

        /// <summary>
        /// Maximum NPV from all simulations
        /// </summary>
        private double MaxNPVValue;

        public double MaxNPV

        {

            get { return this.MaxNPVValue; }

            set { SetProperty(ref MaxNPVValue, value); }

        }

        /// <summary>
        /// Probability of achieving negative NPV (risk of loss)
        /// </summary>
        private double ProbabilityOfLossValue;

        public double ProbabilityOfLoss

        {

            get { return this.ProbabilityOfLossValue; }

            set { SetProperty(ref ProbabilityOfLossValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for real options analysis including expansion, abandonment, and switching options.
    /// </summary>
    public class RealOptionsAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Initial project NPV without considering options
        /// </summary>
        private double InitialNPVValue;

        public double InitialNPV

        {

            get { return this.InitialNPVValue; }

            set { SetProperty(ref InitialNPVValue, value); }

        }

        /// <summary>
        /// Expected project life in years
        /// </summary>
        private int ProjectLifeValue;

        public int ProjectLife

        {

            get { return this.ProjectLifeValue; }

            set { SetProperty(ref ProjectLifeValue, value); }

        }

        /// <summary>
        /// Cash flow volatility (used for option valuation)
        /// </summary>
        private double VolatilityValue;

        public double Volatility

        {

            get { return this.VolatilityValue; }

            set { SetProperty(ref VolatilityValue, value); }

        }

        /// <summary>
        /// List of option valuations (expansion, abandonment, switching)
        /// </summary>
        private List<OptionValuation> OptionsValue;

        public List<OptionValuation> Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }

        /// <summary>
        /// Total value of all strategic options combined
        /// </summary>
        private double TotalOptionValueValue;

        public double TotalOptionValue

        {

            get { return this.TotalOptionValueValue; }

            set { SetProperty(ref TotalOptionValueValue, value); }

        }

        /// <summary>
        /// Project value including option values
        /// </summary>
        private double ProjectValueWithOptionsValue;

        public double ProjectValueWithOptions

        {

            get { return this.ProjectValueWithOptionsValue; }

            set { SetProperty(ref ProjectValueWithOptionsValue, value); }

        }

        /// <summary>
        /// Flexibility premium as percentage of initial NPV
        /// </summary>
        private double FlexibilityPremiumValue;

        public double FlexibilityPremium

        {

            get { return this.FlexibilityPremiumValue; }

            set { SetProperty(ref FlexibilityPremiumValue, value); }

        }
    }

    /// <summary>
    /// DTO representing the valuation of a single strategic option.
    /// </summary>
    public class OptionValuation : ModelEntityBase
    {
        /// <summary>
        /// Type of option: Expansion, Abandonment, or Switching
        /// </summary>
        private string OptionTypeValue;

        public string OptionType

        {

            get { return this.OptionTypeValue; }

            set { SetProperty(ref OptionTypeValue, value); }

        }

        /// <summary>
        /// Calculated value of the option
        /// </summary>
        private double OptionValueValue;

        public double OptionValue

        {

            get { return this.OptionValueValue; }

            set { SetProperty(ref OptionValueValue, value); }

        }

        /// <summary>
        /// Project value if the option scenario occurs
        /// </summary>
        private double ScenarioValueValue;

        public double ScenarioValue

        {

            get { return this.ScenarioValueValue; }

            set { SetProperty(ref ScenarioValueValue, value); }

        }

        /// <summary>
        /// Cost to exercise the expansion option
        /// </summary>
        private double ExerciseCostValue;

        public double ExerciseCost

        {

            get { return this.ExerciseCostValue; }

            set { SetProperty(ref ExerciseCostValue, value); }

        }

        /// <summary>
        /// Salvage value if abandonment option is exercised
        /// </summary>
        private double SalvageValueValue;

        public double SalvageValue

        {

            get { return this.SalvageValueValue; }

            set { SetProperty(ref SalvageValueValue, value); }

        }

        /// <summary>
        /// NPV of alternative project if switching option is exercised
        /// </summary>
        private double AlternativeNPVValue;

        public double AlternativeNPV

        {

            get { return this.AlternativeNPVValue; }

            set { SetProperty(ref AlternativeNPVValue, value); }

        }

        /// <summary>
        /// Cost to switch to alternative project
        /// </summary>
        private double SwitchingCostValue;

        public double SwitchingCost

        {

            get { return this.SwitchingCostValue; }

            set { SetProperty(ref SwitchingCostValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for decision tree analysis with multi-stage investment scenarios.
    /// </summary>
    public class DecisionTreeAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Initial capital investment required
        /// </summary>
        private double InitialInvestmentValue;

        public double InitialInvestment

        {

            get { return this.InitialInvestmentValue; }

            set { SetProperty(ref InitialInvestmentValue, value); }

        }

        /// <summary>
        /// Probability of success (0.0 to 1.0)
        /// </summary>
        private double SuccessProbabilityValue;

        public double SuccessProbability

        {

            get { return this.SuccessProbabilityValue; }

            set { SetProperty(ref SuccessProbabilityValue, value); }

        }

        /// <summary>
        /// Success scenario with NPV and IRR
        /// </summary>
        private DecisionScenario SuccessScenarioValue;

        public DecisionScenario SuccessScenario

        {

            get { return this.SuccessScenarioValue; }

            set { SetProperty(ref SuccessScenarioValue, value); }

        }

        /// <summary>
        /// Failure scenario with NPV and IRR
        /// </summary>
        private DecisionScenario FailureScenarioValue;

        public DecisionScenario FailureScenario

        {

            get { return this.FailureScenarioValue; }

            set { SetProperty(ref FailureScenarioValue, value); }

        }

        /// <summary>
        /// Expected NPV across all scenarios (probability-weighted)
        /// </summary>
        private double ExpectedNPVValue;

        public double ExpectedNPV

        {

            get { return this.ExpectedNPVValue; }

            set { SetProperty(ref ExpectedNPVValue, value); }

        }

        /// <summary>
        /// Variance of NPV across scenarios (risk measure)
        /// </summary>
        private double VarianceOfNPVValue;

        public double VarianceOfNPV

        {

            get { return this.VarianceOfNPVValue; }

            set { SetProperty(ref VarianceOfNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV (volatility)
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Decision recommendation: "Proceed" or "Do Not Proceed"
        /// </summary>
        private string DecisionValue;

        public string Decision

        {

            get { return this.DecisionValue; }

            set { SetProperty(ref DecisionValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a scenario in decision tree analysis.
    /// </summary>
    public class DecisionScenario : ModelEntityBase
    {
        /// <summary>
        /// Name of the scenario (e.g., "Success", "Failure")
        /// </summary>
        private string ScenarioNameValue;

        public string ScenarioName

        {

            get { return this.ScenarioNameValue; }

            set { SetProperty(ref ScenarioNameValue, value); }

        }

        /// <summary>
        /// Probability of this scenario occurring
        /// </summary>
        private double ProbabilityValue;

        public double Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }

        /// <summary>
        /// Net Present Value of this scenario
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return for this scenario
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Cumulative cash flow across all periods in this scenario
        /// </summary>
        private double CumulativeCashFlowValue;

        public double CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for after-tax economic analysis considering corporate taxation and depreciation.
    /// </summary>
    public class AfterTaxAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Corporate tax rate applied (as decimal, e.g., 0.35 for 35%)
        /// </summary>
        private double TaxRateValue;

        public double TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }

        /// <summary>
        /// NPV before considering tax impacts
        /// </summary>
        private double PreTaxNPVValue;

        public double PreTaxNPV

        {

            get { return this.PreTaxNPVValue; }

            set { SetProperty(ref PreTaxNPVValue, value); }

        }

        /// <summary>
        /// NPV after tax deductions and depreciation benefits
        /// </summary>
        private double AfterTaxNPVValue;

        public double AfterTaxNPV

        {

            get { return this.AfterTaxNPVValue; }

            set { SetProperty(ref AfterTaxNPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return after tax adjustments
        /// </summary>
        private double AfterTaxIRRValue;

        public double AfterTaxIRR

        {

            get { return this.AfterTaxIRRValue; }

            set { SetProperty(ref AfterTaxIRRValue, value); }

        }

        /// <summary>
        /// Tax shield value from depreciation and deductions
        /// </summary>
        private double TaxShieldValue;

        public double TaxShield

        {

            get { return this.TaxShieldValue; }

            set { SetProperty(ref TaxShieldValue, value); }

        }

        /// <summary>
        /// Effective tax rate on the project
        /// </summary>
        private double EffectiveTaxRateValue;

        public double EffectiveTaxRate

        {

            get { return this.EffectiveTaxRateValue; }

            set { SetProperty(ref EffectiveTaxRateValue, value); }

        }

        /// <summary>
        /// After-tax cash flows by period
        /// </summary>
        private List<CashFlow> AfterTaxCashFlowsValue;

        public List<CashFlow> AfterTaxCashFlows

        {

            get { return this.AfterTaxCashFlowsValue; }

            set { SetProperty(ref AfterTaxCashFlowsValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for DCF (Discounted Cash Flow) enterprise valuation.
    /// </summary>
    public class DCFValuationResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Terminal growth rate beyond explicit forecast period
        /// </summary>
        private double TerminalGrowthRateValue;

        public double TerminalGrowthRate

        {

            get { return this.TerminalGrowthRateValue; }

            set { SetProperty(ref TerminalGrowthRateValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital (discount rate)
        /// </summary>
        private double WACCValue;

        public double WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }

        /// <summary>
        /// Present value components for each year of the forecast period
        /// </summary>
        private List<PVComponent> PresentValueComponentsValue;

        public List<PVComponent> PresentValueComponents

        {

            get { return this.PresentValueComponentsValue; }

            set { SetProperty(ref PresentValueComponentsValue, value); }

        }

        /// <summary>
        /// PV of explicit forecast period cash flows
        /// </summary>
        private double ExplicitPeriodValueValue;

        public double ExplicitPeriodValue

        {

            get { return this.ExplicitPeriodValueValue; }

            set { SetProperty(ref ExplicitPeriodValueValue, value); }

        }

        /// <summary>
        /// Terminal value of cash flows beyond forecast period
        /// </summary>
        private double TerminalValueValue;

        public double TerminalValue

        {

            get { return this.TerminalValueValue; }

            set { SetProperty(ref TerminalValueValue, value); }

        }

        /// <summary>
        /// Present value of terminal value
        /// </summary>
        private double PVTerminalValueValue;

        public double PVTerminalValue

        {

            get { return this.PVTerminalValueValue; }

            set { SetProperty(ref PVTerminalValueValue, value); }

        }

        /// <summary>
        /// Total enterprise value (sum of explicit period and terminal values)
        /// </summary>
        private double EnterpriseValueValue;

        public double EnterpriseValue

        {

            get { return this.EnterpriseValueValue; }

            set { SetProperty(ref EnterpriseValueValue, value); }

        }

        /// <summary>
        /// Percentage of enterprise value attributable to terminal value
        /// </summary>
        private double TerminalValuePercentageValue;

        public double TerminalValuePercentage

        {

            get { return this.TerminalValuePercentageValue; }

            set { SetProperty(ref TerminalValuePercentageValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a single year's present value component in DCF analysis.
    /// </summary>
    public class PVComponent : ModelEntityBase
    {
        /// <summary>
        /// Forecast year number
        /// </summary>
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }

        /// <summary>
        /// Cash flow in this year (before discounting)
        /// </summary>
        private double CashFlowValue;

        public double CashFlow

        {

            get { return this.CashFlowValue; }

            set { SetProperty(ref CashFlowValue, value); }

        }

        /// <summary>
        /// Discount factor applied (1 / (1 + WACC)^year)
        /// </summary>
        private double DiscountFactorValue;

        public double DiscountFactor

        {

            get { return this.DiscountFactorValue; }

            set { SetProperty(ref DiscountFactorValue, value); }

        }

        /// <summary>
        /// Present value of the cash flow
        /// </summary>
        private double PresentValueValue;

        public double PresentValue

        {

            get { return this.PresentValueValue; }

            set { SetProperty(ref PresentValueValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for lease vs. buy financial decision analysis.
    /// </summary>
    public class LeaseBuyAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Cost to purchase the asset
        /// </summary>
        private double AssetCostValue;

        public double AssetCost

        {

            get { return this.AssetCostValue; }

            set { SetProperty(ref AssetCostValue, value); }

        }

        /// <summary>
        /// Duration of lease in years
        /// </summary>
        private int LeaseTermValue;

        public int LeaseTerm

        {

            get { return this.LeaseTermValue; }

            set { SetProperty(ref LeaseTermValue, value); }

        }

        /// <summary>
        /// Residual value of asset at end of lease term
        /// </summary>
        private double SalvageValueValue;

        public double SalvageValue

        {

            get { return this.SalvageValueValue; }

            set { SetProperty(ref SalvageValueValue, value); }

        }

        /// <summary>
        /// Net Present Value of buying option
        /// </summary>
        private double BuyNPVValue;

        public double BuyNPV

        {

            get { return this.BuyNPVValue; }

            set { SetProperty(ref BuyNPVValue, value); }

        }

        /// <summary>
        /// Net Present Value of leasing option
        /// </summary>
        private double LeaseNPVValue;

        public double LeaseNPV

        {

            get { return this.LeaseNPVValue; }

            set { SetProperty(ref LeaseNPVValue, value); }

        }

        /// <summary>
        /// Net advantage of leasing vs buying (positive favors leasing)
        /// </summary>
        private double NetAdvantageOfLeasingValue;

        public double NetAdvantageOfLeasing

        {

            get { return this.NetAdvantageOfLeasingValue; }

            set { SetProperty(ref NetAdvantageOfLeasingValue, value); }

        }

        /// <summary>
        /// Recommendation: "Lease" or "Buy"
        /// </summary>
        private string RecommendationValue;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for capital structure optimization analysis.
    /// </summary>
    public class CapitalStructureAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Unlevered (all-equity) firm value
        /// </summary>
        private double UnleveredValueValue;

        public double UnleveredValue

        {

            get { return this.UnleveredValueValue; }

            set { SetProperty(ref UnleveredValueValue, value); }

        }

        /// <summary>
        /// Corporate tax rate used in analysis
        /// </summary>
        private double TaxRateValue;

        public double TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }

        /// <summary>
        /// Capital structure scenarios analyzed (different debt ratios)
        /// </summary>
        private List<CapitalStructureScenario> ScenariosValue;

        public List<CapitalStructureScenario> Scenarios

        {

            get { return this.ScenariosValue; }

            set { SetProperty(ref ScenariosValue, value); }

        }

        /// <summary>
        /// Optimal debt ratio that minimizes WACC
        /// </summary>
        private double OptimalDebtRatioValue;

        public double OptimalDebtRatio

        {

            get { return this.OptimalDebtRatioValue; }

            set { SetProperty(ref OptimalDebtRatioValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital at optimal capital structure
        /// </summary>
        private double OptimalWACCValue;

        public double OptimalWACC

        {

            get { return this.OptimalWACCValue; }

            set { SetProperty(ref OptimalWACCValue, value); }

        }

        /// <summary>
        /// Levered firm value at optimal capital structure
        /// </summary>
        private double OptimalLeveredValueValue;

        public double OptimalLeveredValue

        {

            get { return this.OptimalLeveredValueValue; }

            set { SetProperty(ref OptimalLeveredValueValue, value); }

        }

        /// <summary>
        /// Value created by using optimal capital structure
        /// </summary>
        private double ValueCreationValue;

        public double ValueCreation

        {

            get { return this.ValueCreationValue; }

            set { SetProperty(ref ValueCreationValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a single capital structure scenario with different debt ratios.
    /// </summary>
    public class CapitalStructureScenario : ModelEntityBase
    {
        /// <summary>
        /// Debt as percentage of total capital (0.0 to 1.0)
        /// </summary>
        private double DebtRatioValue;

        public double DebtRatio

        {

            get { return this.DebtRatioValue; }

            set { SetProperty(ref DebtRatioValue, value); }

        }

        /// <summary>
        /// Equity as percentage of total capital (0.0 to 1.0)
        /// </summary>
        private double EquityRatioValue;

        public double EquityRatio

        {

            get { return this.EquityRatioValue; }

            set { SetProperty(ref EquityRatioValue, value); }

        }

        /// <summary>
        /// Total market value of debt
        /// </summary>
        private double DebtValueValue;

        public double DebtValue

        {

            get { return this.DebtValueValue; }

            set { SetProperty(ref DebtValueValue, value); }

        }

        /// <summary>
        /// Total market value of equity
        /// </summary>
        private double EquityValueValue;

        public double EquityValue

        {

            get { return this.EquityValueValue; }

            set { SetProperty(ref EquityValueValue, value); }

        }

        /// <summary>
        /// Tax benefit from interest deductions
        /// </summary>
        private double TaxShieldValue;

        public double TaxShield

        {

            get { return this.TaxShieldValue; }

            set { SetProperty(ref TaxShieldValue, value); }

        }

        /// <summary>
        /// Levered firm value with this capital structure
        /// </summary>
        private double LeveredValueValue;

        public double LeveredValue

        {

            get { return this.LeveredValueValue; }

            set { SetProperty(ref LeveredValueValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital for this structure
        /// </summary>
        private double WACCValue;

        public double WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }

        /// <summary>
        /// Financial risk classification: Low, Medium, or High
        /// </summary>
        private string FinancialRiskValue;

        public string FinancialRisk

        {

            get { return this.FinancialRiskValue; }

            set { SetProperty(ref FinancialRiskValue, value); }

        }
    }

    /// <summary>
    /// Result DTO for commodity price sensitivity analysis.
    /// </summary>
    public class CommodityPriceSensitivityResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Base commodity price used in analysis
        /// </summary>
        private double BasePriceValue;

        public double BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Scenarios showing NPV and IRR at different price points
        /// </summary>
        private List<PriceScenario> PriceScenariosValue;

        public List<PriceScenario> PriceScenarios

        {

            get { return this.PriceScenariosValue; }

            set { SetProperty(ref PriceScenariosValue, value); }

        }

        /// <summary>
        /// Price at which project NPV breaks even (approximately zero)
        /// </summary>
        private double BreakevenPriceValue;

        public double BreakevenPrice

        {

            get { return this.BreakevenPriceValue; }

            set { SetProperty(ref BreakevenPriceValue, value); }

        }

        /// <summary>
        /// NPV at base price point
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }
    }

    /// <summary>
    /// DTO representing project economics at a specific commodity price point.
    /// </summary>
    public class PriceScenario : ModelEntityBase
    {
        /// <summary>
        /// Commodity price for this scenario
        /// </summary>
        private double PriceValue;

        public double Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }

        /// <summary>
        /// Net Present Value at this price
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return at this price
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Indicates if this is the breakeven price point
        /// </summary>
        private bool IsBreakevenValue;

        public bool IsBreakeven

        {

            get { return this.IsBreakevenValue; }

            set { SetProperty(ref IsBreakevenValue, value); }

        }
    }

    /// <summary>
    /// DTO for sensitivity analysis results showing how parameters affect NPV.
    /// </summary>
    public class SensitivityAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Base case NPV
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }

        /// <summary>
        /// Base case IRR
        /// </summary>
        private double BaseIRRValue;

        public double BaseIRR

        {

            get { return this.BaseIRRValue; }

            set { SetProperty(ref BaseIRRValue, value); }

        }

        /// <summary>
        /// List of parameters tested for sensitivity
        /// </summary>
        private List<SensitivityParameter> ParametersValue;

        public List<SensitivityParameter> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
    }

    /// <summary>
    /// DTO representing sensitivity of a single parameter.
    /// </summary>
    public class SensitivityParameter : ModelEntityBase
    {
        /// <summary>
        /// Name of the parameter being tested
        /// </summary>
        private string ParameterNameValue;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }

        /// <summary>
        /// Base value of the parameter
        /// </summary>
        private double BaseValueValue;

        public double BaseValue

        {

            get { return this.BaseValueValue; }

            set { SetProperty(ref BaseValueValue, value); }

        }

        /// <summary>
        /// NPV with parameter reduced by 10%
        /// </summary>
        private double NegativeVariationNPVValue;

        public double NegativeVariationNPV

        {

            get { return this.NegativeVariationNPVValue; }

            set { SetProperty(ref NegativeVariationNPVValue, value); }

        }

        /// <summary>
        /// NPV with parameter increased by 10%
        /// </summary>
        private double PositiveVariationNPVValue;

        public double PositiveVariationNPV

        {

            get { return this.PositiveVariationNPVValue; }

            set { SetProperty(ref PositiveVariationNPVValue, value); }

        }

        /// <summary>
        /// Absolute impact on NPV from variation
        /// </summary>
        private double NPVImpactValue;

        public double NPVImpact

        {

            get { return this.NPVImpactValue; }

            set { SetProperty(ref NPVImpactValue, value); }

        }

        /// <summary>
        /// Sensitivity index (percentage change in NPV per 1% change in parameter)
        /// </summary>
        private double SensitivityIndexValue;

        public double SensitivityIndex

        {

            get { return this.SensitivityIndexValue; }

            set { SetProperty(ref SensitivityIndexValue, value); }

        }
    }

    /// <summary>
    /// DTO for scenario analysis with best, base, and worst case results.
    /// </summary>
    public class ScenarioAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// NPV in base case scenario
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }

        /// <summary>
        /// NPV in best case scenario
        /// </summary>
        private double BestCaseNPVValue;

        public double BestCaseNPV

        {

            get { return this.BestCaseNPVValue; }

            set { SetProperty(ref BestCaseNPVValue, value); }

        }

        /// <summary>
        /// NPV in worst case scenario
        /// </summary>
        private double WorstCaseNPVValue;

        public double WorstCaseNPV

        {

            get { return this.WorstCaseNPVValue; }

            set { SetProperty(ref WorstCaseNPVValue, value); }

        }

        /// <summary>
        /// Probability-weighted expected NPV
        /// </summary>
        private double ExpectedValueNPVValue;

        public double ExpectedValueNPV

        {

            get { return this.ExpectedValueNPVValue; }

            set { SetProperty(ref ExpectedValueNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV across scenarios
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Coefficient of variation (stdDev / mean)
        /// </summary>
        private double CoefficientOfVariationValue;

        public double CoefficientOfVariation

        {

            get { return this.CoefficientOfVariationValue; }

            set { SetProperty(ref CoefficientOfVariationValue, value); }

        }

        /// <summary>
        /// Detailed results for each scenario
        /// </summary>
        private List<ScenarioResult> ScenariosValue;

        public List<ScenarioResult> Scenarios

        {

            get { return this.ScenariosValue; }

            set { SetProperty(ref ScenariosValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a single scenario's results in scenario analysis.
    /// </summary>
    public class ScenarioResult : ModelEntityBase
    {
        /// <summary>
        /// Name of the scenario (e.g., "Best Case", "Base Case", "Worst Case")
        /// </summary>
        private string ScenarioNameValue;

        public string ScenarioName

        {

            get { return this.ScenarioNameValue; }

            set { SetProperty(ref ScenarioNameValue, value); }

        }

        /// <summary>
        /// Probability of this scenario occurring
        /// </summary>
        private double ProbabilityValue;

        public double Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }

        /// <summary>
        /// Net Present Value for this scenario
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return for this scenario
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Payback period for this scenario (in years)
        /// </summary>
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }
    }

    /// <summary>
    /// DTO for comprehensive financial metrics including NPV, IRR, ROI, and profitability measures.
    /// </summary>
    public class FinancialMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the metrics calculation
        /// </summary>
        private string MetricsIdValue;

        public string MetricsId

        {

            get { return this.MetricsIdValue; }

            set { SetProperty(ref MetricsIdValue, value); }

        }

        /// <summary>
        /// Date the metrics were calculated
        /// </summary>
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }

        /// <summary>
        /// Net Present Value
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Payback period in years
        /// </summary>
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Profitability Index (PV of future cash flows / Initial investment)
        /// </summary>
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Return on Investment
        /// </summary>
        private double ROIValue;

        public double ROI

        {

            get { return this.ROIValue; }

            set { SetProperty(ref ROIValue, value); }

        }

        /// <summary>
        /// Modified Internal Rate of Return (accounts for financing and reinvestment)
        /// </summary>
        private double ModifiedIRRValue;

        public double ModifiedIRR

        {

            get { return this.ModifiedIRRValue; }

            set { SetProperty(ref ModifiedIRRValue, value); }

        }

        /// <summary>
        /// Equivalent Annual Cost for comparing projects of different lifespans
        /// </summary>
        private double EquivalentAnnualCostValue;

        public double EquivalentAnnualCost

        {

            get { return this.EquivalentAnnualCostValue; }

            set { SetProperty(ref EquivalentAnnualCostValue, value); }

        }

        /// <summary>
        /// Vestigial value (remaining value after cost recovery)
        /// </summary>
        private double VestigialValueValue;

        public double VestigialValue

        {

            get { return this.VestigialValueValue; }

            set { SetProperty(ref VestigialValueValue, value); }

        }
    }

    /// <summary>
    /// DTO for breakeven analysis results.
    /// </summary>
    public class BreakevenAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Price or cost level at which NPV = 0 (breakeven point)
        /// </summary>
        private double BreakevenPriceValue;

        public double BreakevenPrice

        {

            get { return this.BreakevenPriceValue; }

            set { SetProperty(ref BreakevenPriceValue, value); }

        }

        /// <summary>
        /// Margin of safety (distance from base case to breakeven)
        /// </summary>
        private double MarginOfSafetyValue;

        public double MarginOfSafety

        {

            get { return this.MarginOfSafetyValue; }

            set { SetProperty(ref MarginOfSafetyValue, value); }

        }

        /// <summary>
        /// Contribution margin percentage
        /// </summary>
        private double ContributionMarginValue;

        public double ContributionMargin

        {

            get { return this.ContributionMarginValue; }

            set { SetProperty(ref ContributionMarginValue, value); }

        }

        /// <summary>
        /// Detailed breakeven points at various variable levels
        /// </summary>
        private List<BreakevenPoint> BreakevenPointsValue;

        public List<BreakevenPoint> BreakevenPoints

        {

            get { return this.BreakevenPointsValue; }

            set { SetProperty(ref BreakevenPointsValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a single point in breakeven analysis.
    /// </summary>
    public class BreakevenPoint : ModelEntityBase
    {
        /// <summary>
        /// Variable value at this point (multiplier or percentage)
        /// </summary>
        private double VariableValue;

        public double Variable

        {

            get { return this.VariableValue; }

            set { SetProperty(ref VariableValue, value); }

        }

        /// <summary>
        /// NPV calculated at this variable level
        /// </summary>
        private double NPVAtVariableValue;

        public double NPVAtVariable

        {

            get { return this.NPVAtVariableValue; }

            set { SetProperty(ref NPVAtVariableValue, value); }

        }

        /// <summary>
        /// Indicates if this is (approximately) the breakeven point
        /// </summary>
        private bool IsBreakevenPointValue;

        public bool IsBreakevenPoint

        {

            get { return this.IsBreakevenPointValue; }

            set { SetProperty(ref IsBreakevenPointValue, value); }

        }
    }

    /// <summary>
    /// DTO for risk metrics analysis including VaR and CVaR.
    /// </summary>
    public class RiskMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the risk analysis
        /// </summary>
        private string RiskAnalysisIdValue;

        public string RiskAnalysisId

        {

            get { return this.RiskAnalysisIdValue; }

            set { SetProperty(ref RiskAnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Expected NPV across scenarios
        /// </summary>
        private double ExpectedNPVValue;

        public double ExpectedNPV

        {

            get { return this.ExpectedNPVValue; }

            set { SetProperty(ref ExpectedNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV distribution
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Variance of NPV distribution
        /// </summary>
        private double VarianceValue;

        public double Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }

        /// <summary>
        /// Coefficient of variation (risk per unit of return)
        /// </summary>
        private double CoefficientOfVariationValue;

        public double CoefficientOfVariation

        {

            get { return this.CoefficientOfVariationValue; }

            set { SetProperty(ref CoefficientOfVariationValue, value); }

        }

        /// <summary>
        /// Value at Risk at 95% confidence level (worst 5% of outcomes)
        /// </summary>
        private double ValueAtRiskValue;

        public double ValueAtRisk

        {

            get { return this.ValueAtRiskValue; }

            set { SetProperty(ref ValueAtRiskValue, value); }

        }

        /// <summary>
        /// Conditional Value at Risk (average of worst 5% outcomes)
        /// </summary>
        private double ConditionalVaRValue;

        public double ConditionalVaR

        {

            get { return this.ConditionalVaRValue; }

            set { SetProperty(ref ConditionalVaRValue, value); }

        }

        /// <summary>
        /// Probability of achieving negative NPV
        /// </summary>
        private double ProbabilityOfLossValue;

        public double ProbabilityOfLoss

        {

            get { return this.ProbabilityOfLossValue; }

            set { SetProperty(ref ProbabilityOfLossValue, value); }

        }
    }

    /// <summary>
    /// DTO for project comparison and ranking analysis.
    /// </summary>
    public class ProjectComparison : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the comparison
        /// </summary>
        private string ComparisonIdValue;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }

        /// <summary>
        /// Date the comparison was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Method used for ranking (NPV, IRR, PI, Payback)
        /// </summary>
        private string RankingMethodValue;

        public string RankingMethod

        {

            get { return this.RankingMethodValue; }

            set { SetProperty(ref RankingMethodValue, value); }

        }

        /// <summary>
        /// List of projects with their metrics
        /// </summary>
        private List<ProjectMetrics> ProjectsValue;

        public List<ProjectMetrics> Projects

        {

            get { return this.ProjectsValue; }

            set { SetProperty(ref ProjectsValue, value); }

        }

        /// <summary>
        /// Name of the highest-ranked (recommended) project
        /// </summary>
        private string RecommendedProjectValue;

        public string RecommendedProject

        {

            get { return this.RecommendedProjectValue; }

            set { SetProperty(ref RecommendedProjectValue, value); }

        }
    }

    /// <summary>
    /// DTO representing metrics and ranking for a single project.
    /// </summary>
    public class ProjectMetrics : ModelEntityBase
    {
        /// <summary>
        /// Name or identifier of the project
        /// </summary>
        private string ProjectNameValue;

        public string ProjectName

        {

            get { return this.ProjectNameValue; }

            set { SetProperty(ref ProjectNameValue, value); }

        }

        /// <summary>
        /// Net Present Value
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Payback period in years
        /// </summary>
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Profitability Index
        /// </summary>
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Rank among compared projects (1 = best)
        /// </summary>
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }

        /// <summary>
        /// Scoring points based on ranking
        /// </summary>
        private double ScoreValue;

        public double Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
    }
    public class EconomicAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ProjectIdValue;

        public string? ProjectId

        {

            get { return this.ProjectIdValue; }

            set { SetProperty(ref ProjectIdValue, value); }

        }
        private string AnalysisTypeValue = "NPV";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // NPV, IRR, PAYBACK, ROI, BREAKEVEN
        
        // Economic parameters
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private decimal? InflationRateValue;

        public decimal? InflationRate

        {

            get { return this.InflationRateValue; }

            set { SetProperty(ref InflationRateValue, value); }

        }
        private decimal? OperatingCostPerUnitValue;

        public decimal? OperatingCostPerUnit

        {

            get { return this.OperatingCostPerUnitValue; }

            set { SetProperty(ref OperatingCostPerUnitValue, value); }

        }
        private decimal? CapitalInvestmentValue;

        public decimal? CapitalInvestment

        {

            get { return this.CapitalInvestmentValue; }

            set { SetProperty(ref CapitalInvestmentValue, value); }

        }
        
        // Production forecast
        private List<EconomicProductionPoint>? ProductionForecastValue;

        public List<EconomicProductionPoint>? ProductionForecast

        {

            get { return this.ProductionForecastValue; }

            set { SetProperty(ref ProductionForecastValue, value); }

        }
        
        // Time parameters
        private DateTime? AnalysisStartDateValue;

        public DateTime? AnalysisStartDate

        {

            get { return this.AnalysisStartDateValue; }

            set { SetProperty(ref AnalysisStartDateValue, value); }

        }
        private DateTime? AnalysisEndDateValue;

        public DateTime? AnalysisEndDate

        {

            get { return this.AnalysisEndDateValue; }

            set { SetProperty(ref AnalysisEndDateValue, value); }

        }
        private int? AnalysisPeriodYearsValue;

        public int? AnalysisPeriodYears

        {

            get { return this.AnalysisPeriodYearsValue; }

            set { SetProperty(ref AnalysisPeriodYearsValue, value); }

        }
        
        // Fiscal terms
        private decimal? RoyaltyRateValue;

        public decimal? RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private decimal? TaxRateValue;

        public decimal? TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }
        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? ForecastIdValue;

        public string? ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private decimal? OperatingCostPerBarrelValue;

        public decimal? OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }
        private decimal? FixedOpexPerPeriodValue;

        public decimal? FixedOpexPerPeriod

        {

            get { return this.FixedOpexPerPeriodValue; }

            set { SetProperty(ref FixedOpexPerPeriodValue, value); }

        }
        private List<CapitalScheduleItem>? CapitalScheduleValue;

        public List<CapitalScheduleItem>? CapitalSchedule

        {

            get { return this.CapitalScheduleValue; }

            set { SetProperty(ref CapitalScheduleValue, value); }

        }
    }

    public class EconomicProductionPoint : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal? OilVolumeValue;

        public decimal? OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal? GasVolumeValue;

        public decimal? GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal? WaterVolumeValue;

        public decimal? WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }
        private decimal? OperatingCostValue;

        public decimal? OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private decimal? RevenueValue;

        public decimal? Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }
    }

    public class CapitalScheduleItem : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }

    public class EconomicAnalysisResult : ModelEntityBase
    {
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal NPVValue;

        public decimal NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private decimal IRRValue;

        public decimal IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }
        private decimal PaybackPeriodValue;

        public decimal PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }
        private decimal ROIValue;

        public decimal ROI

        {

            get { return this.ROIValue; }

            set { SetProperty(ref ROIValue, value); }

        }
        private decimal BreakevenPriceValue;

        public decimal BreakevenPrice

        {

            get { return this.BreakevenPriceValue; }

            set { SetProperty(ref BreakevenPriceValue, value); }

        }
        private List<double> CashFlowsValue;

        public List<double> CashFlows

        {

            get { return this.CashFlowsValue; }

            set { SetProperty(ref CashFlowsValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object>? AdditionalResults { get; set; }
    }
}




