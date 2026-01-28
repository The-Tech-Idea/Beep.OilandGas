using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Operations
{
    public class RankProspectsRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        public Dictionary<string, decimal> RankingCriteria { get; set; } = new();
    }
}
