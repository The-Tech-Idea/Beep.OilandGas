using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AnalogSearchRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> SearchCriteriaValue = new();

        public List<string> SearchCriteria

        {

            get { return this.SearchCriteriaValue; }

            set { SetProperty(ref SearchCriteriaValue, value); }

        }
        private decimal SimilarityThresholdValue = 0.7m;

        public decimal SimilarityThreshold

        {

            get { return this.SimilarityThresholdValue; }

            set { SetProperty(ref SimilarityThresholdValue, value); }

        }
    }
}
