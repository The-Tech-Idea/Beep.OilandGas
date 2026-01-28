using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class WaterfloodPerformanceAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
        private double RecoveryFactorValue;

        public double RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private double IncrementalRecoveryFactorValue;

        public double IncrementalRecoveryFactor

        {

            get { return this.IncrementalRecoveryFactorValue; }

            set { SetProperty(ref IncrementalRecoveryFactorValue, value); }

        }
        private double PressureMaintenanceEfficiencyValue;

        public double PressureMaintenanceEfficiency

        {

            get { return this.PressureMaintenanceEfficiencyValue; }

            set { SetProperty(ref PressureMaintenanceEfficiencyValue, value); }

        }
        private WaterCutTrend WaterCutTrendValue;

        public WaterCutTrend WaterCutTrend

        {

            get { return this.WaterCutTrendValue; }

            set { SetProperty(ref WaterCutTrendValue, value); }

        }
        private double FloodFrontVelocityValue;

        public double FloodFrontVelocity

        {

            get { return this.FloodFrontVelocityValue; }

            set { SetProperty(ref FloodFrontVelocityValue, value); }

        }
        private double SweepEfficiencyValue;

        public double SweepEfficiency

        {

            get { return this.SweepEfficiencyValue; }

            set { SetProperty(ref SweepEfficiencyValue, value); }

        }
        private double ProjectedRecovery20YearsValue;

        public double ProjectedRecovery20Years

        {

            get { return this.ProjectedRecovery20YearsValue; }

            set { SetProperty(ref ProjectedRecovery20YearsValue, value); }

        }
    }
}
