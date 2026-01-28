using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RiskAnalysisResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private int ProjectDurationValue;

        public int ProjectDuration

        {

            get { return this.ProjectDurationValue; }

            set { SetProperty(ref ProjectDurationValue, value); }

        }
        private double ProjectedNPVValue;

        public double ProjectedNPV

        {

            get { return this.ProjectedNPVValue; }

            set { SetProperty(ref ProjectedNPVValue, value); }

        }
        private List<RiskItem> TechnicalRisksValue;

        public List<RiskItem> TechnicalRisks

        {

            get { return this.TechnicalRisksValue; }

            set { SetProperty(ref TechnicalRisksValue, value); }

        }
        private List<RiskItem> CommercialRisksValue;

        public List<RiskItem> CommercialRisks

        {

            get { return this.CommercialRisksValue; }

            set { SetProperty(ref CommercialRisksValue, value); }

        }
        private List<RiskItem> OperationalRisksValue;

        public List<RiskItem> OperationalRisks

        {

            get { return this.OperationalRisksValue; }

            set { SetProperty(ref OperationalRisksValue, value); }

        }
        private List<RiskItem> RegulatoryRisksValue;

        public List<RiskItem> RegulatoryRisks

        {

            get { return this.RegulatoryRisksValue; }

            set { SetProperty(ref RegulatoryRisksValue, value); }

        }
        private List<RiskItem> EnvironmentalRisksValue;

        public List<RiskItem> EnvironmentalRisks

        {

            get { return this.EnvironmentalRisksValue; }

            set { SetProperty(ref EnvironmentalRisksValue, value); }

        }
        private int TotalIdentifiedRisksValue;

        public int TotalIdentifiedRisks

        {

            get { return this.TotalIdentifiedRisksValue; }

            set { SetProperty(ref TotalIdentifiedRisksValue, value); }

        }
        private List<RiskItem> HighPriorityRisksValue;

        public List<RiskItem> HighPriorityRisks

        {

            get { return this.HighPriorityRisksValue; }

            set { SetProperty(ref HighPriorityRisksValue, value); }

        }
        private List<string> MitigationStrategiesValue;

        public List<string> MitigationStrategies

        {

            get { return this.MitigationStrategiesValue; }

            set { SetProperty(ref MitigationStrategiesValue, value); }

        }
        private double ContingencyReserveValue;

        public double ContingencyReserve

        {

            get { return this.ContingencyReserveValue; }

            set { SetProperty(ref ContingencyReserveValue, value); }

        }
        private string OverallRiskRatingValue;

        public string OverallRiskRating

        {

            get { return this.OverallRiskRatingValue; }

            set { SetProperty(ref OverallRiskRatingValue, value); }

        }
    }
}
