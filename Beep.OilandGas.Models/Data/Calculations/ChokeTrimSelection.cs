using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeTrimSelection : ModelEntityBase
    {
        private string SelectionIdValue = string.Empty;

        public string SelectionId

        {

            get { return this.SelectionIdValue; }

            set { SetProperty(ref SelectionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // Bean, Venturi, Orifice
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal ExpectedSandRateValue;

        public decimal ExpectedSandRate

        {

            get { return this.ExpectedSandRateValue; }

            set { SetProperty(ref ExpectedSandRateValue, value); }

        }
        private decimal PredictedErosionRateValue;

        public decimal PredictedErosionRate

        {

            get { return this.PredictedErosionRateValue; }

            set { SetProperty(ref PredictedErosionRateValue, value); }

        }
        private List<TrimOption> TrimOptionsValue = new();

        public List<TrimOption> TrimOptions

        {

            get { return this.TrimOptionsValue; }

            set { SetProperty(ref TrimOptionsValue, value); }

        }
        private string RecommendedTrimValue = string.Empty;

        public string RecommendedTrim

        {

            get { return this.RecommendedTrimValue; }

            set { SetProperty(ref RecommendedTrimValue, value); }

        }
        private string RecommendedMaterialValue = string.Empty;

        public string RecommendedMaterial

        {

            get { return this.RecommendedMaterialValue; }

            set { SetProperty(ref RecommendedMaterialValue, value); }

        } // WC, Tungsten Carbide, 17-4 Steel
        private decimal EstimatedChokeLifeValue;

        public decimal EstimatedChokeLife

        {

            get { return this.EstimatedChokeLifeValue; }

            set { SetProperty(ref EstimatedChokeLifeValue, value); }

        } // years
        private decimal CostPerChokeValue;

        public decimal CostPerChoke

        {

            get { return this.CostPerChokeValue; }

            set { SetProperty(ref CostPerChokeValue, value); }

        }
        private decimal LifetimeCostValue;

        public decimal LifetimeCost

        {

            get { return this.LifetimeCostValue; }

            set { SetProperty(ref LifetimeCostValue, value); }

        } // $/year
        private string JustificationValue = string.Empty;

        public string Justification

        {

            get { return this.JustificationValue; }

            set { SetProperty(ref JustificationValue, value); }

        }
    }
}
