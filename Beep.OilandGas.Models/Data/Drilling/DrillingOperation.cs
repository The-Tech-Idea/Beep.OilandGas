using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public class DrillingOperation : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;
        public string OperationId
        {
            get { return this.OperationIdValue; }
            set { SetProperty(ref OperationIdValue, value); }
        }
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }
        private string WellNameValue = string.Empty;
        public string WellName
        {
            get { return this.WellNameValue; }
            set { SetProperty(ref WellNameValue, value); }
        }
        private DateTime? SpudDateValue;
        public DateTime? SpudDate
        {
            get { return this.SpudDateValue; }
            set { SetProperty(ref SpudDateValue, value); }
        }
        private DateTime? CompletionDateValue;
        public DateTime? CompletionDate
        {
            get { return this.CompletionDateValue; }
            set { SetProperty(ref CompletionDateValue, value); }
        }
        private string? StatusValue;
        public string? Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
        private decimal? CurrentDepthValue;
        public decimal? CurrentDepth
        {
            get { return this.CurrentDepthValue; }
            set { SetProperty(ref CurrentDepthValue, value); }
        }
        private decimal? TargetDepthValue;
        public decimal? TargetDepth
        {
            get { return this.TargetDepthValue; }
            set { SetProperty(ref TargetDepthValue, value); }
        }
        private string? DrillingContractorValue;
        public string? DrillingContractor
        {
            get { return this.DrillingContractorValue; }
            set { SetProperty(ref DrillingContractorValue, value); }
        }
        private string? RigNameValue;
        public string? RigName
        {
            get { return this.RigNameValue; }
            set { SetProperty(ref RigNameValue, value); }
        }
        private decimal? DailyCostValue;
        public decimal? DailyCost
        {
            get { return this.DailyCostValue; }
            set { SetProperty(ref DailyCostValue, value); }
        }
        private decimal? TotalCostValue;
        public decimal? TotalCost
        {
            get { return this.TotalCostValue; }
            set { SetProperty(ref TotalCostValue, value); }
        }
        private string? CurrencyValue;
        public string? Currency
        {
            get { return this.CurrencyValue; }
            set { SetProperty(ref CurrencyValue, value); }
        }
        private List<DrillingReport> ReportsValue = new();
        public List<DrillingReport> Reports
        {
            get { return this.ReportsValue; }
            set { SetProperty(ref ReportsValue, value); }
        }
    }
}
