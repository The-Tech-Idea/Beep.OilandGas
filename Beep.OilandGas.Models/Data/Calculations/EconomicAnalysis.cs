using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

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
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Number of simulations performed
        /// </summary>
        public int SimulationCount { get; set; }

        /// <summary>
        /// Distribution of NPV values from all simulations
        /// </summary>
        public List<double> NPVDistribution { get; set; }

        /// <summary>
        /// Distribution of IRR values from all simulations
        /// </summary>
        public List<double> IRRDistribution { get; set; }

        /// <summary>
        /// Mean (average) NPV across all simulations
        /// </summary>
        public double MeanNPV { get; set; }

        /// <summary>
        /// Mean (average) IRR across all simulations
        /// </summary>
        public double MeanIRR { get; set; }

        /// <summary>
        /// Standard deviation of NPV distribution (volatility measure)
        /// </summary>
        public double StdDevNPV { get; set; }

        /// <summary>
        /// Standard deviation of IRR distribution (volatility measure)
        /// </summary>
        public double StdDevIRR { get; set; }

        /// <summary>
        /// 10th percentile NPV (downside scenario)
        /// </summary>
        public double P10NPV { get; set; }

        /// <summary>
        /// 50th percentile NPV (base case)
        /// </summary>
        public double P50NPV { get; set; }

        /// <summary>
        /// 90th percentile NPV (upside scenario)
        /// </summary>
        public double P90NPV { get; set; }

        /// <summary>
        /// Minimum NPV from all simulations
        /// </summary>
        public double MinNPV { get; set; }

        /// <summary>
        /// Maximum NPV from all simulations
        /// </summary>
        public double MaxNPV { get; set; }

        /// <summary>
        /// Probability of achieving negative NPV (risk of loss)
        /// </summary>
        public double ProbabilityOfLoss { get; set; }
    }

    /// <summary>
    /// Result DTO for real options analysis including expansion, abandonment, and switching options.
    /// </summary>
    public class RealOptionsAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Initial project NPV without considering options
        /// </summary>
        public double InitialNPV { get; set; }

        /// <summary>
        /// Expected project life in years
        /// </summary>
        public int ProjectLife { get; set; }

        /// <summary>
        /// Cash flow volatility (used for option valuation)
        /// </summary>
        public double Volatility { get; set; }

        /// <summary>
        /// List of option valuations (expansion, abandonment, switching)
        /// </summary>
        public List<OptionValuation> Options { get; set; }

        /// <summary>
        /// Total value of all strategic options combined
        /// </summary>
        public double TotalOptionValue { get; set; }

        /// <summary>
        /// Project value including option values
        /// </summary>
        public double ProjectValueWithOptions { get; set; }

        /// <summary>
        /// Flexibility premium as percentage of initial NPV
        /// </summary>
        public double FlexibilityPremium { get; set; }
    }

    /// <summary>
    /// DTO representing the valuation of a single strategic option.
    /// </summary>
    public class OptionValuation : ModelEntityBase
    {
        /// <summary>
        /// Type of option: Expansion, Abandonment, or Switching
        /// </summary>
        public string OptionType { get; set; }

        /// <summary>
        /// Calculated value of the option
        /// </summary>
        public double OptionValue { get; set; }

        /// <summary>
        /// Project value if the option scenario occurs
        /// </summary>
        public double ScenarioValue { get; set; }

        /// <summary>
        /// Cost to exercise the expansion option
        /// </summary>
        public double ExerciseCost { get; set; }

        /// <summary>
        /// Salvage value if abandonment option is exercised
        /// </summary>
        public double SalvageValue { get; set; }

        /// <summary>
        /// NPV of alternative project if switching option is exercised
        /// </summary>
        public double AlternativeNPV { get; set; }

        /// <summary>
        /// Cost to switch to alternative project
        /// </summary>
        public double SwitchingCost { get; set; }
    }

    /// <summary>
    /// Result DTO for decision tree analysis with multi-stage investment scenarios.
    /// </summary>
    public class DecisionTreeAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Initial capital investment required
        /// </summary>
        public double InitialInvestment { get; set; }

        /// <summary>
        /// Probability of success (0.0 to 1.0)
        /// </summary>
        public double SuccessProbability { get; set; }

        /// <summary>
        /// Success scenario with NPV and IRR
        /// </summary>
        public DecisionScenario SuccessScenario { get; set; }

        /// <summary>
        /// Failure scenario with NPV and IRR
        /// </summary>
        public DecisionScenario FailureScenario { get; set; }

        /// <summary>
        /// Expected NPV across all scenarios (probability-weighted)
        /// </summary>
        public double ExpectedNPV { get; set; }

        /// <summary>
        /// Variance of NPV across scenarios (risk measure)
        /// </summary>
        public double VarianceOfNPV { get; set; }

        /// <summary>
        /// Standard deviation of NPV (volatility)
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Decision recommendation: "Proceed" or "Do Not Proceed"
        /// </summary>
        public string Decision { get; set; }
    }

    /// <summary>
    /// DTO representing a scenario in decision tree analysis.
    /// </summary>
    public class DecisionScenario : ModelEntityBase
    {
        /// <summary>
        /// Name of the scenario (e.g., "Success", "Failure")
        /// </summary>
        public string ScenarioName { get; set; }

        /// <summary>
        /// Probability of this scenario occurring
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Net Present Value of this scenario
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return for this scenario
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Cumulative cash flow across all periods in this scenario
        /// </summary>
        public double CumulativeCashFlow { get; set; }
    }

    /// <summary>
    /// Result DTO for after-tax economic analysis considering corporate taxation and depreciation.
    /// </summary>
    public class AfterTaxAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Corporate tax rate applied (as decimal, e.g., 0.35 for 35%)
        /// </summary>
        public double TaxRate { get; set; }

        /// <summary>
        /// NPV before considering tax impacts
        /// </summary>
        public double PreTaxNPV { get; set; }

        /// <summary>
        /// NPV after tax deductions and depreciation benefits
        /// </summary>
        public double AfterTaxNPV { get; set; }

        /// <summary>
        /// Internal Rate of Return after tax adjustments
        /// </summary>
        public double AfterTaxIRR { get; set; }

        /// <summary>
        /// Tax shield value from depreciation and deductions
        /// </summary>
        public double TaxShield { get; set; }

        /// <summary>
        /// Effective tax rate on the project
        /// </summary>
        public double EffectiveTaxRate { get; set; }

        /// <summary>
        /// After-tax cash flows by period
        /// </summary>
        public List<CashFlow> AfterTaxCashFlows { get; set; }
    }

    /// <summary>
    /// Result DTO for DCF (Discounted Cash Flow) enterprise valuation.
    /// </summary>
    public class DCFValuationResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Terminal growth rate beyond explicit forecast period
        /// </summary>
        public double TerminalGrowthRate { get; set; }

        /// <summary>
        /// Weighted Average Cost of Capital (discount rate)
        /// </summary>
        public double WACC { get; set; }

        /// <summary>
        /// Present value components for each year of the forecast period
        /// </summary>
        public List<PVComponent> PresentValueComponents { get; set; }

        /// <summary>
        /// PV of explicit forecast period cash flows
        /// </summary>
        public double ExplicitPeriodValue { get; set; }

        /// <summary>
        /// Terminal value of cash flows beyond forecast period
        /// </summary>
        public double TerminalValue { get; set; }

        /// <summary>
        /// Present value of terminal value
        /// </summary>
        public double PVTerminalValue { get; set; }

        /// <summary>
        /// Total enterprise value (sum of explicit period and terminal values)
        /// </summary>
        public double EnterpriseValue { get; set; }

        /// <summary>
        /// Percentage of enterprise value attributable to terminal value
        /// </summary>
        public double TerminalValuePercentage { get; set; }
    }

    /// <summary>
    /// DTO representing a single year's present value component in DCF analysis.
    /// </summary>
    public class PVComponent : ModelEntityBase
    {
        /// <summary>
        /// Forecast year number
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Cash flow in this year (before discounting)
        /// </summary>
        public double CashFlow { get; set; }

        /// <summary>
        /// Discount factor applied (1 / (1 + WACC)^year)
        /// </summary>
        public double DiscountFactor { get; set; }

        /// <summary>
        /// Present value of the cash flow
        /// </summary>
        public double PresentValue { get; set; }
    }

    /// <summary>
    /// Result DTO for lease vs. buy financial decision analysis.
    /// </summary>
    public class LeaseBuyAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Cost to purchase the asset
        /// </summary>
        public double AssetCost { get; set; }

        /// <summary>
        /// Duration of lease in years
        /// </summary>
        public int LeaseTerm { get; set; }

        /// <summary>
        /// Residual value of asset at end of lease term
        /// </summary>
        public double SalvageValue { get; set; }

        /// <summary>
        /// Net Present Value of buying option
        /// </summary>
        public double BuyNPV { get; set; }

        /// <summary>
        /// Net Present Value of leasing option
        /// </summary>
        public double LeaseNPV { get; set; }

        /// <summary>
        /// Net advantage of leasing vs buying (positive favors leasing)
        /// </summary>
        public double NetAdvantageOfLeasing { get; set; }

        /// <summary>
        /// Recommendation: "Lease" or "Buy"
        /// </summary>
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// Result DTO for capital structure optimization analysis.
    /// </summary>
    public class CapitalStructureAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Unlevered (all-equity) firm value
        /// </summary>
        public double UnleveredValue { get; set; }

        /// <summary>
        /// Corporate tax rate used in analysis
        /// </summary>
        public double TaxRate { get; set; }

        /// <summary>
        /// Capital structure scenarios analyzed (different debt ratios)
        /// </summary>
        public List<CapitalStructureScenario> Scenarios { get; set; }

        /// <summary>
        /// Optimal debt ratio that minimizes WACC
        /// </summary>
        public double OptimalDebtRatio { get; set; }

        /// <summary>
        /// Weighted Average Cost of Capital at optimal capital structure
        /// </summary>
        public double OptimalWACC { get; set; }

        /// <summary>
        /// Levered firm value at optimal capital structure
        /// </summary>
        public double OptimalLeveredValue { get; set; }

        /// <summary>
        /// Value created by using optimal capital structure
        /// </summary>
        public double ValueCreation { get; set; }
    }

    /// <summary>
    /// DTO representing a single capital structure scenario with different debt ratios.
    /// </summary>
    public class CapitalStructureScenario : ModelEntityBase
    {
        /// <summary>
        /// Debt as percentage of total capital (0.0 to 1.0)
        /// </summary>
        public double DebtRatio { get; set; }

        /// <summary>
        /// Equity as percentage of total capital (0.0 to 1.0)
        /// </summary>
        public double EquityRatio { get; set; }

        /// <summary>
        /// Total market value of debt
        /// </summary>
        public double DebtValue { get; set; }

        /// <summary>
        /// Total market value of equity
        /// </summary>
        public double EquityValue { get; set; }

        /// <summary>
        /// Tax benefit from interest deductions
        /// </summary>
        public double TaxShield { get; set; }

        /// <summary>
        /// Levered firm value with this capital structure
        /// </summary>
        public double LeveredValue { get; set; }

        /// <summary>
        /// Weighted Average Cost of Capital for this structure
        /// </summary>
        public double WACC { get; set; }

        /// <summary>
        /// Financial risk classification: Low, Medium, or High
        /// </summary>
        public string FinancialRisk { get; set; }
    }

    /// <summary>
    /// Result DTO for commodity price sensitivity analysis.
    /// </summary>
    public class CommodityPriceSensitivityResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Base commodity price used in analysis
        /// </summary>
        public double BasePrice { get; set; }

        /// <summary>
        /// Scenarios showing NPV and IRR at different price points
        /// </summary>
        public List<PriceScenario> PriceScenarios { get; set; }

        /// <summary>
        /// Price at which project NPV breaks even (approximately zero)
        /// </summary>
        public double BreakevenPrice { get; set; }

        /// <summary>
        /// NPV at base price point
        /// </summary>
        public double BaseNPV { get; set; }
    }

    /// <summary>
    /// DTO representing project economics at a specific commodity price point.
    /// </summary>
    public class PriceScenario : ModelEntityBase
    {
        /// <summary>
        /// Commodity price for this scenario
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Net Present Value at this price
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return at this price
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Indicates if this is the breakeven price point
        /// </summary>
        public bool IsBreakeven { get; set; }
    }

    /// <summary>
    /// DTO for sensitivity analysis results showing how parameters affect NPV.
    /// </summary>
    public class SensitivityAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Base case NPV
        /// </summary>
        public double BaseNPV { get; set; }

        /// <summary>
        /// Base case IRR
        /// </summary>
        public double BaseIRR { get; set; }

        /// <summary>
        /// List of parameters tested for sensitivity
        /// </summary>
        public List<SensitivityParameter> Parameters { get; set; }
    }

    /// <summary>
    /// DTO representing sensitivity of a single parameter.
    /// </summary>
    public class SensitivityParameter : ModelEntityBase
    {
        /// <summary>
        /// Name of the parameter being tested
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Base value of the parameter
        /// </summary>
        public double BaseValue { get; set; }

        /// <summary>
        /// NPV with parameter reduced by 10%
        /// </summary>
        public double NegativeVariationNPV { get; set; }

        /// <summary>
        /// NPV with parameter increased by 10%
        /// </summary>
        public double PositiveVariationNPV { get; set; }

        /// <summary>
        /// Absolute impact on NPV from variation
        /// </summary>
        public double NPVImpact { get; set; }

        /// <summary>
        /// Sensitivity index (percentage change in NPV per 1% change in parameter)
        /// </summary>
        public double SensitivityIndex { get; set; }
    }

    /// <summary>
    /// DTO for scenario analysis with best, base, and worst case results.
    /// </summary>
    public class ScenarioAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// NPV in base case scenario
        /// </summary>
        public double BaseNPV { get; set; }

        /// <summary>
        /// NPV in best case scenario
        /// </summary>
        public double BestCaseNPV { get; set; }

        /// <summary>
        /// NPV in worst case scenario
        /// </summary>
        public double WorstCaseNPV { get; set; }

        /// <summary>
        /// Probability-weighted expected NPV
        /// </summary>
        public double ExpectedValueNPV { get; set; }

        /// <summary>
        /// Standard deviation of NPV across scenarios
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Coefficient of variation (stdDev / mean)
        /// </summary>
        public double CoefficientOfVariation { get; set; }

        /// <summary>
        /// Detailed results for each scenario
        /// </summary>
        public List<ScenarioResult> Scenarios { get; set; }
    }

    /// <summary>
    /// DTO representing a single scenario's results in scenario analysis.
    /// </summary>
    public class ScenarioResult : ModelEntityBase
    {
        /// <summary>
        /// Name of the scenario (e.g., "Best Case", "Base Case", "Worst Case")
        /// </summary>
        public string ScenarioName { get; set; }

        /// <summary>
        /// Probability of this scenario occurring
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Net Present Value for this scenario
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return for this scenario
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Payback period for this scenario (in years)
        /// </summary>
        public double PaybackPeriod { get; set; }
    }

    /// <summary>
    /// DTO for comprehensive financial metrics including NPV, IRR, ROI, and profitability measures.
    /// </summary>
    public class FinancialMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the metrics calculation
        /// </summary>
        public string MetricsId { get; set; }

        /// <summary>
        /// Date the metrics were calculated
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Net Present Value
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Payback period in years
        /// </summary>
        public double PaybackPeriod { get; set; }

        /// <summary>
        /// Profitability Index (PV of future cash flows / Initial investment)
        /// </summary>
        public double ProfitabilityIndex { get; set; }

        /// <summary>
        /// Return on Investment
        /// </summary>
        public double ROI { get; set; }

        /// <summary>
        /// Modified Internal Rate of Return (accounts for financing and reinvestment)
        /// </summary>
        public double ModifiedIRR { get; set; }

        /// <summary>
        /// Equivalent Annual Cost for comparing projects of different lifespans
        /// </summary>
        public double EquivalentAnnualCost { get; set; }

        /// <summary>
        /// Vestigial value (remaining value after cost recovery)
        /// </summary>
        public double VestigialValue { get; set; }
    }

    /// <summary>
    /// DTO for breakeven analysis results.
    /// </summary>
    public class BreakevenAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Price or cost level at which NPV = 0 (breakeven point)
        /// </summary>
        public double BreakevenPrice { get; set; }

        /// <summary>
        /// Margin of safety (distance from base case to breakeven)
        /// </summary>
        public double MarginOfSafety { get; set; }

        /// <summary>
        /// Contribution margin percentage
        /// </summary>
        public double ContributionMargin { get; set; }

        /// <summary>
        /// Detailed breakeven points at various variable levels
        /// </summary>
        public List<BreakevenPoint> BreakevenPoints { get; set; }
    }

    /// <summary>
    /// DTO representing a single point in breakeven analysis.
    /// </summary>
    public class BreakevenPoint : ModelEntityBase
    {
        /// <summary>
        /// Variable value at this point (multiplier or percentage)
        /// </summary>
        public double Variable { get; set; }

        /// <summary>
        /// NPV calculated at this variable level
        /// </summary>
        public double NPVAtVariable { get; set; }

        /// <summary>
        /// Indicates if this is (approximately) the breakeven point
        /// </summary>
        public bool IsBreakevenPoint { get; set; }
    }

    /// <summary>
    /// DTO for risk metrics analysis including VaR and CVaR.
    /// </summary>
    public class RiskMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the risk analysis
        /// </summary>
        public string RiskAnalysisId { get; set; }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Expected NPV across scenarios
        /// </summary>
        public double ExpectedNPV { get; set; }

        /// <summary>
        /// Standard deviation of NPV distribution
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Variance of NPV distribution
        /// </summary>
        public double Variance { get; set; }

        /// <summary>
        /// Coefficient of variation (risk per unit of return)
        /// </summary>
        public double CoefficientOfVariation { get; set; }

        /// <summary>
        /// Value at Risk at 95% confidence level (worst 5% of outcomes)
        /// </summary>
        public double ValueAtRisk { get; set; }

        /// <summary>
        /// Conditional Value at Risk (average of worst 5% outcomes)
        /// </summary>
        public double ConditionalVaR { get; set; }

        /// <summary>
        /// Probability of achieving negative NPV
        /// </summary>
        public double ProbabilityOfLoss { get; set; }
    }

    /// <summary>
    /// DTO for project comparison and ranking analysis.
    /// </summary>
    public class ProjectComparison : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the comparison
        /// </summary>
        public string ComparisonId { get; set; }

        /// <summary>
        /// Date the comparison was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Method used for ranking (NPV, IRR, PI, Payback)
        /// </summary>
        public string RankingMethod { get; set; }

        /// <summary>
        /// List of projects with their metrics
        /// </summary>
        public List<ProjectMetrics> Projects { get; set; }

        /// <summary>
        /// Name of the highest-ranked (recommended) project
        /// </summary>
        public string RecommendedProject { get; set; }
    }

    /// <summary>
    /// DTO representing metrics and ranking for a single project.
    /// </summary>
    public class ProjectMetrics : ModelEntityBase
    {
        /// <summary>
        /// Name or identifier of the project
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Net Present Value
        /// </summary>
        public double NPV { get; set; }

        /// <summary>
        /// Internal Rate of Return
        /// </summary>
        public double IRR { get; set; }

        /// <summary>
        /// Payback period in years
        /// </summary>
        public double PaybackPeriod { get; set; }

        /// <summary>
        /// Profitability Index
        /// </summary>
        public double ProfitabilityIndex { get; set; }

        /// <summary>
        /// Rank among compared projects (1 = best)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Scoring points based on ranking
        /// </summary>
        public double Score { get; set; }
    }
}

