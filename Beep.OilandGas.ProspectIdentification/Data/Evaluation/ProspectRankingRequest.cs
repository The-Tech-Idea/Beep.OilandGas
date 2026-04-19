using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectRankingRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        private List<string> CriteriaValue = new();

        public List<string> Criteria

        {

            get { return this.CriteriaValue; }

            set { SetProperty(ref CriteriaValue, value); }

        }
    }
}
