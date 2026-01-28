using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
    public class ProductionTrendsRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? WellIdsValue;

        public List<string>? WellIds

        {

            get { return this.WellIdsValue; }

            set { SetProperty(ref WellIdsValue, value); }

        }
        private List<string>? LeaseIdsValue;

        public List<string>? LeaseIds

        {

            get { return this.LeaseIdsValue; }

            set { SetProperty(ref LeaseIdsValue, value); }

        }
    }
}
