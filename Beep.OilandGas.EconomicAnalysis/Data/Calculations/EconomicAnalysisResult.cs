using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EconomicAnalysisResult : ModelEntityBase
    {
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        public string CalculationId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
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

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        public DateTime CalculationDate

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
        public decimal NetPresentValue

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
        public decimal InternalRateOfReturn

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
        public decimal ReturnOnInvestment

        {

            get { return this.ROIValue; }

            set { SetProperty(ref ROIValue, value); }

        }
        private decimal ProfitabilityIndexValue;

        public decimal ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

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
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public EconomicAnalysisAdditionalResults? AdditionalResults { get; set; }
        public List<EconomicCashFlowPoint> CashFlowPoints { get; set; } = new List<EconomicCashFlowPoint>();
        public decimal? TotalRevenue { get; set; }
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? NetCashFlow { get; set; }
    }
}
