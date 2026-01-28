using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class StrategiesComparisonResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ComparisonDateValue;

        public DateTime ComparisonDate

        {

            get { return this.ComparisonDateValue; }

            set { SetProperty(ref ComparisonDateValue, value); }

        }
        private int StrategyCountValue;

        public int StrategyCount

        {

            get { return this.StrategyCountValue; }

            set { SetProperty(ref StrategyCountValue, value); }

        }
        private List<DevelopmentStrategy> StrategiesValue;

        public List<DevelopmentStrategy> Strategies

        {

            get { return this.StrategiesValue; }

            set { SetProperty(ref StrategiesValue, value); }

        }
        private CostComparisonResult CostComparisonValue;

        public CostComparisonResult CostComparison

        {

            get { return this.CostComparisonValue; }

            set { SetProperty(ref CostComparisonValue, value); }

        }
        private EconomicComparisonResult EconomicComparisonValue;

        public EconomicComparisonResult EconomicComparison

        {

            get { return this.EconomicComparisonValue; }

            set { SetProperty(ref EconomicComparisonValue, value); }

        }
        private RiskComparisonResult RiskComparisonValue;

        public RiskComparisonResult RiskComparison

        {

            get { return this.RiskComparisonValue; }

            set { SetProperty(ref RiskComparisonValue, value); }

        }
        private ScheduleComparisonResult ScheduleComparisonValue;

        public ScheduleComparisonResult ScheduleComparison

        {

            get { return this.ScheduleComparisonValue; }

            set { SetProperty(ref ScheduleComparisonValue, value); }

        }
        private EnvironmentalComparisonResult EnvironmentalComparisonValue;

        public EnvironmentalComparisonResult EnvironmentalComparison

        {

            get { return this.EnvironmentalComparisonValue; }

            set { SetProperty(ref EnvironmentalComparisonValue, value); }

        }
        private string RecommendedStrategyValue;

        public string RecommendedStrategy

        {

            get { return this.RecommendedStrategyValue; }

            set { SetProperty(ref RecommendedStrategyValue, value); }

        }
        private List<StrategyRanking> AlternativeRankingsValue;

        public List<StrategyRanking> AlternativeRankings

        {

            get { return this.AlternativeRankingsValue; }

            set { SetProperty(ref AlternativeRankingsValue, value); }

        }
        private TradeOffAnalysisResult TradeOffAnalysisValue;

        public TradeOffAnalysisResult TradeOffAnalysis

        {

            get { return this.TradeOffAnalysisValue; }

            set { SetProperty(ref TradeOffAnalysisValue, value); }

        }
    }
}
