using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentPhaseScheduleResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ScheduleDateValue;

        public DateTime ScheduleDate

        {

            get { return this.ScheduleDateValue; }

            set { SetProperty(ref ScheduleDateValue, value); }

        }
        private DateTime DevelopmentStartDateValue;

        public DateTime DevelopmentStartDate

        {

            get { return this.DevelopmentStartDateValue; }

            set { SetProperty(ref DevelopmentStartDateValue, value); }

        }
        private int PhaseCountValue;

        public int PhaseCount

        {

            get { return this.PhaseCountValue; }

            set { SetProperty(ref PhaseCountValue, value); }

        }
        private List<DevelopmentPhase> PhasesValue;

        public List<DevelopmentPhase> Phases

        {

            get { return this.PhasesValue; }

            set { SetProperty(ref PhasesValue, value); }

        }
        private int TotalProjectDurationValue;

        public int TotalProjectDuration

        {

            get { return this.TotalProjectDurationValue; }

            set { SetProperty(ref TotalProjectDurationValue, value); }

        }
        private string CriticalPathValue;

        public string CriticalPath

        {

            get { return this.CriticalPathValue; }

            set { SetProperty(ref CriticalPathValue, value); }

        }
        private DateTime ProjectCompletionValue;

        public DateTime ProjectCompletion

        {

            get { return this.ProjectCompletionValue; }

            set { SetProperty(ref ProjectCompletionValue, value); }

        }
    }
}
