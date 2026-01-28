using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PlayAnalysis : ModelEntityBase
    {
        private string PlayIdValue = string.Empty;

        public string PlayId

        {

            get { return this.PlayIdValue; }

            set { SetProperty(ref PlayIdValue, value); }

        }
        private List<PlayStatistics> StatisticsValue = new();

        public List<PlayStatistics> Statistics

        {

            get { return this.StatisticsValue; }

            set { SetProperty(ref StatisticsValue, value); }

        }
    }
}
