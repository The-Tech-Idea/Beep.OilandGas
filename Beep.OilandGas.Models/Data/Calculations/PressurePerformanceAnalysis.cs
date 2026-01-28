using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PressurePerformanceAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private double InitialReservoirPressureValue;

        public double InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }
        private double CurrentReservoirPressureValue;

        public double CurrentReservoirPressure

        {

            get { return this.CurrentReservoirPressureValue; }

            set { SetProperty(ref CurrentReservoirPressureValue, value); }

        }
        private double InjectionRateValue;

        public double InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private int OperationMonthsValue;

        public int OperationMonths

        {

            get { return this.OperationMonthsValue; }

            set { SetProperty(ref OperationMonthsValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double PressureChangeValue;

        public double PressureChange

        {

            get { return this.PressureChangeValue; }

            set { SetProperty(ref PressureChangeValue, value); }

        }
        private double PressureChangePerMonthValue;

        public double PressureChangePerMonth

        {

            get { return this.PressureChangePerMonthValue; }

            set { SetProperty(ref PressureChangePerMonthValue, value); }

        }
        private double EffectiveCompressibilityValue;

        public double EffectiveCompressibility

        {

            get { return this.EffectiveCompressibilityValue; }

            set { SetProperty(ref EffectiveCompressibilityValue, value); }

        }
        private double PressureMaintenanceEfficiencyValue;

        public double PressureMaintenanceEfficiency

        {

            get { return this.PressureMaintenanceEfficiencyValue; }

            set { SetProperty(ref PressureMaintenanceEfficiencyValue, value); }

        }
        private double ProjectedPressure12MonthsValue;

        public double ProjectedPressure12Months

        {

            get { return this.ProjectedPressure12MonthsValue; }

            set { SetProperty(ref ProjectedPressure12MonthsValue, value); }

        }
        private double PressureGradientToBoundaryValue;

        public double PressureGradientToBoundary

        {

            get { return this.PressureGradientToBoundaryValue; }

            set { SetProperty(ref PressureGradientToBoundaryValue, value); }

        }
        private string OverPressureRiskValue;

        public string OverPressureRisk

        {

            get { return this.OverPressureRiskValue; }

            set { SetProperty(ref OverPressureRiskValue, value); }

        }
    }
}
