using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class TradeOffAnalysisResult : ModelEntityBase
    {
        private string CostVsScheduleValue;

        public string CostVsSchedule

        {

            get { return this.CostVsScheduleValue; }

            set { SetProperty(ref CostVsScheduleValue, value); }

        }
        private string RiskVsRewardValue;

        public string RiskVsReward

        {

            get { return this.RiskVsRewardValue; }

            set { SetProperty(ref RiskVsRewardValue, value); }

        }
        private string EnvironmentalVsEconomicValue;

        public string EnvironmentalVsEconomic

        {

            get { return this.EnvironmentalVsEconomicValue; }

            set { SetProperty(ref EnvironmentalVsEconomicValue, value); }

        }
    }
}
