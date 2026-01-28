using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentCostAnalysisResult : ModelEntityBase
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
        private int WellCountValue;

        public int WellCount

        {

            get { return this.WellCountValue; }

            set { SetProperty(ref WellCountValue, value); }

        }
        private double DrillingCostPerWellValue;

        public double DrillingCostPerWell

        {

            get { return this.DrillingCostPerWellValue; }

            set { SetProperty(ref DrillingCostPerWellValue, value); }

        }
        private double FacilityCapexValue;

        public double FacilityCapex

        {

            get { return this.FacilityCapexValue; }

            set { SetProperty(ref FacilityCapexValue, value); }

        }
        private double TotalDrillingCostValue;

        public double TotalDrillingCost

        {

            get { return this.TotalDrillingCostValue; }

            set { SetProperty(ref TotalDrillingCostValue, value); }

        }
        private double CompletionCostValue;

        public double CompletionCost

        {

            get { return this.CompletionCostValue; }

            set { SetProperty(ref CompletionCostValue, value); }

        }
        private double EhsAndPermitsValue;

        public double EhsAndPermits

        {

            get { return this.EhsAndPermitsValue; }

            set { SetProperty(ref EhsAndPermitsValue, value); }

        }
        private double TotalCapexValue;

        public double TotalCapex

        {

            get { return this.TotalCapexValue; }

            set { SetProperty(ref TotalCapexValue, value); }

        }
        private double AnnualLaborCostValue;

        public double AnnualLaborCost

        {

            get { return this.AnnualLaborCostValue; }

            set { SetProperty(ref AnnualLaborCostValue, value); }

        }
        private double AnnualMaterialsCostValue;

        public double AnnualMaterialsCost

        {

            get { return this.AnnualMaterialsCostValue; }

            set { SetProperty(ref AnnualMaterialsCostValue, value); }

        }
        private double AnnualMaintenanceValue;

        public double AnnualMaintenance

        {

            get { return this.AnnualMaintenanceValue; }

            set { SetProperty(ref AnnualMaintenanceValue, value); }

        }
        private double AnnualOpexValue;

        public double AnnualOpex

        {

            get { return this.AnnualOpexValue; }

            set { SetProperty(ref AnnualOpexValue, value); }

        }
        public List<DevelopmentCostEscalationFactorEntry> CostEscalationFactors { get; set; } = new();
        private double ContingencyAllowanceValue;

        public double ContingencyAllowance

        {

            get { return this.ContingencyAllowanceValue; }

            set { SetProperty(ref ContingencyAllowanceValue, value); }

        }
        private double TotalProjectCostValue;

        public double TotalProjectCost

        {

            get { return this.TotalProjectCostValue; }

            set { SetProperty(ref TotalProjectCostValue, value); }

        }
        private CostBreakdownResult CostBreakdownValue;

        public CostBreakdownResult CostBreakdown

        {

            get { return this.CostBreakdownValue; }

            set { SetProperty(ref CostBreakdownValue, value); }

        }
    }
}
