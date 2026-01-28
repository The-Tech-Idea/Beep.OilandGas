using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DecommissioningSchedule : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private int PriorityLevelValue;
        public int PriorityLevel
        {
            get { return this.PriorityLevelValue; }
            set { SetProperty(ref PriorityLevelValue, value); }
        }

        private DateTime PlannedStartDateValue;
        public DateTime PlannedStartDate
        {
            get { return this.PlannedStartDateValue; }
            set { SetProperty(ref PlannedStartDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<DecommissioningPhase> ProjectPhasesValue = new();
        public List<DecommissioningPhase> ProjectPhases
        {
            get { return this.ProjectPhasesValue; }
            set { SetProperty(ref ProjectPhasesValue, value); }
        }

        private int EstimatedTotalDaysValue;
        public int EstimatedTotalDays
        {
            get { return this.EstimatedTotalDaysValue; }
            set { SetProperty(ref EstimatedTotalDaysValue, value); }
        }

        private DateTime EstimatedCompletionDateValue;
        public DateTime EstimatedCompletionDate
        {
            get { return this.EstimatedCompletionDateValue; }
            set { SetProperty(ref EstimatedCompletionDateValue, value); }
        }

        private List<string> CriticalPathItemsValue = new();
        public List<string> CriticalPathItems
        {
            get { return this.CriticalPathItemsValue; }
            set { SetProperty(ref CriticalPathItemsValue, value); }
        }

        private string ScheduleRiskLevelValue = string.Empty;
        public string ScheduleRiskLevel
        {
            get { return this.ScheduleRiskLevelValue; }
            set { SetProperty(ref ScheduleRiskLevelValue, value); }
        }

        private int ContingencyDaysValue;
        public int ContingencyDays
        {
            get { return this.ContingencyDaysValue; }
            set { SetProperty(ref ContingencyDaysValue, value); }
        }

        private DateTime FinalEstimatedDateValue;
        public DateTime FinalEstimatedDate
        {
            get { return this.FinalEstimatedDateValue; }
            set { SetProperty(ref FinalEstimatedDateValue, value); }
        }

        private int EstimatedCrewSizeValue;
        public int EstimatedCrewSize
        {
            get { return this.EstimatedCrewSizeValue; }
            set { SetProperty(ref EstimatedCrewSizeValue, value); }
        }

        private List<string> EstimatedEquipmentNeedsValue = new();
        public List<string> EstimatedEquipmentNeeds
        {
            get { return this.EstimatedEquipmentNeedsValue; }
            set { SetProperty(ref EstimatedEquipmentNeedsValue, value); }
        }
    }
}
