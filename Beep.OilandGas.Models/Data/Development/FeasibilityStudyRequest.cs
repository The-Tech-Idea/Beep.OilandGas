using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FeasibilityStudyRequest : ModelEntityBase
    {
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

        } // e.g., "ECONOMIC", "TECHNICAL", "REGULATORY", "COMPREHENSIVE"
        
        // Project scope
        private int? NumberOfWellsValue;

        public int? NumberOfWells

        {

            get { return this.NumberOfWellsValue; }

            set { SetProperty(ref NumberOfWellsValue, value); }

        }
        private int? NumberOfFacilitiesValue;

        public int? NumberOfFacilities

        {

            get { return this.NumberOfFacilitiesValue; }

            set { SetProperty(ref NumberOfFacilitiesValue, value); }

        }
        private decimal? DevelopmentAreaValue;

        public decimal? DevelopmentArea

        {

            get { return this.DevelopmentAreaValue; }

            set { SetProperty(ref DevelopmentAreaValue, value); }

        }
        private string? DevelopmentAreaOuomValue;

        public string? DevelopmentAreaOuom

        {

            get { return this.DevelopmentAreaOuomValue; }

            set { SetProperty(ref DevelopmentAreaOuomValue, value); }

        }
        
        // Capital costs
        private decimal? DrillingCostPerWellValue;

        public decimal? DrillingCostPerWell

        {

            get { return this.DrillingCostPerWellValue; }

            set { SetProperty(ref DrillingCostPerWellValue, value); }

        }
        private string? DrillingCostCurrencyValue;

        public string? DrillingCostCurrency

        {

            get { return this.DrillingCostCurrencyValue; }

            set { SetProperty(ref DrillingCostCurrencyValue, value); }

        }
        private decimal? CompletionCostPerWellValue;

        public decimal? CompletionCostPerWell

        {

            get { return this.CompletionCostPerWellValue; }

            set { SetProperty(ref CompletionCostPerWellValue, value); }

        }
        private string? CompletionCostCurrencyValue;

        public string? CompletionCostCurrency

        {

            get { return this.CompletionCostCurrencyValue; }

            set { SetProperty(ref CompletionCostCurrencyValue, value); }

        }
        private decimal? FacilityCostValue;

        public decimal? FacilityCost

        {

            get { return this.FacilityCostValue; }

            set { SetProperty(ref FacilityCostValue, value); }

        }
        private string? FacilityCostCurrencyValue;

        public string? FacilityCostCurrency

        {

            get { return this.FacilityCostCurrencyValue; }

            set { SetProperty(ref FacilityCostCurrencyValue, value); }

        }
        private decimal? InfrastructureCostValue;

        public decimal? InfrastructureCost

        {

            get { return this.InfrastructureCostValue; }

            set { SetProperty(ref InfrastructureCostValue, value); }

        } // Pipelines, roads, etc.
        private string? InfrastructureCostCurrencyValue;

        public string? InfrastructureCostCurrency

        {

            get { return this.InfrastructureCostCurrencyValue; }

            set { SetProperty(ref InfrastructureCostCurrencyValue, value); }

        }
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
        
        // Operating costs
        private decimal? OperatingCostPerUnitValue;

        public decimal? OperatingCostPerUnit

        {

            get { return this.OperatingCostPerUnitValue; }

            set { SetProperty(ref OperatingCostPerUnitValue, value); }

        } // Per volume unit
        private string? OperatingCostCurrencyValue;

        public string? OperatingCostCurrency

        {

            get { return this.OperatingCostCurrencyValue; }

            set { SetProperty(ref OperatingCostCurrencyValue, value); }

        }
        private decimal? AnnualOperatingCostValue;

        public decimal? AnnualOperatingCost

        {

            get { return this.AnnualOperatingCostValue; }

            set { SetProperty(ref AnnualOperatingCostValue, value); }

        }
        private string? AnnualOperatingCostCurrencyValue;

        public string? AnnualOperatingCostCurrency

        {

            get { return this.AnnualOperatingCostCurrencyValue; }

            set { SetProperty(ref AnnualOperatingCostCurrencyValue, value); }

        }
        
        // Production forecast
        private List<FeasibilityProductionPoint>? ProductionForecastValue;

        public List<FeasibilityProductionPoint>? ProductionForecast

        {

            get { return this.ProductionForecastValue; }

            set { SetProperty(ref ProductionForecastValue, value); }

        }
        private int? ProductionLifetimeYearsValue;

        public int? ProductionLifetimeYears

        {

            get { return this.ProductionLifetimeYearsValue; }

            set { SetProperty(ref ProductionLifetimeYearsValue, value); }

        }
        
        // Economic parameters
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private string? OilPriceCurrencyValue;

        public string? OilPriceCurrency

        {

            get { return this.OilPriceCurrencyValue; }

            set { SetProperty(ref OilPriceCurrencyValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private string? GasPriceCurrencyValue;

        public string? GasPriceCurrency

        {

            get { return this.GasPriceCurrencyValue; }

            set { SetProperty(ref GasPriceCurrencyValue, value); }

        }
        private decimal? DiscountRateValue;

        public decimal? DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        } // Percentage
        private decimal? InflationRateValue;

        public decimal? InflationRate

        {

            get { return this.InflationRateValue; }

            set { SetProperty(ref InflationRateValue, value); }

        } // Percentage
        
        // Fiscal terms
        private decimal? RoyaltyRateValue;

        public decimal? RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        } // Percentage
        private decimal? TaxRateValue;

        public decimal? TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        } // Percentage
        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        } // Percentage
        
        // Technical constraints
        private decimal? MaximumProductionRateValue;

        public decimal? MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        }
        private string? MaximumProductionRateOuomValue;

        public string? MaximumProductionRateOuom

        {

            get { return this.MaximumProductionRateOuomValue; }

            set { SetProperty(ref MaximumProductionRateOuomValue, value); }

        }
        private List<string>? RegulatoryRequirementsValue;

        public List<string>? RegulatoryRequirements

        {

            get { return this.RegulatoryRequirementsValue; }

            set { SetProperty(ref RegulatoryRequirementsValue, value); }

        }
        private List<string>? TechnicalConstraintsValue;

        public List<string>? TechnicalConstraints

        {

            get { return this.TechnicalConstraintsValue; }

            set { SetProperty(ref TechnicalConstraintsValue, value); }

        }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
