using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProbabilisticAssessment : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal P10Value;

        public decimal P10

        {

            get { return this.P10Value; }

            set { SetProperty(ref P10Value, value); }

        }
        private decimal P50Value;

        public decimal P50

        {

            get { return this.P50Value; }

            set { SetProperty(ref P50Value, value); }

        }
        private decimal P90Value;

        public decimal P90

        {

            get { return this.P90Value; }

            set { SetProperty(ref P90Value, value); }

        }
    }
}
