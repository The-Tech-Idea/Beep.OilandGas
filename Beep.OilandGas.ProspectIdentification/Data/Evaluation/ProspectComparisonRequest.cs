using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectComparisonRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        private List<string> ComparisonCriteriaValue = new();

        public List<string> ComparisonCriteria

        {

            get { return this.ComparisonCriteriaValue; }

            set { SetProperty(ref ComparisonCriteriaValue, value); }

        }
    }
}
