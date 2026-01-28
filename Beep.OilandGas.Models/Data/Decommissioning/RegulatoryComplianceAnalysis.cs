using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RegulatoryComplianceAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string JurisdictionValue = string.Empty;
        public string Jurisdiction
        {
            get { return this.JurisdictionValue; }
            set { SetProperty(ref JurisdictionValue, value); }
        }

        private string WellClassValue = string.Empty;
        public string WellClass
        {
            get { return this.WellClassValue; }
            set { SetProperty(ref WellClassValue, value); }
        }

        private DateTime AbandonmentDateValue;
        public DateTime AbandonmentDate
        {
            get { return this.AbandonmentDateValue; }
            set { SetProperty(ref AbandonmentDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> ApplicableRegulationsValue = new();
        public List<string> ApplicableRegulations
        {
            get { return this.ApplicableRegulationsValue; }
            set { SetProperty(ref ApplicableRegulationsValue, value); }
        }

        private List<string> ComplianceRequirementsValue = new();
        public List<string> ComplianceRequirements
        {
            get { return this.ComplianceRequirementsValue; }
            set { SetProperty(ref ComplianceRequirementsValue, value); }
        }

        private DateTime ComplianceDeadlineDateValue;
        public DateTime ComplianceDeadlineDate
        {
            get { return this.ComplianceDeadlineDateValue; }
            set { SetProperty(ref ComplianceDeadlineDateValue, value); }
        }

        private List<string> RequiredDocumentationValue = new();
        public List<string> RequiredDocumentation
        {
            get { return this.RequiredDocumentationValue; }
            set { SetProperty(ref RequiredDocumentationValue, value); }
        }

        private string BondingRequirementsValue = string.Empty;
        public string BondingRequirements
        {
            get { return this.BondingRequirementsValue; }
            set { SetProperty(ref BondingRequirementsValue, value); }
        }

        private List<string> InspectionRequirementsValue = new();
        public List<string> InspectionRequirements
        {
            get { return this.InspectionRequirementsValue; }
            set { SetProperty(ref InspectionRequirementsValue, value); }
        }

        private double ComplianceCostEstimateValue;
        public double ComplianceCostEstimate
        {
            get { return this.ComplianceCostEstimateValue; }
            set { SetProperty(ref ComplianceCostEstimateValue, value); }
        }
    }
}
