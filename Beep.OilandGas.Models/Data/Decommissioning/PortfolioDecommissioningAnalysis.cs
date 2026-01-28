using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PortfolioDecommissioningAnalysis : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;
        public string FieldId
        {
            get { return this.FieldIdValue; }
            set { SetProperty(ref FieldIdValue, value); }
        }

        private int WellsToDecommissionValue;
        public int WellsToDecommission
        {
            get { return this.WellsToDecommissionValue; }
            set { SetProperty(ref WellsToDecommissionValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<PortfolioWellDecommissioning> WellAnalysesValue = new();
        public List<PortfolioWellDecommissioning> WellAnalyses
        {
            get { return this.WellAnalysesValue; }
            set { SetProperty(ref WellAnalysesValue, value); }
        }

        private double TotalEstimatedCostValue;
        public double TotalEstimatedCost
        {
            get { return this.TotalEstimatedCostValue; }
            set { SetProperty(ref TotalEstimatedCostValue, value); }
        }

        private int TotalEstimatedDaysValue;
        public int TotalEstimatedDays
        {
            get { return this.TotalEstimatedDaysValue; }
            set { SetProperty(ref TotalEstimatedDaysValue, value); }
        }

        private double AverageCostPerWellValue;
        public double AverageCostPerWell
        {
            get { return this.AverageCostPerWellValue; }
            set { SetProperty(ref AverageCostPerWellValue, value); }
        }

        private double AverageDaysPerWellValue;
        public double AverageDaysPerWell
        {
            get { return this.AverageDaysPerWellValue; }
            set { SetProperty(ref AverageDaysPerWellValue, value); }
        }

        private int PhaseCountValue;
        public int PhaseCount
        {
            get { return this.PhaseCountValue; }
            set { SetProperty(ref PhaseCountValue, value); }
        }

        private int WellsPerPhaseValue;
        public int WellsPerPhase
        {
            get { return this.WellsPerPhaseValue; }
            set { SetProperty(ref WellsPerPhaseValue, value); }
        }

        private double ContingencyPercentageValue;
        public double ContingencyPercentage
        {
            get { return this.ContingencyPercentageValue; }
            set { SetProperty(ref ContingencyPercentageValue, value); }
        }

        private double TotalWithContingencyValue;
        public double TotalWithContingency
        {
            get { return this.TotalWithContingencyValue; }
            set { SetProperty(ref TotalWithContingencyValue, value); }
        }
    }
}
