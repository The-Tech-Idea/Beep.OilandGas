using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FeasibilityStudyResponse : ModelEntityBase
    {
        private string StudyIdValue = string.Empty;

        public string StudyId

        {

            get { return this.StudyIdValue; }

            set { SetProperty(ref StudyIdValue, value); }

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
        private string? StudyTypeValue;

        public string? StudyType

        {

            get { return this.StudyTypeValue; }

            set { SetProperty(ref StudyTypeValue, value); }

        }
        private DateTime StudyDateValue;

        public DateTime StudyDate

        {

            get { return this.StudyDateValue; }

            set { SetProperty(ref StudyDateValue, value); }

        }
        
        // Economic feasibility
        private decimal? NetPresentValueValue;

        public decimal? NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        } // NPV
        private decimal? InternalRateOfReturnValue;

        public decimal? InternalRateOfReturn

        {

            get { return this.InternalRateOfReturnValue; }

            set { SetProperty(ref InternalRateOfReturnValue, value); }

        } // IRR (percentage)
        private decimal? PaybackPeriodValue;

        public decimal? PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        } // Years
        private decimal? ReturnOnInvestmentValue;

        public decimal? ReturnOnInvestment

        {

            get { return this.ReturnOnInvestmentValue; }

            set { SetProperty(ref ReturnOnInvestmentValue, value); }

        } // ROI (percentage)
        private decimal? ProfitabilityIndexValue;

        public decimal? ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private bool? IsEconomicallyFeasibleValue;

        public bool? IsEconomicallyFeasible

        {

            get { return this.IsEconomicallyFeasibleValue; }

            set { SetProperty(ref IsEconomicallyFeasibleValue, value); }

        }
        
        // Technical feasibility
        private bool? IsTechnicallyFeasibleValue;

        public bool? IsTechnicallyFeasible

        {

            get { return this.IsTechnicallyFeasibleValue; }

            set { SetProperty(ref IsTechnicallyFeasibleValue, value); }

        }
        private List<string> TechnicalChallengesValue = new List<string>();

        public List<string> TechnicalChallenges

        {

            get { return this.TechnicalChallengesValue; }

            set { SetProperty(ref TechnicalChallengesValue, value); }

        }
        private List<string> TechnicalRecommendationsValue = new List<string>();

        public List<string> TechnicalRecommendations

        {

            get { return this.TechnicalRecommendationsValue; }

            set { SetProperty(ref TechnicalRecommendationsValue, value); }

        }
        
        // Regulatory feasibility
        private bool? IsRegulatorilyFeasibleValue;

        public bool? IsRegulatorilyFeasible

        {

            get { return this.IsRegulatorilyFeasibleValue; }

            set { SetProperty(ref IsRegulatorilyFeasibleValue, value); }

        }
        private List<string> RegulatoryRequirementsValue = new List<string>();

        public List<string> RegulatoryRequirements

        {

            get { return this.RegulatoryRequirementsValue; }

            set { SetProperty(ref RegulatoryRequirementsValue, value); }

        }
        private List<string> RegulatoryChallengesValue = new List<string>();

        public List<string> RegulatoryChallenges

        {

            get { return this.RegulatoryChallengesValue; }

            set { SetProperty(ref RegulatoryChallengesValue, value); }

        }
        
        // Overall feasibility assessment
        private bool? IsFeasibleValue;

        public bool? IsFeasible

        {

            get { return this.IsFeasibleValue; }

            set { SetProperty(ref IsFeasibleValue, value); }

        }
        private string? FeasibilityStatusValue;

        public string? FeasibilityStatus

        {

            get { return this.FeasibilityStatusValue; }

            set { SetProperty(ref FeasibilityStatusValue, value); }

        } // e.g., "FEASIBLE", "MARGINAL", "NOT_FEASIBLE"
        private string? FeasibilityRecommendationValue;

        public string? FeasibilityRecommendation

        {

            get { return this.FeasibilityRecommendationValue; }

            set { SetProperty(ref FeasibilityRecommendationValue, value); }

        } // Overall recommendation
        
        // Cost analysis
        private decimal? TotalCapitalCostValue;

        public decimal? TotalCapitalCost

        {

            get { return this.TotalCapitalCostValue; }

            set { SetProperty(ref TotalCapitalCostValue, value); }

        }
        private string? TotalCapitalCostCurrencyValue;

        public string? TotalCapitalCostCurrency

        {

            get { return this.TotalCapitalCostCurrencyValue; }

            set { SetProperty(ref TotalCapitalCostCurrencyValue, value); }

        }
        private decimal? TotalOperatingCostValue;

        public decimal? TotalOperatingCost

        {

            get { return this.TotalOperatingCostValue; }

            set { SetProperty(ref TotalOperatingCostValue, value); }

        }
        private string? TotalOperatingCostCurrencyValue;

        public string? TotalOperatingCostCurrency

        {

            get { return this.TotalOperatingCostCurrencyValue; }

            set { SetProperty(ref TotalOperatingCostCurrencyValue, value); }

        }
        private decimal? TotalRevenueValue;

        public decimal? TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private string? TotalRevenueCurrencyValue;

        public string? TotalRevenueCurrency

        {

            get { return this.TotalRevenueCurrencyValue; }

            set { SetProperty(ref TotalRevenueCurrencyValue, value); }

        }
        private decimal? NetCashFlowValue;

        public decimal? NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
        
        // Cash flow analysis
        private List<FeasibilityCashFlowPoint> CashFlowPointsValue = new List<FeasibilityCashFlowPoint>();

        public List<FeasibilityCashFlowPoint> CashFlowPoints

        {

            get { return this.CashFlowPointsValue; }

            set { SetProperty(ref CashFlowPointsValue, value); }

        }
        
        // Production analysis
        private decimal? TotalOilProductionValue;

        public decimal? TotalOilProduction

        {

            get { return this.TotalOilProductionValue; }

            set { SetProperty(ref TotalOilProductionValue, value); }

        }
        private decimal? TotalGasProductionValue;

        public decimal? TotalGasProduction

        {

            get { return this.TotalGasProductionValue; }

            set { SetProperty(ref TotalGasProductionValue, value); }

        }
        private string? ProductionOuomValue;

        public string? ProductionOuom

        {

            get { return this.ProductionOuomValue; }

            set { SetProperty(ref ProductionOuomValue, value); }

        }
        private decimal? PeakProductionRateValue;

        public decimal? PeakProductionRate

        {

            get { return this.PeakProductionRateValue; }

            set { SetProperty(ref PeakProductionRateValue, value); }

        }
        private int? PeakProductionYearValue;

        public int? PeakProductionYear

        {

            get { return this.PeakProductionYearValue; }

            set { SetProperty(ref PeakProductionYearValue, value); }

        }
        
        // Risk assessment
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        } // e.g., "LOW", "MEDIUM", "HIGH"
        private List<string> KeyRisksValue = new List<string>();

        public List<string> KeyRisks

        {

            get { return this.KeyRisksValue; }

            set { SetProperty(ref KeyRisksValue, value); }

        }
        private List<string> RiskMitigationStrategiesValue = new List<string>();

        public List<string> RiskMitigationStrategies

        {

            get { return this.RiskMitigationStrategiesValue; }

            set { SetProperty(ref RiskMitigationStrategiesValue, value); }

        }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
