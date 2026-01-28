using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class StrategyRanking : ModelEntityBase
    {
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }
        private string StrategyValue;

        public string Strategy

        {

            get { return this.StrategyValue; }

            set { SetProperty(ref StrategyValue, value); }

        }
        private double ScoreValue;

        public double Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
    }
}
