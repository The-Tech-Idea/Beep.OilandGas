using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentPhase : ModelEntityBase
    {
        private int PhaseNumberValue;

        public int PhaseNumber

        {

            get { return this.PhaseNumberValue; }

            set { SetProperty(ref PhaseNumberValue, value); }

        }
        private string PhaseNameValue;

        public string PhaseName

        {

            get { return this.PhaseNameValue; }

            set { SetProperty(ref PhaseNameValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private int DurationValue;

        public int Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string> MilestonesValue;

        public List<string> Milestones

        {

            get { return this.MilestonesValue; }

            set { SetProperty(ref MilestonesValue, value); }

        }
        private List<int> DependenciesValue;

        public List<int> Dependencies

        {

            get { return this.DependenciesValue; }

            set { SetProperty(ref DependenciesValue, value); }

        }
    }
}
