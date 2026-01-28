using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RankedEORMethod : ModelEntityBase
    {
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }
        private string MethodNameValue;

        public string MethodName

        {

            get { return this.MethodNameValue; }

            set { SetProperty(ref MethodNameValue, value); }

        }
        private EORMethodScore ScoreValue;

        public EORMethodScore Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
    }
}
