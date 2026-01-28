using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class InvestmentEvaluationResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime EvaluationDateValue;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private double InitialCapexValue;

        public double InitialCapex

        {

            get { return this.InitialCapexValue; }

            set { SetProperty(ref InitialCapexValue, value); }

        }
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private List<double> ProjectedCashFlowsValue;

        public List<double> ProjectedCashFlows

        {

            get { return this.ProjectedCashFlowsValue; }

            set { SetProperty(ref ProjectedCashFlowsValue, value); }

        }
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private double TotalProjectValueValue;

        public double TotalProjectValue

        {

            get { return this.TotalProjectValueValue; }

            set { SetProperty(ref TotalProjectValueValue, value); }

        }
        private InvestmentEvaluationResult SensitivityAnalysisValue;

        public InvestmentEvaluationResult SensitivityAnalysis

        {

            get { return this.SensitivityAnalysisValue; }

            set { SetProperty(ref SensitivityAnalysisValue, value); }

        }
        private ScenarioAnalysisResult ScenarioAnalysisValue;

        public ScenarioAnalysisResult ScenarioAnalysis

        {

            get { return this.ScenarioAnalysisValue; }

            set { SetProperty(ref ScenarioAnalysisValue, value); }

        }
        private string InvestmentRatingValue;

        public string InvestmentRating

        {

            get { return this.InvestmentRatingValue; }

            set { SetProperty(ref InvestmentRatingValue, value); }

        }

        public double SensitivityLowCost { get; set; }
        public double SensitivityHighCost { get; set; }
        public double SensitivityLowPrice { get; set; }
        public double SensitivityHighPrice { get; set; }
    }
}
