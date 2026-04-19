using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AnalogProspect : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal SimilarityScoreValue;

        public decimal SimilarityScore

        {

            get { return this.SimilarityScoreValue; }

            set { SetProperty(ref SimilarityScoreValue, value); }

        }
    }
}
