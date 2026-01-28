using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
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
        public EconomicAnalysisOptions? AdditionalParameters { get; set; }
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
}
