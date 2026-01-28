using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DrillingProgramResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ProgramDateValue;

        public DateTime ProgramDate

        {

            get { return this.ProgramDateValue; }

            set { SetProperty(ref ProgramDateValue, value); }

        }
        private int TotalWellsPlannedValue;

        public int TotalWellsPlanned

        {

            get { return this.TotalWellsPlannedValue; }

            set { SetProperty(ref TotalWellsPlannedValue, value); }

        }
        private int PhaseDurationValue;

        public int PhaseDuration

        {

            get { return this.PhaseDurationValue; }

            set { SetProperty(ref PhaseDurationValue, value); }

        }
        public List<DevelopmentWellTypeDistributionEntry> WellTypeDistribution { get; set; } = new();
        private int RigsRequiredValue;

        public int RigsRequired

        {

            get { return this.RigsRequiredValue; }

            set { SetProperty(ref RigsRequiredValue, value); }

        }
        private int AverageDaysPerWellValue;

        public int AverageDaysPerWell

        {

            get { return this.AverageDaysPerWellValue; }

            set { SetProperty(ref AverageDaysPerWellValue, value); }

        }
        private List<DateTime> SpudScheduleValue;

        public List<DateTime> SpudSchedule

        {

            get { return this.SpudScheduleValue; }

            set { SetProperty(ref SpudScheduleValue, value); }

        }
        private List<DateTime> CompletionScheduleValue;

        public List<DateTime> CompletionSchedule

        {

            get { return this.CompletionScheduleValue; }

            set { SetProperty(ref CompletionScheduleValue, value); }

        }
        private List<string> OperationalChallengesValue;

        public List<string> OperationalChallenges

        {

            get { return this.OperationalChallengesValue; }

            set { SetProperty(ref OperationalChallengesValue, value); }

        }
        private List<string> LogisticsRequirementsValue;

        public List<string> LogisticsRequirements

        {

            get { return this.LogisticsRequirementsValue; }

            set { SetProperty(ref LogisticsRequirementsValue, value); }

        }
        private List<string> EnvironmentalConsiderationsValue;

        public List<string> EnvironmentalConsiderations

        {

            get { return this.EnvironmentalConsiderationsValue; }

            set { SetProperty(ref EnvironmentalConsiderationsValue, value); }

        }
    }
}
